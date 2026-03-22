using DarkUI.Docking;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Features.DockableViews.ContentBrowser;

public partial class ContentBrowser : DarkToolWindow
{
	private static readonly Logger logger = LogManager.GetCurrentClassLogger();

	private readonly Editor _editor;
	private readonly ContentBrowserViewModel _viewModel;
	private OffscreenItemRenderer _renderer;

	private Timer _thumbnailTimer;
	private List<AssetItemViewModel> _thumbnailQueue;
	private int _thumbnailQueueIndex;

	private bool _suppressEditorSync;

	private bool _refreshPending;

	private const int ThumbnailBatchSize = 10;

	public ContentBrowser()
	{
		InitializeComponent();

		_editor = Editor.Instance;
		_viewModel = new ContentBrowserViewModel();

		// Set the WPF view's DataContext.
		contentBrowserView.DataContext = _viewModel;

		_viewModel.SelectedItemsChanged += ViewModel_SelectedItemsChanged;
		_viewModel.DragDropRequested += ViewModel_DragDropRequested;
		_viewModel.ThumbnailRenderRequested += ViewModel_ThumbnailRenderRequested;
		_viewModel.LocateItemRequested += ViewModel_LocateItemRequested;
		_viewModel.AddItemRequested += ViewModel_AddItemRequested;
		_viewModel.AddWadRequested += ViewModel_AddWadRequested;
		_viewModel.PropertyChanged += ViewModel_PropertyChanged;
		_viewModel.FavoriteToggled += ViewModel_FavoriteToggled;

		// Load saved tile width from configuration.
		_viewModel.TileWidth = (double)_editor.Configuration.ContentBrowser_TileWidth;

		// Timer for batched/deferred thumbnail rendering (50ms between batches).
		_thumbnailTimer = new Timer { Interval = 50 };
		_thumbnailTimer.Tick += ThumbnailTimer_Tick;

		// Accept file drops from Windows Explorer (WAD files and 3D geometry files).
		AllowDrop = true;
		_viewModel.FilesDropped += ViewModel_FilesDropped;

		// Subscribe to viewport scroll for lazy thumbnail rendering.
		contentBrowserView.ViewportScrolled += ContentBrowserView_ViewportScrolled;

		// Subscribe to editor events.
		_editor.EditorEventRaised += EditorEventRaised;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_editor.EditorEventRaised -= EditorEventRaised;

			_viewModel.SelectedItemsChanged -= ViewModel_SelectedItemsChanged;
			_viewModel.DragDropRequested -= ViewModel_DragDropRequested;
			_viewModel.ThumbnailRenderRequested -= ViewModel_ThumbnailRenderRequested;
			_viewModel.LocateItemRequested -= ViewModel_LocateItemRequested;
			_viewModel.AddItemRequested -= ViewModel_AddItemRequested;
			_viewModel.AddWadRequested -= ViewModel_AddWadRequested;
			_viewModel.PropertyChanged -= ViewModel_PropertyChanged;
			_viewModel.FavoriteToggled -= ViewModel_FavoriteToggled;
			_viewModel.FilesDropped -= ViewModel_FilesDropped;

			contentBrowserView.ViewportScrolled -= ContentBrowserView_ViewportScrolled;

			_thumbnailTimer?.Stop();
			_thumbnailTimer?.Dispose();
			_thumbnailTimer = null;

			_renderer?.Dispose();
			_renderer = null;
		}

		if (disposing && components is not null)
			components.Dispose();

