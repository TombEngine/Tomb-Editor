using DarkUI.Controls;
using System;

namespace TombEditor.Features.Panel3D.ObjectBrush;

public partial class ObjectBrushToolbox : DarkFloatingToolbox
{
	public ObjectBrushToolbox()
	{
		VerticalGrip = false;
		GripSize = 12;
		AutoAnchor = true;
		SnapToBorders = true;
		DragAnyPoint = true;

		InitializeComponent();

		_toolboxView.Loaded += OnToolboxViewLoaded;
	}

	private void OnToolboxViewLoaded(object sender, System.Windows.RoutedEventArgs e)
	{
		_toolboxView.Loaded -= OnToolboxViewLoaded;
		UpdateSizeFromContent();
	}

	// Measures WPF content and resizes the floating toolbox to fit.
	private void UpdateSizeFromContent()
	{
		_toolboxView.Measure(new System.Windows.Size(
			double.PositiveInfinity, double.PositiveInfinity));

		var desired = _toolboxView.DesiredSize;
		float dpiScale = DeviceDpi / 96f;

		int contentWidth = (int)Math.Ceiling(desired.Width * dpiScale);
		int contentHeight = (int)Math.Ceiling(desired.Height * dpiScale);

		Size = new System.Drawing.Size(
			contentWidth + Padding.Left + Padding.Right,
			contentHeight + Padding.Top + Padding.Bottom);
	}
}
