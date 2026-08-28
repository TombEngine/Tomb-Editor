using System;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Tests;

[TestClass]
public class TextEditorSettingsValidationTests
{
	[TestMethod]
	public void MinZoom_NonPositive_Throws()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => editor.MinZoom = 0);
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => editor.MinZoom = -1);
		});
	}

	[TestMethod]
	public void MaxZoom_NonPositive_Throws()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => editor.MaxZoom = 0);
		});
	}

	[TestMethod]
	public void ZoomStepSize_NonPositive_Throws()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => editor.ZoomStepSize = 0);
		});
	}

	[TestMethod]
	public void EngineVersion_Null_Throws()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();

			Assert.ThrowsException<ArgumentNullException>(() => editor.EngineVersion = null!);
		});
	}

	[TestMethod]
	public void AutoClosingStrings_Null_Throws()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();

			Assert.ThrowsException<ArgumentNullException>(() => editor.ParenthesesClosingString = null!);
			Assert.ThrowsException<ArgumentNullException>(() => editor.BracesClosingString = null!);
			Assert.ThrowsException<ArgumentNullException>(() => editor.BracketsClosingString = null!);
			Assert.ThrowsException<ArgumentNullException>(() => editor.QuotesClosingString = null!);
		});
	}
}
