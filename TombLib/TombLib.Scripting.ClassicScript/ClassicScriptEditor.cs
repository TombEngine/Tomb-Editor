using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Rendering;
using Nickelony.IDEKit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Signatures;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Nickelony.IDEKit.Core.Comments;
using Nickelony.IDEKit.Core.Formatting;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;
using TombLib.Scripting.ClassicScript.Cleaning;
using TombLib.Scripting.ClassicScript.Completion;
using TombLib.Scripting.ClassicScript.Highlighting;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Bases;
using Nickelony.IDEKit.AvalonEdit.Documents;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.ClassicScript;

/// <summary>
/// The ClassicScript editor.
/// </summary>
public sealed partial class ClassicScriptEditor : TextEditorBase, ISyntaxPreviewSource, INameBasedObjectNavigator
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly ClassicScriptLanguageServices _languageServices;

	/// <inheritdoc/>
	public override string DefaultFileExtension => ".txt";

	// Properties

	/// <summary>
	/// Gets the document formatter used by the editor.
	/// </summary>
	public ClassicScriptDocumentFormatter Formatter { get; } = new();

	/// <inheritdoc/>
	protected override ITextDocumentFormatter DocumentFormatter => Formatter;

	private bool _showSectionSeparators;

	/// <summary>
	/// Gets or sets whether section separator lines are rendered.
	/// </summary>
	public bool ShowSectionSeparators
	{
		get => _showSectionSeparators;
		set
		{
			_showSectionSeparators = value;

			if (_showSectionSeparators)
			{
				if (!TextArea.TextView.BackgroundRenderers.Contains(_sectionRenderer))
					TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
			}
			else
			{
				if (TextArea.TextView.BackgroundRenderers.Contains(_sectionRenderer))
					TextArea.TextView.BackgroundRenderers.Remove(_sectionRenderer);
			}

			TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		}
	}

	/// <summary>
	/// Gets or sets whether completion is suppressed in the editor.
	/// </summary>
	public bool SuppressCompletion { get; set; }

	// Fields

	private readonly ClassicScriptCompletionSessionCoordinator _completionCoordinator;

	private readonly SectionRenderer _sectionRenderer;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptEditor"/> class.
	/// </summary>
	/// <param name="engineVersion">The engine version the editor targets.</param>
	/// <param name="languageServices">The language services used by the editor.</param>
	public ClassicScriptEditor(Version engineVersion, ClassicScriptLanguageServices languageServices) : base(engineVersion)
	{
		_languageServices = languageServices;
		_completionCoordinator = languageServices.CreateCompletionCoordinator();

		InitializeDefinitionNavigation(TryNavigateDefinition);
		InitializeHover(BuildHoverRequestState, RequestHover);
		InitializeDiagnostics(_languageServices.ErrorDetector);

		_sectionRenderer = InitializeRenderers();

		CommentSyntax = new CommentSyntax(";", null, StringLiteralStyle.None);
	}

	private SectionRenderer InitializeRenderers()
	{
		var sectionRenderer = new SectionRenderer(this, _languageServices.LineService);

		if (ShowSectionSeparators)
			TextArea.TextView.BackgroundRenderers.Add(sectionRenderer);

		return sectionRenderer;
	}

	// Events

	/// <inheritdoc/>
	protected override void OnLanguageTextEntering(TextCompositionEventArgs e)
	{
		if (!SuppressCompletion)
			TryHandleCtrlSpaceCompletion(e, QueueCtrlSpaceCompletionDecision);
	}

	/// <inheritdoc/>
	protected override void OnLanguageTextEntered(TextCompositionEventArgs e)
	{
		if (IntelliSenseEnabled && CompletionEnabled && !SuppressCompletion)
			QueueTextEnteredCompletionDecision(e.Text);
	}

	// Completion

	private void QueueCtrlSpaceCompletionDecision()
		=> QueueCompletionDecision(_completionCoordinator.GetCtrlSpaceDecisionAsync(Text, FilePath, CaretOffset, CompletionController.ActiveWindow is not null));

	private void QueueTextEnteredCompletionDecision(string inputText)
		=> QueueCompletionDecision(_completionCoordinator.GetTextEnteredDecisionAsync(Text, FilePath, CaretOffset, inputText, CompletionController.ActiveWindow is not null));

	private void QueueCompletionDecision(Task<TextCompletionSessionDecision> decisionTask)
	{
		string requestText = Text;
		int requestCaretOffset = CaretOffset;
		long requestToken = CompletionController.Requests.BeginRequest();
		_ = ApplyCompletionDecisionAsync(decisionTask, requestText, requestCaretOffset, requestToken);
	}

	private async Task ApplyCompletionDecisionAsync(
		Task<TextCompletionSessionDecision> decisionTask,
		string requestText,
		int requestCaretOffset,
		long requestToken)
	{
		TextCompletionSessionDecision decision;

		try
		{
			decision = await decisionTask;
		}
		catch (OperationCanceledException)
		{
			return;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to resolve the ClassicScript completion decision.");
			return;
		}

		if (!CompletionController.Requests.IsCurrent(requestToken)
			|| CompletionController.ActiveWindow is not null
			|| decision.Items is null
			|| !decision.StartOffset.HasValue
			|| !decision.EndOffset.HasValue
			|| !string.Equals(Text, requestText, StringComparison.Ordinal)
			|| CaretOffset != requestCaretOffset)
		{
			return;
		}

		CompletionController.ApplyDecision(decision);
	}

	/// <inheritdoc/>
	protected override ICompletionData CreateCompletionData(TextCompletionItem item)
		=> new CompletionData(item, ClassicScriptCompletionIconProvider.GetImage);

	// Navigation

	private Task<bool> TryNavigateDefinition(int offset, CancellationToken cancellationToken)
	{
		// The definition provider is synchronous; the token is honored before the request starts.
		cancellationToken.ThrowIfCancellationRequested();

		return Task.FromResult(
			TryGoToDefinition(_languageServices.DefinitionProvider, _languageServices.HoverProvider, offset));
	}

	/// <summary>
	/// Inserts the next free trigger index at the caret.
	/// </summary>
	public void InputFreeIndex()
	{
		ITextSnapshot source = new TextDocumentSnapshot(Document);
		int nextFreeIndex = _languageServices.IndexService.GetNextFreeIndex(source, CaretOffset);

		if (nextFreeIndex == -1)
			return;

		InsertText(CaretOffset, nextFreeIndex.ToString());
	}

	/// <inheritdoc/>
	public override void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
	{
		EnsureNotDisposed();

		if (configuration is not ClassicScriptEditorConfiguration config)
			return;

		SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

		Background = ScriptingColorParser.CreateBrush(config.ColorScheme.Background, ScriptingColorParser.DefaultBackgroundColor);
		Foreground = ScriptingColorParser.CreateBrush(config.ColorScheme.Foreground, ScriptingColorParser.DefaultForegroundColor);

		_sectionRenderer.UpdateSectionColor(config.ColorScheme.Sections.HtmlColor);

		ShowSectionSeparators = config.ShowSectionSeparators;

		Formatter.SpaceBeforeEquals = config.SpaceBeforeEquals;
		Formatter.SpaceAfterEquals = config.SpaceAfterEquals;
		Formatter.SpaceBeforeComma = config.SpaceBeforeComma;
		Formatter.SpaceAfterComma = config.SpaceAfterComma;
		Formatter.CollapseMultipleSpaces = config.CollapseMultipleSpaces;

		base.UpdateSettings(configuration);
	}

	/// <inheritdoc/>
	public void GoToObject(string objectName, TextDefinitionDiscriminator? identifyingObject = null)
		=> GoToDefinition(_languageServices.DefinitionProvider, objectName, identifyingObject);

	/// <summary>
	/// Gets the syntax preview at the caret.
	/// </summary>
	/// <returns>The signature help info for the current syntax, or <c>null</c> when none is available.</returns>
	public TextSignatureHelp? GetSyntaxPreview()
		=> _languageServices.SignatureHelpProvider.GetSignatureHelp(new TextSignatureHelpRequest(Document.Text, CaretOffset));

	/// <summary>
	/// Appends a new plugin entry to the Options section.
	/// </summary>
	/// <param name="pluginString">The plugin definition string to add.</param>
	/// <returns><c>true</c> when the plugin entry was added; otherwise, <c>false</c>.</returns>
	public bool TryAddNewPluginEntry(string pluginString)
	{
		ITextSnapshot source = new TextDocumentSnapshot(Document);
		int? optionsSectionLineNumber = _languageServices.CommandService.FindDocumentLineOfSection(source, "Options");

		if (optionsSectionLineNumber is null)
			return false;

		if (_languageServices.CommandService.IsPluginDefined(source, pluginString))
			return false;

		ITextLine optionsLine = source.GetLineByNumber(optionsSectionLineNumber.Value);
		int nextFreePluginIndex = _languageServices.IndexService.GetNextFreeIndex(source, optionsLine.Offset, "Plugin");
		int? lastSectionLineNumber = _languageServices.CommandService.GetLastLineOfCurrentSection(source, optionsLine.Offset);

		if (lastSectionLineNumber is null)
			return false;

		ITextLine lastSectionLine = source.GetLineByNumber(lastSectionLineNumber.Value);
		int insertOffset = lastSectionLine.Offset + lastSectionLine.Length;

		InsertText(insertOffset, $"{Environment.NewLine}Plugin= {nextFreePluginIndex}, {pluginString}, IGNORE");

		return true;
	}
}
