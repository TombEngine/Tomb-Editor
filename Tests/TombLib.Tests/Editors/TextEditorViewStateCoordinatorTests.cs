using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using TombLib.Scripting.UI.Editors;

namespace TombLib.Tests;

[TestClass]
public class TextEditorViewStateCoordinatorTests
{
	[TestMethod]
	public void ZoomPercent_DefaultsToOneHundred()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });
			Assert.AreEqual(100, coordinator.ZoomPercent);
		});
	}

	[TestMethod]
	public void ZoomPercent_NonPositiveValue_ThrowsArgumentOutOfRangeException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => coordinator.ZoomPercent = 0);
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => coordinator.ZoomPercent = -10);
			Assert.AreEqual(100, coordinator.ZoomPercent);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_PositiveDelta_UpdatesZoomAndRaisesCallbacks()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var zoomCallbacks = 0;
			var appliedFontSize = 0.0;

			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => zoomCallbacks++);

			var changed = coordinator.TryApplyZoomStep(120, CreateZoomOptions(), size => appliedFontSize = size);

			Assert.IsTrue(changed);
			Assert.AreEqual(110, coordinator.ZoomPercent);
			Assert.AreEqual(1, zoomCallbacks);
			Assert.AreEqual(13.2, appliedFontSize, 0.001);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_NegativeDelta_DecreasesZoom()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			var changed = coordinator.TryApplyZoomStep(-120, CreateZoomOptions(), _ => { });

			Assert.IsTrue(changed);
			Assert.AreEqual(90, coordinator.ZoomPercent);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_AtMaxZoom_ReturnsFalse()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var zoomCallbacks = 0;

			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => zoomCallbacks++);
			coordinator.ZoomPercent = 200;

			var changed = coordinator.TryApplyZoomStep(120, CreateZoomOptions(), _ => { });

			Assert.IsFalse(changed);
			Assert.AreEqual(200, coordinator.ZoomPercent);
			Assert.AreEqual(0, zoomCallbacks);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_AtMinZoom_ReturnsFalse()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });
			coordinator.ZoomPercent = 50;

			var changed = coordinator.TryApplyZoomStep(-120, CreateZoomOptions(), _ => { });

			Assert.IsFalse(changed);
			Assert.AreEqual(50, coordinator.ZoomPercent);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_ZeroDelta_ReturnsFalse()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var zoomCallbacks = 0;
			var appliedFontSize = 0.0;

			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => zoomCallbacks++);

			var changed = coordinator.TryApplyZoomStep(0, CreateZoomOptions(), size => appliedFontSize = size);

			Assert.IsFalse(changed);
			Assert.AreEqual(100, coordinator.ZoomPercent);
			Assert.AreEqual(0, zoomCallbacks);
			Assert.AreEqual(0.0, appliedFontSize);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_CallbacksObserveStoredZoomByValueAndInOrder()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var observedDuringFontSizeCallback = -1;
			var observedDuringZoomCallback = -1;
			var sequence = new List<string>();

			TextEditorViewStateCoordinator? coordinator = null;

			coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () =>
			{
				observedDuringZoomCallback = coordinator!.ZoomPercent;
				sequence.Add("zoom");
			});

			using (coordinator)
			{
				var changed = coordinator.TryApplyZoomStep(
					120,
					CreateZoomOptions(),
					_ =>
					{
						observedDuringFontSizeCallback = coordinator!.ZoomPercent;
						sequence.Add("font");
					});

				// The font-size callback runs before the stored zoom changes and reads the old value; the
				// zoom-changed callback runs after it and reads the new value.
				Assert.IsTrue(changed);
				Assert.AreEqual(100, observedDuringFontSizeCallback);
				Assert.AreEqual(110, observedDuringZoomCallback);
				Assert.AreEqual(110, coordinator.ZoomPercent);
				CollectionAssert.AreEqual(new[] { "font", "zoom" }, sequence);
			}
		});
	}

	[TestMethod]
	public void Attach_RaisesStatusOnCaretMove()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var textArea = new TextArea { Document = new TextDocument("ab") };
			var statusCallbacks = 0;

			using var coordinator = new TextEditorViewStateCoordinator(textArea, () => statusCallbacks++, () => { });
			coordinator.Attach();

			textArea.Caret.Position = new TextViewPosition(1, 2);

			Assert.AreEqual(1, statusCallbacks);
		});
	}

	[TestMethod]
	public void Dispose_StopsRaisingStatusOnCaretMove()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var textArea = new TextArea { Document = new TextDocument("ab") };
			var statusCallbacks = 0;

			var coordinator = new TextEditorViewStateCoordinator(textArea, () => statusCallbacks++, () => { });

			coordinator.Attach();
			coordinator.Dispose();

			textArea.Caret.Position = new TextViewPosition(1, 2);

			Assert.AreEqual(0, statusCallbacks);
		});
	}

	[TestMethod]
	public void Attach_Twice_RaisesOneStatusCallback()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var textArea = new TextArea { Document = new TextDocument("ab") };
			var statusCallbacks = 0;

			using var coordinator = new TextEditorViewStateCoordinator(textArea, () => statusCallbacks++, () => { });

			coordinator.Attach();
			coordinator.Attach();

			textArea.Caret.Position = new TextViewPosition(1, 2);

			Assert.AreEqual(1, statusCallbacks);
		});
	}

	[TestMethod]
	public void Attach_RaisesStatusOnSelectionChange()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var textArea = new TextArea { Document = new TextDocument("abcd") };
			var statusCallbacks = 0;

			using var coordinator = new TextEditorViewStateCoordinator(textArea, () => statusCallbacks++, () => { });
			coordinator.Attach();

			textArea.Caret.Position = new TextViewPosition(1, 4);

			statusCallbacks = 0;

			textArea.Selection = Selection.Create(textArea, 1, 2);

			Assert.AreEqual(1, statusCallbacks);
		});
	}

	[TestMethod]
	public void Dispose_Twice_UnsubscribesOnceAndDoesNotThrow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var textArea = new TextArea { Document = new TextDocument("ab") };
			var statusCallbacks = 0;

			var coordinator = new TextEditorViewStateCoordinator(textArea, () => statusCallbacks++, () => { });

			coordinator.Attach();
			coordinator.Dispose();
			coordinator.Dispose();

			textArea.Caret.Position = new TextViewPosition(1, 2);

			Assert.AreEqual(0, statusCallbacks);
		});
	}

	[TestMethod]
	public void Attach_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });
			coordinator.Dispose();

			Assert.ThrowsException<ObjectDisposedException>(() => coordinator.Attach());
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_MinZoomGreaterThanMaxZoom_ThrowsArgumentOutOfRangeException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				coordinator.TryApplyZoomStep(120, CreateZoomOptions(minZoom: 200, maxZoom: 50), _ => { }));
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_NonPositiveMinZoom_ThrowsArgumentOutOfRangeException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				coordinator.TryApplyZoomStep(120, CreateZoomOptions(minZoom: 0), _ => { }));
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_NonPositiveZoomStepSize_ThrowsArgumentOutOfRangeException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				coordinator.TryApplyZoomStep(120, CreateZoomOptions(stepSize: 0), _ => { }));
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_NonFiniteReferenceFontSize_ThrowsArgumentOutOfRangeException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				coordinator.TryApplyZoomStep(120, CreateZoomOptions(referenceFontSize: double.PositiveInfinity), _ => { }));
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_ZoomAboveMax_ZoomingOut_ClampsToBoundBeforeStepping()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });
			coordinator.ZoomPercent = 205;

			// The stored value is clamped to maxZoom (200) before the step is applied.
			var changed = coordinator.TryApplyZoomStep(-120, CreateZoomOptions(), _ => { });

			Assert.IsTrue(changed);
			Assert.AreEqual(190, coordinator.ZoomPercent);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_ZoomAboveMax_ZoomingIn_DoesNothing()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var zoomCallbacks = 0;

			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => zoomCallbacks++);
			coordinator.ZoomPercent = 205;

			var changed = coordinator.TryApplyZoomStep(120, CreateZoomOptions(), _ => { });

			Assert.IsFalse(changed);
			Assert.AreEqual(205, coordinator.ZoomPercent);
			Assert.AreEqual(0, zoomCallbacks);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_ZoomBelowMin_ZoomingIn_ClampsToBoundThenSteps()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var appliedFontSize = 0.0;

			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });
			coordinator.ZoomPercent = 30;

			// The stored value is clamped to minZoom (50) before the step is applied, so the
			// font-size callback never receives a size scaled from an out-of-range zoom.
			var changed = coordinator.TryApplyZoomStep(120, CreateZoomOptions(), size => appliedFontSize = size);

			Assert.IsTrue(changed);
			Assert.AreEqual(60, coordinator.ZoomPercent);
			Assert.AreEqual(7.2, appliedFontSize, 0.001);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_ZoomBelowMin_ZoomingOut_DoesNothing()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var zoomCallbacks = 0;
			var appliedFontSize = 0.0;

			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => zoomCallbacks++);
			coordinator.ZoomPercent = 30;

			var changed = coordinator.TryApplyZoomStep(-120, CreateZoomOptions(), size => appliedFontSize = size);

			Assert.IsFalse(changed);
			Assert.AreEqual(30, coordinator.ZoomPercent);
			Assert.AreEqual(0, zoomCallbacks);
			Assert.AreEqual(0.0, appliedFontSize);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_StepOvershoot_ClampsToBounds()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			coordinator.ZoomPercent = 190;

			Assert.IsTrue(coordinator.TryApplyZoomStep(120, CreateZoomOptions(stepSize: 25), _ => { }));
			Assert.AreEqual(200, coordinator.ZoomPercent);

			coordinator.ZoomPercent = 60;

			Assert.IsTrue(coordinator.TryApplyZoomStep(-120, CreateZoomOptions(stepSize: 25), _ => { }));
			Assert.AreEqual(50, coordinator.ZoomPercent);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_ExtremeStepSize_ClampsToBoundWithoutOverflow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			// The step size is larger than the headroom to the bound, so a naive addition would overflow.
			var changed = coordinator.TryApplyZoomStep(120, CreateZoomOptions(stepSize: int.MaxValue), _ => { });

			Assert.IsTrue(changed);
			Assert.AreEqual(200, coordinator.ZoomPercent);

			changed = coordinator.TryApplyZoomStep(-120, CreateZoomOptions(stepSize: int.MaxValue), _ => { });

			Assert.IsTrue(changed);
			Assert.AreEqual(50, coordinator.ZoomPercent);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_NonPositiveReferenceFontSize_ThrowsArgumentOutOfRangeException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				coordinator.TryApplyZoomStep(120, CreateZoomOptions(referenceFontSize: 0.0), _ => { }));
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_ApplyFontSizeThrows_LeavesStoredZoomUnchanged()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var zoomCallbacks = 0;

			using var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => zoomCallbacks++);

			Assert.ThrowsException<InvalidOperationException>(() =>
				coordinator.TryApplyZoomStep(120, CreateZoomOptions(), _ => throw new InvalidOperationException()));

			// The font-size callback runs before the stored zoom changes, so the failure leaves it untouched.
			Assert.AreEqual(100, coordinator.ZoomPercent);
			Assert.AreEqual(0, zoomCallbacks);
		});
	}

	[TestMethod]
	public void TryApplyZoomStep_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var coordinator = new TextEditorViewStateCoordinator(new TextArea(), () => { }, () => { });

			coordinator.Dispose();

			Assert.ThrowsException<ObjectDisposedException>(() =>
				coordinator.TryApplyZoomStep(120, CreateZoomOptions(), _ => { }));
		});
	}

	private static ZoomOptions CreateZoomOptions(
		int minZoom = 50,
		int maxZoom = 200,
		int stepSize = 10,
		double referenceFontSize = 12.0)
		=> new(minZoom, maxZoom, stepSize, referenceFontSize);
}
