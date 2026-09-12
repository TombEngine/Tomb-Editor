using Nickelony.IDEKit.Core.Infrastructure;
	using Nickelony.IDEKit.IntelliSense.Diagnostics;
	using Nickelony.IDEKit.IntelliSense.Hover;
	using System.Threading;
	using System.Threading.Tasks;

namespace TombLib.Scripting.ClassicScript;

public sealed partial class ClassicScriptEditor
{
	private TextHoverRequestState BuildHoverRequestState(int hoveredOffset)
	{
		TryGetDiagnosticInfo(hoveredOffset, out TextEditorDiagnostic? diagnosticInfo, allowLineFallback: false);

		return new TextHoverRequestState(
			ShouldRequestHover: true,
			RequestOffset: hoveredOffset,
			CanShowToolTip: true,
			CanShowDiagnosticFallback: false,
			DiagnosticInfo: diagnosticInfo);
	}

	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
	{
		return SynchronousRequestAdapter.Adapt(
			() => _languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)),
			cancellationToken);
	}
}
