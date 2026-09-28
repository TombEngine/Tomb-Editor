using System.Drawing;
using System.Reflection;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.Shared;
using TombIDE.Shared.Local;

namespace TombIDE.ScriptingStudio.CommandSurface;

internal static class StudioCommandSurfaceResources
{
	public static string GetItemText(StudioToolStripItem item)
		=> typeof(Localization).GetProperty(item.LangKey)?.GetValue(Strings.Default)?.ToString() ?? item.LangKey;

	public static Image FindImageInResources(string key)
		=> typeof(Resources).GetProperty(key, BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as Image;
}
