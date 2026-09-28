#nullable enable

using System;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Controls;

public sealed class EditorControlEventArgs : EventArgs
{
	public EditorControlEventArgs(IEditorControl editor)
	{
		Editor = editor ?? throw new ArgumentNullException(nameof(editor));
	}

	public IEditorControl Editor { get; }
}
