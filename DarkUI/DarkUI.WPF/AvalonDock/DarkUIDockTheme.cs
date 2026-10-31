using System;

namespace DarkUI.WPF.AvalonDock;

public sealed class DarkUIDockTheme : global::AvalonDock.Themes.Theme
{
	// AvalonDock evaluates GetResourceUri() each time the DockingManager.Theme
	// property changes — so callers can swap dock themes at runtime by setting
	// DockingManager.Theme to a new DarkUIDockTheme instance after the editor
	// changes Defaults.CurrentTheme.
	public override Uri GetResourceUri()
	{
		var relative = Defaults.CurrentTheme switch
		{
			Theme.WinRoomEdit => "/DarkUI.WPF;component/Styles/AvalonDock/Theme_WinRoomEdit.xaml",
			_ => "/DarkUI.WPF;component/Styles/AvalonDock/Theme.xaml"
		};

		return new Uri(relative, UriKind.Relative);
	}
}
