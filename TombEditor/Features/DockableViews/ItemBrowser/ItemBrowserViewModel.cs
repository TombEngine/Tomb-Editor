#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TombEditor.Features.DockableViews.ResourcePreview;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.ItemBrowser;

public sealed class ItemBrowserEntry
{
    public IWadObject WadObject { get; }
    public string DisplayName { get; }

    public ItemBrowserEntry(IWadObject wadObject, TRVersion.Game gameVersion)
    {
        WadObject = wadObject;
        DisplayName = wadObject.ToString(gameVersion);
    }
}

public partial class ItemBrowserViewModel : ObservableObject
{
    private readonly Editor _editor;
    private bool _suppressEditorSync;
    private bool _disposed;
    private bool _hideInternalObjects;

    /// <summary>Gets the preview loading state and resource replacement action.</summary>
    public ResourcePreviewViewModel Preview { get; }

    public ObservableCollection<ItemBrowserEntry> Items { get; } = new();

    private ItemBrowserEntry? _selectedItem;
    public ItemBrowserEntry? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value))
                return;

            ((RelayCommand)PreviousCommand).NotifyCanExecuteChanged();
            ((RelayCommand)NextCommand).NotifyCanExecuteChanged();

            UpdateFromWadLabel(value?.WadObject);
            SelectedItemChanged?.Invoke(this, value?.WadObject);

            if (_suppressEditorSync)
                return;

            if (value?.WadObject is { } wadObject)
                _editor.ChosenItems = new[] { wadObject };
        }
    }

    private string _fromWadText = string.Empty;
    public string FromWadText
    {
        get => _fromWadText;
        private set => SetProperty(ref _fromWadText, value);
    }

    private string _fromWadTooltip = string.Empty;
    public string FromWadTooltip
    {
        get => _fromWadTooltip;
        private set => SetProperty(ref _fromWadTooltip, value);
    }

    private bool _showFromWad;
    public bool ShowFromWad
    {
        get => _showFromWad;
        private set => SetProperty(ref _showFromWad, value);
    }

    // Raised when SelectedItem changes; the View applies FindLaraSkin / sets panelItem.CurrentObject.
    public event EventHandler<IWadObject?>? SelectedItemChanged;

    public ICommand AddCommand { get; }
    public ICommand LocateCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }

    public ItemBrowserViewModel(Editor editor)
    {
        _editor = editor;
        Preview = new ResourcePreviewViewModel(editor, importedGeometry: false);
        _editor.EditorEventRaised += OnEditorEventRaised;

        AddCommand = CommandHandler.GetCommand(
            "AddItem",
            () => new CommandArgs { Editor = _editor, Window = WPFUtils.GetWindowOwner() });

        LocateCommand = CommandHandler.GetCommand(
            "LocateItem",
            () => new CommandArgs { Editor = _editor, Window = WPFUtils.GetWindowOwner() });

        PreviousCommand = new RelayCommand(SelectPrevious, () => Items.Count > 0);
        NextCommand = new RelayCommand(SelectNext, () => Items.Count > 0);
    }

    public void Cleanup()
    {
        if (_disposed)
            return;
        _disposed = true;
        Preview.Dispose();
        _editor.EditorEventRaised -= OnEditorEventRaised;
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        bool hideInternalObjectsChanged = obj is Editor.ConfigurationChangedEvent &&
                                          _editor.Configuration.RenderingItem_HideInternalObjects != _hideInternalObjects;

        if (obj is Editor.LoadedWadsChangedEvent or Editor.GameVersionChangedEvent or Editor.LevelChangedEvent or Editor.InitEvent
            || hideInternalObjectsChanged)
        {
            RebuildList();
        }

        if (obj is Editor.ChosenItemsChangedEvent)
        {
            var wadObject = _editor.GetFirstWadObject();
            if (wadObject is null)
                return;

            _suppressEditorSync = true;
            try
            {
                SelectedItem = Items.FirstOrDefault(i => Equals(i.WadObject, wadObject));
            }
            finally
            {
                _suppressEditorSync = false;
            }
        }

        if (obj is Editor.ConfigurationChangedEvent || obj is Editor.InitEvent)
            ShowFromWad = _editor.Configuration.RenderingItem_ShowMultipleWadsPrompt;
    }

    private void RebuildList()
    {
        _suppressEditorSync = true;
        try
        {
            if (_editor.Level?.Settings is not { } settings)
            {
                Items.Clear();
                return;
            }

            var previousObject = SelectedItem?.WadObject ?? _editor.GetFirstWadObject();
            var gameVersion = settings.GameVersion;
            var hideInternal = _editor.Configuration.RenderingItem_HideInternalObjects;
            _hideInternalObjects = hideInternal;

            Items.Clear();

            foreach (var moveable in settings.WadGetAllMoveables().Values)
            {
                if (hideInternal && TrCatalog.IsHidden(gameVersion, moveable.Id.TypeId))
                    continue;
                Items.Add(new ItemBrowserEntry(moveable, gameVersion));
            }

            foreach (var staticMesh in settings.WadGetAllStatics().Values)
                Items.Add(new ItemBrowserEntry(staticMesh, gameVersion));

            if (Items.Count == 0)
            {
                SelectedItem = null;
                return;
            }

            SelectedItem =
                Items.FirstOrDefault(i => Equals(i.WadObject, previousObject))
                ?? Items.FirstOrDefault();
        }
        finally
        {
            _suppressEditorSync = false;
            ((RelayCommand)PreviousCommand).NotifyCanExecuteChanged();
            ((RelayCommand)NextCommand).NotifyCanExecuteChanged();
        }
    }

    private void UpdateFromWadLabel(IWadObject? wadObject)
    {
        if (wadObject is null || _editor.Level?.Settings is not { } settings)
        {
            FromWadText = string.Empty;
            FromWadTooltip = string.Empty;
            return;
        }

        ItemType? itemType = wadObject switch
        {
            WadMoveable m => new ItemType(m.Id, settings),
            WadStatic s => new ItemType(s.Id, settings),
            _ => null
        };

        if (itemType is null)
        {
            FromWadText = string.Empty;
            FromWadTooltip = string.Empty;
            return;
        }

        var wad = settings.WadTryGetWad(itemType.Value, out bool multiple);
        if (wad is null)
        {
            FromWadText = string.Empty;
            FromWadTooltip = string.Empty;
            return;
        }

        FromWadText = "From " + Path.GetFileName(wad.Path);
        FromWadTooltip = (multiple
            ? "This object exists in several wads.\nUsed one is: "
            : "From: ") + settings.MakeAbsolute(wad.Path);
    }

    private void SelectPrevious()
    {
        if (Items.Count == 0)
            return;
        var index = SelectedItem is null ? 0 : Items.IndexOf(SelectedItem);
        SelectedItem = Items[(index - 1 + Items.Count) % Items.Count];
    }

    private void SelectNext()
    {
        if (Items.Count == 0)
            return;
        var index = SelectedItem is null ? -1 : Items.IndexOf(SelectedItem);
        SelectedItem = Items[(index + 1) % Items.Count];
    }
}
