#nullable enable

using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaTrackedDocumentStateService(ITextEditorHost textEditorHost, ILuaIntelliSenseProvider intellisenseProvider)
{
	private readonly ITextEditorHost _textEditorHost = textEditorHost ?? throw new ArgumentNullException(nameof(textEditorHost));
	private readonly ILuaIntelliSenseProvider _intellisenseProvider = intellisenseProvider ?? throw new ArgumentNullException(nameof(intellisenseProvider));

	public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
		=> _intellisenseProvider.GetDiagnostics(filePath);

	public void OpenDocument(LuaEditor editor)
	{
		ArgumentNullException.ThrowIfNull(editor);

		_intellisenseProvider.OpenDocument(editor.FilePath, editor.Text);
		ApplyTrackedState(editor);
	}

	public void UpdateDocument(LuaEditor editor)
	{
		ArgumentNullException.ThrowIfNull(editor);

		_intellisenseProvider.UpdateDocument(editor.FilePath, editor.Text);
	}

	public void RenameDocument(string oldFilePath, string newFilePath, LuaEditor editor)
	{
		ArgumentNullException.ThrowIfNull(editor);

		_intellisenseProvider.RenameDocument(oldFilePath, newFilePath, editor.Text);
	}

	public void ApplyTrackedState(LuaEditor editor)
	{
		ArgumentNullException.ThrowIfNull(editor);

		ApplyDiagnosticsToEditor(editor, _intellisenseProvider.GetDiagnostics(editor.FilePath));
		ApplySemanticTokensToEditor(editor, _intellisenseProvider.GetSemanticTokens(editor.FilePath));
	}

	public void ApplyTrackedStateToEditors(string filePath)
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _intellisenseProvider.GetDiagnostics(filePath);
		IReadOnlyList<LuaSemanticToken> semanticTokens = _intellisenseProvider.GetSemanticTokens(filePath);

		ApplyDiagnosticsUpdate(filePath, diagnostics);
		ApplySemanticTokensUpdate(filePath, semanticTokens);
	}

	public void ApplyDiagnosticsUpdate(string filePath, IReadOnlyList<TextEditorDiagnostic> diagnostics)
	{
		foreach (LuaEditor editor in GetOpenLuaEditors(filePath))
			ApplyDiagnosticsToEditor(editor, diagnostics);
	}

	public void ApplySemanticTokensUpdate(string filePath, IReadOnlyList<LuaSemanticToken> semanticTokens)
	{
		foreach (LuaEditor editor in GetOpenLuaEditors(filePath))
			ApplySemanticTokensToEditor(editor, semanticTokens);
	}

	private IEnumerable<LuaEditor> GetOpenLuaEditors(string filePath)
		=> _textEditorHost.GetOpenEditors(filePath).OfType<LuaEditor>();

	private static void ApplyDiagnosticsToEditor(LuaEditor editor, IReadOnlyList<TextEditorDiagnostic> diagnostics)
		=> editor.SetDiagnostics(editor.LiveErrorUnderlining ? diagnostics : []);

	private static void ApplySemanticTokensToEditor(LuaEditor editor, IReadOnlyList<LuaSemanticToken> semanticTokens)
		=> editor.SetSemanticTokens(semanticTokens ?? []);
}
