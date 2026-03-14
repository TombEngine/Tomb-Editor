#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TombEditor.Models;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.ViewModels;

/// <summary>
/// Main ViewModel for the Content Browser tool window.
/// Manages the collection of assets, search/filtering, and type-based sorting.
/// </summary>
public partial class ContentBrowserViewModel : ObservableObject
{
	private readonly ILocalizationService _localizationService;

	// Full collection of all asset items.
	public ObservableCollection<AssetItemViewModel> AllItems { get; } = new();

	// Filtered view of the items.
	public ICollectionView FilteredItems { get; }

	// Search text for filtering assets by name.
	[ObservableProperty]
	private string _searchText = string.Empty;

	// Currently selected filter option (type or category).
	[ObservableProperty]
	private FilterOption? _selectedFilter;

	// Currently selected asset item.
	[ObservableProperty]
	private AssetItemViewModel? _selectedItem;

	// WAD source info for the selected item.
	[ObservableProperty]
	private string _selectedItemWadInfo = string.Empty;

	// Tile size (width) in pixels. Controls the grid tile dimensions.
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TileHeight))]
	[NotifyPropertyChangedFor(nameof(ThumbSize))]
	private double _tileWidth = 88;

	// Computed tile height: thumbnail size + 4px margin + fixed label row height.
	public double TileHeight => ThumbSize + 4 + 22;

	// Computed thumbnail square size based on tile width.
	public double ThumbSize => TileWidth * 0.78;

	// Minimum tile width.
	public double MinTileWidth { get; } = 60;

	// Maximum tile width.
	public double MaxTileWidth { get; } = 180;

	// Cached count of items currently visible after filtering.
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(FormattedItemCount))]
	private int _itemCount;

	// Formatted item count string for display.
	public string FormattedItemCount => _localizationService.Format("ItemCount", ItemCount);

	// Available filter options (type filters + optional splitter + category filters).
	public ObservableCollection<FilterOption> FilterOptions { get; } = new();

	// The default "All" filter option.
	private readonly FilterOption _allFilter;

	// Cached lowered search text to avoid per-item allocation in FilterPredicate.
	private string _searchTextLower = string.Empty;

	public ContentBrowserViewModel(ILocalizationService? localizationService = null)
	{
		_localizationService = ServiceLocator.ResolveService(localizationService)
			.WithKeysFor(this);

		_allFilter = FilterOption.CreateTypeFilter(AssetCategory.All, _localizationService["FilterAll"]);

		FilteredItems = CollectionViewSource.GetDefaultView(AllItems);
		FilteredItems.Filter = FilterPredicate;

		// Initialize default type filters.
		FilterOptions.Add(_allFilter);
		FilterOptions.Add(FilterOption.CreateTypeFilter(AssetCategory.Moveables, _localizationService["FilterMoveables"]));
		FilterOptions.Add(FilterOption.CreateTypeFilter(AssetCategory.Statics, _localizationService["FilterStatics"]));
		FilterOptions.Add(FilterOption.CreateTypeFilter(AssetCategory.ImportedGeometry, _localizationService["FilterImportedGeometry"]));
		FilterOptions.Add(FilterOption.CreateSplitter());
		FilterOptions.Add(FilterOption.CreateFavoritesFilter(_localizationService["FilterFavorites"]));

		SelectedFilter = _allFilter;

		// Use a custom comparer for natural (numeric-aware) sorting of asset names.
		// SortDescriptions uses lexicographic order which sorts (1), (10), (100), (2)...
		if (FilteredItems is ListCollectionView lcv)
		{
			lcv.CustomSort = new AssetItemComparer();
		}
		else
		{
			// Fallback for non-list views (shouldn't happen with ObservableCollection).
			FilteredItems.SortDescriptions.Add(new SortDescription(nameof(AssetItemViewModel.CategoryOrder), ListSortDirection.Ascending));
			FilteredItems.SortDescriptions.Add(new SortDescription(nameof(AssetItemViewModel.Name), ListSortDirection.Ascending));
		}
	}

	// Refreshes the filtered view and updates the cached item count.
	private void RefreshFilteredItems()
	{
		FilteredItems.Refresh();
		ItemCount = FilteredItems.Cast<object>().Count();
	}

	partial void OnSearchTextChanged(string value)
	{
		_searchTextLower = value.ToLowerInvariant();
		RefreshFilteredItems();
	}

	partial void OnSelectedFilterChanged(FilterOption? value)
	{
		// Prevent selecting splitter items.
		if (value is not null && value.IsSplitter)
		{
			SelectedFilter = _allFilter;
			return;
		}

		RefreshFilteredItems();
	}

	partial void OnSelectedItemChanged(AssetItemViewModel? value)
	{
		if (value is not null)
		{
			SelectedItemWadInfo = value.IsInMultipleWads
				? _localizationService.Format("WadSourceMultiple", value.WadSource)
				: _localizationService.Format("WadSourceSingle", value.WadSource);
		}
		else
		{
			SelectedItemWadInfo = string.Empty;
		}

		SelectedItemChanged?.Invoke(this, value);
	}

	// Event raised when the selected item changes; used by WinForms host to update Editor state.
	public event EventHandler<AssetItemViewModel?>? SelectedItemChanged;

	// Event raised when the selected items change (multi-selection).
	public event EventHandler<IReadOnlyList<AssetItemViewModel>>? SelectedItemsChanged;

	// Event raised when a drag-drop operation is requested; carries selected items.
	public event EventHandler<IReadOnlyList<AssetItemViewModel>>? DragDropRequested;

	// Event raised when thumbnails need rendering; host handles it on UI thread.
	public event EventHandler? ThumbnailRenderRequested;

	// Event raised when the user requests to locate a specific item (Alt+click).
	public event EventHandler<AssetItemViewModel>? LocateItemRequested;

	// Event raised when the user requests to add/place the selected item.
	public event EventHandler? AddItemRequested;

	// Event raised when the user requests to add a new WAD file.
	public event EventHandler? AddWadRequested;

	// Event raised when files are dropped from Windows Explorer onto the Content Browser.
	public event EventHandler<string[]>? FilesDropped;

	// Event raised when a favorite is toggled; host persists the change to LevelSettings.
	public event EventHandler<AssetItemViewModel>? FavoriteToggled;

	// Current list of all selected items (for multi-selection).
	public IReadOnlyList<AssetItemViewModel> SelectedItems { get; private set; } = Array.Empty<AssetItemViewModel>();

	// Updates the selected items from the view's ListBox selection.
	public void UpdateSelectedItems(IReadOnlyList<AssetItemViewModel> items)
	{
		SelectedItems = items;

		var first = items.Count > 0 ? items[0] : null;

		if (first != SelectedItem)
			SelectedItem = first;

		SelectedItemsChanged?.Invoke(this, items);
	}

	// In-memory thumbnail cache keyed by CacheKey.
	private readonly Dictionary<string, BitmapSource> _thumbnailCache = new();

	// Stored for use in FilterPredicate (set during RefreshAssets).
	private TRVersion.Game _gameVersion;

	private bool _hideInternalObjects;

	// Whether any WADs are loaded in the level.
	[ObservableProperty]
	private bool _hasLoadedWads;

	/// <summary>
	/// Refreshes all assets from the current level settings.
	/// Builds moveable and static item lists in parallel for maximum performance.
	/// Also scans for catalog categories and populates filter options.
	/// </summary>
	public void RefreshAssets(LevelSettings settings, bool hideInternalObjects = false)
	{
		var previousSelection = SelectedItem?.WadObject;
		var previousFilterName = SelectedFilter?.DisplayName;

		var gameVersion = settings.GameVersion;
		bool isTombEngine = gameVersion == TRVersion.Game.TombEngine;

		_gameVersion = gameVersion;
		_hideInternalObjects = hideInternalObjects;

		// Cache localized category names for use in parallel tasks.
		string moveablesCategory = _localizationService["CategoryMoveables"];
		string staticsCategory = _localizationService["CategoryStatics"];
		string importedGeometryCategory = _localizationService["CategoryImportedGeometry"];

		// Fetch all objects upfront (these iterate wad files).
		var allMoveables = settings.WadGetAllMoveables();
		var allStatics = settings.WadGetAllStatics();

		// Pre-compute wad file versions for version-aware cache keys.
		// Read-only in parallel tasks below, so no locking is needed.
		var wadFileVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		foreach (var wad in settings.Wads)
		{
			if (wad.Wad is not null && !string.IsNullOrEmpty(wad.Path))
				wadFileVersions[wad.Path] = GetFileVersion(settings.MakeAbsolute(wad.Path));
		}

		// Build moveable and static items in parallel using thread-safe collections.
		// Each item construction is independent - name resolution, wad source lookup.
		// initials, and cache key are all pure/readonly operations.
		var moveableItems = new ConcurrentBag<AssetItemViewModel>();
		var staticItems = new ConcurrentBag<AssetItemViewModel>();

		var moveableTask = Task.Run(() =>
		{
			Parallel.ForEach(allMoveables, kvp =>
			{
				var moveable = kvp.Value;
				string name = moveable.ToString(gameVersion);

				var itemType = new ItemType(moveable.Id, settings);
				var wad = settings.WadTryGetWad(itemType, out bool multiple);

				string wadSource = wad is not null ? Path.GetFileName(wad.Path) : "Unknown";
				var fileVersion = wad is not null && wadFileVersions.TryGetValue(wad.Path, out var wadVer) ? wadVer : string.Empty;
				string catalogCategory = TrCatalog.GetMoveableCategory(gameVersion, moveable.Id.TypeId);

				var item = new AssetItemViewModel(moveable, name, AssetCategory.Moveables, moveablesCategory, wadSource, multiple, catalogCategory, fileVersion);

				// Add the primary catalog category.
				if (!string.IsNullOrEmpty(catalogCategory))
					item.EffectiveCategories.Add(catalogCategory);

				moveableItems.Add(item);
			});
		});

		var staticTask = Task.Run(() =>
		{
			Parallel.ForEach(allStatics, kvp =>
			{
				var staticMesh = kvp.Value;
				string name = staticMesh.ToString(gameVersion);

				var itemType = new ItemType(staticMesh.Id, settings);
				var wad = settings.WadTryGetWad(itemType, out bool multiple);
				string wadSource = wad is not null ? Path.GetFileName(wad.Path) : "Unknown";
				var fileVersion = wad is not null && wadFileVersions.TryGetValue(wad.Path, out var wadVer) ? wadVer : string.Empty;

				string catalogCategory = TrCatalog.GetStaticCategory(gameVersion, staticMesh.Id.TypeId);

				var item = new AssetItemViewModel(staticMesh, name, AssetCategory.Statics, staticsCategory, wadSource, multiple, catalogCategory, fileVersion);

				// Add the primary catalog category.
				if (!string.IsNullOrEmpty(catalogCategory))
					item.EffectiveCategories.Add(catalogCategory);

				// Determine if this static is shatterable.
				// WadStatic.Shatter flag takes priority for TombEngine, otherwise fall back to TrCatalog.IsStaticShatterable.
				bool isShatterable = TrCatalog.IsStaticShatterable(gameVersion, staticMesh.Id.TypeId);

				if (isTombEngine && staticMesh.Shatter)
					isShatterable = true;

				if (isShatterable && !item.EffectiveCategories.Contains("Shatterable"))
					item.EffectiveCategories.Add("Shatterable");

				// Also tolerate catalog category string "Shatterable" even if no shatterable flag.
				if (string.Equals(catalogCategory, "Shatterable", StringComparison.OrdinalIgnoreCase) && !item.EffectiveCategories.Contains("Shatterable"))
					item.EffectiveCategories.Add("Shatterable");

				staticItems.Add(item);
			});
		});

		// Build imported geometry items (lightweight - no parallelization needed).
		var geoItems = new List<AssetItemViewModel>();

		foreach (var geo in settings.ImportedGeometries)
		{
			if (geo.LoadException is not null)
				continue;

			string name = string.IsNullOrEmpty(geo.Info.Name) ? Path.GetFileNameWithoutExtension(geo.Info.Path) ?? "Unnamed" : geo.Info.Name;
			string wadSource = !string.IsNullOrEmpty(geo.Info.Path) ? Path.GetFileName(geo.Info.Path) : "Inline";
			var fileVersion = !string.IsNullOrEmpty(geo.Info.Path) ? GetFileVersion(settings.MakeAbsolute(geo.Info.Path)) : string.Empty;

			geoItems.Add(new AssetItemViewModel(geo, name, AssetCategory.ImportedGeometry, importedGeometryCategory, wadSource, false, string.Empty, fileVersion));
		}

		// Wait for parallel moveable and static building to complete.
		Task.WaitAll(moveableTask, staticTask);

		// Batch-populate the ObservableCollection (must happen on UI thread).
		// Detach filter during bulk insert to avoid per-item filtering overhead.
		FilteredItems.Filter = null;
		AllItems.Clear();

		foreach (var item in moveableItems)
			AllItems.Add(item);
		foreach (var item in staticItems)
			AllItems.Add(item);
		foreach (var item in geoItems)
			AllItems.Add(item);

		FilteredItems.Filter = FilterPredicate;

		// Track whether any WADs are loaded.
		HasLoadedWads = AllItems.Count > 0;

		// Apply favorite state from saved settings.
		foreach (var item in AllItems)
		{
			if (settings.Favorites.Contains(item.FavoriteKey))
				item.IsFavorite = true;
		}

		// Build category list from all moveable and static items.
		var allCategories = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var item in AllItems)
		{
			foreach (var cat in item.EffectiveCategories)
			{
				if (!string.IsNullOrEmpty(cat))
					allCategories.Add(cat);
			}
		}

		// Rebuild filter options.
		FilterOptions.Clear();
		FilterOptions.Add(_allFilter);
		FilterOptions.Add(FilterOption.CreateTypeFilter(AssetCategory.Moveables, _localizationService["FilterMoveables"]));
		FilterOptions.Add(FilterOption.CreateTypeFilter(AssetCategory.Statics, _localizationService["FilterStatics"]));
		FilterOptions.Add(FilterOption.CreateTypeFilter(AssetCategory.ImportedGeometry, _localizationService["FilterImportedGeometry"]));
		FilterOptions.Add(FilterOption.CreateSplitter());
		FilterOptions.Add(FilterOption.CreateFavoritesFilter(_localizationService["FilterFavorites"]));

		if (allCategories.Count > 0)
		{
			FilterOptions.Add(FilterOption.CreateSplitter());

			foreach (var cat in allCategories)
				FilterOptions.Add(FilterOption.CreateCategoryFilter(cat));
		}

		// Restore previous filter selection or default to All.
		SelectedFilter = FilterOptions.FirstOrDefault(f =>
			f.DisplayName == previousFilterName && !f.IsSplitter) ?? _allFilter;

		RefreshFilteredItems();

		// Try to restore previous selection.
		if (previousSelection is not null)
		{
			SelectedItem = AllItems.FirstOrDefault(i => ReferenceEquals(i.WadObject, previousSelection))
				?? AllItems.FirstOrDefault(i => i.WadObject.Id is not null && previousSelection.Id is not null
					&& i.WadObject.Id.GetType() == previousSelection.Id.GetType()
					&& i.WadObject.Id.CompareTo(previousSelection.Id) == 0);
		}

		// Apply cached thumbnails and request rendering for uncached items.
		bool needsRender = false;

		foreach (var item in AllItems)
		{
			if (_thumbnailCache.TryGetValue(item.CacheKey, out var cached))
				item.Thumbnail = cached;
			else
				needsRender = true;
		}

		if (needsRender)
			ThumbnailRenderRequested?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Called by the host to set a rendered thumbnail for an item and cache it.
	/// </summary>
	public void SetThumbnail(AssetItemViewModel item, BitmapSource? thumbnail)
	{
		if (thumbnail is null)
			return;

		if (!thumbnail.IsFrozen)
			thumbnail.Freeze();

		item.Thumbnail = thumbnail;
		_thumbnailCache[item.CacheKey] = thumbnail;
	}

	/// <summary>
	/// Returns true if a cached thumbnail exists for this item.
	/// </summary>
	public bool HasCachedThumbnail(AssetItemViewModel item)
	{
		return _thumbnailCache.ContainsKey(item.CacheKey);
	}

	/// <summary>
	/// Clears the entire thumbnail cache, forcing re-render on next refresh.
	/// </summary>
	public void InvalidateThumbnailCache()
	{
		_thumbnailCache.Clear();

		foreach (var item in AllItems)
			item.Thumbnail = null;
	}

	private static string GetFileVersion(string absolutePath)
	{
		try
		{
			return File.GetLastWriteTimeUtc(absolutePath).Ticks.ToString();
		}
		catch
		{
			return "0";
		}
	}

	/// <summary>
	/// Gets all items that still need thumbnails rendered.
	/// </summary>
	public IEnumerable<AssetItemViewModel> GetItemsNeedingThumbnails()
	{
		return AllItems.Where(i => i.Thumbnail is null);
	}

	/// <summary>
	/// Requests a drag-drop operation for a list of selected items.
	/// </summary>
	public void RequestDragDrop(IReadOnlyList<AssetItemViewModel> items)
	{
		if (items.Count > 0)
			DragDropRequested?.Invoke(this, items);
	}

	/// <summary>
	/// Requests locating a specific item in the level (Alt+click).
	/// </summary>
	public void RequestLocateItem(AssetItemViewModel item)
	{
		LocateItemRequested?.Invoke(this, item);
	}

	[RelayCommand]
	private void ClearSearch()
	{
		SearchText = string.Empty;
	}

	[RelayCommand]
	private void LocateItem()
	{
		if (SelectedItem is not null)
			LocateItemRequested?.Invoke(this, SelectedItem);
	}

	[RelayCommand]
	private void AddItem()
	{
		if (SelectedItem is not null)
			AddItemRequested?.Invoke(this, EventArgs.Empty);
	}

	[RelayCommand]
	private void AddWad()
	{
		AddWadRequested?.Invoke(this, EventArgs.Empty);
	}

	// Toggles the favorite state of an item and notifies the host.
	public void ToggleFavorite(AssetItemViewModel item)
	{
		item.IsFavorite = !item.IsFavorite;
		FavoriteToggled?.Invoke(this, item);
	}

	// Routes a file drop from the view to subscribing hosts.
	public void HandleFileDrop(string[] files)
	{
		FilesDropped?.Invoke(this, files);
	}

	private bool FilterPredicate(object obj)
	{
		if (obj is not AssetItemViewModel item)
			return false;

		// Hide internal/engine-only moveables when configured
		if (_hideInternalObjects &&
			item.Category == AssetCategory.Moveables &&
			item.WadObject is WadMoveable mov &&
			TrCatalog.IsHidden(_gameVersion, mov.Id.TypeId))
		{
			return false;
		}

		var filter = SelectedFilter;

		// Apply type or category filter
		if (filter?.IsSplitter == false)
		{
			if (filter.IsFavoritesFilter)
			{
				if (!item.IsFavorite)
					return false;
			}
			else if (filter.IsTypeFilter)
			{
				// Type filter: filter by asset type (All shows everything)
				if (filter.TypeFilter != AssetCategory.All && item.Category != filter.TypeFilter)
					return false;
			}
			else
			{
				// Category filter: show all items matching this category regardless of type
				if (!item.EffectiveCategories.Contains(filter.CategoryFilter, StringComparer.OrdinalIgnoreCase))
					return false;
			}
		}

		// Text search
		if (!string.IsNullOrWhiteSpace(SearchText))
		{
			// Exact substring match
			if (item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
				return true;

			if (item.WadSource.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
				return true;

			// Fuzzy match using Levenshtein distance (for queries with 3+ chars)
			if (_searchTextLower.Length >= 3)
			{
				int distance = Levenshtein.DistanceSubstring(
					item.Name.ToLowerInvariant(), _searchTextLower, out _);

				if (distance < 2)
					return true;
			}

			return false;
		}

		return true;
	}

	/// <summary>
	/// Comparer for sorting asset items by type (CategoryOrder) then by name using
	/// natural numeric ordering so that e.g. (2) sorts before (10).
	/// </summary>
	private sealed class AssetItemComparer : IComparer
	{
		public int Compare(object? x, object? y)
		{
			if (x is not AssetItemViewModel a || y is not AssetItemViewModel b)
				return 0;

			int catCmp = a.CategoryOrder.CompareTo(b.CategoryOrder);

			if (catCmp != 0)
				return catCmp;

			return NaturalComparer.Do(a.Name, b.Name);
		}
	}
}
