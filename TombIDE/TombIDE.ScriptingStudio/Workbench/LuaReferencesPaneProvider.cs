#nullable enable

using System;
using System.Collections.Generic;
using Nickelony.IDEKit.AvalonEdit.Navigation;
using Nickelony.IDEKit.Core.Navigation;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class LuaReferencesPaneProvider : IStudioPaneContributionProvider
{
	private readonly ScriptingWorkspaceProfile _profile;
	private readonly IEditorDocumentController _documentController;

	public LuaReferencesPaneProvider(
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
		if (!_profile.SupportsLua || !_profile.SupportsView(UICommand.LuaReferencesResults))
			return [];

		var pane = new TextReferencesResultsToolWindow(
			Shared.Strings.Default.LuaReferencesResults,
			"LuaReferencesResults",
			new TextReferencesPresentation(
				Shared.Strings.Default.LuaReferencesNoDocument,
				Shared.Strings.Default.LuaReferencesUnsupported,
				Shared.Strings.Default.LuaReferencesLoading,
				Shared.Strings.Default.NoReferencesFound),
			NavigateToReference);

		return [new StudioPaneContribution(UICommand.LuaReferencesResults, pane.SerializationKey, () => pane)];
	}

	private void NavigateToReference(TextReferenceListItem reference)
	{
		NavigateToLocation(
			reference.FilePath,
			textEditor => EditorNavigationHelper.CreateRangeLocation(
				textEditor,
				reference.FilePath,
				reference.Range.StartLineNumber,
				reference.Range.StartColumnNumber,
				reference.Range.EndLineNumber,
				reference.Range.EndColumnNumber));
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
