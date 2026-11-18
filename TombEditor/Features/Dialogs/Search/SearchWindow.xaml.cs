using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Search;

public partial class SearchWindow : Window
{
	public SearchWindow()
	{
		InitializeComponent();
		this.HookModalAutoClose();
		Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormSearch");
		Closed += (_, _) =>
		{
			if (DataContext is SearchWindowViewModel vm)
				vm.Dispose();
		};
	}

	private void ResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (DataContext is SearchWindowViewModel vm && sender is DataGrid grid)
			vm.SelectedRows = [.. grid.SelectedItems.OfType<SearchWindowViewModel.SearchResultRow>()];
	}
}
