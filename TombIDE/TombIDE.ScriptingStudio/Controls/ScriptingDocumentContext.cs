#nullable enable

using System;
using TombIDE.ScriptingStudio.Editors;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Controls;

/// <summary>
/// Identifies the document currently owned by the scripting workspace.
/// </summary>
public sealed class ScriptingDocumentContext
{
	public ScriptingDocumentContext(
		long generation,
		IEditorControl? editor,
		string? filePath,
		ScriptingDocumentRegistration? registration)
	{
		Generation = generation;
		Editor = editor;
		FilePath = filePath;
		Registration = registration;
	}

	public long Generation { get; }

	public IEditorControl? Editor { get; }

	public string? FilePath { get; }

	public ScriptingDocumentRegistration? Registration { get; }

	public bool HasDocument => Editor is not null && Registration is not null;

	public static ScriptingDocumentContext Empty { get; } = new(0, null, null, null);
}

public sealed class ScriptingDocumentContextChangedEventArgs : EventArgs
{
	public ScriptingDocumentContextChangedEventArgs(ScriptingDocumentContext context)
	{
		Context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public ScriptingDocumentContext Context { get; }
}