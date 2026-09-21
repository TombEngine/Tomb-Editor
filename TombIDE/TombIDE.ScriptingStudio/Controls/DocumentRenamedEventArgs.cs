using System;

namespace TombIDE.ScriptingStudio.Controls;

public class DocumentRenamedEventArgs : EventArgs
{
	public string OldFilePath { get; }
	public string NewFilePath { get; }

	public DocumentRenamedEventArgs(string oldFilePath, string newFilePath)
	{
		OldFilePath = oldFilePath;
		NewFilePath = newFilePath;
	}
}
