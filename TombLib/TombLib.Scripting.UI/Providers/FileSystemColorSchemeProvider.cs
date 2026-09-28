using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Providers;

/// <summary>
/// File-system-backed color scheme provider for a specific editor configuration type.
/// </summary>
/// <typeparam name="TConfig">The editor configuration type exposing the selected scheme name.</typeparam>
public class FileSystemColorSchemeProvider<TConfig> : FileSystemColorSchemeProviderBase
	where TConfig : TextEditorConfigBase, IColorSchemeConfig
{
	/// <summary>
	/// Initializes a new instance of the <see cref="FileSystemColorSchemeProvider{TConfig}"/> class.
	/// </summary>
	/// <param name="colorSchemesDirectory">The directory that contains the color scheme files.</param>
	public FileSystemColorSchemeProvider(string colorSchemesDirectory)
		: base(colorSchemesDirectory)
	{ }

	/// <inheritdoc/>
	public override string GetSelectedName(TextEditorConfigBase config)
		=> TextEditorColorProviderGuard.GetConfig<TConfig>(config, GetType().Name).SelectedColorSchemeName;

	/// <inheritdoc/>
	public override void SetSelectedName(TextEditorConfigBase config, string name)
		=> TextEditorColorProviderGuard.GetConfig<TConfig>(config, GetType().Name).SelectedColorSchemeName = name;
}
