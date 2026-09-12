#nullable enable

using Nickelony.LanguageServer.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.UI.Presentation;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.AvalonEdit.Documents;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaReferenceSearchService(
	ITextEditorHost textEditorHost,
	ITextReferencesProvider referencesProvider,
	string scriptRootDirectoryPath,
	IWorkspaceDocumentManager? documentManager = null)
{
	private readonly ITextEditorHost _textEditorHost = textEditorHost ?? throw new ArgumentNullException(nameof(textEditorHost));
	private readonly ITextReferencesProvider _referencesProvider = referencesProvider ?? throw new ArgumentNullException(nameof(referencesProvider));
	private readonly string _scriptRootDirectoryPath = scriptRootDirectoryPath ?? string.Empty;
	private readonly IWorkspaceDocumentManager? _documentManager = documentManager;

	public bool SupportsReferences => _referencesProvider.SupportsReferences;

	public async Task<IReadOnlyList<TextReferenceGroup>> FindReferencesAsync(LuaEditor editor, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(editor);

		IReadOnlyList<TextReferenceLocation> references = await _referencesProvider
			.GetReferencesAsync(
				new TextReferenceRequest(
					editor.FilePath,
					editor.Text,
					Math.Max(0, editor.CurrentRow - 1),
					Math.Max(0, editor.CurrentColumn - 1)),
				cancellationToken)
			.ConfigureAwait(true);

		return await BuildReferenceGroupsAsync(references, cancellationToken).ConfigureAwait(true);
	}

	private async Task<IReadOnlyList<TextReferenceGroup>> BuildReferenceGroupsAsync(
		IReadOnlyList<TextReferenceLocation> references,
		CancellationToken cancellationToken)
	{
		if (references.Count == 0)
			return [];

		var snapshotCache = new Dictionary<string, ITextSnapshot?>(StringComparer.OrdinalIgnoreCase);
		var groups = new List<TextReferenceGroup>();

		foreach (IGrouping<string, TextReferenceLocation> fileGroup in references
			.Where(reference => !string.IsNullOrWhiteSpace(reference.FilePath))
			.GroupBy(reference => reference.FilePath, StringComparer.OrdinalIgnoreCase)
			.OrderBy(group => GetDisplayPath(group.Key), StringComparer.OrdinalIgnoreCase))
		{
			var items = new List<TextReferenceListItem>();
			foreach (TextReferenceLocation reference in fileGroup
				.OrderBy(reference => reference.StartLineNumber)
				.ThenBy(reference => reference.StartColumnNumber))
			{
				items.Add(new TextReferenceListItem(
					reference.FilePath,
					new TextDocumentRange(
						reference.StartLineNumber,
						reference.StartColumnNumber,
						reference.EndLineNumber,
						reference.EndColumnNumber),
					reference.StartLineNumber,
					reference.StartColumnNumber,
					await GetPreviewTextAsync(
						reference.FilePath,
						reference.StartLineNumber,
						snapshotCache,
						cancellationToken).ConfigureAwait(true)));
			}

			groups.Add(new TextReferenceGroup(fileGroup.Key, GetDisplayPath(fileGroup.Key), [.. items]));
		}

		return groups;
	}

	private string GetDisplayPath(string filePath)
	{
		string fullFilePath = Path.GetFullPath(filePath);
		string fullScriptRootPath = Path.GetFullPath(_scriptRootDirectoryPath);

		if (!fullScriptRootPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
			fullScriptRootPath += Path.DirectorySeparatorChar;

		if (fullFilePath.StartsWith(fullScriptRootPath, StringComparison.OrdinalIgnoreCase))
			return Path.GetRelativePath(fullScriptRootPath, fullFilePath);

		return fullFilePath;
	}

	private async Task<string> GetPreviewTextAsync(
		string filePath,
		int lineNumber,
		Dictionary<string, ITextSnapshot?> snapshotCache,
		CancellationToken cancellationToken)
	{
		if (!snapshotCache.TryGetValue(filePath, out ITextSnapshot? snapshot))
		{
			snapshot = await TryGetSnapshotAsync(filePath, cancellationToken).ConfigureAwait(true);
			snapshotCache[filePath] = snapshot;
		}

		return TryGetSnapshotLineText(snapshot, lineNumber)?.Trim() ?? string.Empty;
	}

	private async Task<ITextSnapshot?> TryGetSnapshotAsync(string filePath, CancellationToken cancellationToken)
	{
		TextEditorBase? textEditor = _textEditorHost.GetOpenEditors(filePath)
			.OfType<TextEditorBase>()
			.FirstOrDefault();

		if (textEditor is not null)
			return new TextDocumentSnapshot(textEditor.Document);

		if (_documentManager is null)
			return null;

		WorkspaceDocumentOpenResult result = await _documentManager
			.OpenAsync(filePath, DefaultWorkspaceOpenOptions, cancellationToken)
			.ConfigureAwait(true);

		return result.Snapshot?.Text;
	}

	private static string? TryGetSnapshotLineText(ITextSnapshot? snapshot, int lineNumber)
	{
		if (snapshot is null || lineNumber < 1 || lineNumber > snapshot.LineCount)
			return null;

		ITextLine line = snapshot.GetLineByNumber(lineNumber);
		return snapshot.GetText(line.Offset, line.Length);
	}

	private static readonly WorkspaceDocumentOpenOptions DefaultWorkspaceOpenOptions = new(
		TextEncodingKind.Utf8,
		new TextFileFormat(TextEncodingKind.Utf8, false, TextNewlineStyle.Lf));
}
