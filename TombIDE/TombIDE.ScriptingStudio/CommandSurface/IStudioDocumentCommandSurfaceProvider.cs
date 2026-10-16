using System.Collections.Generic;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.CommandSurface;

/// <summary>
/// Provides document-mode-specific command surface items such as context menus,
/// menu strips, and tool strips.
/// </summary>
public interface IStudioDocumentCommandSurfaceProvider
{
	/// <summary>
	/// Gets the context menu items for the registered document.
	/// </summary>
	IReadOnlyList<StudioToolStripItem> GetContextMenuItems(IEditorControl editor);

	/// <summary>
	/// Gets the menu strip items for the registered document.
	/// </summary>
	IReadOnlyList<StudioToolStripItem> GetMenuStripItems(IEditorControl editor);

	/// <summary>
	/// Gets the tool strip items for the registered document.
	/// </summary>
	IReadOnlyList<StudioToolStripItem> GetToolStripItems(IEditorControl editor);
}
