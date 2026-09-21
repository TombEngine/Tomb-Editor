namespace TombLib.Scripting.TRX.Models;

/// <summary>
/// Describes the load state of the GameFlow schema resource.
/// </summary>
public enum TRXSchemaLoadState
{
	/// <summary>
	/// The schema resource was found and parsed successfully.
	/// </summary>
	Loaded,

	/// <summary>
	/// The schema resource could not be found or read.
	/// </summary>
	MissingResource,

	/// <summary>
	/// The schema resource was read but could not be parsed as a valid schema.
	/// </summary>
	InvalidSchema
}
