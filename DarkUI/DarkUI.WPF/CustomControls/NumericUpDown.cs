using System;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DarkUI.WPF.CustomControls;

[TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_IncreaseButton", Type = typeof(ButtonBase))]
[TemplatePart(Name = "PART_DecreaseButton", Type = typeof(ButtonBase))]
public class NumericUpDown : Control
{
	private const NumberStyles NumericTextStyle = NumberStyles.Number;
	private const string DecreaseButtonPartName = "PART_DecreaseButton";
	private const string IncreaseButtonPartName = "PART_IncreaseButton";
	private const string TextBoxPartName = "PART_TextBox";

	public static readonly DependencyProperty ValueProperty;
	public static readonly DependencyProperty MinimumProperty;
	public static readonly DependencyProperty MaximumProperty;
	public static readonly DependencyProperty IncrementProperty;
	public static readonly DependencyProperty LargeIncrementProperty;
	public static readonly DependencyProperty DecimalPlacesProperty;
	public static readonly DependencyProperty FormatStringProperty;
	public static readonly DependencyProperty TextAlignmentProperty;
	public static readonly DependencyProperty LoopValuesProperty;
	public static readonly DependencyProperty ChangeValueOnMouseWheelProperty;
	public static readonly DependencyProperty RequireFocusForMouseWheelProperty;
	public static readonly DependencyProperty RepeatDelayProperty;
	public static readonly DependencyProperty RepeatIntervalProperty;

	public event EventHandler? ValueChanged;

	static NumericUpDown()
	{
		ValueProperty = DependencyProperty.Register(
			nameof(Value),
			typeof(double),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue),
			IsValidNumber);

		MinimumProperty = DependencyProperty.Register(
			nameof(Minimum),
			typeof(double),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(double.MinValue, OnMinimumChanged),
			IsValidNumber);

		MaximumProperty = DependencyProperty.Register(
			nameof(Maximum),
			typeof(double),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(double.MaxValue, OnMaximumChanged, CoerceMaximum),
			IsValidNumber);

		IncrementProperty = DependencyProperty.Register(
			nameof(Increment),
			typeof(double),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(1.0),
			IsValidPositiveNumber);

		LargeIncrementProperty = DependencyProperty.Register(
			nameof(LargeIncrement),
			typeof(double),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(0.0),
			IsValidLargeIncrement);

		DecimalPlacesProperty = DependencyProperty.Register(
			nameof(DecimalPlaces),
			typeof(int),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(-1, OnDecimalPlacesChanged),
			IsValidDecimalPlaces);

		FormatStringProperty = DependencyProperty.Register(
			nameof(FormatString),
			typeof(string),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata("F0"),
			value => value is string);

		TextAlignmentProperty = DependencyProperty.Register(
			nameof(TextAlignment),
			typeof(TextAlignment),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(TextAlignment.Right));

		LoopValuesProperty = DependencyProperty.Register(
			nameof(LoopValues),
			typeof(bool),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(false));

		ChangeValueOnMouseWheelProperty = DependencyProperty.Register(
			nameof(ChangeValueOnMouseWheel),
			typeof(bool),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(true));

		RequireFocusForMouseWheelProperty = DependencyProperty.Register(
			nameof(RequireFocusForMouseWheel),
			typeof(bool),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(false));

		RepeatDelayProperty = DependencyProperty.Register(
			nameof(RepeatDelay),
			typeof(int),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(400),
			IsValidNonNegativeInteger);

		RepeatIntervalProperty = DependencyProperty.Register(
			nameof(RepeatInterval),
			typeof(int),
			typeof(NumericUpDown),
			new FrameworkPropertyMetadata(60),
			IsValidPositiveInteger);
	}

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

	public double LargeIncrement
	{
		get => (double)GetValue(LargeIncrementProperty);
		set => SetValue(LargeIncrementProperty, value);
	}

	public int DecimalPlaces
	{
		get => (int)GetValue(DecimalPlacesProperty);
		set => SetValue(DecimalPlacesProperty, value);
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

	public bool LoopValues
	{
		get => (bool)GetValue(LoopValuesProperty);
		set => SetValue(LoopValuesProperty, value);
	}

	public bool ChangeValueOnMouseWheel
	{
		get => (bool)GetValue(ChangeValueOnMouseWheelProperty);
		set => SetValue(ChangeValueOnMouseWheelProperty, value);
	}

	public bool RequireFocusForMouseWheel
	{
		get => (bool)GetValue(RequireFocusForMouseWheelProperty);
		set => SetValue(RequireFocusForMouseWheelProperty, value);
	}

	public int RepeatDelay
	{
		get => (int)GetValue(RepeatDelayProperty);
		set => SetValue(RepeatDelayProperty, value);
	}

	public int RepeatInterval
	{
		get => (int)GetValue(RepeatIntervalProperty);
		set => SetValue(RepeatIntervalProperty, value);
	}

	public TextBox? TextBox { get; private set; }
	public ButtonBase? IncreaseButton { get; private set; }
	public ButtonBase? DecreaseButton { get; private set; }

	public override void OnApplyTemplate()
	{
		ClearTemplatePartHandlers();

		base.OnApplyTemplate();

		TextBox = GetTemplateChild(TextBoxPartName) as TextBox;
		IncreaseButton = GetTemplateChild(IncreaseButtonPartName) as ButtonBase;
		DecreaseButton = GetTemplateChild(DecreaseButtonPartName) as ButtonBase;

		if (TextBox is not null)
		{
			ApplyTextBoxProperties();

			TextBox.PreviewTextInput += TextBox_PreviewTextInput;
			TextBox.KeyDown += TextBox_KeyDown;
			TextBox.LostFocus += TextBox_LostFocus;

			DataObject.AddPastingHandler(TextBox, TextBox_Pasting);
		}

		if (IncreaseButton is not null)
			IncreaseButton.Click += IncreaseButton_Click;

		if (DecreaseButton is not null)
			DecreaseButton.Click += DecreaseButton_Click;

		UpdateButtonStates();
	}

	protected override AutomationPeer OnCreateAutomationPeer()
		=> new NumericUpDownAutomationPeer(this);

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);

		if (e.Property == ValueProperty || e.Property == FormatStringProperty || e.Property == DecimalPlacesProperty)
			UpdateTextBoxText();
		else if (e.Property == TextAlignmentProperty)
			UpdateTextAlignment();

		if (e.Property == ValueProperty || e.Property == MinimumProperty || e.Property == MaximumProperty || e.Property == LoopValuesProperty || e.Property == IsEnabledProperty)
			UpdateButtonStates();
	}

	protected override void OnPreviewKeyDown(KeyEventArgs e)
	{
		base.OnPreviewKeyDown(e);

		if (e.Handled)
			return;

		switch (e.Key)
		{
			case Key.Enter:
				CommitText();
				e.Handled = true;
				break;

			case Key.Escape:
				UpdateTextBoxText();
				e.Handled = true;
				break;

			case Key.Up:
				CommitText();
				ChangeValue(GetKeyboardIncrement());
				e.Handled = true;
				break;

			case Key.Down:
				CommitText();
				ChangeValue(-GetKeyboardIncrement());
				e.Handled = true;
				break;

			case Key.PageUp:
				CommitText();
				ChangeValue(GetLargeIncrement());
				e.Handled = true;
				break;

			case Key.PageDown:
				CommitText();
				ChangeValue(-GetLargeIncrement());
				e.Handled = true;
				break;

			case Key.Home:
				CommitText();
				Value = Minimum;
				e.Handled = true;
				break;

			case Key.End:
				CommitText();
				Value = Maximum;
				e.Handled = true;
				break;
		}
	}

	protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
	{
		base.OnPreviewMouseWheel(e);

		if (e.Handled || !ChangeValueOnMouseWheel || (RequireFocusForMouseWheel && !IsKeyboardFocusWithin))
			return;

		CommitText();

		if (e.Delta > 0)
			ChangeValue(GetKeyboardIncrement());
		else if (e.Delta < 0)
			ChangeValue(-GetKeyboardIncrement());
		else
			return;

		e.Handled = true;
	}

	private void ClearTemplatePartHandlers()
	{
		if (TextBox is not null)
		{
			TextBox.PreviewTextInput -= TextBox_PreviewTextInput;
			TextBox.KeyDown -= TextBox_KeyDown;
			TextBox.LostFocus -= TextBox_LostFocus;

			DataObject.RemovePastingHandler(TextBox, TextBox_Pasting);
		}

		if (IncreaseButton is not null)
			IncreaseButton.Click -= IncreaseButton_Click;

		if (DecreaseButton is not null)
			DecreaseButton.Click -= DecreaseButton_Click;
	}

	private void ApplyTextBoxProperties()
	{
		UpdateTextAlignment();
		UpdateTextBoxText();
	}

	private void IncreaseButton_Click(object sender, RoutedEventArgs e)
	{
		CommitText();
		ChangeValue(GetKeyboardIncrement());
	}

	private void DecreaseButton_Click(object sender, RoutedEventArgs e)
	{
		CommitText();
		ChangeValue(-GetKeyboardIncrement());
	}

	private void TextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Enter)
			return;

		CommitText();
		e.Handled = true;
	}

	private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		if (sender is not TextBox textBox || !IsTextAllowed(GetProposedText(textBox, e.Text), allowIntermediate: true))
			e.Handled = true;
	}

	private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
	{
		if (sender is not TextBox textBox || !e.DataObject.GetDataPresent(DataFormats.Text))
		{
			e.CancelCommand();
			return;
		}

		if (e.DataObject.GetData(DataFormats.Text) is not string pastedText
			|| !IsTextAllowed(GetProposedText(textBox, pastedText), allowIntermediate: false))
		{
			e.CancelCommand();
		}

	}

	private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		=> CommitText();

	private bool CommitText()
	{
		if (TextBox is null)
			return false;

		if (TryParseText(TextBox.Text, out var value))
		{
			Value = NormalizeValue(value);
			UpdateTextBoxText();
			return true;
		}

		UpdateTextBoxText();
		return false;
	}

	private void ChangeValue(double delta)
		=> Value = GetNextValue(delta);

	private double GetNextValue(double delta)
	{
		var nextValue = RoundValue(Value + delta);

		if (!LoopValues || Maximum <= Minimum)
			return ClampValue(nextValue);

		if (nextValue > Maximum)
			return Minimum;

		if (nextValue < Minimum)
			return Maximum;

		return nextValue;
	}

	private double GetKeyboardIncrement()
		=> Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? GetLargeIncrement() : Increment;

	private double GetLargeIncrement()
		=> LargeIncrement > 0.0 ? LargeIncrement : Increment * 10.0;

	private double NormalizeValue(double value)
		=> ClampValue(RoundValue(value));

	private double ClampValue(double value)
		=> Math.Min(Math.Max(value, Minimum), Maximum);

	private double RoundValue(double value)
	{
		if (DecimalPlaces < 0)
			return value;

		return Math.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero);
	}

	private void UpdateTextBoxText()
	{
		if (TextBox is not null)
			TextBox.Text = FormatValue(Value);
	}

	private void UpdateTextAlignment()
	{
		if (TextBox is not null)
			TextBox.TextAlignment = TextAlignment;
	}

	private void UpdateButtonStates()
	{
		var canIncrease = IsEnabled && (LoopValues || Value < Maximum);
		var canDecrease = IsEnabled && (LoopValues || Value > Minimum);

		if (IncreaseButton is not null)
			IncreaseButton.IsEnabled = canIncrease;

		if (DecreaseButton is not null)
			DecreaseButton.IsEnabled = canDecrease;
	}

	private string FormatValue(double value)
	{
		// DecimalPlaces controls the value's rounding; without this it would have no effect on
		// the DISPLAY when FormatString is left at its "F0" default (0.2 rendered as "0").
		string format = DecimalPlaces >= 0 && ReadLocalValue(FormatStringProperty) == DependencyProperty.UnsetValue
			? "F" + DecimalPlaces.ToString(CultureInfo.InvariantCulture)
			: FormatString;

		try
		{
			return value.ToString(format, CultureInfo.CurrentCulture);
		}
		catch (FormatException)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}
	}

	private bool IsTextAllowed(string text, bool allowIntermediate)
	{
		if (TryParseText(text, out _))
			return true;

		return allowIntermediate && IsIntermediateText(text);
	}

	private static bool TryParseText(string text, out double value)
	{
		if (!double.TryParse(text, NumericTextStyle, CultureInfo.CurrentCulture, out value))
			return false;

		return IsValidNumber(value);
	}

	private bool IsIntermediateText(string text)
	{
		var trimmedText = text.Trim();
		var numberFormat = CultureInfo.CurrentCulture.NumberFormat;
		var negativeDecimalPrefix = numberFormat.NegativeSign + numberFormat.NumberDecimalSeparator;
		var positiveDecimalPrefix = numberFormat.PositiveSign + numberFormat.NumberDecimalSeparator;

		return trimmedText.Length == 0 ||
			trimmedText == numberFormat.NumberDecimalSeparator ||
			trimmedText == numberFormat.PositiveSign ||
			trimmedText == positiveDecimalPrefix ||
			(Minimum < 0.0 && (trimmedText == numberFormat.NegativeSign || trimmedText == negativeDecimalPrefix));
	}

	private static string GetProposedText(TextBox textBox, string input)
	{
		var text = textBox.Text;
		return text.Remove(textBox.SelectionStart, textBox.SelectionLength).Insert(textBox.SelectionStart, input);
	}

	private static object CoerceValue(DependencyObject dependency, object baseValue)
	{
		var control = (NumericUpDown)dependency;
		return control.NormalizeValue((double)baseValue);
	}

	private static object CoerceMaximum(DependencyObject dependency, object baseValue)
	{
		var control = (NumericUpDown)dependency;
		return Math.Max((double)baseValue, control.Minimum);
	}

	private static void OnMinimumChanged(DependencyObject dependency, DependencyPropertyChangedEventArgs e)
	{
		var control = (NumericUpDown)dependency;
		control.CoerceValue(MaximumProperty);
		control.CoerceValue(ValueProperty);
	}

	private static void OnMaximumChanged(DependencyObject dependency, DependencyPropertyChangedEventArgs e)
	{
		var control = (NumericUpDown)dependency;
		control.CoerceValue(ValueProperty);
	}

	private static void OnDecimalPlacesChanged(DependencyObject dependency, DependencyPropertyChangedEventArgs e)
	{
		var control = (NumericUpDown)dependency;
		control.CoerceValue(ValueProperty);
	}

	private static void OnValueChanged(DependencyObject dependency, DependencyPropertyChangedEventArgs e)
	{
		var control = (NumericUpDown)dependency;
		control.ValueChanged?.Invoke(control, EventArgs.Empty);

		if (UIElementAutomationPeer.FromElement(control) is NumericUpDownAutomationPeer peer)
			peer.RaiseValueChanged((double)e.OldValue, (double)e.NewValue);
	}

	private static bool IsValidNumber(object value)
		=> value is double number && IsValidNumber(number);

	private static bool IsValidNumber(double value)
		=> !double.IsNaN(value) && !double.IsInfinity(value);

	private static bool IsValidPositiveNumber(object value)
		=> value is double number && IsValidNumber(number) && number > 0.0;

	private static bool IsValidLargeIncrement(object value)
		=> value is double number && IsValidNumber(number) && number >= 0.0;

	private static bool IsValidDecimalPlaces(object value)
		=> value is int decimalPlaces && decimalPlaces >= -1 && decimalPlaces <= 15;

	private static bool IsValidNonNegativeInteger(object value)
		=> value is int number && number >= 0;

	private static bool IsValidPositiveInteger(object value)
		=> value is int number && number > 0;

	private sealed class NumericUpDownAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
	{
		public NumericUpDownAutomationPeer(NumericUpDown owner)
			: base(owner)
		{ }

		private NumericUpDown OwnerControl => (NumericUpDown)Owner;

		public bool IsReadOnly => !OwnerControl.IsEnabled;

		public double LargeChange => OwnerControl.GetLargeIncrement();

		public double Maximum => OwnerControl.Maximum;

		public double Minimum => OwnerControl.Minimum;

		public double SmallChange => OwnerControl.Increment;

		public double Value => OwnerControl.Value;

		public override object? GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.RangeValue)
				return this;

			return base.GetPattern(patternInterface);
		}

		public void RaiseValueChanged(double oldValue, double newValue)
			=> RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, oldValue, newValue);

		public void SetValue(double value)
		{
			if (!OwnerControl.IsEnabled)
				throw new ElementNotEnabledException();

			if (value < OwnerControl.Minimum || value > OwnerControl.Maximum)
				throw new ArgumentOutOfRangeException(nameof(value));

			OwnerControl.Value = value;
		}

		protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Spinner;

		protected override string GetClassNameCore() => nameof(NumericUpDown);
	}
}
