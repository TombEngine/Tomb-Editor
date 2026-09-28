using TombLib.Scripting.TRX.Models;

namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Loads the GameFlow level schema once and serves the derived immutable models used by
/// completion, hover and highlighting. The service is a catalog: it does not validate
/// documents and it exposes no raw schema representation.
/// </summary>
public interface ITRXGameFlowSchemaService
{
	/// <summary>
	/// Gets the result of loading the schema resource.
	/// </summary>
	TRXSchemaLoadState LoadState { get; }

	/// <summary>
	/// Gets the loaded schema model, or null when the schema could not be loaded.
	/// </summary>
	TRXGameFlowSchemaModel? Model { get; }

	/// <summary>
	/// Gets the schema-derived keyword categories, or <see cref="TRXSchemaKeywords.Empty"/> when the
	/// schema is unavailable.
	/// </summary>
	TRXSchemaKeywords Keywords { get; }
}
