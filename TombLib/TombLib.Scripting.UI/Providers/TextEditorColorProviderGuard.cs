using System;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Providers;

/// <summary>
/// Guards color-provider configuration access against mismatched configuration types.
/// </summary>
public static class TextEditorColorProviderGuard
{
	/// <summary>
	/// Returns the supplied configuration cast to the expected provider-specific type, throwing
	/// an <see cref="ArgumentException"/> when the configuration type does not match.
	/// </summary>
	/// <typeparam name="TConfig">The configuration type the provider operates on.</typeparam>
	/// <param name="config">The configuration received by the provider.</param>
	/// <param name="providerName">The name of the provider, used in the error message.</param>
	/// <returns>The configuration cast to <typeparamref name="TConfig"/>.</returns>
	/// <exception cref="ArgumentException">The configuration type does not match <typeparamref name="TConfig"/>.</exception>
	public static TConfig GetConfig<TConfig>(TextEditorConfigBase config, string providerName)
		where TConfig : TextEditorConfigBase
	{
		return config as TConfig
			?? throw new ArgumentException(
				$"The color provider '{providerName}' requires a configuration of type '{typeof(TConfig).Name}', but received '{config.GetType().Name}'.",
				nameof(config));
	}
}
