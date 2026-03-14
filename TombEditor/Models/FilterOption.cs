#nullable enable

namespace TombEditor.Models;

/// <summary>
/// Represents a single entry in the filter combobox.
/// Can be a type filter (All/Moveables/Statics/ImportedGeometry),
/// a category filter (from TrCatalog), or a visual splitter.
/// </summary>
public sealed class FilterOption
{
	/// <summary>
	/// Display name shown in the combobox.
	/// </summary>
	public string DisplayName { get; }

	/// <summary>
	/// True if this is a type-based filter (All, Moveables, Statics, ImportedGeometry).
	/// </summary>
	public bool IsTypeFilter { get; }

	/// <summary>
	/// True if this is a non-selectable visual separator.
	/// </summary>
	public bool IsSplitter { get; }

	/// <summary>
	/// True if this is the special "Favorites" filter.
	/// </summary>
	public bool IsFavoritesFilter { get; }

	/// <summary>
	/// The type filter value, only meaningful when IsTypeFilter is true.
	/// </summary>
	public AssetCategory TypeFilter { get; }

	/// <summary>
	/// The category name for category-based filtering.
	/// Only meaningful when IsTypeFilter is false and IsSplitter is false.
	/// </summary>
	public string CategoryFilter { get; }

	private FilterOption(string displayName, bool isTypeFilter, bool isSplitter, bool isFavoritesFilter, AssetCategory typeFilter, string categoryFilter)
	{
		DisplayName = displayName;
		IsTypeFilter = isTypeFilter;
		IsSplitter = isSplitter;
		IsFavoritesFilter = isFavoritesFilter;
		TypeFilter = typeFilter;
		CategoryFilter = categoryFilter;
	}

	public static FilterOption CreateTypeFilter(AssetCategory category, string displayName)
		=> new(displayName, true, false, false, category, string.Empty);

	public static FilterOption CreateSplitter()
		=> new(string.Empty, false, true, false, AssetCategory.All, string.Empty);

	public static FilterOption CreateCategoryFilter(string categoryName)
		=> new(categoryName, false, false, false, AssetCategory.All, categoryName);

	public static FilterOption CreateFavoritesFilter(string displayName)
		=> new(displayName, false, false, true, AssetCategory.All, string.Empty);

	public override string ToString() => DisplayName;
}
