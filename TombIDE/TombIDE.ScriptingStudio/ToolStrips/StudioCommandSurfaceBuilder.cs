#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.ToolStrips;

internal sealed class StudioCommandSurfaceBuilder
{
	private readonly IKeyBindingService<UICommand>? _keyBindingService;

	public StudioCommandSurfaceBuilder(IKeyBindingService<UICommand>? keyBindingService = null)
	{
		_keyBindingService = keyBindingService;
	}

	public IReadOnlyList<StudioCommandSurfaceItemViewModel> BuildMenuItems(
		IReadOnlyList<StudioToolStripItem>? workspaceItems,
		IReadOnlyList<StudioToolStripItem>? documentModeItems,
		Action<UICommand> invokeCommand)
		=> BuildItems(CombineMenuItems(workspaceItems, documentModeItems), false, invokeCommand);

	public IReadOnlyList<StudioCommandSurfaceItemViewModel> BuildToolStripItems(
		IReadOnlyList<StudioToolStripItem>? workspaceItems,
		IReadOnlyList<StudioToolStripItem>? documentModeItems,
		Action<UICommand> invokeCommand)
	{
		var items = new List<StudioToolStripItem>();

		if (workspaceItems is not null)
			items.AddRange(workspaceItems);

		if (documentModeItems is not null)
			items.AddRange(documentModeItems);

		return BuildItems(items, true, invokeCommand);
	}

	private IReadOnlyList<StudioCommandSurfaceItemViewModel> BuildItems(
		IEnumerable<StudioToolStripItem> items,
		bool toolStripMode,
		Action<UICommand> invokeCommand)
		=> items.Select(item => BuildItem(item, toolStripMode, invokeCommand)).ToArray();

	private StudioCommandSurfaceItemViewModel BuildItem(
		StudioToolStripItem item,
		bool toolStripMode,
		Action<UICommand> invokeCommand)
	{
		if (item is StudioSeparator)
		{
			return new StudioCommandSurfaceItemViewModel(
				UICommand.None,
				string.Empty,
				string.Empty,
				string.Empty,
				null,
				checkOnClick: false,
				showText: false,
				isSeparator: true,
				invokeCommand: null);
		}

		UICommand command = item.Command;
		string text = StudioCommandSurfaceResources.GetItemText(item);
		string shortcutDisplayText = GetDisplayText(command, item.ShortcutDisplayText);
		var childItems = new ObservableCollection<StudioCommandSurfaceItemViewModel>(
			item.DropDownItems.Select(child => BuildItem(child, toolStripMode, invokeCommand)));

		return new StudioCommandSurfaceItemViewModel(
			command,
			text,
			text,
			shortcutDisplayText,
			WpfImageSourceFactory.Create(item.Icon),
			item.CheckOnClick,
			toolStripMode && item is StudioToolStripButton,
			isSeparator: false,
			invokeCommand,
			childItems);
	}

	private string GetDisplayText(UICommand command, string fallbackDisplayText)
		=> _keyBindingService?.GetDisplayText(command, fallbackDisplayText) ?? fallbackDisplayText;

	private static IReadOnlyList<StudioToolStripItem> CombineMenuItems(
		IReadOnlyList<StudioToolStripItem>? workspaceItems,
		IReadOnlyList<StudioToolStripItem>? documentModeItems)
	{
		var combined = new List<(StudioToolStripItem Item, int Position, int Order)>();
		int order = 0;

		AddRange(workspaceItems);
		AddRange(documentModeItems);

		return combined
			.OrderBy(entry => entry.Position)
			.ThenBy(entry => entry.Order)
			.Select(entry => entry.Item)
			.ToArray();

		void AddRange(IReadOnlyList<StudioToolStripItem>? items)
		{
			if (items is null)
				return;

			foreach (StudioToolStripItem item in items)
			{
				combined.Add((item, item.Position ?? 1000 + order, order));
				order++;
			}
		}
	}
}
