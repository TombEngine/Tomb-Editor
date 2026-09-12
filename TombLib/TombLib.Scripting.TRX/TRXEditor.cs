using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Nickelony.IDEKit.Core.Comments;
using Nickelony.IDEKit.IntelliSense.Navigation;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Highlighting;
using Nickelony.IDEKit.Core.Infrastructure;
	using TombLib.Scripting.UI.Bases;
	using TombLib.Scripting.UI.Completion;
	using TombLib.Scripting.UI.Editors;
	using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.TRX;

/// <summary>
/// The TRX (Tomb Raider X) gameflow script editor.
/// </summary>
public sealed partial class TRXEditor : TextEditorBase, INameBasedObjectNavigator
{
	/// <inheritdoc/>
	public override string DefaultFileExtension => ".json5";

	// One-shot pending marker set while the user's Enter is being entered inside a bracket pair.
	// Direct document edits no longer re-enter the language handlers, so only this intent flag is needed.
	private bool _pendingBracketAutospacing;

	private readonly TRXLanguageServices _languageServices;
	private readonly TRXCompletionSessionCoordinator _completionCoordinator;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXEditor"/> class.
	/// </summary>
	/// <param name="engineVersion">The engine version the editor targets.</param>
	/// <param name="languageServices">The language services used by the editor.</param>
	public TRXEditor(Version engineVersion, TRXLanguageServices languageServices) : base(engineVersion)
	{
		ArgumentNullException.ThrowIfNull(languageServices);

		_languageServices = languageServices;
		_completionCoordinator = languageServices.CreateCompletionCoordinator();

		InitializeDefinitionNavigation(TryNavigateDefinition);
		InitializeHover(BuildStandardHoverRequestState, RequestHover);

		InitializeDiagnostics(EngineVersion, _languageServices.ErrorDetector);

		CommentSyntax = new CommentSyntax("//", null, null, StringLiteralStyle.DoubleQuoted);
	}

	// Event handlers

	/// <inheritdoc/>
	protected override void OnLanguageTextEntering(TextCompositionEventArgs e)
	{
		if (TryHandleCtrlSpaceCompletion(
			e,
			() => CompletionController.ApplyDecision(
				_completionCoordinator.GetCtrlSpaceDecision(Document, CaretOffset, CompletionController.ActiveWindow is not null),
				item => new CompletionData(item, TRXCompletionIconProvider.GetImage))))
		{
			return;
		}

		if (e.Text == "\n" && CaretOffset > 0 && CaretOffset < Document.TextLength)
		{
			char? prevChar = GetPreviousChar();
			char? nextChar = GetNextChar();

			if (prevChar.HasValue && nextChar.HasValue)
			{
				char prev = prevChar.Value;
				char next = nextChar.Value;

				if ((prev == '{' && next == '}') || (prev == '[' && next == ']'))
					_pendingBracketAutospacing = true;
			}
		}
	}

	/// <inheritdoc/>
	protected override void OnLanguageTextEntered(TextCompositionEventArgs e)
	{
		if (IntelliSenseEnabled && CompletionEnabled)
			CompletionController.ApplyDecision(
				_completionCoordinator.GetTextEnteredDecision(Document, CaretOffset, e.Text, CompletionController.ActiveWindow is not null),
				item => new CompletionData(item, TRXCompletionIconProvider.GetImage));

		HandleBracketAutospacing();
	}

	// Text manipulation helpers

	private char? GetPreviousChar()
		=> CaretOffset > 0 ? Document.GetCharAt(CaretOffset - 1) : null;

	private char? GetNextChar()
		=> CaretOffset < Document.TextLength ? Document.GetCharAt(CaretOffset) : null;

	private void HandleBracketAutospacing()
	{
		if (!_pendingBracketAutospacing)
			return;

		_pendingBracketAutospacing = false;
		InsertText(CaretOffset, Environment.NewLine + GetIndentationUnit());
	}

	private string GetIndentationUnit()
	{
		if (!Options.ConvertTabsToSpaces)
			return "\t";

		int indentationSize = Options.IndentationSize > 0 ? Options.IndentationSize : 4;
		return new string(' ', indentationSize);
	}

	// Public methods

	/// <inheritdoc/>
	public override void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
	{
		EnsureNotDisposed();

		if (configuration is not TRXEditorConfiguration config)
			return;

		SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme, _languageServices.SchemaService);

		Background = ScriptingColorParser.CreateBrush(config.ColorScheme.Background, ScriptingColorParser.DefaultBackgroundColor);
		Foreground = ScriptingColorParser.CreateBrush(config.ColorScheme.Foreground, ScriptingColorParser.DefaultForegroundColor);

		BracesClosingString = config.AutoAddCommas ? "}," : "}";
		BracketsClosingString = config.AutoAddCommas ? "]," : "]";

		base.UpdateSettings(configuration);
	}

	private Task<bool> TryNavigateDefinition(int offset, CancellationToken cancellationToken)
	{
		return SynchronousRequestAdapter.Adapt(
			() => TryGoToDefinition(_languageServices.DefinitionProvider, _languageServices.HoverProvider, offset),
			cancellationToken);
	}

	/// <inheritdoc/>
	public void GoToObject(string objectName, TextDefinitionDiscriminator? identifyingObject = null)
		=> GoToDefinition(_languageServices.DefinitionProvider, objectName, identifyingObject);
}
