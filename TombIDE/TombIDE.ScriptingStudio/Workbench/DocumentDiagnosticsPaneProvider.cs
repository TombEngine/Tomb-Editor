#nullable enable

using System;
using System.Collections.Generic;
using Nickelony.IDEKit.AvalonEdit.Navigation;
using Nickelony.IDEKit.Core.Navigation;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Diagnostics;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class DocumentDiagnosticsPaneProvider : IStudioPaneContributionProvider
{
	private readonly ScriptingWorkspaceProfile _profile;
	private readonly IEditorDocumentController _documentController;

	public DocumentDiagnosticsPaneProvider(
		ScriptingWorkspaceProfile profile,
		IEditorDocumentController documentController)
	{
		ArgumentNullException.ThrowIfNull(profile);
		ArgumentNullException.ThrowIfNull(documentController);

		_profile = profile;
		_documentController = documentController;
	}

	public IReadOnlyList<StudioPaneContribution> GetPaneContributions()
	{
		if (!_profile.SupportsView(UICommand.LuaDiagnostics))
			return [];

		var pane = new TextDiagnosticsToolWindow(
			Shared.Strings.Default.LuaDiagnostics,
			"LuaDiagnostics",
			new TextDiagnosticsPresentation(
				Shared.Strings.Default.LuaDiagnosticsNoDocument,
				Shared.Strings.Default.LuaDiagnosticsUpdating,
				Shared.Strings.Default.NoDiagnostics,
				Shared.Strings.Default.Errors,
				Shared.Strings.Default.Warnings,
				Shared.Strings.Default.Messages,
				Shared.Strings.Default.Severity,
				Shared.Strings.Default.LineHeader,
				Shared.Strings.Default.ColumnHeader,
				Shared.Strings.Default.Message),
			NavigateToDiagnostic);

		return [new StudioPaneContribution(UICommand.LuaDiagnostics, pane.SerializationKey, () => pane)];
	}

	private void NavigateToDiagnostic(TextDiagnosticListItem diagnostic)
	{
		NavigateToLocation(
			diagnostic.FilePath,
			_ => new NavigationLocation(
				diagnostic.FilePath,
				diagnostic.StartOffset,
				diagnostic.StartOffset,
				Math.Max(0, diagnostic.EndOffset - diagnostic.StartOffset),
				diagnostic.LineNumber));
	}

	private void NavigateToLocation(string filePath, Func<TextEditorBase, NavigationLocation?> locationFactory)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return;

		_documentController.OpenFile(filePath);

		if (_documentController.CurrentEditor is not TextEditorBase textEditor)
			return;

		NavigationLocation? location = locationFactory(textEditor);

		if (location is null)
			return;

		EditorNavigationHelper.ApplyLocation(textEditor, location.Value);
	}
}