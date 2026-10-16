#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.ClassicScript.StringTables;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

public partial class StringEditorView : UserControl, IEditorControl, IStringSectionNavigator, INameBasedObjectNavigator
{
	#region IEditorControl properties

	public EditorType EditorType => EditorType.Strings;
	public string DefaultFileExtension => ".txt";

	public string FilePath
	{
		get => _contentPersistenceCoordinator.FilePath;
		set => _contentPersistenceCoordinator.FilePath = value;
	}

	private readonly EditorProcessingModeScope _processingModeScope = new();

	public EditorProcessingMode ProcessingMode
		=> _processingModeScope.CurrentMode;

	public IDisposable BeginProcessingScope(EditorProcessingMode mode)
		=> _processingModeScope.Begin(mode);

	public bool CreateBackupFiles
	{
		get => ProcessingMode == EditorProcessingMode.Suppressed
			? false
			: _contentPersistenceCoordinator.CreateBackupFiles;
		set => _contentPersistenceCoordinator.CreateBackupFiles = value;
	}

	public new string Content
	{
		get => BuildWorkspaceContent();
		set => UpdateContent(value);
	}

	public bool IsContentChanged { get; set; }

	public DateTime LastModified { get; set; }

	public int CurrentRow
	{
		get
		{
			DataGrid? grid = GetCurrentDataGrid();

			if (grid is null || grid.CurrentCell.Item is not StringTableRow row)
				return 0;

			return grid.Items.IndexOf(row);
		}
	}

	public int CurrentColumn
	{
		get
		{
			DataGrid? grid = GetCurrentDataGrid();
			return grid?.CurrentColumn?.DisplayIndex ?? 0;
		}
	}

	public string? SelectedContent
	{
		get
		{
			DataGrid? grid = GetCurrentDataGrid();

			if (grid is null)
				return null;

			if (grid.CurrentCell.Item is StringTableRow row && grid.CurrentColumn is not null)
				return GetCellValue(row, grid.CurrentColumn.DisplayIndex)?.ToString();

			return null;
		}
	}

	public int SelectionLength => GetCurrentDataGrid()?.SelectedCells.Count ?? 0;

	public int Zoom
	{
		get => _viewModel.ZoomLevel;
		set
		{
			_viewModel.ZoomLevel = value;
			ApplyZoomToAllGrids();
		}
	}

	public int MinZoom { get; set; } = 25;
	public int MaxZoom { get; set; } = 400;
	public int ZoomStepSize { get; set; } = 15;

	public bool CanUndo => _viewModel.CanUndo;
	public bool CanRedo => _viewModel.CanRedo;

	public Version EngineVersion { get; set; } = new Version(0, 0);

	#endregion IEditorControl properties

	#region IStringSectionNavigator

	string? IStringSectionNavigator.CurrentSectionName => _viewModel.SelectedSection?.SectionName;

	public void GoToPreviousSection()
	{
		if (_viewModel.SelectedSectionIndex > 0)
			_viewModel.SelectedSectionIndex--;
	}

	public void GoToNextSection()
	{
		if (_viewModel.SelectedSectionIndex < _viewModel.Sections.Count - 1)
			_viewModel.SelectedSectionIndex++;
	}

	public void ClearSelectedString()
	{
		DataGrid? grid = GetCurrentDataGrid();

		if (grid?.CurrentCell.Item is StringTableRow row)
		{
			string? cachedValue = row.StringValue;
			row.StringValue = "NULL";

			_viewModel.PushUndo(new DataGridUndoItem(
				_viewModel.SelectedSection?.SectionName ?? string.Empty,
				grid.Items.IndexOf(row),
				2,
				cachedValue));
		}
	}

	public void RemoveLastString()
	{
		StringTableSection? section = _viewModel.SelectedSection;

		if (section is null || !section.IsExtraNG)
			return;

		if (section.Rows.Count > 0)
		{
			StringTableRow lastRow = section.Rows[^1];
			section.Rows.Remove(lastRow);

			IsContentChanged = true;
			RunContentChangedWorker();
		}
	}

	#endregion IStringSectionNavigator

	#region Fields

	private readonly StringEditorViewModel _viewModel;
	private readonly ContentPersistenceCoordinator _contentPersistenceCoordinator;
	#endregion Fields

	#region Construction

	public StringEditorView(Version engineVersion)
	{
		_viewModel = new StringEditorViewModel();
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;

		DataContext = _viewModel;

		_contentPersistenceCoordinator = new ContentPersistenceCoordinator(() => Content, () => ProcessingMode == EditorProcessingMode.Suppressed);
		_contentPersistenceCoordinator.ContentChangedWorkerRunCompleted += (s, e) =>
			OnContentChangedWorkerRunCompleted(EventArgs.Empty);

		EngineVersion = engineVersion;

		InitializeComponent();
	}

	#endregion Construction

	#region Dispose

	public void Dispose()
	{
		DetachRowCollectionListeners();
		_contentPersistenceCoordinator?.Dispose();
		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
	}

