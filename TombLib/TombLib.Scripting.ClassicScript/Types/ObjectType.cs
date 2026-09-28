namespace TombLib.Scripting.ClassicScript.Types;

/// <summary>
/// Identifies the kind of a ClassicScript object.
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
	/// An include directive.
	/// </summary>
	Include,

	/// <summary>
	/// A define directive.
	/// </summary>
	Define
}
