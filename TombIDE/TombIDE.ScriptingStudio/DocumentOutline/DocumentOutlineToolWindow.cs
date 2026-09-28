#nullable enable

using System;
using System.Windows;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.DocumentOutline;

public sealed class DocumentOutlineToolWindow : StudioDockPane
{
	private readonly DocumentOutlineView _view;
	private readonly DocumentOutlineViewModel _viewModel;

	public DocumentOutlineToolWindow(DocumentOutlineViewModel viewModel)
		: base(Strings.Default.ContentExplorer, "ContentExplorer", StudioDockPaneLocation.Right, new Size(260, 320))
	{
		ArgumentNullException.ThrowIfNull(viewModel);

		_viewModel = viewModel;
		_view = new DocumentOutlineView
		{
			DataContext = _viewModel
		};

		_view.NodeInvoked += View_NodeInvoked;
	}

	public event ObjectClickedEventHandler? ObjectClicked;

	public override UIElement Content => _view;

	/// <summary>
	/// Gets or sets the document context the outline is bound to.
	/// </summary>
	public ScriptingDocumentContext? DocumentContext
	{
		get => _viewModel.DocumentContext;
		set => _viewModel.DocumentContext = value;
	}

	public void SelectNode(string nodeText)
		=> _view.SelectNode(nodeText);

	public override void Dispose()
	{
		_view.NodeInvoked -= View_NodeInvoked;
		ObjectClicked = null;
		_viewModel.Dispose();
	}

	private void View_NodeInvoked(object? sender, DocumentOutlineNodeViewModel node)
		=> ObjectClicked?.Invoke(this, new ObjectClickedEventArgs(node.Text, node.IdentifyingObject));
}
