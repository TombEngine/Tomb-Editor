#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.ToolStrips;

internal sealed partial class StudioCommandSurfaceItemViewModel : ObservableObject
{
	private readonly Action<UICommand>? _invokeCommand;

	public StudioCommandSurfaceItemViewModel(
		UICommand command,
		string text,
		string toolTipText,
		string shortcutDisplayText,
		ImageSource? icon,
		bool checkOnClick,
		bool showText,
		bool isSeparator,
		Action<UICommand>? invokeCommand,
		ObservableCollection<StudioCommandSurfaceItemViewModel>? items = null)
	{
		Command = command;
		Text = text;
		ToolTipText = toolTipText;
		ShortcutDisplayText = shortcutDisplayText;
		Icon = icon;
		CheckOnClick = checkOnClick;
		ShowText = showText;
		IsSeparator = isSeparator;
		Items = items ?? [];
		_invokeCommand = invokeCommand;
	}

	public UICommand Command { get; }

	public string ShortcutDisplayText { get; }

	public ImageSource? Icon { get; }

	public bool CheckOnClick { get; }

	public bool ShowText { get; }

	public bool IsSeparator { get; }

	public ObservableCollection<StudioCommandSurfaceItemViewModel> Items { get; }

	public bool HasChildren => Items.Count > 0;

	[ObservableProperty]
	private string _text;

	[ObservableProperty]
	private string _toolTipText;

	[ObservableProperty]
	private bool _isEnabled = true;

	[ObservableProperty]
	private bool _isVisible = true;

	[ObservableProperty]
	private bool _isChecked;

	public void Invoke()
	{
		if (IsSeparator || HasChildren || Command == UICommand.None)
			return;

		_invokeCommand?.Invoke(Command);
	}
}
