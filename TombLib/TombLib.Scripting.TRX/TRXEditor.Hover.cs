
	using Nickelony.IDEKit.IntelliSense.Hover;
	using System.Threading;
	using System.Threading.Tasks;

namespace TombLib.Scripting.TRX;

public sealed partial class TRXEditor
{
	/// <inheritdoc/>
	protected override bool CanShowDiagnosticFallback => true;

	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
	{
		// The hover provider is synchronous; the token is honored before the request starts.
		cancellationToken.ThrowIfCancellationRequested();

		return Task.FromResult(
			_languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)));
	}
}
