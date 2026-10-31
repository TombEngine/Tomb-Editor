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

		private void WindowTemplate_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is not FrameworkElement root || Window.GetWindow(root) is not Window window)
				return;

			// Workaround for the WPF WindowChrome + SizeToContent bug (dotnet/wpf#8419): the HWND
			// gets the default non-client frame metrics added even though the chrome draws no
			// frame, so the window ends up larger than its visual tree and shows an unpainted
			// black band at the bottom/right. Once the template is arranged, pin the window to
			// the size its visual root actually occupies.
			SizeToContent mode = window.SizeToContent;
			if (mode == SizeToContent.Manual || window.WindowState != WindowState.Normal)
				return;

			window.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				// DesiredSize (not ActualWidth/Height): the root gets *arranged* to fill the
				// inflated client area, but its measured desired size is the true content size.
				Size desired = root.DesiredSize;
				if (desired.Width <= 0 || desired.Height <= 0 || window.WindowState != WindowState.Normal)
					return;

				double deltaW = window.ActualWidth - desired.Width;
				double deltaH = window.ActualHeight - desired.Height;

				window.SizeToContent = SizeToContent.Manual;

				if (mode is SizeToContent.Width or SizeToContent.WidthAndHeight)
					window.Width = desired.Width;
				if (mode is SizeToContent.Height or SizeToContent.WidthAndHeight)
					window.Height = desired.Height;

				// The window was centered using the inflated size; shift by half the removed
				// excess so it stays visually centered.
				if (window.WindowStartupLocation is WindowStartupLocation.CenterOwner or WindowStartupLocation.CenterScreen)
				{
					if (mode is SizeToContent.Width or SizeToContent.WidthAndHeight)
						window.Left += deltaW / 2.0;
					if (mode is SizeToContent.Height or SizeToContent.WidthAndHeight)
						window.Top += deltaH / 2.0;
				}
			}), System.Windows.Threading.DispatcherPriority.Loaded);
		}
	}
}
