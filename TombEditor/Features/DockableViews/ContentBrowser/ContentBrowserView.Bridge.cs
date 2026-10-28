#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using TombLib.Controls;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Imaging;
using TombLib.Wad;

namespace TombEditor.Features.DockableViews.ContentBrowser;

/// <summary>
/// Editor bridge for <see cref="ContentBrowserView"/>. The WPF shell calls
/// <see cref="AttachToEditor"/> to wire up editor events, thumbnail rendering, drag-and-drop,
/// and ViewModel callbacks. <see cref="Cleanup"/> tears the bridge back down.
/// </summary>
public partial class ContentBrowserView
{
    private const int ThumbnailBatchSize = 10;

    private static readonly Logger _bridgeLogger = LogManager.GetCurrentClassLogger();

    private Editor? _bridgeEditor;
    private ContentBrowserViewModel? _bridgeViewModel;
    private OffscreenItemRenderer? _bridgeRenderer;
    private DispatcherTimer? _bridgeThumbnailTimer;
    private List<AssetItemViewModel>? _bridgeThumbnailQueue;
    private int _bridgeThumbnailQueueIndex;
    private bool _bridgeSuppressEditorSync;
    private bool _bridgeRefreshPending;
    private Action<object>? _bridgeStartDrag;

    public void AttachToEditor(Editor editor, Action<object> startDrag)
    {
        if (_bridgeEditor is not null)
            return;

        _bridgeEditor = editor;
        _bridgeStartDrag = startDrag;
        _bridgeViewModel = new ContentBrowserViewModel();
        DataContext = _bridgeViewModel;

        _bridgeViewModel.SelectedItemsChanged += BridgeOnSelectedItemsChanged;
        _bridgeViewModel.DragDropRequested += BridgeOnDragDropRequested;
        _bridgeViewModel.ThumbnailRenderRequested += BridgeOnThumbnailRenderRequested;
        _bridgeViewModel.LocateItemRequested += BridgeOnLocateItemRequested;
        _bridgeViewModel.AddItemRequested += BridgeOnAddItemRequested;
        _bridgeViewModel.AddWadRequested += BridgeOnAddWadRequested;
        _bridgeViewModel.FavoriteToggled += BridgeOnFavoriteToggled;
        _bridgeViewModel.FilesDropped += BridgeOnFilesDropped;
        _bridgeViewModel.PropertyChanged += BridgeOnViewModelPropertyChanged;

        _bridgeViewModel.TileWidth = (double)_bridgeEditor.Configuration.ContentBrowser_TileWidth;

        // Batched thumbnail rendering on the UI thread (D3D11 cannot be touched off-thread).
        _bridgeThumbnailTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(50),
        };
        _bridgeThumbnailTimer.Tick += BridgeOnThumbnailTimerTick;


        ViewportScrolled += BridgeOnViewportScrolled;

