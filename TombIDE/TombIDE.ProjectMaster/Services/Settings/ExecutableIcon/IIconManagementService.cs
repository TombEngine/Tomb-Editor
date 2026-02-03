using System.Drawing;
using System.Threading.Tasks;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.ExecutableIcon;

/// <summary>
/// Represents different icon sizes that can be extracted.
/// </summary>
public enum IconPreviewSize
{
	Size256,
	Size128,
	Size48,
	Size16
}

/// <summary>
/// Contains icon previews at different sizes.
/// </summary>
public sealed class IconPreviewSet
{
	public Bitmap? Icon256 { get; set; }
	public Bitmap? Icon128 { get; set; }
	public Bitmap? Icon48 { get; set; }
	public Bitmap? Icon16 { get; set; }

	public bool IsLowResolution { get; set; } // True if only 48x48 and smaller are available
}

/// <summary>
/// Provides functionality for managing game executable icons.
/// </summary>
public interface IIconManagementService
{
	/// <summary>
	/// Extracts icon previews from the game's launcher executable.
	/// </summary>
	/// <param name="project">The project containing the launcher.</param>
	/// <returns>A set of icon previews at different sizes.</returns>
	Task<IconPreviewSet?> ExtractIconPreviewsAsync(IGameProject project);

	/// <summary>
	/// Injects a new icon into the game's launcher executable.
	/// </summary>
	/// <param name="project">The project containing the launcher.</param>
	/// <param name="iconFilePath">The path to the .ico file to inject.</param>
	Task InjectIconAsync(IGameProject project, string iconFilePath);

	/// <summary>
	/// Restores the default icon for the game version.
	/// </summary>
	/// <param name="project">The project to restore the icon for.</param>
	Task RestoreDefaultIconAsync(IGameProject project);

	/// <summary>
	/// Checks if icon management is supported for the project structure.
	/// </summary>
	/// <param name="project">The project to check.</param>
	/// <returns><see langword="true"/> if supported; otherwise, <see langword="false"/>.</returns>
	bool IsSupported(IGameProject project);
}
