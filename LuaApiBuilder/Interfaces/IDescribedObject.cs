namespace LuaApiBuilder.Interfaces;

/// <summary>
/// Represents an object with a description.
/// </summary>
public interface IDescribedObject
{
	/// <summary>
	/// A detailed description of the object.
	/// </summary>
	string Description { get; set; }
}
