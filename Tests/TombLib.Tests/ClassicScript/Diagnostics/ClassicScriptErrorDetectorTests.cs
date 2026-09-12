using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;

namespace TombLib.Tests.ClassicScript.Diagnostics;

/// <summary>
/// Direct tests for <see cref="ErrorDetector"/> covering invalid sections,
/// wrong-section commands, continuation markers, argument counts, empty
/// arguments, and NG strings.
/// </summary>
[TestClass]
public class ClassicScriptErrorDetectorTests
{
	private readonly ErrorDetector _errorDetector;

	public ClassicScriptErrorDetectorTests()
	{
		var lineService = new ClassicScriptLineService();
		var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
		var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
		var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);

		_errorDetector = new ErrorDetector(lineService, commandService, syntaxCatalogService);
	}

	[TestMethod]
	public void GetDiagnostics_EmptyContent_ReturnsNoDiagnostics()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest(string.Empty, new Version(1, 0)));

		Assert.AreEqual(0, diagnostics.Count);
	}

	[TestMethod]
	public void GetDiagnostics_InvalidSectionName_ReturnsSectionDiagnostic()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[Bogus]", new Version(1, 0)));

		Assert.AreEqual(1, diagnostics.Count);
		StringAssert.Contains(diagnostics[0].Message, "Invalid section name");
	}

	[TestMethod]
	public void GetDiagnostics_ValidSectionHeader_ReturnsNoDiagnostics()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[Level]", new Version(1, 0)));

		Assert.AreEqual(0, diagnostics.Count);
	}

	[TestMethod]
	public void GetDiagnostics_UnknownCommand_ReturnsCommandDiagnostic()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[Level]\nBogus= 1", new Version(1, 0)));

		Assert.AreEqual(1, diagnostics.Count);
		StringAssert.Contains(diagnostics[0].Message, "Invalid command");
	}

	[TestMethod]
	public void GetDiagnostics_WrongSectionCommand_ReturnsSectionDiagnostic()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[Options]\nLegend= 42", new Version(1, 0)));

		Assert.AreEqual(1, diagnostics.Count);
		StringAssert.Contains(diagnostics[0].Message, "wrong section");
	}

	[TestMethod]
	public void GetDiagnostics_ValidCommandInLevelSection_ReturnsNoDiagnostics()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[Level]\nLegend= 42", new Version(1, 0)));

		Assert.AreEqual(0, diagnostics.Count);
	}

	[TestMethod]
	public void GetDiagnostics_InvalidArgumentCount_ReturnsArgumentCountDiagnostic()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[Level]\nName= Level1, extra", new Version(1, 0)));

		Assert.AreEqual(1, diagnostics.Count);
		StringAssert.Contains(diagnostics[0].Message, "Invalid argument count");
	}

	[TestMethod]
	public void GetDiagnostics_EmptyArgument_ReturnsEmptyArgumentDiagnostic()
	{
		// AddEffect accepts array arguments, so the empty middle argument is detected
		// instead of being rejected as an argument-count mismatch.
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[Level]\nAddEffect= 1, , 2", new Version(1, 0)));

		Assert.AreEqual(1, diagnostics.Count);
		StringAssert.Contains(diagnostics[0].Message, "Empty arguments");
	}

	[TestMethod]
	public void GetDiagnostics_NGStringWithoutIndex_ReturnsNgStringDiagnostic()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[ExtraNG]\nnotAnIndexedString", new Version(1, 0)));

		Assert.AreEqual(1, diagnostics.Count);
		StringAssert.Contains(diagnostics[0].Message, "NG string must start with an index");
	}

	[TestMethod]
	public void GetDiagnostics_ValidNGString_ReturnsNoDiagnostics()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[ExtraNG]\n0: First String", new Version(1, 0)));

		Assert.AreEqual(0, diagnostics.Count);
	}

	[TestMethod]
	public void GetDiagnostics_MultipleMisplacedContinuationMarkers_ReturnsDiagnostic()
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _errorDetector.GetDiagnostics(new TextDiagnosticsRequest("[Level]\nName= Level1 > >", new Version(1, 0)));

		Assert.AreEqual(1, diagnostics.Count);
		StringAssert.Contains(diagnostics[0].Message, "Misplaced");
	}
}
