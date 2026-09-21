using Nickelony.IDEKit.Core.Text;

namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Provides document-level operations for TRX gameflow scripts.
/// All methods accept <see cref="ITextSnapshot"/> and return nullable
/// one-based line numbers instead of AvalonEdit types.
/// </summary>
public interface ITRXDocumentService
{
	/// <summary>
	/// Returns <see langword="true"/> when a level with the given
	/// <paramref name="levelName"/> is defined in <paramref name="source"/>.
	/// Matches against <c>"title"</c> JSON properties, applying the
	/// JSON name normalization (<c>.Trim().TrimEnd(',').Trim('"')</c>).
	/// </summary>
	bool IsLevelScriptDefined(ITextSnapshot source, string levelName);

	/// <summary>
	/// Finds a level by <paramref name="levelName"/> using a
	/// <c>StartsWith</c> match against normalized title property values,
	/// and falls back to comment-name matching
	/// (<c>// Level N: Name</c> patterns). Returns the one-based line
	/// number, or <see langword="null"/> when no matching line is found.
	/// </summary>
	int? FindDocumentLineOfLevel(ITextSnapshot source, string levelName);
}
