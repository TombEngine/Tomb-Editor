using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Completion;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Extensions;

namespace TombLib.Scripting.ClassicScript.Completion;

/// <summary>
/// Coordinates completion session decisions for the ClassicScript editor.
/// </summary>
public sealed class ClassicScriptCompletionSessionCoordinator
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly IClassicScriptLineService _lineService;
	private readonly IClassicScriptCommandService _commandService;
	private readonly ITextCompletionProvider _completionProvider;
	private readonly CompletionSessionKernel _kernel = new();

	private int _latestRequestId;

	private static readonly TimeSpan IncludeDiscoveryCacheDuration = TimeSpan.FromSeconds(30);
	private readonly object _includeDiscoverySyncRoot = new();
	private readonly Dictionary<string, IncludeDiscoveryCacheEntry> _includeDiscoveryCache = new();
	private CancellationTokenSource? _includeDiscoveryCancellation;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptCompletionSessionCoordinator"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to analyze document lines.</param>
	/// <param name="commandService">The command service used to resolve command context.</param>
	/// <param name="mnemonicCatalogService">The mnemonic catalog service used by the completion provider.</param>
	public ClassicScriptCompletionSessionCoordinator(
		IClassicScriptLineService lineService,
		IClassicScriptCommandService commandService,
		ClassicScriptMnemonicCatalogService mnemonicCatalogService)
	{
		_lineService = lineService;
		_commandService = commandService;
		_completionProvider = new ClassicScriptCompletionProvider(commandService, mnemonicCatalogService);
	}

	/// <summary>
	/// Gets the decision for a manual Ctrl+Space completion request.
	/// </summary>
	/// <param name="documentText">The current document text.</param>
	/// <param name="filePath">The path of the document, or <c>null</c> when unsaved.</param>
	/// <param name="caretOffset">The caret offset.</param>
	/// <param name="completionWindowIsOpen">Whether a completion window is already open.</param>
	/// <returns>The completion session decision.</returns>
	public Task<TextCompletionSessionDecision> GetCtrlSpaceDecisionAsync(
		string documentText,
		string? filePath,
		int caretOffset,
		bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen)
			return Task.FromResult(TextCompletionSessionDecision.None);

		TextDocument document = CreateDocument(documentText, filePath);
		ITextSnapshot source = new StringTextSnapshot(document.Text, document.FileName);
		string? wholeLineText = _commandService.GetWholeCommandLineText(source, caretOffset);

		return string.IsNullOrEmpty(wholeLineText)
			? ResolveCtrlSpaceFromEmptyLineAsync(document, source, filePath, caretOffset)
			: ResolveCtrlSpaceFromContextAsync(document, source, filePath, caretOffset);
	}

	/// <summary>
	/// Gets the decision for a completion triggered by entered text.
	/// </summary>
	/// <param name="documentText">The current document text.</param>
	/// <param name="filePath">The path of the document, or <c>null</c> when unsaved.</param>
	/// <param name="caretOffset">The caret offset.</param>
	/// <param name="inputText">The entered text.</param>
	/// <param name="completionWindowIsOpen">Whether a completion window is already open.</param>
	/// <returns>The completion session decision.</returns>
	public async Task<TextCompletionSessionDecision> GetTextEnteredDecisionAsync(
		string documentText,
		string? filePath,
		int caretOffset,
		string inputText,
		bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen || caretOffset <= 0)
			return TextCompletionSessionDecision.None;

		TextDocument document = CreateDocument(documentText, filePath);
		ITextSnapshot source = new StringTextSnapshot(document.Text, document.FileName);

		if (EditorCompletionTriggerHelper.IsSingleCharacterLine(document.GetText(document.GetLineByOffset(caretOffset))))
			return GetEmptyLineDecision(source, caretOffset);

		if (inputText == "_" && caretOffset > 1)
			return GetWordDecision(document, caretOffset);

		if (inputText == "\"" && caretOffset > 1)
		{
			TextCompletionSessionDecision afterSpaceDecision = await GetAfterSpaceDecisionAsync(document, source, filePath, caretOffset).ConfigureAwait(false);

			if (HasItems(afterSpaceDecision))
				return afterSpaceDecision;

			return await GetIncludeDecisionAsync(document, source, filePath, caretOffset).ConfigureAwait(false);
		}

		if (caretOffset > 1)
			return await GetAfterSpaceDecisionAsync(document, source, filePath, caretOffset).ConfigureAwait(false);

		return TextCompletionSessionDecision.None;
	}

	private async Task<TextCompletionSessionDecision> ResolveCtrlSpaceFromEmptyLineAsync(TextDocument document, ITextSnapshot source, string? filePath, int caretOffset)
	{
		TextCompletionSessionDecision emptyLineDecision = GetEmptyLineDecision(source, caretOffset);

		if (HasItems(emptyLineDecision))
			return emptyLineDecision;

		TextCompletionSessionDecision includeDecision = await GetIncludeDecisionAsync(document, source, filePath, caretOffset).ConfigureAwait(false);

		return HasItems(includeDecision)
			? includeDecision
			: GetWordDecision(document, caretOffset);
	}

	private async Task<TextCompletionSessionDecision> ResolveCtrlSpaceFromContextAsync(TextDocument document, ITextSnapshot source, string? filePath, int caretOffset)
	{
		TextCompletionSessionDecision includeDecision = await GetIncludeDecisionAsync(document, source, filePath, caretOffset).ConfigureAwait(false);
		TextCompletionSessionDecision wordDecision = HasItems(includeDecision)
			? TextCompletionSessionDecision.None
			: GetWordDecision(document, caretOffset);

		TextCompletionSessionDecision contextualDecision = await GetContextualDecisionAsync(document, caretOffset).ConfigureAwait(false);

		if (HasItems(contextualDecision))
			return contextualDecision;

		return HasItems(includeDecision)
			? includeDecision
			: wordDecision;
	}

	private async Task<TextCompletionSessionDecision> GetAfterSpaceDecisionAsync(TextDocument document, ITextSnapshot source, string? filePath, int caretOffset)
	{
		if (caretOffset >= 2 && source.GetCharAt(caretOffset - 2) is '=' or ',' or '_' or '+' or '-' or '*' or '/')
			return await GetContextualDecisionAsync(document, caretOffset, insertAtCaret: true).ConfigureAwait(false);

		return await GetIncludeDecisionAsync(document, source, filePath, caretOffset).ConfigureAwait(false);
	}

	private async Task<TextCompletionSessionDecision> GetIncludeDecisionAsync(TextDocument document, ITextSnapshot source, string? filePath, int caretOffset)
	{
		ITextLine currentLine = source.GetLineByOffset(caretOffset);
		string lineText = source.GetText(currentLine.Offset, currentLine.Length);

		if (!_lineService.IsValidIncludeLine(lineText))
			return TextCompletionSessionDecision.None;

		int? startOffset = null;
		int? endOffset = null;

		if (caretOffset > 0 && source.GetCharAt(caretOffset - 1) == '"')
		{
			startOffset = caretOffset - 1;
		}
		else if (caretOffset > 0 && source.GetCharAt(caretOffset - 1) != ' ')
		{
			int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

			if (wordStartOffset >= 0)
			{
				string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);

				if (!word.StartsWith('#'))
				{
					startOffset = wordStartOffset;

					if (wordStartOffset - 1 > 0 && source.GetCharAt(wordStartOffset - 1) == '"')
						startOffset--;
				}
			}
		}

		if (caretOffset < source.TextLength && source.GetCharAt(caretOffset) == '"')
			endOffset = caretOffset + 1;

		if (string.IsNullOrWhiteSpace(filePath))
			return TextCompletionSessionDecision.None;

		string? directoryPath = Path.GetDirectoryName(filePath);

		if (string.IsNullOrWhiteSpace(directoryPath))
			return TextCompletionSessionDecision.None;

		// Recursive directory enumeration is I/O-bound and can be slow for large trees, so it
		// must not run on the editor input path. Results are cached per document and invalidated
		// after a short interval or when a newer request supersedes an in-flight enumeration.
		IReadOnlyList<TextCompletionItem> completionItems;

		try
		{
			completionItems = await GetIncludeCompletionItemsAsync(directoryPath, filePath).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			return TextCompletionSessionDecision.None;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to enumerate ClassicScript include files; suppressing the include completion session.");
			return TextCompletionSessionDecision.None;
		}

		return completionItems.Count == 0
			? TextCompletionSessionDecision.None
			: CreateOpenDecision(completionItems, startOffset, endOffset);
	}

	private Task<IReadOnlyList<TextCompletionItem>> GetIncludeCompletionItemsAsync(string directoryPath, string filePath)
	{
		string cacheKey = directoryPath + "|" + filePath;
		CancellationTokenSource? cancellationSource;

		lock (_includeDiscoverySyncRoot)
		{
			if (_includeDiscoveryCache.TryGetValue(cacheKey, out IncludeDiscoveryCacheEntry cachedEntry)
				&& DateTime.UtcNow - cachedEntry.EnumeratedAt < IncludeDiscoveryCacheDuration)
			{
				return Task.FromResult(cachedEntry.Items);
			}

			_includeDiscoveryCancellation?.Cancel();
			_includeDiscoveryCancellation?.Dispose();
			cancellationSource = _includeDiscoveryCancellation = new CancellationTokenSource();
		}

		CancellationToken cancellationToken = cancellationSource.Token;

		return Task.Run(
			() =>
			{
				List<TextCompletionItem> items = EnumerateIncludeCompletionItems(directoryPath, filePath, cancellationToken);

				lock (_includeDiscoverySyncRoot)
					_includeDiscoveryCache[cacheKey] = new IncludeDiscoveryCacheEntry(DateTime.UtcNow, items);

				return (IReadOnlyList<TextCompletionItem>)items;
			},
			cancellationToken);
	}

	private static List<TextCompletionItem> EnumerateIncludeCompletionItems(string directoryPath, string filePath, CancellationToken cancellationToken)
	{
		var completionItems = new List<TextCompletionItem>();
		var fileDirectory = new DirectoryInfo(directoryPath);

		foreach (FileInfo file in fileDirectory.GetFiles("*.txt", SearchOption.AllDirectories))
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (file.FullName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
				continue;

			string pathPart = file.FullName.Replace(directoryPath, string.Empty).TrimStart('\\');
			completionItems.Add(new TextCompletionItem($"\"{pathPart}\""));
		}

		return completionItems;
	}

	private readonly record struct IncludeDiscoveryCacheEntry(DateTime EnumeratedAt, IReadOnlyList<TextCompletionItem> Items);

	private TextCompletionSessionDecision GetWordDecision(TextDocument document, int caretOffset)
	{
		if (caretOffset <= 0)
			return TextCompletionSessionDecision.None;

		int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset - 1, LogicalDirection.Backward, CaretPositioningMode.WordStart);

		if (wordStartOffset < 0)
			return TextCompletionSessionDecision.None;

		string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);
		var wordInfo = new CompletionWordInfo(word, new Nickelony.IDEKit.Core.Text.TextRange(wordStartOffset, caretOffset - wordStartOffset));

		return _kernel.GetDecision(
			new StringTextSnapshot(document.Text),
			caretOffset,
			_completionProvider,
			TextCompletionTrigger.Word,
			wordInfo);
	}

	private TextCompletionSessionDecision GetEmptyLineDecision(ITextSnapshot source, int caretOffset)
	{
		string? currentSection = _commandService.GetCurrentSectionName(source, caretOffset);

		if (currentSection is not null && currentSection.IgnoreCaseEqualsAny("Strings", "PSXStrings", "PCStrings", "ExtraNG"))
			return TextCompletionSessionDecision.None;

		// The empty-line replacement range starts at the line start; an empty filter word keeps the
		// full command and section catalog offered by the provider.
		int lineStartOffset = source.GetLineByOffset(caretOffset).Offset;
		var wordInfo = new CompletionWordInfo(string.Empty, new Nickelony.IDEKit.Core.Text.TextRange(lineStartOffset, caretOffset - lineStartOffset));

		return _kernel.GetDecision(source, caretOffset, _completionProvider, TextCompletionTrigger.EmptyLine, wordInfo);
	}

	private async Task<TextCompletionSessionDecision> GetContextualDecisionAsync(TextDocument document, int caretOffset, bool insertAtCaret = false)
	{
		int requestId = Interlocked.Increment(ref _latestRequestId);
		var source = new StringTextSnapshot(document.Text);

		int wordStartOffset = insertAtCaret
			? caretOffset
			: TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

		if (wordStartOffset < 0)
			return TextCompletionSessionDecision.None;

		string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);
		int? startOffset = null;

		if (insertAtCaret)
			startOffset = caretOffset;
		else if (!word.StartsWithAny('=', ',', '+', '-', '*', '/'))
			startOffset = wordStartOffset;

		if (!startOffset.HasValue)
			return TextCompletionSessionDecision.None;

		// The provider returns the fully context-filtered candidate set (ENABLED/DISABLED or
		// prefix-matched mnemonics), so the kernel is told not to filter by word; the replacement
		// range is still supplied here so the kernel owns the open/no-op decision with it.
		var wordInfo = new CompletionWordInfo(string.Empty, new Nickelony.IDEKit.Core.Text.TextRange(startOffset.Value, caretOffset - startOffset.Value));

		try
		{
			// The completion provider scans large catalogs to build items, so the work is CPU-bound.
			// The provider contract (ITextCompletionProvider) explicitly permits background execution.
			TextCompletionSessionDecision decision = await _kernel.GetDecisionAsync(
				source,
				caretOffset,
				_completionProvider,
				TextCompletionTrigger.Contextual,
				wordInfo).ConfigureAwait(false);

			return requestId == Volatile.Read(ref _latestRequestId)
				? decision
				: TextCompletionSessionDecision.None;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to retrieve ClassicScript completion items; suppressing the completion session.");
			return TextCompletionSessionDecision.None;
		}
	}

	private static TextCompletionSessionDecision CreateOpenDecision(IReadOnlyList<TextCompletionItem> items, int? startOffset, int? endOffset)
	{
		if (items.Count == 0 || !startOffset.HasValue || !endOffset.HasValue)
			return TextCompletionSessionDecision.None;

		return TextCompletionSessionDecision.Open(items, startOffset.Value, endOffset.Value);
	}

	private static bool HasItems(TextCompletionSessionDecision decision)
		=> decision.Items is not null && decision.Items.Count > 0;

	private static TextDocument CreateDocument(string documentText, string? filePath) => new(documentText)
	{
		FileName = filePath ?? string.Empty
	};
}
