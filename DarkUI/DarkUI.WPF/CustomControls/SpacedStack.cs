using System;
using System.Windows;
using System.Windows.Controls;

namespace DarkUI.WPF.CustomControls;

public class SpacedStack : StackPanel
{
	public static readonly DependencyProperty SpacingProperty =
		DependencyProperty.Register(
			nameof(Spacing),
			typeof(double),
			typeof(SpacedStack),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

	/// <summary>
	/// Gets or sets the spacing between items.
	/// </summary>
	public double Spacing
	{
		get => (double)GetValue(SpacingProperty);
		set => SetValue(SpacingProperty, value);
	}

	protected override Size MeasureOverride(Size constraint)
	{
		Size availableSize = constraint;
		Size stackSize = new();
		Size childSize;

		UIElementCollection children = InternalChildren;

		// If no children, return empty size
		if (children.Count == 0)
			return stackSize;

		bool isHorizontal = Orientation == Orientation.Horizontal;
		double spacing = Spacing;
		double spacingCount = Math.Max(0, children.Count - 1);

		// Adjust available size for children based on orientation and spacing
		if (isHorizontal && !double.IsInfinity(availableSize.Width))
			availableSize.Width = Math.Max(0, availableSize.Width - (spacing * spacingCount));
		else if (!isHorizontal && !double.IsInfinity(availableSize.Height))
			availableSize.Height = Math.Max(0, availableSize.Height - (spacing * spacingCount));

		// Measure each child
		for (int i = 0; i < children.Count; i++)
		{
			UIElement child = children[i];

			if (child == null)
				continue;

			child.Measure(availableSize);
			childSize = child.DesiredSize;

			// Accumulate the size depending on orientation
			if (isHorizontal)
			{
				stackSize.Width += childSize.Width;
				stackSize.Height = Math.Max(stackSize.Height, childSize.Height);

				if (i < children.Count - 1)
					stackSize.Width += spacing;
			}
			else
			{
				stackSize.Width = Math.Max(stackSize.Width, childSize.Width);
				stackSize.Height += childSize.Height;

				if (i < children.Count - 1)
					stackSize.Height += spacing;
			}
		}

		return stackSize;
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		UIElementCollection children = InternalChildren;

		if (children.Count == 0)
			return arrangeSize;

		bool isHorizontal = Orientation == Orientation.Horizontal;
		double spacing = Spacing;
		double x = 0;
		double y = 0;

		// Arrange each child
		for (int i = 0; i < children.Count; i++)
		{
			UIElement child = children[i];

			if (child == null)
				continue;

			Size childSize = child.DesiredSize;

			// Determine the position and size based on orientation
			if (isHorizontal)
			{
				double childWidth = childSize.Width;
				child.Arrange(new Rect(x, y, childWidth, arrangeSize.Height));
				x += childWidth + spacing;
			}
			else
			{
				double childHeight = childSize.Height;
				child.Arrange(new Rect(x, y, arrangeSize.Width, childHeight));
				y += childHeight + spacing;
			}
		}

		return arrangeSize;
	}
}
