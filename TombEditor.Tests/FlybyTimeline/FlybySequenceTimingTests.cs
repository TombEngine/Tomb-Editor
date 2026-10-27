using System.Numerics;
using TombEditor.Controls.FlybyTimeline;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib.LevelData;

namespace TombEditor.Tests.FlybyTimeline;

[TestClass]
public class FlybySequenceTimingTests
{
    [TestMethod]
    public void Build_ReturnsEmptyTimingForEmptySequence()
    {
        var timing = FlybySequenceTiming.Build([], useSmoothPause: false);

        Assert.AreEqual(0, timing.CameraCount);
        Assert.AreEqual(0.0f, timing.TotalDuration, 0.001f);
        Assert.AreEqual(0, timing.SplineTimeline.Count);
        Assert.AreEqual(0, timing.CutRegions.Count);
    }

    [TestMethod]
    public void Build_AddsFreezeDurationToLaterCameraTimes()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 1,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f));

        cameras[1].Flags = FlybyConstants.FlagFreezeCamera;
        cameras[1].Timer = FlybyTestFactory.FreezeFrames(30);

        var timing = FlybySequenceTiming.Build(cameras, useSmoothPause: false);

        Assert.AreEqual(1.0f, timing.GetFreezeDuration(1), 0.001f);
        Assert.IsTrue(timing.GetCameraTime(2) - timing.GetCameraTime(1) >= 1.0f);
    }

    [TestMethod]
    public void Build_WithSmoothPauseDelaysTheFreezeBoundary()
    {
        var level = FlybyTestFactory.CreateLevel(TRVersion.Game.TombEngine);
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 2,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f));

        cameras[1].Flags = FlybyConstants.FlagFreezeCamera;
        cameras[1].Timer = FlybyTestFactory.FreezeFrames(30);

        var standardTiming = FlybySequenceTiming.Build(cameras, useSmoothPause: false);
        var smoothTiming = FlybySequenceTiming.Build(cameras, useSmoothPause: true);

        Assert.IsTrue(smoothTiming.GetCameraTime(1) > standardTiming.GetCameraTime(1));
        Assert.IsTrue(smoothTiming.TotalDuration > standardTiming.TotalDuration);
    }

    [TestMethod]
    public void Build_WithFinalFreeze_KeepsSequenceEndAligned()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 3,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f),
            new Vector3(0.0f, 0.0f, 3072.0f));

        cameras[3].Flags = FlybyConstants.FlagFreezeCamera;
        cameras[3].Timer = FlybyTestFactory.FreezeFrames(15);

        var timing = FlybySequenceTiming.Build(cameras, useSmoothPause: false);
        float expectedEndTime = timing.GetCameraTime(3) + timing.GetFreezeDuration(3);

        Assert.AreEqual(expectedEndTime, timing.TotalDuration, 0.001f);
    }

    [TestMethod]
    public void Build_CapturesCutRegionsAndBypassDurations()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 3,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f),
            new Vector3(0.0f, 0.0f, 3072.0f));

        cameras[1].Flags = FlybyConstants.FlagCameraCut;
        cameras[1].Timer = 3;

        var timing = FlybySequenceTiming.Build(cameras, useSmoothPause: false);

        Assert.AreEqual(1, timing.CutRegions.Count);
        Assert.IsTrue(timing.GetCutBypassDuration(1) > 0.0f);
        Assert.IsTrue(timing.CutRegions[0].EndTime > timing.CutRegions[0].StartTime);
        Assert.IsTrue(timing.TotalDuration >= timing.GetCameraTime(3));
    }

    [TestMethod]
    public void Build_ResolvesCutTargetsByCameraNumber()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 5,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f),
            new Vector3(0.0f, 0.0f, 3072.0f));

        cameras[0].Number = 0;
        cameras[1].Number = 2;
        cameras[2].Number = 4;
        cameras[3].Number = 6;

        cameras[1].Flags = FlybyConstants.FlagCameraCut;
        cameras[1].Timer = 4;

        var timing = FlybySequenceTiming.Build(cameras, useSmoothPause: false);
        float expectedBypass = timing.GetCameraTime(2) - timing.GetCameraTime(1);
        float cutRegionDuration = timing.CutRegions[0].EndTime - timing.CutRegions[0].StartTime;

        Assert.AreEqual(1, timing.CutRegions.Count);
        Assert.AreEqual(expectedBypass, timing.GetCutBypassDuration(1), 0.001f);
        Assert.AreEqual(expectedBypass, cutRegionDuration, FlybyConstants.TimeStep);
    }

    [TestMethod]
    public void Build_IgnoresAmbiguousCutTargetsWhenCameraNumbersDuplicate()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 6,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f),
            new Vector3(0.0f, 0.0f, 3072.0f));

        cameras[0].Number = 0;
        cameras[1].Number = 2;
        cameras[2].Number = 2;
        cameras[3].Number = 4;

        cameras[0].Flags = FlybyConstants.FlagCameraCut;
        cameras[0].Timer = 2;

        var timing = FlybySequenceTiming.Build(cameras, useSmoothPause: false);

        Assert.AreEqual(0.0f, timing.GetCutBypassDuration(0), 0.001f);
        Assert.AreEqual(0, timing.CutRegions.Count);
    }

    [TestMethod]
    public void Build_WithSmoothPauseCutBypassMatchesSkippedFreezeTiming()
    {
        var level = FlybyTestFactory.CreateLevel(TRVersion.Game.TombEngine);
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 7,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f),
            new Vector3(0.0f, 0.0f, 3072.0f));

        cameras[1].Flags = FlybyConstants.FlagCameraCut;
        cameras[1].Timer = 3;
        cameras[2].Flags = FlybyConstants.FlagFreezeCamera;
        cameras[2].Timer = FlybyTestFactory.FreezeFrames(30);

        var timing = FlybySequenceTiming.Build(cameras, useSmoothPause: true);
        float expectedBypass = timing.GetCameraTime(3) - timing.GetCameraTime(1);

        Assert.AreEqual(1, timing.CutRegions.Count);
        Assert.AreEqual(expectedBypass, timing.CutRegions[0].Duration, FlybyConstants.TimeStep);
    }

    [TestMethod]
    public void Build_WithSmoothPauseAndFrozenCutTarget_KeepsFinalCameraAligned()
    {
        var level = FlybyTestFactory.CreateLevel(TRVersion.Game.TombEngine);
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 8,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f),
            new Vector3(0.0f, 0.0f, 3072.0f),
            new Vector3(0.0f, 0.0f, 4096.0f));

        cameras[1].Flags = FlybyConstants.FlagCameraCut;
        cameras[1].Timer = 3;
        cameras[3].Flags = FlybyConstants.FlagFreezeCamera;
        cameras[3].Timer = FlybyTestFactory.FreezeFrames(30);

        var timing = FlybySequenceTiming.Build(cameras, useSmoothPause: true);
        float finalCameraTime = timing.GetCameraTime(4);

        Assert.IsTrue(timing.TotalDuration >= finalCameraTime);
        Assert.AreEqual(finalCameraTime, timing.TotalDuration, FlybyConstants.TimeStep);
    }

    [TestMethod]
    public void Build_CompletesForLongSlowSequences()
    {
        const int cameraCount = 130;

        var level = FlybyTestFactory.CreateLevel();
        var positions = new Vector3[cameraCount];

        for (int i = 0; i < positions.Length; i++)
            positions[i] = new Vector3(0.0f, 0.0f, i * 1024.0f);

        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 4, positions);

        foreach (var camera in cameras)
            camera.Speed = FlybyConstants.MinSpeed;

        var task = Task.Run(() => FlybySequenceTiming.Build(cameras, useSmoothPause: false));

        Assert.IsTrue(task.Wait(TimeSpan.FromSeconds(5.0)));

        var timing = task.Result;
        float lastCameraTime = timing.GetCameraTime(cameras.Count - 1);

        Assert.AreEqual(cameraCount, timing.CameraCount);
        Assert.IsTrue(float.IsFinite(lastCameraTime));
        Assert.IsTrue(lastCameraTime > 0.0f);
    }
}
