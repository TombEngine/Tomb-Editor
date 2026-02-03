using System.Drawing;
using System.IO;
using System.Linq;
using TombIDE.ProjectMaster.Services.Plugins.Models;

namespace TombIDE.ProjectMaster.Services.Plugins.Metadata;

public sealed class TRNGPluginMetadataService : IPluginMetadataService
{
	public PluginInfo ReadPluginMetadata(string pluginDirectoryPath, string dllFileName)
	{
		string dllFilePath = Path.Combine(pluginDirectoryPath, dllFileName);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dllFileName);

		string pluginName = fileNameWithoutExtension;
		string? pluginDescription = null;
		Image? pluginLogo = null;

		// Try to read name from .btn file
		string btnFilePath = Path.Combine(pluginDirectoryPath, fileNameWithoutExtension + ".btn");

		if (File.Exists(btnFilePath))
		{
			string? nameLine = File.ReadAllLines(btnFilePath)
				.FirstOrDefault(line => line.StartsWith("NAME#"));

			if (nameLine is not null)
				pluginName = nameLine.Replace("NAME#", string.Empty).Trim();
		}

		// Try to read description from .txt file
		string descriptionFilePath = Path.Combine(pluginDirectoryPath, fileNameWithoutExtension + ".txt");

		if (File.Exists(descriptionFilePath))
			pluginDescription = File.ReadAllText(descriptionFilePath);

		// Try to read logo from image files
		foreach (string file in Directory.GetFiles(pluginDirectoryPath))
		{
			string extension = Path.GetExtension(file).ToLower();

			if (extension is ".jpg" or ".png" or ".bmp" or ".gif")
			{
				try
				{
					using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
					pluginLogo = Image.FromStream(stream);

					break;
				}
				catch
				{
					// Ignore errors reading image
				}
			}
		}

		return new PluginInfo(dllFilePath, pluginName, pluginDescription, pluginLogo);
	}
}
