using System.Numerics;
using TombEditor.Controls.FlybyTimeline;
using TombEditor.Controls.FlybyTimeline.Preview;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;

namespace TombEditor.Tests.FlybyTimeline;

[TestClass]
public class FlybyPreviewTests
{
	[TestMethod]
	public void GetFrameForCamera_ConvertsToWorldSpaceAndRadians()
	{
		var level = FlybyTestFactory.CreateLevel();
		var room = FlybyTestFactory.CreateRoom(level, 1, new VectorInt3(1024, 256, 2048));
		var camera = FlybyTestFactory.AddCamera(room, 1, 0, new Vector3(128.0f, 64.0f, 256.0f),
			rotationX: 15.0f, rotationY: 45.0f, roll: 30.0f, fov: 80.0f);

		var frame = FlybyPreview.GetFrameForCamera(camera);

		Assert.AreEqual(new Vector3(1152.0f, 320.0f, 2304.0f), frame.Position);
		Assert.AreEqual(MathC.DegToRad(45.0f), frame.RotationY, 0.001f);
		Assert.AreEqual(-MathC.DegToRad(15.0f), frame.RotationX, 0.001f);
		Assert.AreEqual(MathC.DegToRad(30.0f), frame.Roll, 0.001f);
		Assert.AreEqual(MathC.DegToRad(80.0f), frame.Fov, 0.001f);
	}

	[TestMethod]
	public void ApplyFrame_UpdatesCameraStateAndTarget()
	{
		var camera = new FreeCamera(Vector3.Zero, 0.0f, 0.0f, -MathF.PI * 0.5f, MathF.PI * 0.5f, MathC.DegToRad(60.0f));
		var frame = new FlybyFrameState
		{
			Position = new Vector3(10.0f, 20.0f, 30.0f),
			RotationY = MathC.DegToRad(90.0f),
			RotationX = 0.0f,
			Fov = MathC.DegToRad(70.0f)
		};

		FlybyPreview.ApplyFrame(camera, frame);

		Assert.AreEqual(frame.Position, camera.Position);
		Assert.AreEqual(frame.RotationY, camera.RotationY, 0.001f);
		Assert.AreEqual(frame.RotationX, camera.RotationX, 0.001f);
		Assert.AreEqual(frame.Fov, camera.FieldOfView, 0.001f);
		Assert.AreEqual(10.0f + Level.SectorSizeUnit, camera.Target.X, 0.001f);
		Assert.AreEqual(20.0f, camera.Target.Y, 0.001f);
		Assert.AreEqual(30.0f, camera.Target.Z, 0.001f);
	}

	[TestMethod]
	public void SetStaticFrame_PinsAndAppliesTheFrame()
	{
		var savedCamera = new FreeCamera(Vector3.Zero, 0.0f, 0.0f, -MathF.PI * 0.5f, MathF.PI * 0.5f, MathC.DegToRad(60.0f));
		using var preview = new FlybyPreview(savedCamera);
		var previewCamera = new FreeCamera(Vector3.Zero, 0.0f, 0.0f, -MathF.PI * 0.5f, MathF.PI * 0.5f, MathC.DegToRad(60.0f));
		var frame = new FlybyFrameState
		{
			Position = new Vector3(64.0f, 32.0f, 16.0f),
			RotationY = MathC.DegToRad(180.0f),
			RotationX = MathC.DegToRad(-10.0f),
			Roll = MathC.DegToRad(5.0f),
			Fov = MathC.DegToRad(75.0f)
		};

		preview.SetStaticFrame(previewCamera, frame);

		Assert.IsTrue(preview.StaticFrame.HasValue);
		Assert.AreEqual(frame.Position, preview.StaticFrame.Value.Position);
		Assert.AreEqual(frame.Position, previewCamera.Position);
	}

	[TestMethod]
	public void BeginExternalUpdate_AtEndOfSequenceMarksPreviewFinished()
	{
		var level = FlybyTestFactory.CreateLevel();
		FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 2,
			new Vector3(0.0f, 0.0f, 0.0f),
			new Vector3(0.0f, 0.0f, 1024.0f));

		var savedCamera = new FreeCamera(Vector3.Zero, 0.0f, 0.0f, -MathF.PI * 0.5f, MathF.PI * 0.5f, MathC.DegToRad(60.0f));
		using var preview = new FlybyPreview(level, 2, savedCamera);
		float playbackEnd = preview.Cache.Timing.TimelineToPlaybackTime(preview.Cache.TotalDuration + 1.0f);

		preview.BeginExternalUpdate(playbackEnd);

		Assert.IsTrue(preview.IsFinished);
		Assert.AreEqual(preview.Cache.TotalDuration, preview.GetCurrentTimeSeconds(), 0.001f);
	}

	[TestMethod]
	public void BuildViewProjection_ClampsFieldOfViewBelowOneEightyDegrees()
	{
		var savedCamera = new FreeCamera(Vector3.Zero, 0.0f, 0.0f, -MathF.PI * 0.5f, MathF.PI * 0.5f, MathC.DegToRad(60.0f));
		using var preview = new FlybyPreview(savedCamera);
		var previewCamera = new FreeCamera(Vector3.Zero, 0.0f, 0.0f, -MathF.PI * 0.5f, MathF.PI * 0.5f, MathC.DegToRad(60.0f));
		var frame = new FlybyFrameState
		{
			Position = new Vector3(256.0f, 128.0f, 64.0f),
			RotationY = 0.0f,
			RotationX = 0.0f,
			Fov = MathC.DegToRad(220.0f)
		};

		preview.SetStaticFrame(previewCamera, frame);

		var viewProjection = preview.BuildViewProjection(1920.0f, 1080.0f, MathC.DegToRad(220.0f));

		Assert.AreEqual(FlybyConstants.MaxPreviewFieldOfViewRadians, previewCamera.FieldOfView, 0.001f);
		Assert.IsTrue(float.IsFinite(viewProjection.M11));
		Assert.IsTrue(float.IsFinite(viewProjection.M22));
		Assert.IsTrue(float.IsFinite(viewProjection.M33));
	}

	[TestMethod]
	public void FromDegrees_ClampsInvalidFieldOfViewIntoPreviewRange()
	{
		var lowFrame = FlybyFrameState.FromDegrees(Vector3.Zero, 0.0f, 0.0f, 0.0f, -15.0f, 1024, 1024, 0.5f, DofMode.Full);
		var highFrame = FlybyFrameState.FromDegrees(Vector3.Zero, 0.0f, 0.0f, 0.0f, 220.0f, 1024, 1024, 0.5f, DofMode.Full);

		Assert.AreEqual(MathC.DegToRad(FlybyConstants.DefaultPreviewFieldOfViewDegrees), lowFrame.Fov, 0.001f);
		Assert.AreEqual(FlybyConstants.MaxPreviewFieldOfViewRadians, highFrame.Fov, 0.001f);
	}
}
