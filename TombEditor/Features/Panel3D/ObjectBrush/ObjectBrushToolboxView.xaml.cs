#nullable enable

using System.ComponentModel;
using System.Windows.Controls;

namespace TombEditor.Features.Panel3D.ObjectBrush;

public partial class ObjectBrushToolboxView : UserControl
{
    private readonly ObjectBrushToolboxViewModel? _viewModel;

    public ObjectBrushToolboxView()
    {
        InitializeComponent();

        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            _viewModel = new ObjectBrushToolboxViewModel();
            DataContext = _viewModel;
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
