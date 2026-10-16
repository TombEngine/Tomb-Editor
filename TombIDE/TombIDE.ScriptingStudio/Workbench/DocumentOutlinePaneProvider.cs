#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class DocumentOutlinePaneProvider : IStudioPaneContributionProvider
{
	private readonly IEditorDocumentController _documentController;
	private readonly DocumentOutlineViewModel _viewModel;

	public DocumentOutlinePaneProvider(
		IEditorDocumentController documentController,
		DocumentOutlineViewModel viewModel)
	{
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(viewModel);

		_documentController = documentController;
		_viewModel = viewModel;
	}

	public IReadOnlyList<StudioPaneContribution> GetPaneContributions()
	{
		var pane = new DocumentOutlineToolWindow(_viewModel);
		pane.ObjectClicked += DocumentOutline_ObjectClicked;
		return [new StudioPaneContribution(UICommand.ContentExplorer, pane.SerializationKey, () => pane)];
	}

	private void DocumentOutline_ObjectClicked(object? sender, ObjectClickedEventArgs e)
	{
		if (_documentController.CurrentEditor is INameBasedObjectNavigator navigator)
			navigator.GoToObject(e.ObjectName, e.IdentifyingObject);
	}
}
