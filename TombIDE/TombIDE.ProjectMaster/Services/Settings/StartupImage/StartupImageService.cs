using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.StartupImage;

public sealed class StartupImageService : IStartupImageService
{
	public Image? GetStartupImage(IGameProject project)
	{
		string imagePath = GetImagePath(project);

		if (!File.Exists(imagePath))
			return null;

		return Image.FromFile(imagePath);
	}

	public void ApplyStartupImage(IGameProject project, string imageFilePath)
	{
		string destPath = GetImagePath(project);
		File.Copy(imageFilePath, destPath, true);
	}

	public void ApplyBlankImage(IGameProject project)
	{
		using var bitmap = new Bitmap(1, 1);
		bitmap.SetPixel(0, 0, Color.Black);

		string imagePath = GetImagePath(project);
		bitmap.Save(imagePath, ImageFormat.Bmp);
	}

	public void RestoreDefaultImage(IGameProject project)
	{
		string defaultImagePath = Path.Combine(
			DefaultPaths.ProgramDirectory,
			"TIDE",
			"Templates",
			"Defaults",
			"TR4 Resources",
			"load.bmp");

		ApplyStartupImage(project, defaultImagePath);
	}

	public bool IsImageBlank(IGameProject project)
	{
		using var image = GetStartupImage(project);
		return image?.Width == 1 && image.Height == 1;
	}

	private static string GetImagePath(IGameProject project)
		=> Path.Combine(project.GetEngineRootDirectoryPath(), "load.bmp");
}
