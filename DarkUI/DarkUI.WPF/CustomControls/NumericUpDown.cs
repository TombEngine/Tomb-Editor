using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DarkUI.WPF.CustomControls;

[TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_IncreaseButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_DecreaseButton", Type = typeof(Button))]
public class NumericUpDown : Control
{
	public static readonly DependencyProperty ValueProperty;
	public static readonly DependencyProperty MinimumProperty;
	public static readonly DependencyProperty MaximumProperty;
	public static readonly DependencyProperty IncrementProperty;
	public static readonly DependencyProperty FormatStringProperty;
	public static readonly DependencyProperty TextAlignmentProperty;

	public event EventHandler? ValueChanged;

	public double Value
	{
		get => (double)GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}

	public double Minimum
	{
		get => (double)GetValue(MinimumProperty);
		set => SetValue(MinimumProperty, value);
	}

	public double Maximum
	{
		get => (double)GetValue(MaximumProperty);
		set => SetValue(MaximumProperty, value);
	}

	public double Increment
	{
		get => (double)GetValue(IncrementProperty);
		set => SetValue(IncrementProperty, value);
	}

	public string FormatString
	{
		get => (string)GetValue(FormatStringProperty);
		set => SetValue(FormatStringProperty, value);
	}

	public TextAlignment TextAlignment
	{
		get => (TextAlignment)GetValue(TextAlignmentProperty);
		set => SetValue(TextAlignmentProperty, value);
	}

	public TextBox? TextBox { get; set; }
	public Button? IncreaseButton { get; set; }
	public Button? DecreaseButton { get; set; }

	static NumericUpDown()
	{
		ValueProperty = DependencyProperty.Register(
			nameof(Value),
			typeof(double),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

		MinimumProperty = DependencyProperty.Register(
			nameof(Minimum),
			typeof(double),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(double.MinValue));

		MaximumProperty = DependencyProperty.Register(
			nameof(Maximum),
			typeof(double),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(double.MaxValue));

		IncrementProperty = DependencyProperty.Register(
			nameof(Increment),
			typeof(double),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(1.0));

		FormatStringProperty = DependencyProperty.Register(
			nameof(FormatString),
			typeof(string),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata("F0"));

		TextAlignmentProperty = DependencyProperty.Register(
			nameof(TextAlignment),
			typeof(TextAlignment),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(TextAlignment.Right));
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		TextBox = GetTemplateChild("PART_TextBox") as TextBox;
		IncreaseButton = GetTemplateChild("PART_IncreaseButton") as Button;
		DecreaseButton = GetTemplateChild("PART_DecreaseButton") as Button;

		if (TextBox is not null)
		{
			TextBox.Text = Value.ToString(FormatString);
			TextBox.TextAlignment = TextAlignment;
			TextBox.PreviewTextInput += TextBox_PreviewTextInput;
			TextBox.PreviewKeyUp += TextBox_PreviewKeyUp;
			TextBox.LostFocus += TextBox_LostFocus;
		}

		if (IncreaseButton is not null)
			IncreaseButton.Click += (sender, e) => Value = Math.Min(Value + Increment, Maximum);

		if (DecreaseButton is not null)
			DecreaseButton.Click += (sender, e) => Value = Math.Max(Value - Increment, Minimum);
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);

		if (TextBox is not null)
		{
			if (e.Property == ValueProperty)
				TextBox.Text = Value.ToString(FormatString);
			else if (e.Property == TextAlignmentProperty)
				TextBox.TextAlignment = TextAlignment;
		}
	}

	protected override void OnPreviewKeyDown(KeyEventArgs e)
	{
		if (e.Key == Key.Up)
		{
			Value = Math.Min(Value + Increment, Maximum);
			e.Handled = true;
		}
		else if (e.Key == Key.Down)
		{
			Value = Math.Max(Value - Increment, Minimum);
			e.Handled = true;
		}
	}

	protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
	{
		base.OnPreviewMouseWheel(e);

		if (e.Delta > 0)
			Value = Math.Min(Value + Increment, Maximum);
		else if (e.Delta < 0)
			Value = Math.Max(Value - Increment, Minimum);

		e.Handled = true;
	}

	private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
		{
			if (double.TryParse(TextBox!.Text, out double value))
				Value = Math.Min(Math.Max(value, Minimum), Maximum);
			else
				TextBox.Text = Value.ToString(FormatString);
		}
	}

	private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		// Prevent character input, but allow the current culture's decimal separator
		if (!double.TryParse(e.Text, out _) && e.Text != CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
			e.Handled = true;
	}

	private void TextBox_LostFocus(object sender, RoutedEventArgs e)
	{
		if (double.TryParse(TextBox!.Text, out double value))
			Value = Math.Min(Math.Max(value, Minimum), Maximum);
		else
			TextBox.Text = Value.ToString(FormatString);
	}

	private static void OnValueChanged(DependencyObject dependency, DependencyPropertyChangedEventArgs e)
	{
		var control = (NumericUpDown)dependency;
		control.ValueChanged?.Invoke(control, EventArgs.Empty);
	}
}
