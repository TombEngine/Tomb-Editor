#nullable enable

using System;
using System.Windows;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.FindAndReplace;

public sealed class SearchResultsToolWindow : StudioDockPane
{
	private readonly SearchResultsView _view;
	private readonly SearchResultsViewModel _viewModel;

	public SearchResultsToolWindow(Action<string, FindReplaceItem>? navigateToSearchResult)
		: base(Strings.Default.SearchResults, "SearchResults", StudioDockPaneLocation.Bottom, new Size(420, 220))
	{
		_viewModel = new SearchResultsViewModel();
		_view = new SearchResultsView
		{
			DataContext = _viewModel
		};

		if (navigateToSearchResult is not null)
			_view.ResultInvoked += (_, item) => navigateToSearchResult(item.FilePath, item.Item);
	}

	public override UIElement Content => _view;

	public void UpdateResults(FindReplaceEventArgs e)
		=> _viewModel.UpdateResults(e);
}
