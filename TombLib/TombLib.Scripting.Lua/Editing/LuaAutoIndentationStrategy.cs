using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;
using Nickelony.IDEKit.AvalonEdit.Indentation;

namespace TombLib.Scripting.Lua.Editing;

/// <summary>
/// Adapts the Lua indentation policy to AvalonEdit's indentation strategy contract.
/// </summary>
internal sealed class LuaAutoIndentationStrategy : IIndentationStrategy
{
	private readonly PolicyIndentationStrategy _strategy;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaAutoIndentationStrategy"/> class.
	/// </summary>
	/// <param name="options">The editor options that determine the indentation unit.</param>
	public LuaAutoIndentationStrategy(TextEditorOptions options)
		=> _strategy = new PolicyIndentationStrategy(options, LuaIndentationStrategy.Instance, ShouldUseSmartIndent);

	/// <inheritdoc/>
	public void IndentLine(TextDocument document, DocumentLine line)
		=> _strategy.IndentLine(document, line);

	/// <inheritdoc/>
	public void IndentLines(TextDocument document, int beginLine, int endLine)
		=> _strategy.IndentLines(document, beginLine, endLine);

	private static bool ShouldUseSmartIndent(TextDocument document, DocumentLine previousLine)
	{
		if (previousLine.Length == 0)
			return true;

		return !LuaEditorInteractionRules.IsInsideCommentOrString(document, previousLine.EndOffset - 1);
	}
}
