#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TombLib.Scripting.ClassicScript.StringTables;
using Nickelony.IDEKit.IntelliSense.Navigation;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Documents;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

/// <summary>
/// Implements content parsing, row population, and edit commands for the string-table editor.
/// </summary>
public partial class StringEditorView
{
	#region Content

	private void UpdateContent(string content)
	{
		string[] lines = content.Replace("\r", string.Empty).Split('\n');
		UpdateContent(lines);
	}

	private void UpdateContent(string[] lines)
	{
		DetachRowCollectionListeners();
		_viewModel.Sections.Clear();
		_viewModel.ClearUndoRedo();

		int currentLineNumber = 0;
		int totalStringCount = 0;

		while (ContentReader.NextSectionExists(lines, currentLineNumber, out int nextSectionLineNumber))
		{
			string currentSectionName = lines[nextSectionLineNumber];
			List<string> strings = ContentReader.GetStrings(lines, nextSectionLineNumber);

			bool isExtraNG = Regex.IsMatch(currentSectionName, @"^\[ExtraNG\]", RegexOptions.IgnoreCase);

			var section = new StringTableSection
			{
				SectionName = currentSectionName,
				Mode = isExtraNG ? StringTableMode.ExtraNG : StringTableMode.Normal
			};

			if (isExtraNG)
			{
				PopulateExtraNGRows(section, strings);
			}
			else
			{
				PopulateNormalRows(section, strings, totalStringCount);
				totalStringCount += strings.Count;
			}

			_viewModel.Sections.Add(section);

			currentLineNumber = nextSectionLineNumber + (strings.Count == 0 ? 1 : strings.Count);
		}

		if (_viewModel.Sections.Count > 0 && _viewModel.SelectedSectionIndex < 0)
			_viewModel.SelectedSectionIndex = 0;

		AttachRowCollectionListeners();
		ApplyZoomToAllGrids();
		RunContentChangedWorker();
	}

	private static void PopulateNormalRows(StringTableSection section, List<string> strings, int idOffset)
	{
		for (int i = 0; i < strings.Count; i++)
		{
			short id = (short)(idOffset + i);
			string hex = ContentReader.GetShortHex(id, 4);

			section.Rows.Add(new StringTableRow
			{
				Id = id,
				HexValue = hex,
				StringValue = strings[i]
			});
		}
	}

	private static void PopulateExtraNGRows(StringTableSection section, List<string> strings)
	{
		for (int i = 0; i < strings.Count; i++)
		{
			if (!Regex.IsMatch(strings[i], @"^\d+:.*"))
				continue;

			short id = short.Parse(strings[i].Split(':').First());
			string hex = ContentReader.GetShortHex(id, 3);
			string value = Regex.Replace(strings[i], @"^\d+:", string.Empty).TrimStart(' ');

			section.Rows.Add(new StringTableRow
			{
				Id = id,
				HexValue = hex,
				StringValue = value
			});
		}
	}

	#endregion Content

	#region Edit methods

	public void Undo()
	{
		_viewModel.Undo();
		LastModified = DateTime.Now;
		RunContentChangedWorker();
	}

	public void Redo()
	{
		_viewModel.Redo();
		LastModified = DateTime.Now;
		RunContentChangedWorker();
	}

	public void Cut()
	{
		DataGrid? grid = GetCurrentDataGrid();

		if (grid is null)
			return;

		if (grid.CurrentCell.Item is StringTableRow row && grid.CurrentColumn is not null)
		{
			object? value = GetCellValue(row, grid.CurrentColumn.DisplayIndex);

			if (value is not null)
				Clipboard.SetText(value.ToString());

			if (!grid.CurrentColumn.IsReadOnly)
			{
				string? cachedValue = row.StringValue;
				row.StringValue = string.Empty;

				_viewModel.PushUndo(new DataGridUndoItem(
					_viewModel.SelectedSection?.SectionName ?? string.Empty,
					grid.Items.IndexOf(row),
					grid.CurrentColumn.DisplayIndex,
					cachedValue));
			}
		}
	}

	public void Copy()
	{
		DataGrid? grid = GetCurrentDataGrid();

		if (grid is null)
			return;

		if (grid.CurrentCell.Item is StringTableRow row && grid.CurrentColumn is not null)
		{
			object? value = GetCellValue(row, grid.CurrentColumn.DisplayIndex);

			if (value is not null)
				Clipboard.SetText(value.ToString());
		}
	}

	public void Paste()
	{
		DataGrid? grid = GetCurrentDataGrid();

		if (grid is null)
			return;

		if (!Clipboard.ContainsText())
			return;

		if (grid.CurrentCell.Item is StringTableRow row && grid.CurrentColumn is not null && !grid.CurrentColumn.IsReadOnly)
		{
			string? cachedValue = row.StringValue;
			row.StringValue = Clipboard.GetText();

			_viewModel.PushUndo(new DataGridUndoItem(
				_viewModel.SelectedSection?.SectionName ?? string.Empty,
				grid.Items.IndexOf(row),
				grid.CurrentColumn.DisplayIndex,
				cachedValue));

			IsContentChanged = true;
			RunContentChangedWorker();
		}
	}

	public void SelectAll()
	{
		DataGrid? grid = GetCurrentDataGrid();
		grid?.SelectAll();
	}

	public void GoToObject(string objectName, TextDefinitionDiscriminator? identifyingObject = null)
	{
		// Callers pass the bare section name; the model stores it with brackets
		// (e.g. "[Section1]"), matching the old WinForms behavior.
		string bracketedName = $"[{objectName}]";

		for (int i = 0; i < _viewModel.Sections.Count; i++)
		{
			if (_viewModel.Sections[i].SectionName.Equals(bracketedName, StringComparison.OrdinalIgnoreCase))
			{
				_viewModel.SelectedSectionIndex = i;
				return;
			}
		}
	}

	#endregion Edit methods

	#region Settings

	public void UpdateSettings(ConfigurationBase configuration)
	{
		if (configuration is not TextEditorConfigBase config)
			return;

		_viewModel.FontSize = (int)config.FontSize - 4;
		_viewModel.FontFamily = config.FontFamily;

		ApplyZoomToAllGrids();
	}

	#endregion Settings

	#region Zoom

	private void ApplyZoomToAllGrids()
	{
		double fontSize = _viewModel.FontSize * _viewModel.ZoomLevel / 100.0;
		var fontFamily = new FontFamily(_viewModel.FontFamily);

		foreach (DataGrid grid in GetAllDataGrids())
		{
			grid.FontFamily = fontFamily;
			grid.FontSize = fontSize;
			grid.RowHeight = Double.NaN; // Auto row height

			UpdateColumnWidths(grid, fontSize);
		}
	}

	private static void UpdateColumnWidths(DataGrid grid, double fontSize)
	{
		double stringWidth = fontSize * 4.5;

		if (grid.Columns.Count >= 2)
		{
			grid.Columns[0].Width = new DataGridLength(stringWidth);
			grid.Columns[1].Width = new DataGridLength(stringWidth);
		}
	}

	#endregion Zoom
}
