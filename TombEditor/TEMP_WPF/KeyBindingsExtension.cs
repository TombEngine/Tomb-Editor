#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace TombEditor.WPF;

public class KeyBindingsExtension : MarkupExtension, IValueConverter
{
	public string Key { get; set; }

	public KeyBindingsExtension(string key)
		=> Key = key;

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		string keyToUse = Key;
		HotkeySets uiKeys = Editor.Instance.Configuration.UI_Hotkeys;

		if (uiKeys.Any(keyPair => keyPair.Key == keyToUse && keyPair.Value.Count > 0))
		{
			var binding = new Binding($"[{keyToUse}]")
			{
				Mode = BindingMode.OneWay,
				Source = KeyBindingsWrapper.Instance,
				Converter = this
			};

			return binding.ProvideValue(serviceProvider);
		}

		return string.Empty;
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> ((SortedSet<Hotkey>)value).First().ToString();

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}
