namespace LuaApiBuilder.Interfaces;

/// <summary>
/// Represents an object that can be optional, with an optional default value.
/// </summary>
public interface IOptionalObject
{
	/// <summary>
	/// Indicates whether this object is optional.
	/// </summary>
	bool Optional { get; set; }

	/// <summary>
	/// The default value for this object, if it is optional.
	/// </summary>
	string DefaultValue { get; set; }
}
