using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense;
using Nickelony.IDEKit.IntelliSense.Completion;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TombLib.Scripting.UI.Rendering;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Completion;

/// <summary>
/// Describes the text and caret offset to insert when a completion item is applied.
/// </summary>
public readonly record struct CompletionDataInsertionResult(string Text, int? CaretOffset);

/// <summary>
/// Implements AvalonEdit's completion-item UI contract for the shared scripting completion model.
/// </summary>
public sealed class CompletionData : ICompletionData, INotifyPropertyChanged
{
	private const double DescriptionMaxWidth = ToolTipDefaults.PopupMaxWidth;
	private const double DescriptionTextMaxWidth = ToolTipDefaults.TextMaxWidth;

	private static readonly SolidColorBrush DescriptionBorderBrush = TextEditorColorPalette.ToolTipBorder;
	private static readonly SolidColorBrush DescriptionBackgroundBrush = TextEditorColorPalette.ToolTipBackground;
	private static readonly SolidColorBrush DescriptionForegroundBrush = TextEditorColorPalette.ToolTipForeground;
	private static readonly object NoDescriptionSentinel = new();

	private readonly object _resolveSync = new();
	private readonly Func<TextCompletionItem, ImageSource?>? _getImage;
	private readonly Func<TextCompletionItem, bool>? _canApplyItem;
	private readonly Func<TextArea, int, string, int?, CompletionDataInsertionResult>? _normalizeInsertion;

	private TextCompletionItem _item;
	private string? _displayDetail;
	private object? _cachedDescription;
	private Task<TextCompletionItem>? _resolveTask;

	/// <summary>
	/// Initializes a completion item with the given display text.
	/// </summary>
	public CompletionData(string text)
		: this(new TextCompletionItem(text))
	{ }

	/// <summary>
	/// Initializes a completion item with the given display text, insert text and description.
	/// </summary>
	public CompletionData(string text, string insertText, string description = "")
		: this(new TextCompletionItem(text) { InsertText = insertText, Documentation = description })
	{ }

