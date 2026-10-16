using System;
using System.Linq;
using TombLib.Scripting.ClassicScript.StringTables;

namespace TombLib.Tests;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class ClassicScriptStringTableSerializerTests
{
	[TestMethod]
	public void Parse_MixedLineEndingsAndEscapes_UsesExplicitParsedNewline()
	{
		ClassicScriptStringTableParseResult result = ClassicScriptStringTableParser.Parse(
			"\r\n; preamble\n[Strings] \r\nHello\\x3B world\\nnext\r[ExtraNG]\n42: Extra\\q\\nline\r\n",
			new ClassicScriptStringTableParseOptions("\r\n"));

		Assert.IsTrue(result.IsSuccess);
		Assert.IsNotNull(result.Model);
		Assert.AreEqual(2, result.Model.Sections.Count);
		Assert.AreEqual("Strings", result.Model.Sections[0].Name);
		Assert.IsFalse(result.Model.Sections[0].IsExtraNg);
		Assert.AreEqual("Hello; world\r\nnext", result.Model.Sections[0].Rows[0].Value);
		Assert.AreEqual("Extra\\q\r\nline", result.Model.Sections[1].Rows[0].Value);
		Assert.AreEqual(42, result.Model.Sections[1].Rows[0].Id);
	}

	[TestMethod]
	public void Parse_CommentsAndBlankPreamble_AreNotPartOfModel()
	{
		ClassicScriptStringTableParseResult result = ClassicScriptStringTableParser.Parse(
			"; generated\r\n\n  ; another comment\r\n [Strings] \r\n ; ignored\r\n value ; inline comment\r\n");

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(1, result.Model!.Sections.Count);
		Assert.AreEqual(1, result.Model.Sections[0].Rows.Count);
		Assert.AreEqual(" value", result.Model.Sections[0].Rows[0].Value);
	}

	[TestMethod]
	public void Parse_UnexpectedPreambleText_ReturnsTypedDiagnostic()
	{
		ClassicScriptStringTableParseResult result = ClassicScriptStringTableParser.Parse(
			"unexpected text\n[Strings]\nvalue");

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual(1, result.Diagnostics.Count);
		ClassicScriptStringTableDiagnostic diagnostic = result.Diagnostics.Single();
		Assert.AreEqual(ClassicScriptStringTableDiagnosticCode.UnexpectedPreambleText, diagnostic.Code);
		Assert.AreEqual(1, diagnostic.Line);
		Assert.AreEqual(1, diagnostic.Column);
		Assert.IsNull(result.Model);
	}

	[TestMethod]
	public void Parse_MalformedHeadersAndExtraNgRows_ReturnsExactDiagnostics()
	{
		ClassicScriptStringTableParseResult result = ClassicScriptStringTableParser.Parse(
			"[]\n[ExtraNG]\nnot a row\nabc: value\n[Closed\n[Strings]\nvalue");

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual(4, result.Diagnostics.Count);
		AssertDiagnostic(result.Diagnostics[0], ClassicScriptStringTableDiagnosticCode.MalformedSectionHeader, 1, 1);
		AssertDiagnostic(result.Diagnostics[1], ClassicScriptStringTableDiagnosticCode.MalformedExtraNgRow, 3, 1);
		AssertDiagnostic(result.Diagnostics[2], ClassicScriptStringTableDiagnosticCode.InvalidExtraNgId, 4, 1);
		AssertDiagnostic(result.Diagnostics[3], ClassicScriptStringTableDiagnosticCode.MalformedSectionHeader, 5, 1);
		Assert.IsNull(result.Model);
	}

	[TestMethod]
	public void Parse_DuplicateSectionNames_ReturnsDiagnosticWithoutPublishableModel()
	{
		ClassicScriptStringTableParseResult result = ClassicScriptStringTableParser.Parse(
			"[Strings]\nfirst\n[strINGS]\nsecond");

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual(1, result.Diagnostics.Count);
		ClassicScriptStringTableDiagnostic diagnostic = result.Diagnostics.Single();
		Assert.AreEqual(ClassicScriptStringTableDiagnosticCode.DuplicateSectionName, diagnostic.Code);
		Assert.AreEqual(3, diagnostic.Line);
		Assert.AreEqual(2, diagnostic.Column);
		Assert.IsNull(result.Model);
	}

	[TestMethod]
	public void Write_UsesExplicitNewlinesAndHeaderWithoutEnvironmentDependencies()
	{
		var table = new ClassicScriptStringTable(
		[
			new ClassicScriptStringTableSection(
				"Strings",
				false,
				[new ClassicScriptStringTableRow(null, "one;two\r\nthree")]),
			new ClassicScriptStringTableSection(
				"ExtraNG",
				true,
				[new ClassicScriptStringTableRow(7, "seven")])
		]);

		string output = ClassicScriptStringTableWriter.Write(
			table,
			new ClassicScriptStringTableWriteOptions(
				ClassicScriptStringTableNewlineStyle.Cr,
				new ClassicScriptStringTableHeader("TombIDE", "9.9")));

		Assert.AreEqual(
			"; Automatically generated document using TombIDE 9.9\r" +
			"; Do not add any comments into this document as they are\r" +
			"; going to be removed next time the file is regenerated.\r" +
			"\r" +
			"[Strings]\r" +
			"one\\x3Btwo\\nthree\r" +
			"\r" +
			"[ExtraNG]\r" +
			"7: seven\r",
			output);
		Assert.IsFalse(output.Contains("\r\n", StringComparison.Ordinal));
	}

	[TestMethod]
	public void Write_NormalizedOutputCanBeParsedWithExplicitNewline()
	{
		var table = new ClassicScriptStringTable(
		[
			new ClassicScriptStringTableSection(
				"Strings",
				false,
				[new ClassicScriptStringTableRow(null, "line one\nline two")])
		]);

		string output = ClassicScriptStringTableWriter.Write(
			table,
			new ClassicScriptStringTableWriteOptions(ClassicScriptStringTableNewlineStyle.Lf));
		ClassicScriptStringTableParseResult result = ClassicScriptStringTableParser.Parse(
			output,
			new ClassicScriptStringTableParseOptions("\n"));

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("line one\nline two", result.Model!.Sections[0].Rows[0].Value);
	}

	private static void AssertDiagnostic(
		ClassicScriptStringTableDiagnostic diagnostic,
		ClassicScriptStringTableDiagnosticCode code,
		int line,
		int column)
	{
		Assert.AreEqual(code, diagnostic.Code);
		Assert.AreEqual(line, diagnostic.Line);
		Assert.AreEqual(column, diagnostic.Column);
	}
}
