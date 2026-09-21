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
	public void MinZoom_GreaterThanMaxZoom_Throws()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => editor.MinZoom = editor.MaxZoom + 1);
			Assert.AreEqual(25, editor.MinZoom);
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
	public void MaxZoom_LessThanMinZoom_Throws()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();
			editor.MinZoom = 150;

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => editor.MaxZoom = 149);
			Assert.AreEqual(400, editor.MaxZoom);
		});
	}

	[TestMethod]
	public void Zoom_Assignment_IsClampedToConfiguredBounds()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor
			{
				MinZoom = 50,
				MaxZoom = 150
			};

			editor.Zoom = 25;
			Assert.AreEqual(50, editor.Zoom);

			editor.Zoom = 200;
			Assert.AreEqual(150, editor.Zoom);
		});
	}

	[TestMethod]
	public void ChangingZoomBounds_ClampsCurrentZoom()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();

			editor.MinZoom = 125;
			Assert.AreEqual(125, editor.Zoom);

			editor.MaxZoom = 1000;
			editor.Zoom = 900;
			editor.MaxZoom = 200;
			Assert.AreEqual(200, editor.Zoom);
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
