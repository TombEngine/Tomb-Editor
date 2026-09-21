using Nickelony.LanguageServer.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Nickelony.IDEKit.Core.Formatting;
using Nickelony.IDEKit.Core.Text;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Adapts a local document formatter to the shared workspace-edit formatting contract.
/// </summary>
public sealed class TextDocumentFormatterProvider : ILanguageServerFormattingProvider
{
	private readonly ITextDocumentFormatter _documentFormatter;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextDocumentFormatterProvider"/> class.
	/// </summary>
	/// <param name="documentFormatter">The document formatter this provider adapts.</param>
	public TextDocumentFormatterProvider(ITextDocumentFormatter documentFormatter)
	{
		ArgumentNullException.ThrowIfNull(documentFormatter);
		_documentFormatter = documentFormatter;
	}

	/// <summary>
	/// Gets a value indicating whether formatting is supported.
	/// </summary>
	public bool SupportsFormatting => true;

	/// <summary>
	/// Formats the document text in the request and returns the workspace edit, or <c>null</c> when nothing changed.
	/// </summary>
	/// <param name="request">The formatting request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The workspace edit that applies the formatting, or <c>null</c> when the text is unchanged.</returns>
	public Task<TextWorkspaceEdit?> FormatDocumentAsync(TextFormatRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		cancellationToken.ThrowIfCancellationRequested();

		string? formattedText = _documentFormatter.FormatDocument(request.DocumentText);

		// A formatter that declines (null) or returns the same text produces no edit.
		if (formattedText is null || string.Equals(formattedText, request.DocumentText, StringComparison.Ordinal))
			return Task.FromResult<TextWorkspaceEdit?>(null);

		TextPositionRange documentRange = CreateDocumentRange(request.DocumentText);

		return Task.FromResult<TextWorkspaceEdit?>(new TextWorkspaceEdit([
			new TextDocumentEdit(request.FilePath, [
				new TextEdit(documentRange, formattedText)
			])
		]));
	}

	private static TextPositionRange CreateDocumentRange(string content)
	{
		string[] lines = content.Replace("\r", string.Empty).Split('\n');

		return new(
			new TextPosition(0, 0),
			new TextPosition(lines.Length - 1, lines[^1].Length));
	}
}