	/// <summary>
	/// Initializes a completion item from a <see cref="TextCompletionItem"/>, with optional hooks for
	/// icon lookup, apply gating and insertion normalization.
	/// </summary>
	public CompletionData(
		TextCompletionItem item,
		Func<TextCompletionItem, ImageSource?>? getImage = null,
		Func<TextCompletionItem, bool>? canApplyItem = null,
		Func<TextArea, int, string, int?, CompletionDataInsertionResult>? normalizeInsertion = null)
	{
		ArgumentNullException.ThrowIfNull(item);
		_item = item;
		_getImage = getImage;
		_canApplyItem = canApplyItem;
		_normalizeInsertion = normalizeInsertion;
		_displayDetail = FlattenSingleLineText(_item.Detail);
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Gets the item's icon. The image provider is optional and may return null; the null-forgiving
	/// operator keeps the non-nullable AvalonEdit <see cref="ICompletionData.Image"/> contract while
	/// the WPF binding renders null as no icon.
	/// </summary>
	public ImageSource Image => _getImage?.Invoke(_item)!;

	/// <inheritdoc/>
	public string Text => _item.FilterText;

	/// <inheritdoc/>
	public string DisplayText => _item.Label;

	/// <summary>
	/// Gets the single-line detail text shown beside the label, or null when there is none.
	/// </summary>
	public string? DisplayDetail => _displayDetail;

	/// <summary>
	/// Gets the visibility of the detail text in the completion row.
	/// </summary>
	public Visibility DetailVisibility => string.IsNullOrEmpty(_displayDetail) ? Visibility.Collapsed : Visibility.Visible;

	/// <inheritdoc/>
	public object Content => DisplayText;

	/// <inheritdoc/>
	public object? Description
	{
		get
		{
			_cachedDescription ??= BuildDescriptionContent() ?? NoDescriptionSentinel;
			return ReferenceEquals(_cachedDescription, NoDescriptionSentinel) ? null : _cachedDescription;
		}
	}

	/// <inheritdoc/>
	public double Priority => _item.Priority;

	/// <summary>
	/// Gets whether the item supports asynchronous resolution of its description.
	/// </summary>
	public bool CanResolve => _item.CanResolve;

	/// <summary>
	/// Applies the completion item to the document at the given completion segment, honoring any
	/// configured apply gate and insertion normalization.
	/// </summary>
	public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
	{
		ArgumentNullException.ThrowIfNull(textArea);
		ArgumentNullException.ThrowIfNull(completionSegment);
		ArgumentNullException.ThrowIfNull(insertionRequestEventArgs);

		if (!CanApplyCompletionItem(_item, _canApplyItem))
			return;

		TextDocument document = textArea.Document;

		// The edit payload supersedes the plain insertion text (LSP text-edit semantics), so its
		// replacement text is what gets committed when the item carries one.
		string insertText = _item.TextEdit?.NewText ?? _item.InsertText;
		int? insertCaretOffset = null;

		if (_item.InsertTextFormat == TextCompletionInsertTextFormat.Snippet)
		{
			TextSnippetExpansion expansion = TextSnippetExpander.Expand(insertText);
			insertText = expansion.Text;

			// Land the caret after the final tabstop's text when the snippet carries one; tab
			// navigation across the remaining placeholders stays host behavior.
			foreach (TextSnippetPlaceholder placeholder in expansion.Placeholders)
			{
				if (placeholder.Index == 0)
				{
					insertCaretOffset = placeholder.Range.EndOffset;
					break;
				}
			}
		}

		(int replacementOffset, int replacementLength) = ResolveCompletionSegment(
			document,
			completionSegment.Offset,
			completionSegment.Length,
			_item.TextEdit,
			ShouldUseReplaceRange(textArea));

		if (ContainsLineBreak(insertText) && _normalizeInsertion is not null)
		{
			CompletionDataInsertionResult normalizedInsertion = _normalizeInsertion(textArea, replacementOffset, insertText, insertCaretOffset);
			insertText = normalizedInsertion.Text;
			insertCaretOffset = normalizedInsertion.CaretOffset;
		}

		document.Replace(replacementOffset, replacementLength, insertText);
		textArea.Caret.Offset = replacementOffset + (insertCaretOffset ?? insertText.Length);
	}

	/// <summary>
	/// Resolves and returns the item's description, or the cached description when the item does
	/// not support asynchronous resolution.
	/// </summary>
	public async Task<object?> GetDescriptionAsync(CancellationToken cancellationToken = default)
	{
		if (!CanResolve)
			return Description;

		Task<TextCompletionItem> resolveTask;

		lock (_resolveSync)
		{
			if (_resolveTask is null || _resolveTask.IsFaulted || _resolveTask.IsCanceled)
				_resolveTask = _item.ResolveAsync(cancellationToken);

			resolveTask = _resolveTask;
		}

		try
		{
			TextCompletionItem resolvedItem = cancellationToken.CanBeCanceled
				? await resolveTask.WaitAsync(cancellationToken).ConfigureAwait(true)
				: await resolveTask.ConfigureAwait(true);

			ApplyResolvedItem(resolvedItem);
			return Description;
		}
		catch
		{
			// Both cancellation and other failures reset the resolve task so the next request can
			// retry; the exception is rethrown for the caller to handle.
			lock (_resolveSync)
			{
				if (ReferenceEquals(_resolveTask, resolveTask))
					_resolveTask = null;
			}

			throw;
		}
	}

	/// <summary>
	/// Rebases the item's commit context onto the current document version and session generation,
	/// dropping any in-flight resolve task so the next request re-resolves.
	/// </summary>
	public void RebaseForCurrentDocument(int requestDocumentVersion, int requestGeneration)
	{
		_item = _item.WithRequestContext(requestDocumentVersion, requestGeneration).WithoutTextEdit();

		lock (_resolveSync)
			_resolveTask = null;
	}

	private static string? FlattenSingleLineText(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		string[] lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
		return lines.Length == 0 ? null : string.Join(" ", lines).Trim();
	}

	private static bool CanApplyCompletionItem(TextCompletionItem item, Func<TextCompletionItem, bool>? canApplyItem)
		=> canApplyItem?.Invoke(item) ?? true;

	private static bool ContainsLineBreak(string text)
		=> text.IndexOfAny(['\r', '\n']) >= 0;

	private static (int Offset, int Length) ResolveCompletionSegment(
		TextDocument document,
		int fallbackOffset,
		int fallbackLength,
		TextCompletionTextEdit? textEdit,
		bool useReplaceRange)
	{
		ArgumentNullException.ThrowIfNull(document);

		var fallbackSegment = (fallbackOffset, fallbackLength);

		if (textEdit is not TextCompletionTextEdit completionTextEdit)
			return fallbackSegment;

		TextRange range = useReplaceRange
			? completionTextEdit.ReplacementRange
			: completionTextEdit.InsertRange;

		return TryCreateCompletionSegment(document, range, out (int Offset, int Length) replacementSegment)
			? replacementSegment
			: fallbackSegment;
	}

	private static bool TryCreateCompletionSegment(TextDocument document,
		TextRange range,
		out (int Offset, int Length) segment)
	{
		segment = default;

		// Completion ranges are absolute document offsets; they must fit within the document.
		if (range.EndOffset > document.TextLength)
			return false;

		segment = (range.Offset, range.Length);
		return true;
	}

	private static bool ShouldUseReplaceRange(TextArea textArea) => textArea.OverstrikeMode;

	private Border? BuildDescriptionContent()
	{
		bool hasDetail = !string.IsNullOrWhiteSpace(_item.Detail);
		string? descriptionText = BacktickFenceTextNormalizer.NormalizeForPlainText(_item.Documentation, Environment.NewLine);
		bool hasDescription = descriptionText is not null;

		if (!hasDetail && !hasDescription)
			return null;

		var panel = new StackPanel
		{
			MaxWidth = DescriptionMaxWidth
		};

		if (hasDetail)
		{
			panel.Children.Add(new TextBlock
			{
				Text = _item.Detail,
				Foreground = DescriptionForegroundBrush,
				TextWrapping = TextWrapping.Wrap,
				MaxWidth = DescriptionTextMaxWidth,
				FontWeight = FontWeights.SemiBold,
				Margin = hasDescription
					? new Thickness(0.0, 0.0, 0.0, 8.0)
					: new Thickness(0.0)
			});
		}

		if (descriptionText is string displayDescription)
		{
			panel.Children.Add(_item.DocumentationKind == TextMarkupKind.Markdown
				? MarkdownToolTipRenderer.CreateContent(displayDescription, DescriptionForegroundBrush, DescriptionBackgroundBrush, false)
				: MarkdownToolTipRenderer.CreatePlainTextContent(displayDescription, DescriptionForegroundBrush, false));
		}

		return new Border
		{
			Background = DescriptionBackgroundBrush,
			BorderBrush = DescriptionBorderBrush,
			BorderThickness = new Thickness(1.0),
			Padding = new Thickness(8.0, 6.0, 8.0, 6.0),
			Child = panel,
			MaxWidth = DescriptionMaxWidth
		};
	}

	private void ApplyResolvedItem(TextCompletionItem resolvedItem)
	{
		if (resolvedItem is null || ReferenceEquals(resolvedItem, _item))
			return;

		_item = resolvedItem;
		_displayDetail = FlattenSingleLineText(_item.Detail);
		_cachedDescription = null;

		OnPropertyChanged(nameof(Image));
		OnPropertyChanged(nameof(DisplayDetail));
		OnPropertyChanged(nameof(DetailVisibility));
		OnPropertyChanged(nameof(Description));
	}

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
