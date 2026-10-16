#nullable enable

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

public readonly record struct DataGridUndoItem(
	string SectionName,
	int RowIndex,
	int ColumnIndex,
	object? OldValue
);
