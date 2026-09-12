using Nickelony.IDEKit.Core.Infrastructure;
	using Nickelony.IDEKit.IntelliSense.Hover;
	using System.Threading;
	using System.Threading.Tasks;

namespace TombLib.Scripting.GameFlowScript;

public sealed partial class GameFlowEditor
{
	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
	{
		return SynchronousRequestAdapter.Adapt(
			() => _languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)),
			cancellationToken);
	}
}
