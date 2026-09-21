#nullable enable

using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.CommandSurface;

internal static class StudioCommandSurfaceItemFactory
{
	public static StudioToolStripItem CreateCommandItem(
		string langKey,
		UICommand command,
		string icon = "",
		string shortcutDisplayText = "",
		bool checkOnClick = false,
		int? position = null) => new()
		{
			LangKey = langKey,
			Command = command,
			Icon = icon,
			ShortcutDisplayText = shortcutDisplayText,
			CheckOnClick = checkOnClick,
			Position = position
		};

	public static StudioToolStripButton CreateButtonItem(string langKey, UICommand command, string icon) => new()
	{
		LangKey = langKey,
		Command = command,
		Icon = icon
	};

	public static StudioToolStripItem CreateGroupItem(string langKey, params StudioToolStripItem[] dropDownItems) => new()
	{
		LangKey = langKey,
		DropDownItems = [.. dropDownItems]
	};

	public static StudioToolStripItem CreateRootItem(string langKey, int position, params StudioToolStripItem[] dropDownItems) => new()
	{
		LangKey = langKey,
		Position = position,
		DropDownItems = [.. dropDownItems]
	};

	public static StudioSeparator CreateSeparator() => new();

	public static List<StudioToolStripItem> CloneItems(IReadOnlyList<StudioToolStripItem> items)
	{
		var clones = new List<StudioToolStripItem>(items.Count);

		foreach (StudioToolStripItem item in items)
			clones.Add(CloneItem(item));

		return clones;
	}

	public static StudioToolStripItem CloneItem(StudioToolStripItem item)
	{
		if (item is StudioSeparator)
			return CreateSeparator();

		if (item is StudioToolStripButton)
		{
			return new StudioToolStripButton
			{
				LangKey = item.LangKey,
				Command = item.Command,
				Icon = item.Icon,
				CheckOnClick = item.CheckOnClick,
				ShortcutDisplayText = item.ShortcutDisplayText,
				Position = item.Position,
				DropDownItems = CloneItems(item.DropDownItems)
			};
		}

		return new StudioToolStripItem
		{
			LangKey = item.LangKey,
			Command = item.Command,
			Icon = item.Icon,
			CheckOnClick = item.CheckOnClick,
			ShortcutDisplayText = item.ShortcutDisplayText,
			Position = item.Position,
			DropDownItems = CloneItems(item.DropDownItems)
		};
	}
}
