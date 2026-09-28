namespace TombLib.Scripting.GameFlowScript.Types;

/// <summary>
/// Identifies the kind of a GameFlow object.
/// </summary>
public enum ObjectType
{
	/// <summary>
	/// A section definition.
	/// </summary>
	Section,

	/// <summary>
	/// A level definition.
	/// </summary>
	Level,

	/// <summary>
	/// A special property definition.
	/// </summary>
	SpecialProperty,

	/// <summary>
	/// A property definition.
	/// </summary>
	Property,

	/// <summary>
	/// A constant definition.
	/// </summary>
	Constant
}