        _bridgeEditor.EditorEventRaised += BridgeOnEditorEventRaised;
    }

    public void Cleanup()
    {
        if (_bridgeEditor is null)
            return;

        _bridgeEditor.EditorEventRaised -= BridgeOnEditorEventRaised;
        _bridgeEditor = null;

        if (_bridgeViewModel is not null)
        {
            _bridgeViewModel.SelectedItemsChanged -= BridgeOnSelectedItemsChanged;
            _bridgeViewModel.DragDropRequested -= BridgeOnDragDropRequested;
            _bridgeViewModel.ThumbnailRenderRequested -= BridgeOnThumbnailRenderRequested;
            _bridgeViewModel.LocateItemRequested -= BridgeOnLocateItemRequested;
            _bridgeViewModel.AddItemRequested -= BridgeOnAddItemRequested;
            _bridgeViewModel.AddWadRequested -= BridgeOnAddWadRequested;
            _bridgeViewModel.FavoriteToggled -= BridgeOnFavoriteToggled;
            _bridgeViewModel.FilesDropped -= BridgeOnFilesDropped;
            _bridgeViewModel.PropertyChanged -= BridgeOnViewModelPropertyChanged;
            _bridgeViewModel = null;
        }

        ViewportScrolled -= BridgeOnViewportScrolled;

        if (_bridgeThumbnailTimer is not null)
        {
            _bridgeThumbnailTimer.Tick -= BridgeOnThumbnailTimerTick;
            _bridgeThumbnailTimer.Stop();
            _bridgeThumbnailTimer = null;
        }

        _bridgeThumbnailQueue = null;
        _bridgeThumbnailQueueIndex = 0;

        _bridgeRenderer?.Dispose();
        _bridgeRenderer = null;

        _bridgeStartDrag = null;
    }

    private void BridgeOnDragDropRequested(object? sender, IReadOnlyList<AssetItemViewModel> items)
    {
        if (_bridgeStartDrag is null || items.Count == 0)
            return;

        if (items.Count == 1)
        {
            if (items[0].WadObject is { } wadObject)
                _bridgeStartDrag(wadObject);
        }
        else
        {
            var wadObjects = items
                .Where(i => i.WadObject is not null)
                .Select(i => i.WadObject!)
                .ToArray();
            if (wadObjects.Length > 0)
                _bridgeStartDrag(wadObjects);
        }
    }

    private void BridgeOnLocateItemRequested(object? sender, AssetItemViewModel item)
    {
        if (_bridgeEditor is null)
            return;

        if (item.WadObject is ImportedGeometry geo)
        {
            EditorActions.FindImportedGeometry(geo);
        }
        else if (item.WadObject is WadMoveable or WadStatic)
        {
            _bridgeEditor.ChosenItems = new[] { item.WadObject };
            EditorActions.FindItem();
        }

        ScrollToItem(item);
    }

    private void BridgeOnAddItemRequested(object? sender, EventArgs e)
    {
        if (_bridgeEditor is null || _bridgeViewModel?.SelectedItem is not { } selected)
            return;

        string command = selected.WadObject is ImportedGeometry ? "AddImportedGeometry" : "AddItem";

        CommandHandler.GetCommand(command).Execute?.Invoke(new CommandArgs
        {
            Editor = _bridgeEditor,
            Window = WPFUtils.GetWin32WindowOwner(),
        });

        // Validation may have rejected the action; restore the tile animation so the icon
        // doesn't stay grayed out forever.
        if (_bridgeEditor.Action is not EditorActionPlace)
            RestoreLastAnimation();
    }

    private void BridgeOnAddWadRequested(object? sender, EventArgs e)
    {
        EditorActions.AddWad(WPFUtils.GetWin32WindowOwner(), null);
    }

    private void BridgeOnFavoriteToggled(object? sender, AssetItemViewModel item)
    {
        var settings = _bridgeEditor?.Level?.Settings;
        if (settings is null)
            return;

        if (item.IsFavorite)
            settings.Favorites.Add(item.FavoriteKey);
        else
            settings.Favorites.Remove(item.FavoriteKey);
    }

    private void BridgeOnFilesDropped(object? sender, string[] files)
    {
        if (_bridgeEditor?.Level?.Settings is not { } settings || files.Length == 0)
            return;

        var owner = WPFUtils.GetWin32WindowOwner();

        var wadFiles = files
            .Where(f => Wad2.FileExtensions.Matches(f))
            .Select(f => settings.MakeRelative(f, VariableType.LevelDirectory))
            .ToList();

        if (wadFiles.Count > 0)
            EditorActions.AddWad(owner, wadFiles);

        foreach (var file in files.Where(f => BaseGeometryImporter.FileExtensions.Matches(f)))
            EditorActions.AddImportedGeometry(owner, settings.MakeRelative(file, VariableType.LevelDirectory));
    }

    private void BridgeOnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_bridgeEditor is not null
            && _bridgeViewModel is not null
            && e.PropertyName == nameof(ContentBrowserViewModel.TileWidth))
        {
            _bridgeEditor.Configuration.ContentBrowser_TileWidth = (float)_bridgeViewModel.TileWidth;
        }
    }

    private void BridgeOnThumbnailRenderRequested(object? sender, EventArgs e)
        => BridgeQueueVisibleThumbnails();

    private void BridgeOnViewportScrolled(object? sender, EventArgs e)
        => BridgeQueueVisibleThumbnails();

    private void BridgeQueueVisibleThumbnails()
    {
        if (_bridgeViewModel is null)
            return;

        var visibleItems = GetVisibleItems()
            .Where(i => i.Thumbnail is null && !_bridgeViewModel.HasCachedThumbnail(i))
            .ToList();

        if (visibleItems.Count == 0)
            return;

        _bridgeThumbnailQueue = visibleItems;
        _bridgeThumbnailQueueIndex = 0;
        _bridgeThumbnailTimer?.Stop();
        _bridgeThumbnailTimer?.Start();
    }

    private void BridgeOnThumbnailTimerTick(object? sender, EventArgs e)
    {
        if (_bridgeThumbnailQueue is null || _bridgeThumbnailQueueIndex >= _bridgeThumbnailQueue.Count)
        {
            _bridgeThumbnailTimer?.Stop();
            _bridgeThumbnailQueue = null;
            _bridgeThumbnailQueueIndex = 0;
            _bridgeRenderer?.GarbageCollect();
            return;
        }

        BridgeRenderThumbnailBatch();
    }

    private void BridgeRenderThumbnailBatch()
    {
        if (_bridgeEditor?.Level?.Settings is not { } settings
            || _bridgeViewModel is null
            || _bridgeThumbnailQueue is null)
            return;

        try
        {
            _bridgeRenderer ??= new OffscreenItemRenderer();

            int end = Math.Min(_bridgeThumbnailQueueIndex + ThumbnailBatchSize, _bridgeThumbnailQueue.Count);
            for (int i = _bridgeThumbnailQueueIndex; i < end; i++)
            {
                var item = _bridgeThumbnailQueue[i];

                try
                {
                    if (item.Thumbnail is not null)
                        continue;

                    var renderObject = WadObjectRenderHelper.GetRenderObject(item.WadObject, settings);
                    var image = _bridgeRenderer.RenderThumbnail(
                        renderObject,
                        settings.GameVersion,
                        _bridgeEditor.Configuration.UI_ColorScheme.Color3DBackground);

                    var bitmapSource = ImageSourceFactory.FromImage(image);
                    _bridgeViewModel.SetThumbnail(item, bitmapSource);
                }
                catch (Exception ex)
                {
                    _bridgeLogger.Warn(ex, "Failed to render thumbnail for {0}.", item.Name);
                }
            }

            _bridgeThumbnailQueueIndex = end;
        }
        catch (Exception ex)
        {
            _bridgeLogger.Error(ex, "Thumbnail renderer failed.");

            _bridgeThumbnailTimer?.Stop();
            _bridgeThumbnailQueue = null;
            _bridgeThumbnailQueueIndex = 0;
            _bridgeRenderer?.Dispose();
            _bridgeRenderer = null;
        }
    }

    private void BridgeOnEditorEventRaised(IEditorEvent obj)
    {
        _bridgeRefreshPending = false;

        if (obj is Editor.LoadedWadsChangedEvent
            or Editor.LoadedImportedGeometriesChangedEvent
            or Editor.GameVersionChangedEvent
            or Editor.LevelChangedEvent)
        {
            // Wads/geometries changed - drop any in-flight thumbnail work; meshes may differ.
            if (obj is Editor.LoadedWadsChangedEvent or Editor.LoadedImportedGeometriesChangedEvent)
            {
                _bridgeThumbnailTimer?.Stop();
                _bridgeThumbnailQueue = null;
                _bridgeThumbnailQueueIndex = 0;

                _bridgeRenderer?.Dispose();
                _bridgeRenderer = null;
            }

            _bridgeRefreshPending = true;
        }

        // Rebuilding the asset list is expensive (every wad object gets a view model), and the
        // only configuration value it depends on is HideInternalObjects - skip the rebuild for
        // unrelated config changes (e.g. the draw-flag toolbar toggles).
        if (obj is Editor.ConfigurationChangedEvent
            && _bridgeEditor?.Configuration.RenderingItem_HideInternalObjects != _bridgeLastHideInternalObjects)
        {
            _bridgeRefreshPending = true;
        }

        if (obj is Editor.ChosenItemsChangedEvent itemsChanged && !_bridgeSuppressEditorSync)
        {
            var first = itemsChanged.Current?.FirstOrDefault();
            if (first is not null)
                BridgeSelectWadObject(first);
        }

        if (obj is Editor.ActionChangedEvent actionEvent
            && actionEvent.Previous is EditorActionPlace
            && actionEvent.Current is not EditorActionPlace)
        {
            RestoreLastAnimation();
        }

        if (obj is Editor.InitEvent)
            _bridgeRefreshPending = true;

        if (_bridgeRefreshPending)
            BridgeRefreshAssets();
    }

    private bool _bridgeLastHideInternalObjects;

    private void BridgeRefreshAssets()
    {
        if (_bridgeEditor?.Level?.Settings is not { } settings || _bridgeViewModel is null)
            return;

        _bridgeLastHideInternalObjects = _bridgeEditor.Configuration.RenderingItem_HideInternalObjects;
        _bridgeViewModel.RefreshAssets(settings, _bridgeLastHideInternalObjects);
    }

    private void BridgeOnSelectedItemsChanged(object? sender, IReadOnlyList<AssetItemViewModel> items)
    {
        if (_bridgeEditor is null)
            return;

        if (items.Count == 0)
        {
            _bridgeSuppressEditorSync = true;
            try
            {
                var potential = _bridgeEditor.GetFirstWadObject();
                _bridgeEditor.ChosenItems = potential is not null
                    ? new[] { potential }
                    : Array.Empty<IWadObject>();
            }
            finally
            {
                _bridgeSuppressEditorSync = false;
            }
            return;
        }

        var wadObjects = items
            .Where(vm => vm.WadObject is not null)
            .Select(vm => vm.WadObject!)
            .ToArray();
        if (wadObjects.Length == 0)
            return;

        _bridgeSuppressEditorSync = true;
        try
        {
            _bridgeEditor.ChosenItems = wadObjects;
        }
        finally
        {
            _bridgeSuppressEditorSync = false;
        }

        ScrollToItem(items[0]);
    }

    private void BridgeSelectWadObject(IWadObject wadObject)
    {
        if (_bridgeViewModel is null)
            return;

        // Detach to break the feedback loop while we mutate selection programmatically.
        _bridgeViewModel.SelectedItemsChanged -= BridgeOnSelectedItemsChanged;
        try
        {
            var item = _bridgeViewModel.AllItems.FirstOrDefault(i => ReferenceEquals(i.WadObject, wadObject));

            _bridgeViewModel.SelectedItem = item;
            SetSelectionSilently(item);
            ScrollToItem(item);
        }
        finally
        {
            _bridgeViewModel.SelectedItemsChanged += BridgeOnSelectedItemsChanged;
        }
    }
}
