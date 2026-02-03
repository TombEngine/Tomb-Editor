using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;
using TombLib.Utils;

namespace TombIDE.ProjectMaster.Services.Settings.ExecutableIcon;

public sealed class IconManagementService : IIconManagementService
{
	public bool IsSupported(IGameProject project)
	{
		// Icon management is not supported for legacy projects where project directory equals engine directory
		return !project.DirectoryPath.Equals(project.GetEngineRootDirectoryPath(), StringComparison.OrdinalIgnoreCase);
	}

	public async Task<IconPreviewSet?> ExtractIconPreviewsAsync(IGameProject project)
	{
		string launcherFilePath = project.GetLauncherFilePath();
		string launcherDirectoryPath = Path.GetDirectoryName(launcherFilePath) ?? string.Empty;

		// Create a temporary .exe file to make sure the icon cache is up to date
		string tempFilePath = Path.Combine(launcherDirectoryPath, Path.GetRandomFileName() + ".exe");

		try
		{
			// Try copying the file with retry logic
			bool fileCopied = await FileUtils.TryCopyFileWithRetryAsync(launcherFilePath, tempFilePath);

			if (!fileCopied)
				return null; // File copy failed, exit early

			var ico_256 = IconUtilities.ExtractIcon(tempFilePath, IconSize.Jumbo).ToBitmap();

			// Windows doesn't seem to have a name for 128x128 px icons, therefore we must resize the Jumbo one
			var ico_128 = ImageHandling.ResizeImage(ico_256, 128, 128) as Bitmap;

			var ico_48 = IconUtilities.ExtractIcon(tempFilePath, IconSize.ExtraLarge).ToBitmap();
			var ico_16 = IconUtilities.ExtractIcon(tempFilePath, IconSize.Small).ToBitmap();

			bool isLowRes = ico_256.Width == ico_48.Width && ico_256.Height == ico_48.Height;

			return new IconPreviewSet
			{
				Icon256 = isLowRes ? ico_48 : ico_256,
				Icon128 = isLowRes ? ico_48 : ico_128,
				Icon48 = ico_48,
				Icon16 = ico_16,
				IsLowResolution = isLowRes
			};
		}
		finally
		{
			// Now delete the temporary .exe file with retry logic
			await FileUtils.TryDeleteFileWithRetryAsync(tempFilePath);
		}
	}

	public async Task InjectIconAsync(IGameProject project, string iconFilePath)
	{
		IconUtilities.InjectIcon(project.GetLauncherFilePath(), iconFilePath);
		await Task.CompletedTask;
	}

	public async Task RestoreDefaultIconAsync(IGameProject project)
	{
		string defaultIconPath = Path.Combine(
			DefaultPaths.ProgramDirectory,
			"TIDE",
			"Templates",
			"Defaults",
			"Game Icons",
			project.GameVersion + ".ico");

		await InjectIconAsync(project, defaultIconPath);
	}
}
