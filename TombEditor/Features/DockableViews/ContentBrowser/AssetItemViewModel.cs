#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Features.DockableViews.ContentBrowser;

/// <summary>
/// ViewModel representing a single asset item in the Content Browser.
/// </summary>
public partial class AssetItemViewModel : ObservableObject
{
	/// <summary>
	/// The underlying WAD object (WadMoveable, WadStatic, or ImportedGeometry).
	/// </summary>
	public IWadObject WadObject { get; }

	// Display name of the asset.
	public string Name { get; }

	// Category this asset belongs to.
	public AssetCategory Category { get; }

	// Localized category display name for grouping.
	public string CategoryName { get; }

	// Name/path of the WAD file this asset was loaded from.
	public string WadSource { get; }

	// Whether this asset exists in multiple WAD files.
	public bool IsInMultipleWads { get; }

	// Catalog category string from TrCatalog (e.g. "Enemies", "Player").
	// May contain multiple values for special cases like "Shatterable".
	public string CatalogCategory { get; }

	// Combined CategoryName + CatalogCategory for tooltip display.
	public string CategoryDisplayText => string.IsNullOrEmpty(CatalogCategory) ? CategoryName : $"{CategoryName}, {CatalogCategory}";

	// All effective categories including primary and any synthetic ones.
	public List<string> EffectiveCategories { get; } = new();

	// Color brush for placeholder thumbnail based on category.
	public SolidColorBrush ThumbnailBrush { get; }

	// Initials shown on the placeholder thumbnail.
	public string Initials { get; }

	// Sort order for type grouping (Moveables=0, Statics=1, ImportedGeometry=2).
	public int CategoryOrder => Category switch
	{
		AssetCategory.Moveables => 0,
		AssetCategory.Statics => 1,
		AssetCategory.ImportedGeometry => 2,
		_ => 3
	};

	// Unique key used for favorites persistence.
	public string FavoriteKey { get; }

	// Whether this item is marked as a favorite.
	[ObservableProperty]
	private bool _isFavorite;

	// Backing field for thumbnail property.
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasThumbnail))]
	private BitmapSource? _thumbnail;

	// Whether a rendered thumbnail has been set.
	public bool HasThumbnail => Thumbnail is not null;

	// True while covered by rubber-band selection; purely visual feedback.
	[ObservableProperty]
	private bool _isRubberBandSelected;

	// Unique cache key for this asset's thumbnail.
	public string CacheKey { get; }

	private static readonly SolidColorBrush MoveableBrush = new(Color.FromRgb(0x4B, 0x6E, 0xAF));
	private static readonly SolidColorBrush StaticBrush = new(Color.FromRgb(0x5A, 0x8C, 0x46));
	private static readonly SolidColorBrush ImportedGeometryBrush = new(Color.FromRgb(0xC0, 0x7B, 0x38));

	static AssetItemViewModel()
	{
		MoveableBrush.Freeze();
		StaticBrush.Freeze();
		ImportedGeometryBrush.Freeze();
	}

	public AssetItemViewModel(IWadObject wadObject, string name, AssetCategory category, string categoryName, string wadSource,
		bool isInMultipleWads, string catalogCategory = "", string fileVersion = "")
	{
		WadObject = wadObject;
		Name = name;
		Category = category;
		CategoryName = categoryName;
		WadSource = wadSource;
		IsInMultipleWads = isInMultipleWads;
		CatalogCategory = catalogCategory;

		ThumbnailBrush = category switch
		{
			AssetCategory.Moveables => MoveableBrush,
			AssetCategory.Statics => StaticBrush,
			AssetCategory.ImportedGeometry => ImportedGeometryBrush,
			_ => MoveableBrush
		};

		Initials = BuildInitials(name);
		CacheKey = BuildCacheKey(wadObject, category, fileVersion);
		FavoriteKey = BuildFavoriteKey(wadObject, category);
	}

	public static string BuildFavoriteKey(IWadObject wadObject, AssetCategory category)
	{
		string prefix = category.ToString();

		if (wadObject.Id is WadMoveableId movId)
			return $"{prefix}_{movId.TypeId}";

		if (wadObject.Id is WadStaticId statId)
			return $"{prefix}_{statId.TypeId}";

		if (wadObject is ImportedGeometry geo)
			return $"{prefix}_{geo.Info.Name}_{geo.Info.Path}";

		return $"{prefix}_{wadObject.GetHashCode()}";
	}

	private static string BuildCacheKey(IWadObject wadObject, AssetCategory category, string fileVersion = "")
	{
		string prefix = category.ToString();
		var versionSuffix = string.IsNullOrEmpty(fileVersion) ? string.Empty : $"_{fileVersion}";

		if (wadObject.Id is not null)
			return $"{prefix}_{wadObject.Id}{versionSuffix}";

		if (wadObject is ImportedGeometry geo)
			return $"{prefix}_{geo.GetHashCode()}{versionSuffix}";

		return $"{prefix}_{wadObject.GetHashCode()}{versionSuffix}";
	}

	// Converts an ImageC (BGRA byte array) to a frozen WPF BitmapSource.
	public static BitmapSource? ImageCToBitmapSource(ImageC image)
	{
		if (image.Width == 0 || image.Height == 0)
			return null;

		byte[] data = image.ToByteArray();
		int stride = image.Width * 4; // BGRA = 4 bytes per pixel.
		var bmp = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null, data, stride);
		bmp.Freeze();
		return bmp;
	}

	private static string BuildInitials(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return "?";

		var cleaned = name.Trim();

		// Skip leading "(number) " prefix, e.g. "(100) WOLF" -> "WOLF".
		if (cleaned.Length > 0 && cleaned[0] == '(')
		{
			int closeIdx = cleaned.IndexOf(')');

			if (closeIdx > 0 && closeIdx + 1 < cleaned.Length)
				cleaned = cleaned[(closeIdx + 1)..].TrimStart();
		}

		if (string.IsNullOrEmpty(cleaned))
			return "?";

		// Use the first letter of the cleaned name.
		return cleaned[0].ToString().ToUpperInvariant();
	}
}
