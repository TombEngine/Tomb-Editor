using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace DarkUI.WPF.CustomControls;

[TemplatePart(Name = "PART_InputTextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_SuggestionBox", Type = typeof(ListBox))]
public class AutoCompleteBox : Control
{
	public static readonly DependencyProperty TextProperty;
	public static readonly DependencyProperty IsSuggestionVisibleProperty;
	public static readonly DependencyProperty ItemsSourceProperty;
	public static readonly DependencyProperty SelectItemProperty;
	public static readonly DependencyProperty CharacterCasingProperty;

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public bool IsSuggestionVisible
	{
		get => (bool)GetValue(IsSuggestionVisibleProperty);
		set => SetValue(IsSuggestionVisibleProperty, value);
	}

	public IEnumerable ItemsSource
	{
		get => (IEnumerable)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	public ICommand SelectItem
	{
		get => (ICommand)GetValue(SelectItemProperty);
		set => SetValue(SelectItemProperty, value);
	}

	public CharacterCasing CharacterCasing
	{
		get => (CharacterCasing)GetValue(CharacterCasingProperty);
		set => SetValue(CharacterCasingProperty, value);
	}

	public TextBox? InputTextBox { get; set; }
	public Popup? Popup { get; set; }
	public ListBox? SuggestionBox { get; set; }

	static AutoCompleteBox()
	{
		TextProperty = DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(AutoCompleteBox),
			new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		IsSuggestionVisibleProperty = DependencyProperty.Register(
			nameof(IsSuggestionVisible),
			typeof(bool),
			typeof(AutoCompleteBox),
			new PropertyMetadata(false));

		ItemsSourceProperty = DependencyProperty.Register(
			nameof(ItemsSource),
			typeof(IEnumerable),
			typeof(AutoCompleteBox),
			new PropertyMetadata(null));

		SelectItemProperty = DependencyProperty.Register(
			nameof(SelectItem),
			typeof(ICommand),
			typeof(AutoCompleteBox),
			new PropertyMetadata(null));

		CharacterCasingProperty = DependencyProperty.Register(
			nameof(CharacterCasing),
			typeof(CharacterCasing),
			typeof(AutoCompleteBox),
			new PropertyMetadata(CharacterCasing.Normal));
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		InputTextBox = GetTemplateChild("PART_InputTextBox") as TextBox;
		Popup = GetTemplateChild("PART_Popup") as Popup;
		SuggestionBox = GetTemplateChild("PART_SuggestionBox") as ListBox;

		if (InputTextBox is not null)
		{
			InputTextBox.TextChanged += UpdateSuggestion;
			InputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;
			InputTextBox.LostFocus += OnLostFocus;
		}

		if (Popup is not null)
			Popup.Closed += (sender, e) => IsSuggestionVisible = false;

		if (SuggestionBox is not null)
		{
			SuggestionBox.PreviewKeyDown += SuggestionBox_PreviewKeyDown;
			SuggestionBox.MouseDoubleClick += (sender, e) => SelectItem.Execute(SuggestionBox.SelectedItem);
			SuggestionBox.LostFocus += OnLostFocus;
		}

		SelectItem = new RelayCommand(obj =>
		{
			if (InputTextBox is null)
				return;

			string? text = obj?.ToString();

			if (string.IsNullOrEmpty(text))
				return;

			int lastSpaceIndex = InputTextBox.Text.LastIndexOf(' ');
			lastSpaceIndex = lastSpaceIndex == -1 ? 0 : lastSpaceIndex + 1;

			InputTextBox.Text = InputTextBox.Text[..lastSpaceIndex] + text;
			InputTextBox.CaretIndex = InputTextBox.Text.Length;
			InputTextBox.Focus();

			IsSuggestionVisible = false;
		});
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == TextProperty && InputTextBox is not null)
		{
			string newValue = (string)e.NewValue;

			if (!string.Equals(InputTextBox.Text, newValue, StringComparison.Ordinal))
				InputTextBox.Text = newValue;
		}

		base.OnPropertyChanged(e);
	}

	private void OnLostFocus(object sender, RoutedEventArgs e)
	{
		if (!IsSuggestionVisible && InputTextBox is not null)
			Text = InputTextBox.Text;
	}

	private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter && InputTextBox is not null)
		{
			// An open suggestion list with a highlighted match takes the Enter key and inserts the match.
			if (IsSuggestionVisible && SuggestionBox?.SelectedItem is { } highlighted)
			{
				SelectItem.Execute(highlighted);
				e.Handled = true;
				return;
			}

			// Otherwise Enter commits the typed text without waiting for the box to lose focus.
			Text = InputTextBox.Text;
		}

		if (e.Key == Key.Down &&
			IsSuggestionVisible &&
			InputTextBox?.IsFocused == true &&
			SuggestionBox?.ItemContainerGenerator.ContainerFromIndex(0) is ListBoxItem container)
		{
			Dispatcher.BeginInvoke(() => container.Focus(), DispatcherPriority.Normal);
			SuggestionBox.SelectedIndex = 0;
		}
	}

	private void SuggestionBox_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Up && SuggestionBox?.SelectedIndex == 0 && InputTextBox?.IsFocused == false)
		{
			InputTextBox.Focus();
			InputTextBox.CaretIndex = InputTextBox.Text.Length;

			SuggestionBox.SelectedIndex = -1;
		}
	}

	private void UpdateSuggestion(object sender, TextChangedEventArgs e)
	{
		if (SuggestionBox is null || InputTextBox is null)
			return;

		SuggestionBox.SelectedIndex = -1;

		var words = InputTextBox.Text.Split(' ');
		string lastItem = words.Last();

		if (string.IsNullOrWhiteSpace(lastItem))
		{
			IsSuggestionVisible = false;
			return;
		}

		// Like the previous autocomplete box: only words that start with what was typed, and
		// never a word that was already entered.
		var alreadyTyped = words.Take(words.Length - 1).Where(word => word.Length > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);

		SuggestionBox.Items.Filter = obj =>
		{
			string? text = obj?.ToString();

			return !string.IsNullOrEmpty(text)
				&& text.StartsWith(lastItem, StringComparison.OrdinalIgnoreCase)
				&& !alreadyTyped.Contains(text);
		};

		// The first match is highlighted so Enter accepts it.
		SuggestionBox.SelectedIndex = SuggestionBox.Items.Count > 0 ? 0 : -1;

		IsSuggestionVisible = InputTextBox.IsKeyboardFocusWithin && !string.IsNullOrWhiteSpace(InputTextBox.Text);
	}
}
