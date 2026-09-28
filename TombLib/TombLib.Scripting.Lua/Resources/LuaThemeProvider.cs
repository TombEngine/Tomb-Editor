using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Providers;

namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Provides the Lua editor themes through the shared scripting color provider contract.
/// </summary>
public sealed class LuaThemeProvider : ITextEditorColorProvider
{
	/// <inheritdoc/>
	public IReadOnlyList<string> GetAvailableNames()
	{
		return LuaThemeRepository.GetAvailableThemes()
			.Select(static theme => theme.Name)
			.ToArray();
	}

	/// <inheritdoc/>
	public string GetSelectedName(TextEditorConfigBase config)
		=> TextEditorColorProviderGuard.GetConfig<LuaEditorConfiguration>(config, GetType().Name).SelectedThemeName;

	/// <inheritdoc/>
	public void SetSelectedName(TextEditorConfigBase config, string name)
		=> TextEditorColorProviderGuard.GetConfig<LuaEditorConfiguration>(config, GetType().Name).SelectedThemeName = name;
}
