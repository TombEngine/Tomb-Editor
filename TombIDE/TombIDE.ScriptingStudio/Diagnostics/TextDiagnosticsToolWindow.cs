#nullable enable

using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using System.Windows;
using TombIDE.ScriptingStudio.Shell;
using TombLib.Scripting.UI.Presentation;

namespace TombIDE.ScriptingStudio.Diagnostics;

public sealed class TextDiagnosticsToolWindow : StudioDockPane
{
	private readonly TextDiagnosticsViewModel _viewModel;
	private readonly TextDiagnosticsView _view;

	internal TextDiagnosticsToolWindow(
		string dockText,
		string serializationKey,
		TextDiagnosticsPresentation presentation,
		Action<TextDiagnosticListItem>? activateDiagnostic)
		: base(dockText, serializationKey, StudioDockPaneLocation.Bottom, new Size(420, 220))
	{
		_viewModel = new TextDiagnosticsViewModel(presentation);
		_view = new TextDiagnosticsView(_viewModel, activateDiagnostic);
	}

	public override UIElement Content => _view;

	public void ShowNoActiveDocument()
		=> _viewModel.ShowNoActiveDocument();

	public void ShowPending()
		=> _viewModel.ShowPending();

	public void ShowDiagnostics(string filePath, TextDocument document, IReadOnlyList<TextDiagnostic> diagnostics)
		=> _viewModel.ShowDiagnostics(filePath, document, diagnostics);
}
