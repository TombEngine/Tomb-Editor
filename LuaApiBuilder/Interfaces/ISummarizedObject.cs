namespace LuaApiBuilder.Interfaces;

/// <summary>
/// Represents an object with a summary.
/// </summary>
public interface ISummarizedObject
{
	/// <summary>
	/// A brief summary of the object.
	/// </summary>
	string Summary { get; set; }
}
