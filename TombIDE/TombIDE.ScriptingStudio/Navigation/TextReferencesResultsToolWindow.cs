#nullable enable

using System;
using System.Collections.Generic;
using System.Windows;
using TombIDE.ScriptingStudio.Shell;
using TombLib.Scripting.UI.Presentation;

namespace TombIDE.ScriptingStudio.Navigation;

public sealed class TextReferencesResultsToolWindow : StudioDockPane
{
	private readonly TextReferencesResultsViewModel _viewModel;
	private readonly TextReferencesResultsView _view;

	internal TextReferencesResultsToolWindow(
		string dockText,
		string serializationKey,
		TextReferencesPresentation presentation,
		Action<TextReferenceListItem>? activateReference)
		: base(dockText, serializationKey, StudioDockPaneLocation.Bottom, new Size(420, 220))
	{
		_viewModel = new TextReferencesResultsViewModel(presentation);
		_view = new TextReferencesResultsView(_viewModel, activateReference);
	}

	public override UIElement Content => _view;

	public void ShowNoActiveDocument()
		=> _viewModel.ShowNoActiveDocument();

	public void ShowUnsupported()
		=> _viewModel.ShowUnsupported();

	public void ShowLoading()
		=> _viewModel.ShowLoading();

	internal void ShowReferences(IReadOnlyList<TextReferenceGroup> groups)
		=> _viewModel.ShowReferences(groups);
}
