namespace TombLib.Scripting.ClassicScript.Navigation;

/// <summary>
/// Identifies the kind of a ClassicScript reference.
/// </summary>
public enum ReferenceType
{
	/// <summary>
	/// A mnemonic constant reference.
	/// </summary>
	MnemonicConstant,

	/// <summary>
	/// An old command reference.
	/// </summary>
	OldCommand,

	/// <summary>
	/// A new command reference.
	/// </summary>
	NewCommand,

	/// <summary>
	/// An OCB reference.
	/// </summary>
	OCB
}
