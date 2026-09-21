namespace TombLib.Scripting.UI.Editors;

/// <summary>
/// Identifies the type of an editor control.
/// </summary>
public enum EditorType
{
	/// <summary>
	/// Lets the host resolve the most suitable <c>EditorType</c> for the requested file or view.
	/// </summary>
	Default,

	/// <summary>
	/// A plain text editor.
	/// </summary>
	Text,

	/// <summary>
	/// A strings file editor.
	/// </summary>
	Strings
}
