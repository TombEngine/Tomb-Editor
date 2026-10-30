using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Search;

public partial class SearchWindow : Window
{
	private bool _firstRowSelectionQueued;

	public SearchWindow()
	{
		InitializeComponent();
		this.HookModalAutoClose();
		Loaded += (_, _) =>
		{
			WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormSearch");

			if (DataContext is SearchWindowViewModel vm)
				((INotifyCollectionChanged)vm.Rows).CollectionChanged += (_, _) => QueueFirstRowSelection();

			QueueFirstRowSelection();
		};
		Closed += (_, _) =>
		{
			if (DataContext is SearchWindowViewModel vm)
				vm.Dispose();
		};
	}

	// The WinForms grid always has its first row selected.
	private void QueueFirstRowSelection()
	{
		if (_firstRowSelectionQueued)
			return;

		_firstRowSelectionQueued = true;
		Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
		{
			_firstRowSelectionQueued = false;

			if (resultsGrid.SelectedItem is null && resultsGrid.Items.Count > 0)
				resultsGrid.SelectedIndex = 0;
		});
	}

	private void ResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (DataContext is SearchWindowViewModel vm && sender is DataGrid grid)
			vm.SelectedRows = [.. grid.SelectedItems.OfType<SearchWindowViewModel.SearchResultRow>()];
	}
}
