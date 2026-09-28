using Nickelony.IDEKit.AvalonEdit.LanguageFeatures.Hover;
	using Nickelony.IDEKit.IntelliSense.Diagnostics;
	using Nickelony.IDEKit.IntelliSense.Hover;
	using System.Threading;
	using System.Threading.Tasks;

namespace TombLib.Scripting.ClassicScript;

public sealed partial class ClassicScriptEditor
{
	private TextHoverEvaluationState BuildHoverRequestState(int hoveredOffset)
	{
		TryGetDiagnosticInfo(hoveredOffset, out TextDiagnostic? diagnosticInfo, allowLineFallback: false);

		return new TextHoverEvaluationState(
			ShouldRequestHover: true,
			RequestOffset: hoveredOffset,
				CanShowHoverContent: true,
			CanShowDiagnosticFallback: false,
			DiagnosticInfo: diagnosticInfo);
	}

	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
	{
		// The hover provider is synchronous; the token is honored before the request starts.
		cancellationToken.ThrowIfCancellationRequested();

		return Task.FromResult(
			_languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)));
	}
}
