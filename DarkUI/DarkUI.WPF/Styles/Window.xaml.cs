using System.Windows;

namespace DarkUI.WPF.Styles
{
	public partial class CustomWindow : ResourceDictionary
	{
		private void CloseWindow_Event(object sender, RoutedEventArgs e)
		{
			if (e.Source != null)
				try { CloseWindow(Window.GetWindow((FrameworkElement)e.Source)); }
				catch { }
		}

		private void AutoMinimize_Event(object sender, RoutedEventArgs e)
		{
			if (e.Source != null)
				try { MaximizeRestore(Window.GetWindow((FrameworkElement)e.Source)); }
				catch { }
		}

		private void Minimize_Event(object sender, RoutedEventArgs e)
		{
			if (e.Source != null)
				try { MinimizeWindow(Window.GetWindow((FrameworkElement)e.Source)); }
				catch { }
		}

		public static void CloseWindow(Window window) => window.Close();

		public static void MaximizeRestore(Window window)
		{
			if (window.WindowState == WindowState.Maximized)
				window.WindowState = WindowState.Normal;
			else if (window.WindowState == WindowState.Normal)
				window.WindowState = WindowState.Maximized;
		}

		public static void MinimizeWindow(Window window) => window.WindowState = WindowState.Minimized;
	}
}
