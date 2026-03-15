using System;
using System.Drawing;
using System.IO;
using System.Linq;
using TombIDE.ProjectMaster.Services.PakFile;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.Services.Settings.InGameLogo;

public sealed class LogoManagementService : ILogoManagementService
{
	private const int LogoWidth = 512;
	private const int LogoHeight = 256;

	private readonly IPakFileService _pakFileService;

	public LogoManagementService() : this(new PakFileService())
	{ }

	public LogoManagementService(IPakFileService pakFileService)
		=> _pakFileService = pakFileService;

	public Image? GetLogoImage(IGameProject project)
	{
		string pakFilePath = GetPakFilePath(project);

		if (!File.Exists(pakFilePath))
			return null;

		byte[] pakData = _pakFileService.GetDecompressedData(pakFilePath);
		return ImageHandling.GetImageFromRawData(pakData, LogoWidth, LogoHeight);
	}

	public void ApplyLogoImage(IGameProject project, string imageFilePath)
	{
		using var image = Image.FromFile(imageFilePath);

		if (image.Width != LogoWidth || image.Height != LogoHeight)
			throw new ArgumentException($"Wrong image size. The size of the logo has to be {LogoWidth}x{LogoHeight} px.");

		string pakFilePath = GetPakFilePath(project);
		byte[] rawImageData;

		string extension = Path.GetExtension(imageFilePath).ToLower();

		if (extension == ".bmp")
			rawImageData = ImageHandling.GetRawDataFromBitmap((Bitmap)image);
		else if (extension == ".png")
			rawImageData = ImageHandling.GetRawDataFromImage(image);
		else
			throw new ArgumentException("Unsupported image format. Only .bmp and .png files are supported.");

		_pakFileService.SavePakFile(pakFilePath, rawImageData);
	}

	public void ApplyBlankLogo(IGameProject project)
	{
		using var bitmap = new Bitmap(LogoWidth, LogoHeight);
		using (var graphics = Graphics.FromImage(bitmap))
		{
			var imageSize = new Rectangle(0, 0, LogoWidth, LogoHeight);
			graphics.FillRectangle(Brushes.Black, imageSize);
		}

		string pakFilePath = GetPakFilePath(project);
		byte[] rawImageData = ImageHandling.GetRawDataFromBitmap(bitmap);

		_pakFileService.SavePakFile(pakFilePath, rawImageData);
	}

	public void RestoreDefaultLogo(IGameProject project)
	{
		string sourcePakPath = Path.Combine(
			DefaultPaths.ProgramDirectory,
			"TIDE",
			"Templates",
			"Defaults",
			"TR4 Resources",
			"uklogo.pak");

		string destPakPath = GetPakFilePath(project);

		File.Copy(sourcePakPath, destPakPath, true);
	}

	public bool IsLogoBlank(IGameProject project)
	{
		string pakFilePath = GetPakFilePath(project);

		if (!File.Exists(pakFilePath))
			return true;

		byte[] decompressedData = _pakFileService.GetDecompressedData(pakFilePath);
		bool hasAnyNonZeroBytes = decompressedData.Any(b => b != 0);

		return !hasAnyNonZeroBytes; // True if the entire data is zeroes (blank logo)
	}

	private static string GetPakFilePath(IGameProject project)
		=> Path.Combine(project.GetEngineRootDirectoryPath(), @"data\uklogo.pak");
}
