namespace TombIDE.ProjectMaster.Services.Plugins.Models;

/// <summary>
/// Represents the source type for plugin installation.
/// </summary>
public enum PluginInstallationSourceType
{
	Archive,
	Folder
}

/// <summary>
/// Represents a source from which a plugin can be installed.
/// </summary>
public sealed class PluginInstallationSource(string path, PluginInstallationSourceType type)
{
	public string Path { get; } = path;
	public PluginInstallationSourceType Type { get; } = type;
}
