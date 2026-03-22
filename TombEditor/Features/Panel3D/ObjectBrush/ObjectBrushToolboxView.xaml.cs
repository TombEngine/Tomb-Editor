using System.ComponentModel;
using System.Windows.Controls;

namespace TombEditor.Features.Panel3D.ObjectBrush;

public partial class ObjectBrushToolboxView : UserControl
{
	private ObjectBrushToolboxViewModel _viewModel;

	public ObjectBrushToolboxView()
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
