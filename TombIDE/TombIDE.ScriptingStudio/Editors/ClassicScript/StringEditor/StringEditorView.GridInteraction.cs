#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TombLib.Scripting.ClassicScript.StringTables;
using TombLib.Scripting.UI.Documents;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

/// <summary>
/// Handles DataGrid interaction, cell editing, and row change tracking for the string-table editor.
/// </summary>
public partial class StringEditorView
{
	#region DataGrid event handlers

	private void DataGrid_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
	{
		if (e.Row.Item is StringTableRow row && e.Column is not null)
		{
			// Cache the current value for undo before edit begins.
			_cachedBeginEditValue = GetCellValue(row, e.Column.DisplayIndex);
		}
	}

	private void DataGrid_PreparingCellForEdit(object? sender, DataGridPreparingCellForEditEventArgs e)
	{
		// Let the editing TextBox accept newline characters so Shift+Enter
		// can insert them; the DataGrid PreviewKeyDown handler intercepts
		// Shift+Enter to prevent it from committing the edit.
		if (e.EditingElement is TextBox textBox)
			textBox.AcceptsReturn = true;
	}

	private void DataGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
	{
		if (e.Row.Item is StringTableRow row && e.Column is not null)
		{
			object? newValue = GetCellValue(row, e.Column.DisplayIndex);

			if (e.EditAction == DataGridEditAction.Commit && !Equals(_cachedBeginEditValue, newValue))
			{
				_viewModel.PushUndo(new DataGridUndoItem(
					_viewModel.SelectedSection?.SectionName ?? string.Empty,
					e.Row.GetIndex(),
					e.Column.DisplayIndex,
					_cachedBeginEditValue));

				LastModified = DateTime.Now;
			}

			_cachedBeginEditValue = null;
		}

		// Always run the dirty check when a cell edit ends, so the file is
		// marked as modified regardless of whether the cached begin-edit
		// value matches the final value.
		RunContentChangedWorker();
	}

	private void DataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
	{
		if (e.Row.Item is StringTableRow row)
		{
			// Style cells based on value.
			foreach (DataGridColumn column in ((DataGrid)sender!).Columns)
			{
				if (column.GetCellContent(e.Row) is TextBlock textBlock)
				{
					string? cellValue = GetCellValue(row, column.DisplayIndex)?.ToString();

					if (cellValue == "NULL")
						textBlock.Foreground = Brushes.Gray;
					else
						textBlock.Foreground = Brushes.LightSalmon;
				}
			}
		}
	}

	private void DataGrid_InitializingNewItem(object? sender, InitializingNewItemEventArgs e)
	{
		if (e.NewItem is StringTableRow row)
		{
			StringTableSection? section = _viewModel.SelectedSection;

			if (section is not null && section.IsExtraNG)
			{
				int nextId = section.Rows.Count > 0
					? section.Rows[^1].Id + 1
					: 0;

				row.Id = nextId;
				row.HexValue = ContentReader.GetShortHex((short)nextId, 3);
			}
		}
	}

	#endregion DataGrid event handlers

	#region Helpers

	private object? _cachedBeginEditValue;

	private DataGrid? GetCurrentDataGrid()
	{
		if (_viewModel.SelectedSection is null)
			return null;

		var selectedTab = SectionTabs.ItemContainerGenerator
			.ContainerFromIndex(_viewModel.SelectedSectionIndex) as TabItem;

		if (selectedTab is null)
			return null;

		return FindVisualChild<DataGrid>(selectedTab);
	}

	private IEnumerable<DataGrid> GetAllDataGrids()
	{
		for (int i = 0; i < _viewModel.Sections.Count; i++)
		{
			if (SectionTabs.ItemContainerGenerator.ContainerFromIndex(i) is TabItem tabItem)
			{
				DataGrid? grid = FindVisualChild<DataGrid>(tabItem);

				if (grid is not null)
					yield return grid;
			}
		}
	}

	private static DataGrid? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
	{
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);

			if (child is T found)
				return found as DataGrid;

			DataGrid? result = FindVisualChild<T>(child);

			if (result is not null)
				return result;
		}

		return null;
	}

	private static object? GetCellValue(StringTableRow row, int columnIndex) => columnIndex switch
	{
		0 => row.Id,
		1 => row.HexValue,
		2 => row.StringValue,
		_ => null
	};

	#endregion Helpers

	#region Row collection change tracking

	private void AttachRowCollectionListeners()
	{
		foreach (StringTableSection section in _viewModel.Sections)
		{
			section.Rows.CollectionChanged += OnRowsCollectionChanged;
			foreach (StringTableRow row in section.Rows)
				row.PropertyChanged += OnRowPropertyChanged;
		}
	}

	private void DetachRowCollectionListeners()
	{
		foreach (StringTableSection section in _viewModel.Sections)
		{
			section.Rows.CollectionChanged -= OnRowsCollectionChanged;
			foreach (StringTableRow row in section.Rows)
				row.PropertyChanged -= OnRowPropertyChanged;
		}
	}

	private void OnRowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Add ||
			e.Action == NotifyCollectionChangedAction.Remove)
		{
			if (e.OldItems is not null)
				foreach (StringTableRow row in e.OldItems)
					row.PropertyChanged -= OnRowPropertyChanged;

			if (e.NewItems is not null)
				foreach (StringTableRow row in e.NewItems)
					row.PropertyChanged += OnRowPropertyChanged;

			LastModified = DateTime.Now;
			RunContentChangedWorker();
			NotifyWorkspaceContentChanged();
		}
	}

	private void OnRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(StringTableRow.StringValue))
			return;

		LastModified = DateTime.Now;
		RunContentChangedWorker();
		NotifyWorkspaceContentChanged();
	}

	#endregion Row collection change tracking
}
