namespace LuaApiBuilder.Interfaces;

/// <summary>
/// Represents an object with a name.
/// </summary>
public interface INamedObject
{
	/// <summary>
	/// The name of the object.
	/// </summary>
	string Name { get; set; }
}
