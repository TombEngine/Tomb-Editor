#nullable enable

using System;
using System.Linq;
using TombLib.Scripting.ClassicScript.StringTables;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

/// <summary>
/// Holds the workspace view state and content publication for the string-table editor.
/// </summary>
public partial class StringEditorView
{
	private int _projectionSuppressionDepth;
	private bool _workspaceViewAttached;
	private string _workspaceCanonicalContent = string.Empty;
	private long _workspaceCanonicalVersion;
	private string? _workspacePendingContent;

	internal event EventHandler? WorkspaceContentChanged;

	internal string WorkspaceCanonicalContent => _workspaceCanonicalContent;

	internal bool WorkspaceViewAttached => _workspaceViewAttached;

	internal string WorkspacePendingContent => _workspacePendingContent ?? BuildWorkspaceContent();

	internal ClassicScriptStringTableParseResult ParseWorkspaceContent(
		string content,
		string parsedNewline)
		=> ClassicScriptStringTableParser.Parse(
			content,
			new ClassicScriptStringTableParseOptions(parsedNewline));

	internal void ApplyWorkspaceSnapshot(
		WorkspaceDocumentSnapshot snapshot,
		ClassicScriptStringTable table)
	{
		ArgumentNullException.ThrowIfNull(snapshot);
		ArgumentNullException.ThrowIfNull(table);

		using IDisposable resetScope = _contentPersistenceCoordinator.BeginResetScope();
		using IDisposable suppressionScope = BeginWorkspaceSuppression();

		FilePath = snapshot.DisplayPath;
		ApplyWorkspaceTable(table);

		_workspaceViewAttached = true;
		_workspaceCanonicalContent = snapshot.Content;
		_workspaceCanonicalVersion = snapshot.Version;
		_workspacePendingContent = null;
		IsContentChanged = snapshot.IsDirty;
	}

	internal void SetWorkspacePendingContent(string content)
	{
		ArgumentNullException.ThrowIfNull(content);

		_workspacePendingContent = content;
		IsContentChanged = true;
	}

	internal void DetachWorkspaceView()
	{
		_workspaceViewAttached = false;
		_workspaceCanonicalContent = string.Empty;
		_workspaceCanonicalVersion = 0;
		_workspacePendingContent = null;
	}

	private void NotifyWorkspaceContentChanged()
	{
		if (!_workspaceViewAttached || _projectionSuppressionDepth > 0)
			return;

		SetWorkspacePendingContent(BuildWorkspaceContent());
		WorkspaceContentChanged?.Invoke(this, EventArgs.Empty);
	}

	private IDisposable BeginWorkspaceSuppression()
	{
		_projectionSuppressionDepth++;
		return new WorkspaceSuppressionScope(this);
	}

	private sealed class WorkspaceSuppressionScope : IDisposable
	{
		private readonly StringEditorView _view;
		private bool _disposed;

		public WorkspaceSuppressionScope(StringEditorView view)
		{
			_view = view;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_view._projectionSuppressionDepth--;
		}
	}

	private string BuildWorkspaceContent()
	{
		var sections = _viewModel.Sections.Select(section =>
			new ClassicScriptStringTableSection(
				section.SectionName.Trim('[', ']'),
				section.IsExtraNG,
				section.Rows.Select(row => new ClassicScriptStringTableRow(
					section.IsExtraNG ? row.Id : null,
					row.StringValue))));

		return ClassicScriptStringTableWriter.Write(
			new ClassicScriptStringTable(sections),
			new ClassicScriptStringTableWriteOptions(
				ClassicScriptStringTableNewlineStyle.CrLf,
				new ClassicScriptStringTableHeader(
					"TombIDE",
					typeof(StringEditorView).Assembly.GetName().Version?.ToString() ?? "0.0.0")));
	}
}
