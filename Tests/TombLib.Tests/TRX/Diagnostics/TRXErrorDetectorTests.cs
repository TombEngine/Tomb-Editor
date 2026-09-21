using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using TombLib.Scripting.TRX.Diagnostics;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests.TRX.Diagnostics;

/// <summary>
/// Direct tests for <see cref="ErrorDetector"/> covering removed-keyword version
/// boundaries, comments, strings, and multiple diagnostics.
/// </summary>
[TestClass]
public class TRXErrorDetectorTests
{
	private static ErrorDetector CreateDetector(Version engineVersion)
		=> new(new TRXLineService(), engineVersion);

	[TestMethod]
	public void GetDiagnostics_VersionBelow48_ReturnsNoDiagnostics()
	{
		IReadOnlyList<TextDiagnostic> diagnostics = CreateDetector(new Version(4, 7)).GetDiagnostics(new TextDiagnosticsRequest("\"file\": \"level1\""));

		Assert.AreEqual(0, diagnostics.Count);
	}

	[TestMethod]
	public void GetDiagnostics_RemovedProperty_AtRemovalVersion_ReportsDiagnostic()
	{
		IReadOnlyList<TextDiagnostic> diagnostics = CreateDetector(new Version(4, 8)).GetDiagnostics(new TextDiagnosticsRequest("\"file\": \"level1\""));

		Assert.AreEqual(1, diagnostics.Count);
		StringAssert.Contains(diagnostics[0].Message, "property has been removed");
		StringAssert.Contains(diagnostics[0].Message, "TRX 4.8 or newer");
	}

	[TestMethod]
	public void GetDiagnostics_RemovedConstant_AtRemovalVersion_ReportsDiagnostic()
	{
		IReadOnlyList<TextDiagnostic> diagnostics = CreateDetector(new Version(4, 8)).GetDiagnostics(new TextDiagnosticsRequest("level: \"exit_to_cine\""));

		Assert.AreEqual(1, diagnostics.Count);
		StringAssert.Contains(diagnostics[0].Message, "constant has been removed");
	}

	[TestMethod]
	public void GetDiagnostics_RemovedKeyword_BeforeRemovalVersion_NoDiagnostic()
	{
		// draw_distance_fade is removed from 4.10 onward.
		IReadOnlyList<TextDiagnostic> beforeRemoval = CreateDetector(new Version(4, 9)).GetDiagnostics(new TextDiagnosticsRequest("\"draw_distance_fade\": 10"));
		IReadOnlyList<TextDiagnostic> atRemoval = CreateDetector(new Version(4, 10)).GetDiagnostics(new TextDiagnosticsRequest("\"draw_distance_fade\": 10"));

		Assert.AreEqual(0, beforeRemoval.Count);
		Assert.AreEqual(1, atRemoval.Count);
	}

	[TestMethod]
	public void GetDiagnostics_CommentLine_ReturnsNoDiagnostics()
	{
		IReadOnlyList<TextDiagnostic> diagnostics = CreateDetector(new Version(4, 8)).GetDiagnostics(new TextDiagnosticsRequest("// \"file\": \"level1\""));

		Assert.AreEqual(0, diagnostics.Count);
	}

	[TestMethod]
	public void GetDiagnostics_MultipleRemovedKeywordsOnSeparateLines_ReportsMultipleDiagnostics()
	{
		const string content = "\"file\": \"level1\"\n\"music\": \"track1\"";
		IReadOnlyList<TextDiagnostic> diagnostics = CreateDetector(new Version(4, 8)).GetDiagnostics(new TextDiagnosticsRequest(content));

		Assert.AreEqual(2, diagnostics.Count);
	}
}
