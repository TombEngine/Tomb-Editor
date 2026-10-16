
	using Nickelony.IDEKit.IntelliSense.Hover;
	using System.Threading;
	using System.Threading.Tasks;

namespace TombLib.Scripting.GameFlowScript;

public sealed partial class GameFlowEditor
{
	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
	{
		// The hover provider is synchronous; the token is honored before the request starts.
		cancellationToken.ThrowIfCancellationRequested();

		return Task.FromResult(
			_languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)));
	}
}
