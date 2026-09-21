#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.ToolStrips;

public sealed class StudioToolStrip
{
	private readonly Dictionary<UICommand, List<StudioCommandSurfaceItemViewModel>> _itemsByCommand = [];
	private DocumentMode _documentMode;

	public StudioToolStrip()
	{
		View = new StudioToolStripView();
	}

	internal StudioToolStripView View { get; }

	public IKeyBindingService<UICommand>? KeyBindingService { get; set; }

	public IReadOnlyList<StudioToolStripItem> DocumentModeContributionItems { get; set; } = [];

	public IReadOnlyList<StudioToolStripItem> WorkspaceContributionItems { get; set; } = [];

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DocumentMode DocumentMode
	{
		get => _documentMode;
		set
		{
			if (value == _documentMode)
				return;

			_documentMode = value;
			RebuildAllItems();
		}
	}

	public event EventHandler<StudioCommandInvokedEventArgs>? ItemClicked;

	public event EventHandler? WorkspaceContributionsChanged;

	public bool GetCommandChecked(UICommand command)
	{
		if (!TryGetFirstItem(command, out StudioCommandSurfaceItemViewModel? item) || item is null)
			return false;

		return item.IsChecked;
	}

	public void RebuildWorkspaceItems()
	{
		RebuildAllItems();
		WorkspaceContributionsChanged?.Invoke(this, EventArgs.Empty);
	}

	public void RebuildDocumentModeItems()
		=> RebuildAllItems();

	public void SetCommandChecked(UICommand command, bool isChecked)
		=> Apply(command, item => item.IsChecked = isChecked);

	public void SetCommandEnabled(UICommand command, bool isEnabled)
		=> Apply(command, item => item.IsEnabled = isEnabled);

	public void SetCommandText(UICommand command, string text)
		=> Apply(command, item => item.Text = text);

	public void SetCommandToolTip(UICommand command, string toolTipText)
		=> Apply(command, item => item.ToolTipText = toolTipText);

	public void SetCommandVisible(UICommand command, bool isVisible)
		=> Apply(command, item => item.IsVisible = isVisible);

	private void Apply(UICommand command, Action<StudioCommandSurfaceItemViewModel> update)
	{
		if (!_itemsByCommand.TryGetValue(command, out List<StudioCommandSurfaceItemViewModel>? items))
			return;

		foreach (StudioCommandSurfaceItemViewModel item in items)
			update(item);
	}

	private void HandleCommandInvoked(UICommand command)
		=> ItemClicked?.Invoke(this, new StudioCommandInvokedEventArgs(command));

	private void RebuildAllItems()
	{
		var builder = new StudioCommandSurfaceBuilder(KeyBindingService);
		IReadOnlyList<StudioCommandSurfaceItemViewModel> items = builder.BuildToolStripItems(
			WorkspaceContributionItems,
			DocumentModeContributionItems,
			HandleCommandInvoked);

		Reindex(items);
		View.SetItems(items);
	}

	private void Reindex(IEnumerable<StudioCommandSurfaceItemViewModel> items)
	{
		_itemsByCommand.Clear();

		foreach (StudioCommandSurfaceItemViewModel item in items)
			ReindexItem(item);
	}

	private void ReindexItem(StudioCommandSurfaceItemViewModel item)
	{
		if (item.Command != UICommand.None)
		{
			if (!_itemsByCommand.TryGetValue(item.Command, out List<StudioCommandSurfaceItemViewModel>? mappedItems))
			{
				mappedItems = [];
				_itemsByCommand[item.Command] = mappedItems;
			}

			mappedItems.Add(item);
		}

		foreach (StudioCommandSurfaceItemViewModel child in item.Items)
			ReindexItem(child);
	}

	private bool TryGetFirstItem(UICommand command, out StudioCommandSurfaceItemViewModel? item)
	{
		if (_itemsByCommand.TryGetValue(command, out List<StudioCommandSurfaceItemViewModel>? items) && items.Count > 0)
		{
			item = items[0];
			return true;
		}

		item = null;
		return false;
	}
}
