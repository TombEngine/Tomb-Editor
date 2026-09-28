using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.TRX.Completion;

namespace TombLib.Tests.TRX.Completion;

/// <summary>
/// Direct tests for <see cref="TextAnalysisService"/> completion-context analysis.
/// </summary>
[TestClass]
public class TextAnalysisServiceTests
{
	private readonly TextAnalysisService _textAnalysisService = new();

	[TestMethod]
	public void IsValidPositionForCtrlSpaceCompletion_AtDocumentStart_ReturnsTrue()
	{
		var document = new TextDocument("alpha beta");

		Assert.IsTrue(_textAnalysisService.IsValidPositionForCtrlSpaceCompletion(document, 0));
	}

	[TestMethod]
	public void IsValidPositionForCtrlSpaceCompletion_AtDocumentEnd_ReturnsTrue()
	{
		var document = new TextDocument("alpha beta");

		Assert.IsTrue(_textAnalysisService.IsValidPositionForCtrlSpaceCompletion(document, document.TextLength));
	}

	[TestMethod]
	public void IsValidPositionForCtrlSpaceCompletion_InMiddleOfWord_ReturnsFalse()
	{
		var document = new TextDocument("alpha beta");

		Assert.IsFalse(_textAnalysisService.IsValidPositionForCtrlSpaceCompletion(document, 3));
	}

	[TestMethod]
	public void IsValidPositionForCtrlSpaceCompletion_OnWhitespace_ReturnsTrue()
	{
		var document = new TextDocument("alpha beta");

		Assert.IsTrue(_textAnalysisService.IsValidPositionForCtrlSpaceCompletion(document, 5));
	}

	[TestMethod]
	public void IsValidContextForCompletion_OutsideStringLiteral_ReturnsTrue()
	{
		var document = new TextDocument("{\"key\": value}");

		Assert.IsTrue(_textAnalysisService.IsValidContextForCompletion(document, 9));
	}

	[TestMethod]
	public void IsValidContextForCompletion_InsideStringLiteral_ReturnsFalse()
	{
		var document = new TextDocument("{\"key\": \"va");

		Assert.IsFalse(_textAnalysisService.IsValidContextForCompletion(document, document.TextLength));
	}
}
