using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Nickelony.IDEKit.AvalonEdit.Documents;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Scripting.UI.Presentation;

public sealed partial class TextDiagnosticsViewModel : ObservableObject
{
	private readonly TextDiagnosticsPresentation _presentation;
	private readonly ObservableCollection<TextDiagnosticListItem> _diagnostics = [];

	[ObservableProperty]
	private TextDiagnosticListItem? _selectedItem;

	[ObservableProperty]
	private bool _showErrors = true;

	[ObservableProperty]
	private bool _showWarnings = true;

	[ObservableProperty]
	private bool _showMessages = true;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasStatusText))]
	private string _statusText = string.Empty;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextDiagnosticsViewModel"/> class.
	/// </summary>
	/// <param name="presentation">The localized text used by the diagnostics presentation.</param>
	public TextDiagnosticsViewModel(TextDiagnosticsPresentation presentation)
	{
		ArgumentNullException.ThrowIfNull(presentation);
		_presentation = presentation;
		Diagnostics = CollectionViewSource.GetDefaultView(_diagnostics);
		Diagnostics.Filter = FilterDiagnostic;

		ShowNoActiveDocument();
	}

	/// <summary>
	/// Gets the filtered view of the current diagnostics.
	/// </summary>
	public ICollectionView Diagnostics { get; }

	/// <summary>
	/// Gets whether a non-empty status text is currently shown.
	/// </summary>
	public bool HasStatusText => !string.IsNullOrWhiteSpace(StatusText);

	/// <summary>
	/// Gets the localized errors label.
	/// </summary>
	public string ErrorsLabel => _presentation.ErrorsLabel;

	/// <summary>
	/// Gets the localized warnings label.
	/// </summary>
	public string WarningsLabel => _presentation.WarningsLabel;

	/// <summary>
	/// Gets the localized messages label.
	/// </summary>
	public string MessagesLabel => _presentation.MessagesLabel;

	/// <summary>
	/// Gets the localized severity column header.
	/// </summary>
	public string SeverityHeader => _presentation.SeverityHeader;

	/// <summary>
	/// Gets the localized line column header.
	/// </summary>
	public string LineHeader => _presentation.LineHeader;

	/// <summary>
	/// Gets the localized column column header.
	/// </summary>
	public string ColumnHeader => _presentation.ColumnHeader;

	/// <summary>
	/// Gets the localized source column header.
	/// </summary>
	public string SourceHeader => _presentation.SourceHeader;

	/// <summary>
	/// Gets the localized message column header.
	/// </summary>
	public string MessageHeader => _presentation.MessageHeader;

	/// <summary>
	/// Shows the empty state for when no document is active.
	/// </summary>
	public void ShowNoActiveDocument()
		=> ReplaceDiagnostics([], _presentation.NoActiveDocumentText);

	/// <summary>
	/// Shows the empty state for when diagnostics are pending.
	/// </summary>
	public void ShowPending()
		=> ReplaceDiagnostics([], _presentation.PendingText);

	/// <summary>
	/// Shows the diagnostics of the given document.
	/// </summary>
	/// <param name="filePath">The path of the document.</param>
	/// <param name="document">The document the diagnostics belong to.</param>
	/// <param name="diagnostics">The diagnostics to show.</param>
	public void ShowDiagnostics(string filePath, TextDocument document, IReadOnlyList<TextDiagnostic> diagnostics)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(diagnostics);

		TextDiagnosticListItem[] items = new TextDiagnosticListItem[diagnostics.Count];

		for (int i = 0; i < diagnostics.Count; i++)
			items[i] = CreateItem(filePath, document, diagnostics[i]);

		ReplaceDiagnostics(items, items.Length == 0 ? _presentation.EmptyText : string.Empty);
	}

	partial void OnShowErrorsChanged(bool value)
		=> Diagnostics.Refresh();

	partial void OnShowWarningsChanged(bool value)
		=> Diagnostics.Refresh();

	partial void OnShowMessagesChanged(bool value)
		=> Diagnostics.Refresh();

	private void ReplaceDiagnostics(IReadOnlyList<TextDiagnosticListItem> diagnostics, string statusText)
	{
		_diagnostics.Clear();

		foreach (TextDiagnosticListItem diagnostic in diagnostics)
			_diagnostics.Add(diagnostic);

		SelectedItem = _diagnostics.Count > 0 ? _diagnostics[0] : null;
		StatusText = statusText;
		Diagnostics.Refresh();
	}

	private bool FilterDiagnostic(object item)
	{
		if (item is not TextDiagnosticListItem diagnostic)
			return false;

		return diagnostic.Severity switch
		{
			TextDiagnosticSeverity.Error => ShowErrors,
			TextDiagnosticSeverity.Warning => ShowWarnings,
			_ => ShowMessages
		};
	}

	private static TextDiagnosticListItem CreateItem(string filePath, TextDocument document, TextDiagnostic diagnostic)
	{
		int documentLength = document.TextLength;
		int startOffset = document.ClampOffset(diagnostic.StartOffset);
		int endOffset = Math.Max(startOffset, Math.Min(diagnostic.EndOffset, documentLength));

		DocumentLine line = document.GetLineByOffset(startOffset);
		int lineNumber = line.LineNumber;
		int columnNumber = Math.Max(1, startOffset - line.Offset + 1);

		return new TextDiagnosticListItem(
			filePath,
			diagnostic.Severity,
			diagnostic.Severity.ToString(),
			lineNumber,
			columnNumber,
			diagnostic.Message,
			diagnostic.Source,
			startOffset,
			endOffset);
	}
}
