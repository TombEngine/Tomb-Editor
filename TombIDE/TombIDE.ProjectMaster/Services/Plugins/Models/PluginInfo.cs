using System.Drawing;
using System.IO;

namespace TombIDE.ProjectMaster.Services.Plugins.Models;

/// <summary>
/// Represents information about a plugin.
/// </summary>
public sealed class PluginInfo(
	string dllFilePath,
	string name,
	string? description = null,
	Image? logo = null)
{
	/// <summary>
	/// The full path to the plugin DLL file.
	/// </summary>
	public string DllFilePath { get; } = dllFilePath;

	/// <summary>
	/// The DLL file name (e.g., "plugin_example.dll").
	/// </summary>
	public string DllFileName { get; } = Path.GetFileName(dllFilePath);

	/// <summary>
	/// The directory containing the plugin files.
	/// </summary>
	public string DirectoryPath { get; } = Path.GetDirectoryName(dllFilePath) ?? string.Empty;

	/// <summary>
	/// The display name of the plugin.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	/// The description of the plugin, if available.
	/// </summary>
	public string? Description { get; } = description;

	/// <summary>
	/// The logo/image for the plugin, if available.
	/// </summary>
	public Image? Logo { get; } = logo;
}
