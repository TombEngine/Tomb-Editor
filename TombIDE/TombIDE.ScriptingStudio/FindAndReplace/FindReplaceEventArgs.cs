using System;
using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.FindAndReplace;

public class FindReplaceEventArgs : EventArgs
{
	public IReadOnlyList<FindReplaceSource> SourceCollection { get; }

	public FindReplaceEventArgs(IReadOnlyList<FindReplaceSource> collection) => SourceCollection = collection;
}