	private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(StringEditorViewModel.CanUndo) ||
			e.PropertyName == nameof(StringEditorViewModel.CanRedo))
		{
			OnStatusChanged(EventArgs.Empty);
		}
	}

	#endregion Dispose

	#region Events

	public event EventHandler? StatusChanged;

	protected virtual void OnStatusChanged(EventArgs e)
		=> StatusChanged?.Invoke(this, e);

	public event EventHandler? ZoomChanged;

	protected virtual void OnZoomChanged(EventArgs e)
	{
		ZoomChanged?.Invoke(this, e);
		OnStatusChanged(EventArgs.Empty);
	}

	public event EventHandler? ContentChangedWorkerRunCompleted;

	protected virtual void OnContentChangedWorkerRunCompleted(EventArgs e)
		=> ContentChangedWorkerRunCompleted?.Invoke(this, e);

	#endregion Events

	#region File I/O

	public void Load(string filePath)
		=> Load(filePath, default);

	public void Load(string filePath, DocumentLoadOptions options)
	{
		using IDisposable processingScope = BeginProcessingScope(options.ProcessingMode);
		using IDisposable resetScope = _contentPersistenceCoordinator.BeginResetScope();

		string[] fileLines = File.ReadAllLines(filePath);
		UpdateContent(fileLines);

		FilePath = filePath;
		_contentPersistenceCoordinator.SetPersistedContent(Content);

		IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
	}

	public void Save()
		=> Save(FilePath);

	public void Save(string filePath)
	{
		File.WriteAllText(filePath, Content);
		_contentPersistenceCoordinator.SetPersistedContent(Content);
		IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		LastModified = DateTime.Now;
	}

	#endregion File I/O

	#region Content

	public void RunContentChangedWorker()
	{
		if (_workspaceViewAttached)
		{
			IsContentChanged = true;
			return;
		}

		IsContentChanged = _contentPersistenceCoordinator.RunContentChangedCheck();
	}

	public void ApplyPersistedContent(string content)
	{
		UpdateContent(content);
		_contentPersistenceCoordinator.SetPersistedContent(Content);
		IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		LastModified = DateTime.Now;
	}

	internal void ApplyWorkspaceTable(ClassicScriptStringTable table)
	{
		DetachRowCollectionListeners();
		_viewModel.Sections.Clear();
		_viewModel.ClearUndoRedo();

		int totalStringCount = 0;
		foreach (ClassicScriptStringTableSection sourceSection in table.Sections)
		{
			var section = new StringTableSection
			{
				SectionName = $"[{sourceSection.Name}]",
				Mode = sourceSection.IsExtraNg ? StringTableMode.ExtraNG : StringTableMode.Normal
			};

			for (int index = 0; index < sourceSection.Rows.Count; index++)
			{
				ClassicScriptStringTableRow sourceRow = sourceSection.Rows[index];
				int id = sourceRow.Id ?? totalStringCount + index;

				section.Rows.Add(new StringTableRow
				{
					Id = id,
					HexValue = ContentReader.GetShortHex((short)id, sourceSection.IsExtraNg ? 3 : 4),
					StringValue = sourceRow.Value
				});
			}

			if (!sourceSection.IsExtraNg)
				totalStringCount += sourceSection.Rows.Count;

			_viewModel.Sections.Add(section);
		}

		if (_viewModel.Sections.Count > 0 && _viewModel.SelectedSectionIndex < 0)
			_viewModel.SelectedSectionIndex = 0;

		AttachRowCollectionListeners();
		ApplyZoomToAllGrids();
	}

	#endregion Content

	#region Keyboard handlers

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		// Shift+Enter inserts a newline when editing a DataGrid cell,
		// matching the old WinForms DataGridView behavior with WrapMode.
		if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
		{
			if (Keyboard.FocusedElement is TextBox textBox &&
				textBox.TemplatedParent is DataGridCell)
			{
				int caretIndex = textBox.CaretIndex;
				textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);
				textBox.CaretIndex = caretIndex + Environment.NewLine.Length;
				e.Handled = true;
				return;
			}
		}

		if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Undo();
			e.Handled = true;
		}
		else if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Redo();
			e.Handled = true;
		}
		else if (e.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Cut();
			e.Handled = true;
		}
		else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Copy();
			e.Handled = true;
		}
		else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Paste();
			e.Handled = true;
		}
		else if (e.Key == Key.PageUp && Keyboard.Modifiers == ModifierKeys.Control)
		{
			GoToPreviousSection();
			e.Handled = true;
		}
		else if (e.Key == Key.PageDown && Keyboard.Modifiers == ModifierKeys.Control)
		{
			GoToNextSection();
			e.Handled = true;
		}
	}

	private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (Keyboard.Modifiers == ModifierKeys.Control)
		{
			if (e.Delta > 0)
			{
				if (_viewModel.ZoomLevel < MaxZoom)
				{
					_viewModel.ZoomLevel += ZoomStepSize;
					ApplyZoomToAllGrids();
				}
			}
			else
			{
				if (_viewModel.ZoomLevel > MinZoom)
				{
					_viewModel.ZoomLevel -= ZoomStepSize;
					ApplyZoomToAllGrids();
				}
			}

			OnZoomChanged(EventArgs.Empty);
			e.Handled = true;
		}
	}

	#endregion Keyboard handlers

}
