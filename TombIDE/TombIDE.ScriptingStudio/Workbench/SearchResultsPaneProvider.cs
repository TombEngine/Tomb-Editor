#nullable enable

using System;
using System.Collections.Generic;
using Nickelony.IDEKit.AvalonEdit.Navigation;
using Nickelony.IDEKit.Core.Navigation;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class SearchResultsPaneProvider : IStudioPaneContributionProvider
{
	private readonly IEditorDocumentController _documentController;

	public SearchResultsPaneProvider(IEditorDocumentController documentController)
	{
		ArgumentNullException.ThrowIfNull(documentController);
		_documentController = documentController;
	}

	public IReadOnlyList<StudioPaneContribution> GetPaneContributions()
	{
		var pane = new SearchResultsToolWindow(NavigateToSearchResult);
		return [new StudioPaneContribution(UICommand.SearchResults, pane.SerializationKey, () => pane)];
	}

	private void NavigateToSearchResult(string filePath, FindReplaceItem item)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return;

		_documentController.OpenFile(filePath);

		if (_documentController.CurrentEditor is not TextEditorBase textEditor)
			return;

		SearchResultLocation resolved = SearchResultLocationResolver.Resolve(textEditor.Document, filePath, item);

		if (resolved.Status == SearchResultLocationStatus.LineNotFound || resolved.Location is not NavigationLocation location)
			return;

		TextAreaNavigationOperations.ApplyLocation(textEditor.TextArea, location);
	}
}
