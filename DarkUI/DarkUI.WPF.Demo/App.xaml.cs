using System;

namespace DarkUI.WPF.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
	public static void SwitchTheme()
	{
		if (Defaults.CurrentTheme is Theme.Dark)
		{
			Current.Resources.MergedDictionaries[1].Source = new Uri("/DarkUI.WPF;component/Dictionaries/LightColors.xaml", UriKind.RelativeOrAbsolute);
			Defaults.CurrentTheme = Theme.Light;
		}
		else if (Defaults.CurrentTheme is Theme.Light)
		{
			Current.Resources.MergedDictionaries[1].Source = new Uri("/DarkUI.WPF;component/Dictionaries/DarkColors.xaml", UriKind.RelativeOrAbsolute);
			Defaults.CurrentTheme = Theme.Dark;
		}
	}
}
