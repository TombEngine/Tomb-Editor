#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

namespace TombEditor.WPF;

public sealed class DynamicKeyBinding : InputBinding
{
	public static readonly DependencyProperty InputGestureTextProperty = DependencyProperty.Register(nameof(InputGestureText), typeof(string), typeof(DynamicKeyBinding));

	public string? InputGestureText
	{
		get => (string)GetValue(InputGestureTextProperty);
		set => SetValue(InputGestureTextProperty, value);
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == InputGestureTextProperty && e.NewValue is string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				Gesture = new KeyGestureEx();
				return;
			}

			var hotkey = Hotkey.FromString(text);

			ModifierKeys modifiers = ModifierKeys.None;
			Key key = Key.NoName;

			if ((hotkey.Keys & WinForms.Keys.Control) != WinForms.Keys.None)
				modifiers |= ModifierKeys.Control;

			if ((hotkey.Keys & WinForms.Keys.Shift) != WinForms.Keys.None)
				modifiers |= ModifierKeys.Shift;

			if ((hotkey.Keys & WinForms.Keys.Alt) != WinForms.Keys.None)
				modifiers |= ModifierKeys.Alt;

			if (hotkey.MainKey != WinForms.Keys.None)
				key = KeyInterop.KeyFromVirtualKey((int)hotkey.MainKey);

			Gesture = new KeyGestureEx(key, modifiers);
		}

		base.OnPropertyChanged(e);
	}
}

public class KeyGestureEx : InputGesture
{
	public Key Key { get; set; } = Key.NoName;
	public ModifierKeys Modifiers { get; set; } = ModifierKeys.None;

	public KeyGestureEx()
	{ }

	public KeyGestureEx(Key key, ModifierKeys modifiers)
	{
		Key = key;
		Modifiers = modifiers;
	}

	public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
	{
		if (inputEventArgs is not KeyEventArgs keyEventArgs)
			return false;

		if (inputEventArgs.OriginalSource is TextBoxBase or ListBoxItem)
			return false;

		return keyEventArgs.Key == Key && keyEventArgs.KeyboardDevice.Modifiers == Modifiers;
	}
}
