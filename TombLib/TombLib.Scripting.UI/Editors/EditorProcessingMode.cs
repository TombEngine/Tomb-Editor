namespace TombLib.Scripting.UI.Editors;

/// <summary>
/// Controls whether background editor processing (persistence scheduling, backup creation,
/// diagnostics scheduling, delayed notifications, and result publication) runs for an editor.
/// </summary>
public enum EditorProcessingMode
{
	/// <summary>
	/// Background editor processing runs normally.
	/// </summary>
	Normal,

	/// <summary>
	/// Background editor processing is suppressed for the duration of an operation scope.
	/// </summary>
	Suppressed
}

/// <summary>
/// Carries the requested initial <see cref="EditorProcessingMode"/> into an editor or view load.
/// </summary>
/// <param name="ProcessingMode">The processing mode applied for the load.</param>
public readonly record struct DocumentLoadOptions(
	EditorProcessingMode ProcessingMode = EditorProcessingMode.Normal);
