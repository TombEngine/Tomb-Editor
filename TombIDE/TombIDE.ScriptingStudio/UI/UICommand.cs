namespace TombIDE.ScriptingStudio.UI;

public enum UICommand
{
	None,

	// File:

	NewFile,
	Save,
	SaveAs,
	SaveAll,
	Build,
	Exit,

	// Edit:

	Undo,
	Redo,
	Cut,
	Copy,
	Paste,
	Find,
	SelectAll,

	// Document:

	TabsToSpaces,
	SpacesToTabs,

	Reindent,
	TrimWhiteSpace,
	ToggleComment,
	CommentOut,
	Uncomment,
	ToggleBookmark,
	PrevBookmark,
	NextBookmark,
	ClearBookmarks,

	PrevSection,
	NextSection,
	ClearString,
	RemoveLastString,

	// Options:

	UseNewInclude,
	ShowLogsAfterBuild,
	ReindentOnSave,
	Settings,

	// View:

	RestoreDefaultLayout,

	ToolStrip, // View start

	ContentExplorer,
	FileExplorer,
	ReferenceBrowser,
	CompilerLogs,
	SearchResults,
	LuaDiagnostics,
	LuaReferencesResults,

	StatusStrip, // View end

	// Help:

	ScriptingDocumentation,
	Tomb3ExtraCommands,
	LuaBasics,
	About,

	// Other:

	NavigateBack,
	NavigateForward,
	GoToDefinition,
	FindReferences,
	RenameSymbol,
	TypeFirstAvailableId,
	NewFileAtCaret
}
