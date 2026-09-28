#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

public sealed partial class StringEditorViewModel : ObservableObject
{
	private const int MaxUndoStackSize = 256;

	public ObservableCollection<StringTableSection> Sections { get; } = [];

	[ObservableProperty]
	private int _selectedSectionIndex;

	[ObservableProperty]
	private int _zoomLevel = 100;

	[ObservableProperty]
	private string _fontFamily = StringEditorDefaults.FontFamily;

	[ObservableProperty]
	private int _fontSize = StringEditorDefaults.FontSize;

	private Stack<DataGridUndoItem> _undoStack = new(MaxUndoStackSize);
	private Stack<DataGridUndoItem> _redoStack = new(MaxUndoStackSize);

	public bool CanUndo => _undoStack.Count > 0;
	public bool CanRedo => _redoStack.Count > 0;

	public StringTableMode CurrentMode
		=> SelectedSectionIndex >= 0 && SelectedSectionIndex < Sections.Count
			? Sections[SelectedSectionIndex].Mode
			: StringTableMode.Normal;

	public StringTableSection? SelectedSection
		=> SelectedSectionIndex >= 0 && SelectedSectionIndex < Sections.Count
			? Sections[SelectedSectionIndex]
			: null;

	public void PushUndo(DataGridUndoItem item)
	{
		_undoStack.Push(item);

		while (_undoStack.Count > MaxUndoStackSize)
			_undoStack = new Stack<DataGridUndoItem>(_undoStack.Take(MaxUndoStackSize).Reverse());

		_redoStack.Clear();

		OnPropertyChanged(nameof(CanUndo));
		OnPropertyChanged(nameof(CanRedo));
	}

	public void Undo()
	{
		if (_undoStack.Count == 0)
			return;

		DataGridUndoItem item = _undoStack.Pop();
		StringTableSection? section = Sections.FirstOrDefault(s => s.SectionName == item.SectionName);

		if (section is null || item.RowIndex >= section.Rows.Count)
			return;

		StringTableRow row = section.Rows[item.RowIndex];
		object? currentValue = GetCellValue(row, item.ColumnIndex);

		_redoStack.Push(new DataGridUndoItem(item.SectionName, item.RowIndex, item.ColumnIndex, currentValue));

		SetCellValue(row, item.ColumnIndex, item.OldValue);

		OnPropertyChanged(nameof(CanUndo));
		OnPropertyChanged(nameof(CanRedo));
	}

	public void Redo()
	{
		if (_redoStack.Count == 0)
			return;

		DataGridUndoItem item = _redoStack.Pop();
		StringTableSection? section = Sections.FirstOrDefault(s => s.SectionName == item.SectionName);

		if (section is null || item.RowIndex >= section.Rows.Count)
			return;

		StringTableRow row = section.Rows[item.RowIndex];
		object? currentValue = GetCellValue(row, item.ColumnIndex);

		_undoStack.Push(new DataGridUndoItem(item.SectionName, item.RowIndex, item.ColumnIndex, currentValue));

		SetCellValue(row, item.ColumnIndex, item.OldValue);

		OnPropertyChanged(nameof(CanUndo));
		OnPropertyChanged(nameof(CanRedo));
	}

	public void ClearUndoRedo()
	{
		_undoStack.Clear();
		_redoStack.Clear();

		OnPropertyChanged(nameof(CanUndo));
		OnPropertyChanged(nameof(CanRedo));
	}

	private static object? GetCellValue(StringTableRow row, int columnIndex) => columnIndex switch
	{
		0 => row.Id,
		1 => row.HexValue,
		2 => row.StringValue,
		_ => null
	};

	private static void SetCellValue(StringTableRow row, int columnIndex, object? value)
	{
		switch (columnIndex)
		{
			case 0 when value is int id:
				row.Id = id;
				break;

			case 1 when value is string hex:
				row.HexValue = hex;
				break;

			case 2:
				row.StringValue = value?.ToString() ?? string.Empty;
				break;
		}
	}
}
