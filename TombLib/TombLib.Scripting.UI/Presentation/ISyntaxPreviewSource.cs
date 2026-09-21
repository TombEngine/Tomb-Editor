using Nickelony.IDEKit.IntelliSense.Signatures;

namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Provides the current signature syntax preview for an editor presentation.
/// </summary>
/// <remarks>
/// Unlike the request contracts in the IntelliSense packages, the preview describes the current editor
/// state at call time rather than a frozen document snapshot. Implementations are expected to be
/// called on the host's presentation thread and do not need to be thread-safe; hosts that call the
/// contract from other threads synchronize access themselves.
/// </remarks>
public interface ISyntaxPreviewSource
{
	/// <summary>
	/// Gets the current syntax preview, or <see langword="null"/> when none is available.
	/// </summary>
	TextSignatureHelp? GetSyntaxPreview();
}
