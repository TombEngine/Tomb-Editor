#nullable enable

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// Specifies how a stored search result was mapped to a navigation location.
/// </summary>
/// <remarks>
/// <see cref="LineNotFound"/> is the default value, so a default-initialized status represents a failed lookup.
/// </remarks>
public enum SearchResultLocationStatus
{
	/// <summary>
	/// The requested line is outside the document; no location was created.
	/// </summary>
	LineNotFound = 0,

	/// <summary>
	/// The line exists, but the stored match text was not located; the location is a zero-length
	/// selection at the start of the line.
	/// </summary>
	LineStartFallback = 1,

	/// <summary>
	/// The stored match text was located; the location selects it.
	/// </summary>
	MatchLocated = 2
}
