using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace TombLib.WPF;

/// <summary>
/// A markup extension that provides localization support for WPF applications.
/// <para>Automatically resolves localization keys based on the containing class hierarchy.</para>
/// </summary>
[MarkupExtensionReturnType(typeof(BindingExpressionBase))]
public class LocalizeExtension : MarkupExtension, IMultiValueConverter
{
	/// <summary>
	/// Gets or sets the localization key. Can be a simple key or a full path (e.g., "ClassName.Key").
	/// </summary>
	public string Key { get; set; }

	// public bool FindHotkeys { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LocalizeExtension"/> class.
	/// </summary>
	/// <param name="key">The localization key.</param>
	public LocalizeExtension(string key)
		=> Key = key;

	/// <summary>
	/// Provides the binding value for the localization key.
	/// </summary>
	/// <param name="serviceProvider">The service provider from the markup extension context.</param>
	/// <returns>A <see cref="MultiBinding" /> that resolves to the localized string.</returns>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		string resolvedKey = ResolveKey(serviceProvider);
		var binding = CreateLocalizationBinding(resolvedKey);

		/* Disabled for now, as hotkeys in UI strings are not used anywhere at the moment.
		if (FindHotkeys)
		{
			HotkeySets uiKeys = Editor.Instance.Configuration.UI_Hotkeys;

			if (uiKeys.Any(keyPair => keyPair.Key == resolvedKey && keyPair.Value.Count > 0))
			{
				binding.Bindings.Add(new Binding($"[{resolvedKey}]")
				{
					Mode = BindingMode.OneWay,
					Source = uiKeys
				});
			}
		}
		*/

		return binding.ProvideValue(serviceProvider);
	}

	/// <summary>
	/// Resolves the localization key, automatically prepending namespace and class name if needed.
	/// <para>If the key starts with '~', it is treated as an absolute key and returned as-is (without the '~').</para>
	/// </summary>
	/// <param name="serviceProvider">The service provider from the markup extension context.</param>
	/// <returns>The resolved localization key.</returns>
	private string ResolveKey(IServiceProvider serviceProvider)
	{
		bool isAbsoluteKey = Key.StartsWith('~');

		if (isAbsoluteKey)
			return Key.TrimStart('~');

		// Try to get the root object to determine the target type
		if (serviceProvider.GetService(typeof(IRootObjectProvider)) is not IRootObjectProvider rootProvider)
			return Key; // Fallback to the original key if we can't resolve the context

		Type targetType = rootProvider.RootObject.GetType();
		return BuildFullKey(targetType);
	}

	/// <summary>
	/// Builds a full localization key from the target type and the provided key.
	/// </summary>
	/// <param name="targetType">The type to extract namespace and class name from.</param>
	/// <returns>The full localization key.</returns>
	private string BuildFullKey(Type targetType)
	{
		string componentName = targetType.Name;
		string keyWithComponent = $"{componentName}.{Key}";

		// Extract the first part of the namespace (e.g., "TombEditor" from "TombEditor.Views")
		string? namespaceName = targetType.Namespace?.Split('.')[0];

		return string.IsNullOrEmpty(namespaceName)
			? keyWithComponent
			: $"{namespaceName}.{keyWithComponent}";
	}

	/// <summary>
	/// Creates a <see cref="MultiBinding" /> for localization with the resolved key.
	/// </summary>
	/// <param name="resolvedKey">The resolved localization key.</param>
	/// <returns>A configured <see cref="MultiBinding" /> instance.</returns>
	private MultiBinding CreateLocalizationBinding(string resolvedKey)
	{
		var binding = new MultiBinding { Converter = this };

		binding.Bindings.Add(new Binding($"[{resolvedKey}]")
		{
			Mode = BindingMode.OneWay,
			Source = Localizer.Instance
		});

		return binding;
	}

	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		// => values.Length > 1 ? $"{values[0]} ({string.Join(", ", (SortedSet<Hotkey>)values[1])})" : values[0];
		=> values[0];

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		=> throw new NotSupportedException("LocalizeExtension only supports one-way binding.");
}