		base.Dispose(disposing);
	}

	// Delegates drag-drop to WinForms host. Single item passes IWadObject directly.
	// multiple items are wrapped in an IWadObject[] array.
	private void ViewModel_DragDropRequested(object sender, IReadOnlyList<AssetItemViewModel> items)
	{
		if (items is null || items.Count == 0)
			return;

		if (items.Count == 1)
		{
			// Single item: pass IWadObject directly for backward compatibility.
			if (items[0].WadObject is not null)
				DoDragDrop(items[0].WadObject, DragDropEffects.Copy);
		}
		else
		{
			// Multiple items: pass as IWadObject array.
			var wadObjects = items.Where(i => i.WadObject is not null).Select(i => i.WadObject).ToArray();

			if (wadObjects.Length > 0)
				DoDragDrop(wadObjects, DragDropEffects.Copy);
		}
	}

	private void ViewModel_LocateItemRequested(object sender, AssetItemViewModel item)
	{
		if (item is null)
			return;

		if (item.WadObject is ImportedGeometry geo)
		{
			EditorActions.FindImportedGeometry(geo);
		}
		else if (item.WadObject is WadMoveable or WadStatic)
		{
			_editor.ChosenItems = new[] { item.WadObject };
			EditorActions.FindItem();
		}

		// Scroll to make the item visible in the list.
		contentBrowserView.ScrollToItem(item);
	}

	private void ViewModel_AddItemRequested(object sender, EventArgs e)
	{
		var selected = _viewModel.SelectedItem;

		if (selected is null)
			return;

		if (selected.WadObject is ImportedGeometry)
			_editor.Action = new EditorActionPlace(false, (l, r) => new ImportedGeometryInstance());
		else
			CommandHandler.GetCommand("AddItem").Execute?.Invoke(new CommandArgs { Editor = _editor, Window = FindForm() });

		// If the action was not set (e.g. validation failed), restore the tile animation immediately.
		if (_editor.Action is not EditorActionPlace)
			contentBrowserView.RestoreLastAnimation();
	}

	private void ViewModel_AddWadRequested(object sender, EventArgs e)
	{
		EditorActions.AddWad(this, null);
	}

	private void ViewModel_FavoriteToggled(object sender, AssetItemViewModel item)
	{
		var settings = _editor.Level?.Settings;

		if (settings is null)
			return;

		if (item.IsFavorite)
			settings.Favorites.Add(item.FavoriteKey);
		else
			settings.Favorites.Remove(item.FavoriteKey);
	}

	// Handles files dropped from Windows Explorer: WAD files are loaded as object archives.
	// 3D geometry files are added as imported geometry.
	private void ViewModel_FilesDropped(object sender, string[] files)
	{
		if (files is null || files.Length == 0)
			return;

		var wadFiles = files
			.Where(f => Wad2.FileExtensions.Matches(f))
			.Select(f => _editor.Level.Settings.MakeRelative(f, VariableType.LevelDirectory))
			.ToList();

		if (wadFiles.Count > 0)
			EditorActions.AddWad(this, wadFiles);

		foreach (var file in files.Where(f => BaseGeometryImporter.FileExtensions.Matches(f)))
			EditorActions.AddImportedGeometry(this, _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory));
	}

	private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ContentBrowserViewModel.TileWidth))
			_editor.Configuration.ContentBrowser_TileWidth = (float)_viewModel.TileWidth;
	}

	private void ViewModel_ThumbnailRenderRequested(object sender, EventArgs e)
	{
		// Defer rendering until scroll/layout provides visible items.
		QueueVisibleThumbnails();
	}

	private void ContentBrowserView_ViewportScrolled(object sender, EventArgs e)
	{
		QueueVisibleThumbnails();
	}

	// Queues only currently-visible items that still need thumbnails.
	private void QueueVisibleThumbnails()
	{
		var visibleItems = contentBrowserView.GetVisibleItems()
			.Where(i => i.Thumbnail is null && !_viewModel.HasCachedThumbnail(i))
			.ToList();

		if (visibleItems.Count == 0)
			return;

		_thumbnailQueue = visibleItems;
		_thumbnailQueueIndex = 0;
		_thumbnailTimer?.Stop();
		_thumbnailTimer?.Start();
	}

	private void ThumbnailTimer_Tick(object sender, EventArgs e)
	{
		if (_thumbnailQueue is null || _thumbnailQueueIndex >= _thumbnailQueue.Count)
		{
			// All items rendered; stop timer and clean up.
			_thumbnailTimer?.Stop();
			_thumbnailQueue = null;
			_thumbnailQueueIndex = 0;
			_renderer?.GarbageCollect();
			return;
		}

		RenderThumbnailBatch();
	}

	// Renders a small batch of thumbnails (up to ThumbnailBatchSize).
	// Called from the timer tick so D3D11 rendering stays on the UI thread.
	private void RenderThumbnailBatch()
	{
		try
		{
			_renderer ??= new OffscreenItemRenderer();

			int end = Math.Min(_thumbnailQueueIndex + ThumbnailBatchSize, _thumbnailQueue.Count);

			for (int i = _thumbnailQueueIndex; i < end; i++)
			{
				var item = _thumbnailQueue[i];

				try
				{
					// Skip items that already got a thumbnail (e.g. from cache restore).
					if (item.Thumbnail is not null)
						continue;

					// Apply Lara skin substitution for moveables (shared with ItemBrowser).
					var renderObject = WadObjectRenderHelper.GetRenderObject(item.WadObject, _editor.Level.Settings);

					var image = _renderer.RenderThumbnail(renderObject, _editor.Level.Settings.GameVersion, _editor.Configuration.UI_ColorScheme.Color3DBackground);
					var bitmapSource = AssetItemViewModel.ImageCToBitmapSource(image);
					_viewModel.SetThumbnail(item, bitmapSource);
				}
				catch (Exception ex)
				{
					logger.Warn(ex, "Failed to render thumbnail for {0}.", item.Name);
				}
			}

			_thumbnailQueueIndex = end;
		}
		catch (Exception ex)
		{
			// If renderer creation fails, stop rendering.
			logger.Error(ex, "Thumbnail renderer failed.");

			_thumbnailTimer?.Stop();
			_thumbnailQueue = null;
			_thumbnailQueueIndex = 0;
			_renderer?.Dispose();
			_renderer = null;
		}
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		_refreshPending = false;

		// Refresh asset list when wads, geometries, or game version change.
		if (obj is Editor.LoadedWadsChangedEvent or
			Editor.LoadedImportedGeometriesChangedEvent or
			Editor.GameVersionChangedEvent or
			Editor.LevelChangedEvent)
		{
			// Invalidate thumbnail cache when wads change since meshes may differ.
			if (obj is Editor.LoadedWadsChangedEvent or
				Editor.LoadedImportedGeometriesChangedEvent)
			{
				// Stop any in-progress rendering.
				_thumbnailTimer?.Stop();
				_thumbnailQueue = null;
				_thumbnailQueueIndex = 0;

				_renderer?.Dispose();
				_renderer = null;
			}

			_refreshPending = true;
		}

		// Also refresh on configuration change (game version display may change).
		if (obj is Editor.ConfigurationChangedEvent)
			_refreshPending = true;

		// Sync selection when items are chosen from other tool windows.
		if (obj is Editor.ChosenItemsChangedEvent itemsChanged && !_suppressEditorSync)
		{
			var first = itemsChanged.Current?.FirstOrDefault();

			if (first is not null)
				SelectWadObject(first);
		}

		// Update keyboard shortcuts.
		if (obj is Editor.ConfigurationChangedEvent configChanged)
		{
			if (configChanged.UpdateKeyboardShortcuts)
				CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
		}

		// Restore tile animation when the place action ends (object placed or action canceled).
		if (obj is Editor.ActionChangedEvent actionEvent)
		{
			if (actionEvent.Previous is EditorActionPlace && actionEvent.Current is not EditorActionPlace)
				contentBrowserView.RestoreLastAnimation();
		}

		// Activate default control.
		if (obj is Editor.DefaultControlActivationEvent activationEvent)
		{
			if (DockPanel is not null && activationEvent.ContainerName == GetType().Name)
				MakeActive();
		}

		// Initial load.
		if (obj is Editor.InitEvent)
			_refreshPending = true;

		// Coalesce multiple refresh triggers into a single call.
		if (_refreshPending)
			RefreshAssets();
	}

	private void RefreshAssets()
	{
		LevelSettings settings = _editor?.Level?.Settings;

		if (settings is null)
			return;

		_viewModel.RefreshAssets(settings, _editor.Configuration.RenderingItem_HideInternalObjects);
	}

	private void ViewModel_SelectedItemsChanged(object sender, IReadOnlyList<AssetItemViewModel> items)
	{
		// When the user clears the ContentBrowser selection, leave ChosenItems unchanged
		// so the legacy item browser and imported geometry browser still show the last chosen item.

		if (items.Count == 0)
		{
			_suppressEditorSync = true;
			var potentialSelection = _editor.GetFirstWadObject();
			if (potentialSelection != null)
				_editor.ChosenItems = new[] { potentialSelection };
			else
				_editor.ChosenItems = Array.Empty<IWadObject>();
			_suppressEditorSync = false;
			return;
		}

		var wadObjects = items
			.Where(vm => vm.WadObject is not null)
			.Select(vm => vm.WadObject)
			.ToArray();

		if (wadObjects.Length == 0)
			return;

		_suppressEditorSync = true;
		_editor.ChosenItems = wadObjects;
		_suppressEditorSync = false;

		contentBrowserView.ScrollToItem(items[0]);
	}

	private void SelectWadObject(IWadObject wadObject)
	{
		// Temporarily detach event to avoid feedback loop.
		_viewModel.SelectedItemsChanged -= ViewModel_SelectedItemsChanged;

		var item = _viewModel.AllItems.FirstOrDefault(i => ReferenceEquals(i.WadObject, wadObject));

		_viewModel.SelectedItem = item;
		contentBrowserView.SetSelectionSilently(item);
		contentBrowserView.ScrollToItem(item);

		_viewModel.SelectedItemsChanged += ViewModel_SelectedItemsChanged;
	}
}
