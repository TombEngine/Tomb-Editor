namespace TombLib.Scripting.ClassicScript.Mnemonics.Models;

/// <summary>
/// Describes a plugin mnemonic definition.
/// </summary>
/// <param name="FlagName">The flag name of the mnemonic.</param>
/// <param name="Description">The description of the mnemonic.</param>
/// <param name="DecimalValue">The decimal value of the mnemonic.</param>
/// <param name="HexValue">The hexadecimal value of the mnemonic.</param>
public sealed record PluginMnemonicDefinition(string FlagName, string Description, short DecimalValue, string HexValue);
