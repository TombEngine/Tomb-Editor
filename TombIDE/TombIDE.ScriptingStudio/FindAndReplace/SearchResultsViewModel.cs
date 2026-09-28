#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace TombIDE.ScriptingStudio.FindAndReplace;

public sealed partial class SearchResultsViewModel : ObservableObject
{
	public ObservableCollection<SearchResultsSourceViewModel> Sources { get; } = [];

	public void UpdateResults(FindReplaceEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);

		Sources.Clear();

		foreach (FindReplaceSource source in e.SourceCollection)
		{
			var sourceViewModel = new SearchResultsSourceViewModel(source.Name, source.Count);

			foreach (FindReplaceItem item in source)
			{
				sourceViewModel.Items.Add(new SearchResultsItemViewModel(
					source.Name,
					item,
					string.Format(TombIDE.Shared.Strings.Default.SingleMatchNodeText, item.LineNumber, item.LineText)));
			}

			Sources.Add(sourceViewModel);
		}
	}
}

public sealed class SearchResultsSourceViewModel
{
	public SearchResultsSourceViewModel(string filePath, int count)
	{
		FilePath = filePath ?? string.Empty;
		Count = count;
		Title = string.Format(TombIDE.Shared.Strings.Default.MatchSourceNodeText, FilePath, Count);
		Items = [];
	}

	public string FilePath { get; }

	public int Count { get; }

	public string Title { get; }

	public ObservableCollection<SearchResultsItemViewModel> Items { get; }
}

public sealed class SearchResultsItemViewModel
{
	public SearchResultsItemViewModel(string filePath, FindReplaceItem item, string title)
	{
		FilePath = filePath ?? string.Empty;
		Item = item ?? throw new ArgumentNullException(nameof(item));
		Title = title ?? string.Empty;
	}

	public string FilePath { get; }

	public FindReplaceItem Item { get; }

	public string Title { get; }
}
