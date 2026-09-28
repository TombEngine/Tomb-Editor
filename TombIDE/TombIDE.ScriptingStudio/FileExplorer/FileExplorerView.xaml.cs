#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TombIDE.ScriptingStudio.FileExplorer;

public partial class FileExplorerView : UserControl
{
	public FileExplorerView()
		=> InitializeComponent();

	private FileExplorerViewModel? ViewModel => DataContext as FileExplorerViewModel;

	private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (ViewModel?.OpenSelectedItemCommand.CanExecute(null) == true)
			ViewModel.OpenSelectedItemCommand.Execute(null);
	}

	private void TreeView_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (ViewModel is null)
			return;

		switch (e.Key)
		{
			case Key.Enter when ViewModel.OpenSelectedItemCommand.CanExecute(null):
				ViewModel.OpenSelectedItemCommand.Execute(null);
				e.Handled = true;
				break;

			case Key.F2 when ViewModel.RenameSelectedItemCommand.CanExecute(null):
				ViewModel.RenameSelectedItemCommand.Execute(null);
				e.Handled = true;
				break;

			case Key.Delete when ViewModel.DeleteSelectedItemCommand.CanExecute(null):
				ViewModel.DeleteSelectedItemCommand.Execute(null);
				e.Handled = true;
				break;
		}
	}

	private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (ViewModel is not null)
			ViewModel.SelectedItem = e.NewValue as FileExplorerItemViewModel;
	}
}
