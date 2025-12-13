using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace DarkUI.WPF;

/// <summary>
/// Class that provides the Watermark attached property.
/// </summary>
public static class WatermarkService
{
	/// <summary>
	/// Watermark Attached Dependency Property.
	/// </summary>
	public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
	   "Watermark",
	   typeof(object),
	   typeof(WatermarkService),
	   new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnWatermarkChanged)));

	/// <summary>
	/// Dictionary of ItemsControls.
	/// </summary>
	private static readonly Dictionary<object, ItemsControl> _itemsControls = new();

	/// <summary>
	/// Gets the Watermark property. This dependency property indicates the watermark for the control.
	/// </summary>
	/// <param name="d"><see cref="DependencyObject"/> to get the property from.</param>
	/// <returns>The value of the Watermark property.</returns>
	public static object GetWatermark(DependencyObject d)
		=> d.GetValue(WatermarkProperty);

	/// <summary>
	/// Sets the Watermark property. This dependency property indicates the watermark for the control.
	/// </summary>
	/// <param name="d"><see cref="DependencyObject"/> to set the property on.</param>
	/// <param name="value">Value of the property.</param>
	public static void SetWatermark(DependencyObject d, object value)
		=> d.SetValue(WatermarkProperty, value);

	/// <summary>
	/// Handles changes to the Watermark property.
	/// </summary>
	/// <param name="d"><see cref="DependencyObject"/> that fired the event.</param>
	/// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
	private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (Control)d;
		control.Loaded += Control_Loaded;

		if (d is ComboBox comboBox)
		{
			comboBox.GotKeyboardFocus += Control_GotKeyboardFocus;
			comboBox.LostKeyboardFocus += Control_Loaded;
		}
		else if (d is TextBox textBox)
		{
			textBox.GotKeyboardFocus += Control_GotKeyboardFocus;
			textBox.LostKeyboardFocus += Control_Loaded;
			textBox.TextChanged += Control_GotKeyboardFocus;
		}

		if (d is ItemsControl itemsControl and not ComboBox)
		{
			ItemsControl i = itemsControl;

			// For Items property
			i.ItemContainerGenerator.ItemsChanged += ItemsChanged;
			_itemsControls.Add(i.ItemContainerGenerator, i);

			// For ItemsSource property
			var prop = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, i.GetType());
			prop.AddValueChanged(i, ItemsSourceChanged);
		}
	}

	/// <summary>
	/// Handle the GotFocus event on the control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
	private static void Control_GotKeyboardFocus(object sender, RoutedEventArgs e)
	{
		var c = (Control)sender;

		if (ShouldShowWatermark(c))
			ShowWatermark(c);
		else
			RemoveWatermark(c);
	}

	/// <summary>
	/// Handle the Loaded and LostFocus event on the control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
	private static void Control_Loaded(object sender, RoutedEventArgs e)
	{
		var control = (Control)sender;

		if (ShouldShowWatermark(control))
			ShowWatermark(control);
	}

	/// <summary>
	/// Event handler for the items source changed event.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
	private static void ItemsSourceChanged(object? sender, EventArgs e)
	{
		if (sender is not ItemsControl c)
			return;

		if (c.ItemsSource != null)
		{
			if (ShouldShowWatermark(c))
				ShowWatermark(c);
			else
				RemoveWatermark(c);
		}
		else
			ShowWatermark(c);
	}

	/// <summary>
	/// Event handler for the items changed event.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="ItemsChangedEventArgs"/> that contains the event data.</param>
	private static void ItemsChanged(object sender, ItemsChangedEventArgs e)
	{
		if (_itemsControls.TryGetValue(sender, out ItemsControl? control))
		{
			if (ShouldShowWatermark(control))
				ShowWatermark(control);
			else
				RemoveWatermark(control);
		}
	}

	/// <summary>
	/// Remove the watermark from the specified element.
	/// </summary>
	/// <param name="control">Element to remove the watermark from.</param>
	private static void RemoveWatermark(UIElement control)
	{
		var layer = AdornerLayer.GetAdornerLayer(control);

		// layer could be null if control is no longer in the visual tree
		if (layer != null)
		{
			Adorner[] adorners = layer.GetAdorners(control);

			if (adorners == null)
				return;

			foreach (Adorner adorner in adorners)
			{
				if (adorner is WatermarkAdorner)
				{
					adorner.Visibility = Visibility.Hidden;
					layer.Remove(adorner);
				}
			}
		}
	}

	/// <summary>
	/// Show the watermark on the specified control.
	/// </summary>
	/// <param name="control">Control to show the watermark on.</param>
	private static void ShowWatermark(Control control)
	{
		var layer = AdornerLayer.GetAdornerLayer(control);
		layer?.Add(new WatermarkAdorner(control, GetWatermark(control)));
	}

	/// <summary>
	/// Indicates whether or not the watermark should be shown on the specified control.
	/// </summary>
	/// <param name="c"><see cref="Control"/> to test.</param>
	/// <returns><see langword="true"/> if the watermark should be shown; <see langword="false"/> otherwise.</returns>
	private static bool ShouldShowWatermark(Control c)
	{
		if (c is ComboBox comboBox)
			return comboBox.Text == string.Empty;
		else if (c is TextBox textBox)
			return textBox.Text == string.Empty;
		else if (c is ItemsControl itemsControl)
			return itemsControl.Items.Count == 0;
		else
			return false;
	}
}
