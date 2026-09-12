#nullable enable

using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.CommandSurface;

public class StudioToolStripItem
{
	public string LangKey { get; set; } = string.Empty;
	public UICommand Command { get; set; } = UICommand.None;
	public string Icon { get; set; } = string.Empty;
	public bool CheckOnClick { get; set; }

	/// <summary>
	/// Optional explicit shortcut display text override.
	/// When empty, the display text is obtained from <see cref="Nickelony.IDEKit.KeyBindings.IKeyBindingService{TCommandId}"/>.
	/// </summary>
	public string ShortcutDisplayText { get; set; } = string.Empty;

	public int? Position { get; set; }

	public List<StudioToolStripItem> DropDownItems { get; set; } = [];
}

/// <summary>
/// Wide ToolStrip button (icon + text)
/// </summary>
public sealed class StudioToolStripButton : StudioToolStripItem
{
	public static StudioToolStripButton FromNormalItem(StudioToolStripItem item) => new()
	{
		LangKey = item.LangKey,
		Command = item.Command,
		Icon = item.Icon,
		ShortcutDisplayText = item.ShortcutDisplayText,
		CheckOnClick = item.CheckOnClick,
		Position = item.Position,
		DropDownItems = item.DropDownItems
	};
}

public sealed class StudioSeparator : StudioToolStripItem;
