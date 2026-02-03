using System.Drawing;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.SplashScreen;

/// <summary>
/// Provides functionality for managing the splash screen (splash.bmp).
/// </summary>
public interface ISplashScreenService
{
	/// <summary>
	/// Gets the current splash screen image.
	/// </summary>
	/// <param name="project">The project containing the splash screen.</param>
	/// <returns>The splash screen image, or <see langword="null"/> if not found.</returns>
	Image? GetSplashScreenImage(IGameProject project);

	/// <summary>
	/// Applies a new splash screen image from a file.
	/// </summary>
	/// <param name="project">The project to update.</param>
	/// <param name="imageFilePath">The path to the image file (.bmp).</param>
	void ApplySplashScreenImage(IGameProject project, string imageFilePath);

	/// <summary>
	/// Removes the splash screen image.
	/// </summary>
	/// <param name="project">The project to update.</param>
	void RemoveSplashScreen(IGameProject project);

	/// <summary>
	/// Validates if the image has a supported resolution.
	/// </summary>
	/// <param name="image">The image to validate.</param>
	/// <returns><see langword="true"/> if the resolution is supported; otherwise, <see langword="false"/>.</returns>
	bool IsValidResolution(Image image);

	/// <summary>
	/// Checks if splash screen is supported for the project structure.
	/// </summary>
	/// <param name="project">The project to check.</param>
	/// <returns><see langword="true"/> if supported; otherwise, <see langword="false"/>.</returns>
	bool IsSupported(IGameProject project);

	/// <summary>
	/// Checks if the project launcher supports splash screen preview.
	/// </summary>
	/// <param name="project">The project to check.</param>
	/// <returns><see langword="true"/> if preview is supported; otherwise, <see langword="false"/>.</returns>
	bool IsPreviewSupported(IGameProject project);
}
