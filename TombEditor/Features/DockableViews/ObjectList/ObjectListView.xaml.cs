#nullable enable

using System.Windows.Controls;
using System.Windows.Input;

namespace TombEditor.Features.DockableViews.ObjectList;

public partial class ObjectListView : UserControl
{
	private readonly ObjectListViewModel _viewModel;

	public ObjectListView()
	{
		InitializeComponent();
		_viewModel = new ObjectListViewModel(Editor.Instance);
		DataContext = _viewModel;
		_viewModel.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ObjectListViewModel.SelectedItem) && _viewModel.SelectedItem is { } selected)
				ObjectsList.ScrollIntoView(selected);
		};
	}

	public void Cleanup() => _viewModel.Cleanup();

	private void ObjectsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (_viewModel.EditObjectCommand.CanExecute(null))
			_viewModel.EditObjectCommand.Execute(null);
	}
}
