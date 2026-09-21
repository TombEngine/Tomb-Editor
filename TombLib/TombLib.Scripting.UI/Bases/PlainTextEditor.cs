namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// A plain text editor with no language-specific services.
/// </summary>
public sealed class PlainTextEditor : TextEditorBase
{
	/// <inheritdoc/>
	public override string DefaultFileExtension => ".txt";
}
