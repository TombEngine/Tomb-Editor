using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DarkUI.WPF.Styles
{
	public partial class DataGrid : ResourceDictionary
	{
		private void EditingTextBox_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is not TextBox textBox)
				return;

			// Set caret to the end of the text
			textBox.Dispatcher.BeginInvoke(
				() => textBox.CaretIndex = textBox.Text.Length,
				DispatcherPriority.Loaded);
		}
	}
}
