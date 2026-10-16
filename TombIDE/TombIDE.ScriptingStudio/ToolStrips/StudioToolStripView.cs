#nullable enable

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace TombIDE.ScriptingStudio.ToolStrips;

internal sealed class StudioToolStripView : UserControl
{
	private static readonly BooleanToVisibilityConverter VisibilityConverter = new();
	private readonly ToolBar _toolBar;

	public StudioToolStripView()
	{
		_toolBar = new ToolBar();
		_toolBar.SetResourceReference(Control.BackgroundProperty, "Brush_Background");
		_toolBar.SetResourceReference(Control.ForegroundProperty, "Brush_Text");

		var tray = new ToolBarTray
		{
			IsLocked = true
		};

		tray.SetResourceReference(Control.BackgroundProperty, "Brush_Background");
		tray.ToolBars.Add(_toolBar);

		Content = tray;
	}

	public void SetItems(IReadOnlyList<StudioCommandSurfaceItemViewModel> items)
	{
		_toolBar.Items.Clear();

		foreach (StudioCommandSurfaceItemViewModel item in items)
			_toolBar.Items.Add(CreateToolBarElement(item));
	}

	private object CreateToolBarElement(StudioCommandSurfaceItemViewModel item)
	{
		if (item.IsSeparator)
			return new Separator();

		ButtonBase button = item.CheckOnClick ? new ToggleButton() : new Button();
		button.Click += (_, _) => item.Invoke();
		button.SetBinding(UIElement.IsEnabledProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.IsEnabled)) { Source = item });
		button.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.IsVisible)) { Source = item, Converter = VisibilityConverter });
		button.SetBinding(FrameworkElement.ToolTipProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.ToolTipText)) { Source = item });
		button.Margin = new Thickness(1.0, 0.0, 1.0, 0.0);
		button.Padding = item.ShowText ? new Thickness(6.0, 2.0, 6.0, 2.0) : new Thickness(4.0, 2.0, 4.0, 2.0);

		if (button is ToggleButton toggleButton)
		{
			toggleButton.SetBinding(
				ToggleButton.IsCheckedProperty,
				new Binding(nameof(StudioCommandSurfaceItemViewModel.IsChecked))
				{
					Mode = BindingMode.TwoWay,
					Source = item
				});
		}

		ToolBar.SetOverflowMode(button, OverflowMode.Never);
		button.Content = CreateButtonContent(item);
		return button;
	}

	private static object CreateButtonContent(StudioCommandSurfaceItemViewModel item)
	{
		if (item.ShowText)
		{
			var panel = new StackPanel
			{
				Orientation = Orientation.Horizontal
			};

			if (item.Icon is not null)
			{
				panel.Children.Add(new Image
				{
					Height = 16.0,
					Margin = new Thickness(0.0, 0.0, 6.0, 0.0),
					Source = item.Icon,
					Width = 16.0
				});
			}

			var textBlock = new TextBlock();
			textBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.Text)) { Source = item });
			panel.Children.Add(textBlock);

			return panel;
		}

		if (item.Icon is not null)
		{
			return new Image
			{
				Height = 16.0,
				Source = item.Icon,
				Width = 16.0
			};
		}

		var fallbackTextBlock = new TextBlock();
		fallbackTextBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.Text)) { Source = item });
		return fallbackTextBlock;
	}
}
