using System.ComponentModel;
using System.Windows.Controls;

using TombEditor.ViewModels;

namespace TombEditor.Views;

public partial class ObjectBrushToolBox : UserControl
{
	private ObjectBrushToolboxViewModel _viewModel;

	public ObjectBrushToolBox()
	{
		InitializeComponent();

		if (!DesignerProperties.GetIsInDesignMode(this))
		{
			_viewModel = new ObjectBrushToolboxViewModel();
			DataContext = _viewModel;
			Unloaded += (_, _) => _viewModel.Cleanup();
		}
	}

	/// <summary>
	/// Cleans up the ViewModel, unsubscribing from Editor events.
	/// Safe to call multiple times.
	/// </summary>
	public void Cleanup()
	{
		_viewModel?.Cleanup();
	}
}
