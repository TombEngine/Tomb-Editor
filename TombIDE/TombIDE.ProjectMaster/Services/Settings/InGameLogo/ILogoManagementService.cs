using System.Drawing;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.InGameLogo;

/// <summary>
/// Provides functionality for managing the in-game logo (e.g. uklogo.pak).
/// </summary>
public interface ILogoManagementService
{
	/// <summary>
	/// Gets the current in-game logo image (e.g. from a PAK file).
	/// </summary>
	/// <param name="project">The project containing the logo.</param>
	/// <returns>The logo image, or <see langword="null"/> if not found.</returns>
	Image? GetLogoImage(IGameProject project);

	/// <summary>
	/// Applies a new logo image from a file.
	/// </summary>
	/// <param name="project">The project to update.</param>
	/// <param name="imageFilePath">The path to the image file (.bmp or .png).</param>
	void ApplyLogoImage(IGameProject project, string imageFilePath);

	/// <summary>
	/// Applies a blank (black) logo image.
	/// </summary>
	/// <param name="project">The project to update.</param>
	void ApplyBlankLogo(IGameProject project);

	/// <summary>
	/// Restores the default in-game logo.
	/// </summary>
	/// <param name="project">The project to update.</param>
	void RestoreDefaultLogo(IGameProject project);

	/// <summary>
	/// Checks if the current logo is blank (e.g. all black pixels in case of TR4).
	/// </summary>
	/// <param name="project">The project to check.</param>
	/// <returns><see langword="true"/> if the logo is blank; otherwise, <see langword="false"/>.</returns>
	bool IsLogoBlank(IGameProject project);
}
