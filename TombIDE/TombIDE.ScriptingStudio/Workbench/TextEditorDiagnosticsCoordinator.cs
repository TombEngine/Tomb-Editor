#nullable enable

using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class TextEditorDiagnosticsCoordinator : IDisposable
{
	private readonly IEditorDocumentController _documentController;
	private readonly WorkbenchPaneCoordinator _paneCoordinator;
	private TextEditorBase? _diagnosticsEditor;

	public TextEditorDiagnosticsCoordinator(
		IEditorDocumentController documentController,
		WorkbenchPaneCoordinator paneCoordinator)
	{
		_documentController = documentController ?? throw new ArgumentNullException(nameof(documentController));
		_paneCoordinator = paneCoordinator ?? throw new ArgumentNullException(nameof(paneCoordinator));
	}

	public void Update(ScriptingDocumentContext documentContext)
	{
		TextEditorBase? textEditor = documentContext.Registration?.DocumentMode is DocumentMode.ClassicScript or DocumentMode.TRX
			? documentContext.Editor as TextEditorBase
			: null;

		if (ReferenceEquals(_diagnosticsEditor, textEditor))
			return;

		Detach();

		if (textEditor is null)
			return;

		_diagnosticsEditor = textEditor;
		_diagnosticsEditor.DiagnosticsChanged += TextEditor_DiagnosticsChanged;
		ShowDiagnostics(textEditor);
	}

	public void EditorClosed(IEditorControl editor)
	{
		if (ReferenceEquals(_diagnosticsEditor, editor))
			Detach();
	}

	public void Dispose()
		=> Detach();

	private void Detach()
	{
		if (_diagnosticsEditor is null)
			return;

		_diagnosticsEditor.DiagnosticsChanged -= TextEditor_DiagnosticsChanged;
		_diagnosticsEditor = null;
	}

	private void TextEditor_DiagnosticsChanged(object? sender, EventArgs e)
	{
		if (sender is not TextEditorBase textEditor)
			return;

		ScriptingDocumentContext documentContext = _documentController.CurrentDocumentContext ?? ScriptingDocumentContext.Empty;
		if (!ReferenceEquals(documentContext.Editor, textEditor)
			|| !string.Equals(documentContext.FilePath, textEditor.FilePath, StringComparison.OrdinalIgnoreCase)
			|| documentContext.Registration?.DocumentMode is not (DocumentMode.ClassicScript or DocumentMode.TRX))
			return;

		ShowDiagnostics(textEditor);
	}

	private void ShowDiagnostics(TextEditorBase textEditor)
		=> _paneCoordinator.ShowTextEditorDiagnostics(textEditor);
}
