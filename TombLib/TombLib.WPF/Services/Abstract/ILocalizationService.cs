using System.ComponentModel;

namespace TombLib.WPF.Services.Abstract;

/// <summary>
/// Provides localization services with support for namespace and component-based key organization.
/// </summary>
public interface ILocalizationService
{
	/// <summary>
	/// Gets the current namespace name used as a prefix for localization keys.
	/// <para>This value is prepended to all localization keys when resolving translations.</para>
	/// </summary>
	/// <value>The namespace name, or <see langword="null" /> if no namespace is set.</value>
	string? NamespaceName { get; }

	/// <summary>
	/// Gets the current component name used as a prefix for localization keys.
	/// <para>This value is added between the namespace and the actual key when resolving translations.</para>
	/// </summary>
	/// <value>The component name, or <see langword="null" /> if no component is set.</value>
	string? ComponentName { get; }

	/// <summary>
	/// Configures the localization service to use namespace and component context derived from the specified view model.
	/// <para>
	/// The namespace is extracted from the first segment of the view model's namespace,
	/// and the component name is derived from the view model's type name with "ViewModel" suffix removed.
	/// </para>
	/// </summary>
	/// <param name="viewModel">The view model instance used to determine the localization context.</param>
	/// <returns>The current <see cref="ILocalizationService"/> instance with updated namespace and component context.</returns>
	/// <remarks>
	/// <para>This method extracts localization context as follows:</para>
	/// <list type="bullet">
	/// <item><description><c>NamespaceName</c>: Set to the first segment of <paramref name="viewModel"/>'s namespace (e.g., "TombEditor" from "TombEditor.ViewModels")</description></item>
	/// <item><description><c>ComponentName</c>: Set to the view model's type name with "ViewModel" suffix removed (e.g., "MainWindow" from "MainWindowViewModel")</description></item>
	/// </list>
	/// <para>These values are then used to automatically prefix localization keys when using the indexer.</para>
	/// </remarks>
	ILocalizationService WithKeysFor(INotifyPropertyChanged viewModel);

	/// <summary>
	/// Gets the localized string for the specified key using the current namespace and component context.
	/// <para>
	/// The key is automatically prefixed with the component name and namespace (if set) before lookup,
	/// unless the key starts with a <c>~</c> prefix which treats it as an absolute key path.
	/// </para>
	/// </summary>
	/// <param name="key">The localization key to look up. If the key starts with <c>~</c>, it is treated as an absolute key path and namespace/component prefixes are not applied.</param>
	/// <returns>The localized string value for the specified key.</returns>
	/// <remarks>
	/// <para>The final key used for lookup is constructed as follows:</para>
	/// <list type="bullet">
	/// <item><description>If the key starts with <c>~</c>: The <c>~</c> prefix is removed and the key is used as-is without any namespace or component prefixes</description></item>
	/// <item><description>If both <c>NamespaceName</c> and <c>ComponentName</c> are set: <c>"{NamespaceName}.{ComponentName}.{key}"</c></description></item>
	/// <item><description>If only <c>NamespaceName</c> is set: <c>"{NamespaceName}.{key}"</c></description></item>
	/// <item><description>If only <c>ComponentName</c> is set: <c>"{ComponentName}.{key}"</c></description></item>
	/// <item><description>If neither is set: <c>"{key}"</c></description></item>
	/// </list>
	/// <para>The lookup is performed using the underlying <see cref="Localizer.Instance" />.</para>
	/// </remarks>
	string this[string key] { get; }

	/// <summary>
	/// Gets the localized string for the specified key and formats it with the provided arguments.
	/// <para>
	/// The key is automatically prefixed with the component name and namespace (if set) before lookup,
	/// unless the key starts with a <c>~</c> prefix which treats it as an absolute key path.
	/// The resulting localized string is formatted using <see cref="string.Format(string, object[])"/>.
	/// </para>
	/// </summary>
	/// <param name="key">The localization key to look up. If the key starts with <c>~</c>, it is treated as an absolute key path and namespace/component prefixes are not applied.</param>
	/// <param name="args">An array of objects to format the localized string with.</param>
	/// <returns>The formatted localized string value for the specified key.</returns>
	/// <remarks>
	/// <para>This method is equivalent to calling <c>string.Format(this[key], args)</c>.</para>
	/// <para>The localized string should contain format specifiers (e.g., <c>{0}</c>, <c>{1}</c>) that correspond to the provided arguments.</para>
	/// <para>The key resolution follows the same rules as the indexer property. See <see cref="this[string]"/> for details on how the final key is constructed.</para>
	/// </remarks>
	string Format(string key, params object[] args);
}
