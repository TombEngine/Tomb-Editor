#nullable enable

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TombIDE.ScriptingStudio.ToolStrips;

internal sealed class StudioMenuStripView : UserControl
{
	private static readonly BooleanToVisibilityConverter VisibilityConverter = new();
	private readonly Menu _menu;

	public StudioMenuStripView()
	{
		_menu = new Menu();
		_menu.SetResourceReference(Control.BackgroundProperty, "Brush_Background");
		_menu.SetResourceReference(Control.ForegroundProperty, "Brush_Text");
		Content = _menu;
	}

	public void SetItems(IReadOnlyList<StudioCommandSurfaceItemViewModel> items)
	{
		_menu.Items.Clear();

		foreach (StudioCommandSurfaceItemViewModel item in items)
			_menu.Items.Add(CreateMenuElement(item));
	}

	private object CreateMenuElement(StudioCommandSurfaceItemViewModel item)
	{
		if (item.IsSeparator)
			return new Separator();

		var menuItem = new MenuItem
		{
			IsCheckable = item.CheckOnClick,
			InputGestureText = item.ShortcutDisplayText
		};

		menuItem.SetBinding(HeaderedItemsControl.HeaderProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.Text)) { Source = item });
		menuItem.SetBinding(UIElement.IsEnabledProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.IsEnabled)) { Source = item });
		menuItem.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.IsVisible)) { Source = item, Converter = VisibilityConverter });
		menuItem.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.IsChecked)) { Mode = BindingMode.TwoWay, Source = item });
		menuItem.SetBinding(FrameworkElement.ToolTipProperty, new Binding(nameof(StudioCommandSurfaceItemViewModel.ToolTipText)) { Source = item });

		if (item.Icon is not null)
		{
			menuItem.Icon = new Image
			{
				Height = 16.0,
				Source = item.Icon,
				Width = 16.0
			};
		}

		if (item.HasChildren)
		{
			foreach (StudioCommandSurfaceItemViewModel childItem in item.Items)
				menuItem.Items.Add(CreateMenuElement(childItem));
		}
		else
		{
			menuItem.Click += (_, _) => item.Invoke();
		}

		return menuItem;
	}
}
