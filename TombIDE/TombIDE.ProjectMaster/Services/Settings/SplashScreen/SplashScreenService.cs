using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.SplashScreen;

public sealed class SplashScreenService : ISplashScreenService
{
	public bool IsSupported(IGameProject project)
	{
		// Splash screen is not supported for legacy projects where project directory equals engine directory
		return !project.DirectoryPath.Equals(project.GetEngineRootDirectoryPath(), StringComparison.OrdinalIgnoreCase);
	}

	public bool IsPreviewSupported(IGameProject project)
	{
		string launcherExecutable = project.GetLauncherFilePath();

		if (!File.Exists(launcherExecutable))
			return false;

		string? originalFileName = FileVersionInfo.GetVersionInfo(launcherExecutable).OriginalFilename;
		return originalFileName?.Equals("launch.exe", StringComparison.OrdinalIgnoreCase) is true; // TODO: Needs a more robust check
	}

	public Image? GetSplashScreenImage(IGameProject project)
	{
		string imagePath = GetSplashScreenPath(project);

		if (!File.Exists(imagePath))
			return null;

		return Image.FromFile(imagePath);
	}

	public void ApplySplashScreenImage(IGameProject project, string imageFilePath)
	{
		using var image = Image.FromFile(imageFilePath);

		if (!IsValidResolution(image))
			throw new ArgumentException("Wrong image size.");

		string destPath = GetSplashScreenPath(project);
		File.Copy(imageFilePath, destPath, true);
	}

	public void RemoveSplashScreen(IGameProject project)
	{
		string splashImagePath = GetSplashScreenPath(project);

		if (File.Exists(splashImagePath))
			File.Delete(splashImagePath);
	}

	public bool IsValidResolution(Image image)
		=> image.Width is 1024 or 768 or 512 or 384
		&& image.Height is 512 or 384 or 256;

	private static string GetSplashScreenPath(IGameProject project)
		=> Path.Combine(project.GetEngineRootDirectoryPath(), "splash.bmp");
}
