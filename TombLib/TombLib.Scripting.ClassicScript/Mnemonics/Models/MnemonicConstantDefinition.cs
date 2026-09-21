namespace TombLib.Scripting.ClassicScript.Mnemonics.Models;

/// <summary>
/// Describes a standard mnemonic constant definition.
/// </summary>
/// <param name="DecimalValue">The decimal value of the constant.</param>
/// <param name="HexValue">The hexadecimal value of the constant.</param>
/// <param name="FlagName">The flag name of the constant.</param>
public sealed record MnemonicConstantDefinition(string DecimalValue, string HexValue, string FlagName);
