using Nickelony.IDEKit.Core.Editing;
using Nickelony.IDEKit.Core.Text;

namespace TombLib.Scripting.UI.Diagnostics;

/// <summary>
/// Identifies the logical document and operation ownership captured when an asynchronous editor
/// request is admitted.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="LogicalDocumentId"/> names the logical document the request was admitted for.
/// <see cref="DocumentVersion"/> identifies the logical content snapshot the request was computed
/// against; it is zero when the request is content-snapshot based and has no document version, as
/// with background diagnostics. <see cref="SessionGeneration"/> identifies the operation ownership
/// that invalidates asynchronous work.
/// </para>
/// <para>
/// The document version and the session generation are not interchangeable: a text change advances
/// the document version, while a load, replace, rename, or disposal boundary advances the session
/// generation. Both values are <c>long</c> to match
/// <see cref="ITextEditTargetVersion.Version"/>.
/// </para>
/// </remarks>
/// <param name="LogicalDocumentId">The identity of the logical document the request was admitted for, when known.</param>
/// <param name="DocumentVersion">The logical content snapshot version the request was computed against.</param>
/// <param name="SessionGeneration">The operation ownership generation the request was admitted under.</param>
public readonly record struct TextEditorRequestIdentity(string? LogicalDocumentId, long DocumentVersion, long SessionGeneration);
