using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Scripting.UI.Presentation;

public sealed partial class TextReferencesResultsViewModel : ObservableObject
{
	private readonly TextReferencesPresentation _presentation;
	private readonly ObservableCollection<TextReferenceGroup> _groups = [];

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasStatusText))]
	private string _statusText = string.Empty;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextReferencesResultsViewModel"/> class.
	/// </summary>
	/// <param name="presentation">The localized text used by the references presentation.</param>
	public TextReferencesResultsViewModel(TextReferencesPresentation presentation)
	{
		ArgumentNullException.ThrowIfNull(presentation);
		_presentation = presentation;
	}

	/// <summary>
	/// Gets the reference groups currently shown.
	/// </summary>
	public ObservableCollection<TextReferenceGroup> Groups => _groups;

	/// <summary>
	/// Gets whether a non-empty status text is currently shown.
	/// </summary>
	public bool HasStatusText => !string.IsNullOrWhiteSpace(StatusText);

	/// <summary>
	/// Gets whether any reference results are currently shown.
	/// </summary>
	public bool HasResults => _groups.Count > 0;

	/// <summary>
	/// Shows the empty state for when no document is active.
	/// </summary>
	public void ShowNoActiveDocument()
		=> ReplaceGroups([], _presentation.NoActiveDocumentText);

	/// <summary>
	/// Shows the empty state for when references are unsupported by the active editor.
	/// </summary>
	public void ShowUnsupported()
		=> ReplaceGroups([], _presentation.UnsupportedText);

	/// <summary>
	/// Shows the loading state.
	/// </summary>
	public void ShowLoading()
		=> ReplaceGroups([], _presentation.LoadingText);

	/// <summary>
	/// Shows the given reference groups, or the empty state when there are no results.
	/// </summary>
	/// <param name="groups">The reference groups to show.</param>
	public void ShowReferences(IReadOnlyList<TextReferenceGroup> groups)
		=> ReplaceGroups(groups, groups.Count == 0 ? _presentation.EmptyText : string.Empty);

	private void ReplaceGroups(IReadOnlyList<TextReferenceGroup> groups, string statusText)
	{
		_groups.Clear();

		foreach (TextReferenceGroup group in groups)
			_groups.Add(group);

		StatusText = statusText;
		OnPropertyChanged(nameof(HasResults));
	}
}
