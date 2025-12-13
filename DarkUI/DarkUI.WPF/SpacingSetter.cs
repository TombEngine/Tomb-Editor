using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DarkUI.WPF;

public static class SpacingSetter
{
	public static double GetSpacing(DependencyObject obj)
		=> (double)obj.GetValue(SpacingProperty);

	public static void SetSpacing(DependencyObject obj, double value)
		=> obj.SetValue(SpacingProperty, value);

	public static readonly DependencyProperty SpacingProperty
		= DependencyProperty.RegisterAttached("Spacing", typeof(double), typeof(SpacingSetter), new UIPropertyMetadata(0.0, OnSpacingChanged));

	public static void OnSpacingChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is not Panel panel)
			return;

		panel.Loaded += Panel_Loaded;
	}

	private static void Panel_Loaded(object sender, RoutedEventArgs e)
	{
		var panel = (Panel)sender;
		var children = (IList)panel.Children;

		if (children.Count < 2)
			return;

		for (int i = 0; i < children.Count; i++)
		{
			bool isLastElement = i >= children.Count - 1;

			if (isLastElement || children[i] is not FrameworkElement element)
				continue;

			double spacing = GetSpacing(panel);

			element.Margin = panel.LogicalOrientationPublic switch
			{
				Orientation.Horizontal => new Thickness(element.Margin.Left, element.Margin.Top, spacing, element.Margin.Bottom),
				_ => new Thickness(element.Margin.Left, element.Margin.Top, element.Margin.Right, spacing),
			};
		}
	}
}
