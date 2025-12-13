using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace DarkUI.WPF;

/// <summary>
/// Adorner for the watermark.
/// </summary>
public class WatermarkAdorner : Adorner
{
	/// <summary>
	/// <see cref="ContentPresenter"/> that holds the watermark.
	/// </summary>
	private readonly ContentPresenter contentPresenter;

	/// <summary>
	/// Initializes a new instance of the <see cref="WatermarkAdorner"/> class.
	/// </summary>
	/// <param name="adornedElement"><see cref="UIElement"/> to be adorned.</param>
	/// <param name="watermark">The watermark.</param>
	public WatermarkAdorner(UIElement adornedElement, object watermark) : base(adornedElement)
	{
		IsHitTestVisible = false;

		contentPresenter = new ContentPresenter
		{
			Content = watermark,
			Margin = new Thickness(3),
			Opacity = 0.5,
			VerticalAlignment = VerticalAlignment.Center
		};

		if (Control is ItemsControl and not ComboBox)
			contentPresenter.HorizontalAlignment = HorizontalAlignment.Center;

		// Hide the control adorner when the adorned element is hidden
		var binding = new Binding("IsVisible")
		{
			Source = adornedElement,
			Converter = new BooleanToVisibilityConverter()
		};

		SetBinding(VisibilityProperty, binding);
	}

	/// <summary>
	/// Gets the number of children for the <see cref="ContainerVisual"/>.
	/// </summary>
	protected override int VisualChildrenCount => 1;

	/// <summary>
	/// Gets the control that is being adorned
	/// </summary>
	private Control Control => (Control)AdornedElement;

	/// <summary>
	/// Returns a specified child <see cref="Visual"/> for the parent <see cref="ContainerVisual"/>.
	/// </summary>
	/// <param name="index">A 32-bit signed integer that represents the index value of the child <see cref="Visual"/>. The value of index must be between 0 and <see cref="VisualChildrenCount"/> - 1.</param>
	/// <returns>The child <see cref="Visual"/>.</returns>
	protected override Visual GetVisualChild(int index)
		=> contentPresenter;

	/// <summary>
	/// Implements any custom measuring behavior for the adorner.
	/// </summary>
	/// <param name="constraint">A size to constrain the adorner to.</param>
	/// <returns>A <see cref="Size"/> object representing the amount of layout space needed by the adorner.</returns>
	protected override Size MeasureOverride(Size constraint)
	{
		// Here's the secret to getting the adorner to cover the whole control
		contentPresenter.Measure(Control.RenderSize);
		return Control.RenderSize;
	}

	/// <summary>
	/// When overridden in a derived class, positions child elements and determines a size for a <see cref="FrameworkElement"/> derived class.
	/// </summary>
	/// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
	/// <returns>The actual size used.</returns>
	protected override Size ArrangeOverride(Size finalSize)
	{
		contentPresenter.Arrange(new Rect(finalSize));
		return finalSize;
	}
}
