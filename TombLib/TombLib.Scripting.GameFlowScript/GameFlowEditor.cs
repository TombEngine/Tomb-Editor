using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Highlighting;
using Nickelony.IDEKit.Core.Comments;
using Nickelony.IDEKit.IntelliSense.Navigation;
	using TombLib.Scripting.UI.Bases;
	using TombLib.Scripting.UI.Editors;
	using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.GameFlowScript;

/// <summary>
/// The GameFlow script editor.
/// </summary>
public sealed partial class GameFlowEditor : TextEditorBase, INameBasedObjectNavigator
{
	private readonly GameFlowLanguageServices _languageServices;
	private readonly GameFlowCompletionSessionCoordinator _completionCoordinator;

	/// <inheritdoc/>
	public override string DefaultFileExtension => ".txt";

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowEditor"/> class.
	/// </summary>
	/// <param name="engineVersion">The engine version the editor targets.</param>
	/// <param name="languageServices">The language services used by the editor.</param>
	public GameFlowEditor(Version engineVersion, GameFlowLanguageServices languageServices) : base(engineVersion)
	{
		ArgumentNullException.ThrowIfNull(languageServices);

		_languageServices = languageServices;
		_completionCoordinator = languageServices.CreateCompletionCoordinator();

		InitializeDefinitionNavigation(TryNavigateDefinition);
		InitializeHover(BuildStandardHoverRequestState, RequestHover);

		CommentSyntax = new CommentSyntax("//", null, StringLiteralStyle.DoubleQuoted | StringLiteralStyle.TripleDoubleQuoted);
	}

	/// <inheritdoc/>
	protected override void OnLanguageTextEntering(TextCompositionEventArgs e)
	{
		TryHandleCtrlSpaceCompletion(
			e,
			() => CompletionController.ApplyDecision(
				_completionCoordinator.GetOpenDecision(Document, CaretOffset, CompletionController.ActiveWindow is not null)));
	}

	/// <inheritdoc/>
	protected override void OnLanguageTextEntered(TextCompositionEventArgs e)
	{
		if (IntelliSenseEnabled && CompletionEnabled)
		{
			CompletionController.ApplyDecision(
				_completionCoordinator.GetOpenDecision(Document, CaretOffset, CompletionController.ActiveWindow is not null));
		}
	}

	/// <inheritdoc/>
	public override void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
	{
		EnsureNotDisposed();

		if (configuration is not GameFlowEditorConfiguration config)
			return;

		SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

		Background = ScriptingColorParser.CreateBrush(config.ColorScheme.Background, ScriptingColorParser.DefaultBackgroundColor);
		Foreground = ScriptingColorParser.CreateBrush(config.ColorScheme.Foreground, ScriptingColorParser.DefaultForegroundColor);

		base.UpdateSettings(configuration);
	}

	private Task<bool> TryNavigateDefinition(int offset, CancellationToken cancellationToken)
	{
		// The definition provider is synchronous; the token is honored before the request starts.
		cancellationToken.ThrowIfCancellationRequested();

		return Task.FromResult(
			TryGoToDefinition(_languageServices.DefinitionProvider, _languageServices.HoverProvider, offset));
	}

	/// <inheritdoc/>
	public void GoToObject(string objectName, TextDefinitionDiscriminator? identifyingObject = null)
		=> GoToDefinition(_languageServices.DefinitionProvider, objectName, identifyingObject);
}
