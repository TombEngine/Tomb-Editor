using System.Windows.Controls;
using System.Windows.Input;
using TombLib.WPF;

namespace TombLib.Scripting.UI.Rendering;

internal static class TextEditorContextMenuFactory
{
	public static ContextMenu BuildDefault()
	{
		var menu = new ContextMenu();

		var cutItem = new MenuItem { Header = Localizer.Instance["Common.Cut"], Command = ApplicationCommands.Cut };
		var copyItem = new MenuItem { Header = Localizer.Instance["Common.Copy"], Command = ApplicationCommands.Copy };
		var pasteItem = new MenuItem { Header = Localizer.Instance["Common.Paste"], Command = ApplicationCommands.Paste };
		var selectAllItem = new MenuItem { Header = Localizer.Instance["Common.SelectAll"], Command = ApplicationCommands.SelectAll };

		menu.Items.Add(cutItem);
		menu.Items.Add(copyItem);
		menu.Items.Add(pasteItem);
		menu.Items.Add(new Separator());
		menu.Items.Add(selectAllItem);

		return menu;
	}
}
