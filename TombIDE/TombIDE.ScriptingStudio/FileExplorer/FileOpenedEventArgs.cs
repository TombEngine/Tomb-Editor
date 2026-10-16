using System;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.FileExplorer;

public class FileOpenedEventArgs : EventArgs
{
	public string FilePath { get; }
	public EditorType EditorType { get; }
	public bool OpenSourceView { get; }

	public FileOpenedEventArgs(string filePath, EditorType editorType = EditorType.Default, bool openSourceView = false)
	{
		FilePath = filePath;
		EditorType = editorType;
		OpenSourceView = openSourceView;
	}

	public static FileOpenedEventArgs CreateSourceView(string filePath) => new(filePath, EditorType.Default, true);
}
