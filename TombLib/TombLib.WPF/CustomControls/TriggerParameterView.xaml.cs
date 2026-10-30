#nullable enable

using NLog;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.Utils;

namespace TombLib.WPF.CustomControls;

/// <summary>
/// WPF replacement for <c>TombLib.Controls.TriggerParameterControl</c>.
/// Hosts a searchable combo, a numeric up/down and a fallback label, switching between
/// them based on the current <see cref="ParameterRange"/> and <see cref="RawMode"/>.
/// </summary>
public partial class TriggerParameterView : UserControl
{
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public event Action<ObjectInstance>? ViewObject;
	public event Action<Room>? ViewRoom;
	public event EventHandler? ParameterChanged;

	private bool _suppressEvents;

	public TriggerParameterView()
	{
		InitializeComponent();
		UpdateVisibleControls(true);
	}

	#region Dependency properties

	public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register(
		nameof(Parameter),
		typeof(ITriggerParameter),
		typeof(TriggerParameterView),
		new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnParameterChanged));

	public ITriggerParameter? Parameter
	{
		get => (ITriggerParameter?)GetValue(ParameterProperty);
		set => SetValue(ParameterProperty, value);
	}

	private static void OnParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var view = (TriggerParameterView)d;
		bool matched = view.ParameterRange.ParameterMatches(e.OldValue as ITriggerParameter, true);
		bool matches = view.ParameterRange.ParameterMatches(e.NewValue as ITriggerParameter, true);
		view.UpdateVisibleControls(matched != matches);
		view.ParameterChanged?.Invoke(view, EventArgs.Empty);
		view.butViewPart.Visibility = view.GetObjectPointer() is ObjectInstance or Room ? Visibility.Visible : Visibility.Collapsed;
	}

	public static readonly DependencyProperty ParameterRangeProperty = DependencyProperty.Register(
		nameof(ParameterRange),
		typeof(NgParameterRange),
		typeof(TriggerParameterView),
		new PropertyMetadata(default(NgParameterRange), (d, _) => ((TriggerParameterView)d).UpdateVisibleControls(true)));

	public NgParameterRange ParameterRange
	{
		get => (NgParameterRange)GetValue(ParameterRangeProperty);
		set => SetValue(ParameterRangeProperty, value);
	}

	public static readonly DependencyProperty RawModeProperty = DependencyProperty.Register(
		nameof(RawMode),
		typeof(bool),
		typeof(TriggerParameterView),
		new PropertyMetadata(false, (d, _) => ((TriggerParameterView)d).UpdateVisibleControls(true)));

	public bool RawMode
	{
		get => (bool)GetValue(RawModeProperty);
		set => SetValue(RawModeProperty, value);
	}

	public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(
		nameof(Level),
		typeof(Level),
		typeof(TriggerParameterView),
		new PropertyMetadata(null));

	public Level? Level
	{
		get => (Level?)GetValue(LevelProperty);
		set => SetValue(LevelProperty, value);
	}

	#endregion

	private ITriggerParameter? GetObjectPointer()
		=> Parameter is TriggerParameterUshort ushortParam
			? ushortParam.NameObject as ITriggerParameter
			: Parameter;

    private void OnSearchClick(object sender, RoutedEventArgs e) => comboPart.OpenAndFocusSearch();

	private void OnViewClick(object sender, RoutedEventArgs e)
	{
		var target = GetObjectPointer();
		switch (target)
		{
			case ObjectInstance obj: ViewObject?.Invoke(obj); break;
			case Room room:          ViewRoom?.Invoke(room); break;
		}
	}

	private void OnResetClick(object sender, RoutedEventArgs e)
		=> Parameter = null;

	private void OnComboSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_suppressEvents || comboPart.Visibility != Visibility.Visible)
			return;
		Parameter = comboPart.SelectedItem as ITriggerParameter;
	}

	private void OnNumericValueChanged(object? sender, EventArgs e)
	{
		if (_suppressEvents || numericPart.Visibility != Visibility.Visible)
			return;
		// Mirror legacy behaviour: clamp via signed↔unsigned roundtrip.
		var value = (int)numericPart.Value;
		Parameter = new TriggerParameterUshort(BitConverter.ToUInt16(BitConverter.GetBytes(value), 0));
	}

	private void OnLabelMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (numericPart.Visibility == Visibility.Visible && Parameter is null)
			Parameter = new TriggerParameterUshort(0);
	}

	private void UpdateVisibleControls(bool repopulate)
	{
		_suppressEvents = true;
		try
		{
			if (repopulate)
				comboPart.ItemsSource = null;

			bool typeMatches = ParameterRange.ParameterMatches(Parameter, true);
			butResetPart.Visibility = typeMatches ? Visibility.Collapsed : Visibility.Visible;
			if (!typeMatches)
			{
				labelPart.Text = "Wrong parameter: " + (Parameter?.ToString() ?? "<null>");
				comboPart.Visibility = Visibility.Collapsed;
				numericPart.Visibility = Visibility.Collapsed;
				colorPreviewPart.Visibility = Visibility.Collapsed;
				labelPart.Visibility = Visibility.Visible;
				return;
			}

			ITriggerParameter[]? listOfThings;
			try { listOfThings = ParameterRange.BuildList(Level)?.ToArray(); }
			catch (Exception exc)
			{
				_logger.Warn(exc, "Unable to create trigger parameter list.");
				listOfThings = null;
			}

			if (ParameterRange.IsEmpty)
			{
				labelPart.Text = "-";
				comboPart.Visibility = Visibility.Collapsed;
				numericPart.Visibility = Visibility.Collapsed;
				colorPreviewPart.Visibility = Visibility.Collapsed;
				labelPart.Visibility = Visibility.Visible;
			}
			else if (listOfThings is null || (RawMode && !ParameterRange.IsObject && !ParameterRange.IsRoom))
			{
				labelPart.Text = string.Empty;
				comboPart.Visibility = Visibility.Collapsed;
				numericPart.Visibility = Visibility.Visible;
				colorPreviewPart.Visibility = Visibility.Collapsed;
				if (Parameter is TriggerParameterUshort u)
					numericPart.Value = BitConverter.ToInt16(BitConverter.GetBytes(u.Key), 0);
				else
					Parameter = new TriggerParameterUshort(0);
				labelPart.Visibility = Parameter is null ? Visibility.Visible : Visibility.Collapsed;
			}
			else
			{
				if (repopulate)
				{
					if (ParameterRange.Kind is NgParameterKind.FixedEnumeration or NgParameterKind.PluginEnumeration)
					{
						string?[] cachedNames = listOfThings.Select(obj => obj?.ToString()).ToArray();
						bool shouldSort = true;

						if (ParameterRange.Kind is NgParameterKind.PluginEnumeration)
						{
							// Same heuristic as the legacy control: skip sorting when the
							// plugin labels look like an ordered numeric sequence.
							bool allInSampleEndWithDigit = cachedNames.SampleSatisfies(32, name => !string.IsNullOrEmpty(name) && char.IsDigit(name[^1]));
							shouldSort = !allInSampleEndWithDigit;
						}

						if (shouldSort)
							Array.Sort(cachedNames, listOfThings, StringComparer.Ordinal);
					}
					comboPart.ItemsSource = listOfThings;
				}

				var match = listOfThings.FirstOrDefault(item => item.Equals(Parameter));
				comboPart.SelectedItem = match;
				if (match is null)
				{
					if (listOfThings.Length > 0)
					{
						Parameter = listOfThings.First();
						return; // Re-enters via OnParameterChanged.
					}
					// Empty list → leave combo blank.
				}
				comboPart.Visibility = Visibility.Visible;
				numericPart.Visibility = Visibility.Collapsed;
				labelPart.Visibility = Visibility.Collapsed;
				SetupColorPreview(comboPart.SelectedItem?.ToString());
			}
		}
		finally { _suppressEvents = false; }
	}

	private void SetupColorPreview(string? text)
	{
		if (string.IsNullOrEmpty(text)
			|| !byte.TryParse(ParseFor(text, "Red="), NumberStyles.Integer, CultureInfo.InvariantCulture, out byte r)
			|| !byte.TryParse(ParseFor(text, "Green="), NumberStyles.Integer, CultureInfo.InvariantCulture, out byte g)
			|| !byte.TryParse(ParseFor(text, "Blue="), NumberStyles.Integer, CultureInfo.InvariantCulture, out byte b))
		{
			colorPreviewPart.Visibility = Visibility.Collapsed;
			return;
		}

		colorPreviewPart.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
		colorPreviewPart.Visibility = Visibility.Visible;
	}

	private static string? ParseFor(string text, string searched)
	{
		int startPos = text.IndexOf(searched, StringComparison.InvariantCultureIgnoreCase);
		if (startPos == -1)
			return null;
		startPos += searched.Length;

		int endPos = startPos;
		while (endPos < text.Length)
		{
			char c = text[endPos];
			if (char.IsWhiteSpace(c) || c == ')' || c == '(' || c == '=')
				break;
			endPos++;
		}
		return text.Substring(startPos, endPos - startPos);
	}
}
