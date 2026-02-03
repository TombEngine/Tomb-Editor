using System.Drawing;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.StartupImage;

/// <summary>
/// Provides functionality for managing the startup/loading image (load.bmp).
/// </summary>
public interface IStartupImageService
{
	/// <summary>
	/// Gets the current startup image.
	/// </summary>
	/// <param name="project">The project containing the image.</param>
	/// <returns>The startup image, or <see langword="null"/> if not found.</returns>
	Image? GetStartupImage(IGameProject project);

	/// <summary>
	/// Applies a new startup image from a file.
	/// </summary>
	/// <param name="project">The project to update.</param>
	/// <param name="imageFilePath">The path to the image file (.bmp).</param>
	void ApplyStartupImage(IGameProject project, string imageFilePath);

	/// <summary>
	/// Applies a blank (1x1 black pixel) startup image.
	/// </summary>
	/// <param name="project">The project to update.</param>
	void ApplyBlankImage(IGameProject project);

	/// <summary>
	/// Restores the default TR4 startup image.
	/// </summary>
	/// <param name="project">The project to update.</param>
	void RestoreDefaultImage(IGameProject project);

	/// <summary>
	/// Checks if the current startup image is blank.
	/// </summary>
	/// <param name="project">The project to check.</param>
	/// <returns><see langword="true"/> if the image is blank (1x1); otherwise, <see langword="false"/>.</returns>
	bool IsImageBlank(IGameProject project);
}
