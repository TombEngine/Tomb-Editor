#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;
using WpfLocalizer = TombLib.WPF.Localizer;

namespace TombEditor;

/// <summary>
/// Provides a localized tooltip and appends the current UI hotkeys for a command.
/// </summary>
[MarkupExtensionReturnType(typeof(BindingExpressionBase))]
public class CommandToolTipExtension : MarkupExtension, IMultiValueConverter
{
	/// <summary>
	/// Gets or sets the command name used to resolve the hotkey set.
	/// </summary>
	public string CommandName { get; set; }

	/// <summary>
	/// Gets or sets the localization key for the tooltip text.
	/// Defaults to the command name when not specified.
	/// </summary>
	public string? Key { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandToolTipExtension" /> class.
	/// </summary>
	/// <param name="commandName">The command name used for hotkey lookup.</param>
	public CommandToolTipExtension(string commandName)
		=> CommandName = commandName;

	/// <summary>
	/// Provides the binding value for the tooltip.
	/// </summary>
	/// <param name="serviceProvider">The markup extension service provider.</param>
	/// <returns>A <see cref="MultiBinding" /> that resolves to the localized tooltip with hotkeys.</returns>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		string resolvedKey = ResolveKey(serviceProvider, Key ?? CommandName);
		var binding = new MultiBinding { Converter = this };

		binding.Bindings.Add(new Binding($"[{resolvedKey}]")
		{
			Mode = BindingMode.OneWay,
			Source = WpfLocalizer.Instance
		});

		binding.Bindings.Add(new Binding($"[{CommandName}]")
		{
			Mode = BindingMode.OneWay,
			Source = KeyBindingsWrapper.Instance
		});

		return binding.ProvideValue(serviceProvider);
	}

	/// <summary>
	/// Converts the localized tooltip text and hotkeys into a single display string.
	/// </summary>
	/// <param name="values">The localized tooltip text and hotkey set.</param>
	/// <param name="targetType">The target type.</param>
	/// <param name="parameter">The converter parameter.</param>
	/// <param name="culture">The active culture.</param>
	/// <returns>The tooltip text with the hotkey suffix when available.</returns>
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		string text = values.Length > 0 && values[0] is string value ? value : string.Empty;

		if (values.Length < 2 || values[1] is not SortedSet<Hotkey> hotkeys || hotkeys.Count == 0)
			return text;

		return $"{text} ({string.Join(", ", hotkeys)})";
	}

	/// <summary>
	/// Converts the tooltip value back to the source bindings.
	/// </summary>
	/// <param name="value">The target value.</param>
	/// <param name="targetTypes">The source target types.</param>
	/// <param name="parameter">The converter parameter.</param>
	/// <param name="culture">The active culture.</param>
	/// <returns>This method always throws.</returns>
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		=> throw new NotSupportedException("CommandToolTipExtension only supports one-way binding.");

	private static string ResolveKey(IServiceProvider serviceProvider, string key)
	{
		bool isAbsoluteKey = key.StartsWith('~');

		if (isAbsoluteKey)
			return key.TrimStart('~');

		if (serviceProvider.GetService(typeof(IRootObjectProvider)) is not IRootObjectProvider rootProvider)
			return key;

		Type targetType = rootProvider.RootObject.GetType();
		return BuildFullKey(targetType, key);
	}

	private static string BuildFullKey(Type targetType, string key)
	{
		string componentName = targetType.Name;
		string keyWithComponent = $"{componentName}.{key}";

		string? namespaceName = targetType.Namespace?.Split('.')[0];

		return string.IsNullOrEmpty(namespaceName)
			? keyWithComponent
			: $"{namespaceName}.{keyWithComponent}";
	}
}
