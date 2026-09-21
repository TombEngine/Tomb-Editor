using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.FileExplorer;

namespace TombIDE.ScriptingStudio;

public delegate void FileOpenedEventHandler(object sender, FileOpenedEventArgs e);

public delegate void ObjectClickedEventHandler(object sender, ObjectClickedEventArgs e);

public delegate void ReferenceDefinitionRequestedEventHandler(object sender, ReferenceDefinitionEventArgs e);
