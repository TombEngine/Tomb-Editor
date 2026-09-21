using System.Collections.Generic;

namespace TombLib.Scripting.ClassicScript.Mnemonics.Models;

/// <summary>
/// Describes the loaded mnemonic definitions.
/// </summary>
/// <param name="StandardConstants">The standard mnemonic constants.</param>
/// <param name="PluginConstants">The plugin mnemonic definitions.</param>
public sealed record MnemonicDefinitions(
	IReadOnlyList<MnemonicConstantDefinition> StandardConstants,
	IReadOnlyList<PluginMnemonicDefinition> PluginConstants);
