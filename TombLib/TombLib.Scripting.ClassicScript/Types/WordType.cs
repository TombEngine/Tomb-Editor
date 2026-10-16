namespace TombLib.Scripting.ClassicScript.Types;

/// <summary>
/// Identifies the kind of a word in a ClassicScript document.
/// </summary>
public enum WordType
{
	/// <summary>
	/// An unrecognized word.
	/// </summary>
	Unknown,

	/// <summary>
	/// A section header word.
	/// </summary>
	Header,

	/// <summary>
	/// A command word.
	/// </summary>
	Command,

	/// <summary>
	/// A directive word.
	/// </summary>
	Directive,

	/// <summary>
	/// A mnemonic constant word.
	/// </summary>
	MnemonicConstant,

	/// <summary>
	/// A hexadecimal number word.
	/// </summary>
	Hexadecimal,

	/// <summary>
	/// A decimal number word.
	/// </summary>
	Decimal
}
