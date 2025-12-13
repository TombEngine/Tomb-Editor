namespace LuaApiBuilder.Interfaces;

/// <summary>
/// Represents an object with a type.
/// </summary>
public interface ITypedObject
{
	/// <summary>
	/// The type of the object.
	/// </summary>
	string Type { get; set; }
}
