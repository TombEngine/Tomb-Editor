using System.Numerics;
using TombEditor.Controls.FlybyTimeline;
using TombEditor.Controls.FlybyTimeline.Sequence;

namespace TombEditor.Tests.FlybyTimeline;

[TestClass]
public class FlybySequenceCacheTests
{
    [TestMethod]
    public void Constructor_IsInvalidWhenFewerThanTwoAssignedCamerasExist()
    {
        var level = FlybyTestFactory.CreateLevel();
        var camera = FlybyTestFactory.AddCamera(level.Rooms[0], 1, 0, Vector3.Zero);
        var cache = FlybySequenceCache.Build([camera], useSmoothPause: false);

        Assert.IsFalse(cache.IsValid);
        Assert.AreEqual(0.0f, cache.TotalDuration, 0.001f);
    }

    [TestMethod]
    public void SampleAtTime_ClampsToFirstAndLastFrame()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 2,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f));

        var cache = FlybySequenceCache.Build(cameras, useSmoothPause: false);

        var firstFrame = cache.SampleAtTime(float.NegativeInfinity);
        var lastFrame = cache.SampleAtTime(float.PositiveInfinity);

        Assert.AreEqual(cameras[0].Position, firstFrame.Position);
        Assert.AreEqual(cameras[1].Position, lastFrame.Position);
    }

    [TestMethod]
    public void TimelineAndPlaybackTimeConversions_SkipCutRegions()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 3,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f),
            new Vector3(0.0f, 0.0f, 3072.0f));

        cameras[1].Flags = FlybyConstants.FlagCameraCut;
        cameras[1].Timer = 3;

        var cache = FlybySequenceCache.Build(cameras, useSmoothPause: false);
        var cutRegion = cache.Timing.CutRegions[0];
        float playbackAfterCut = cache.Timing.TimelineToPlaybackTime(cutRegion.EndTime + FlybyConstants.TimeStep);
        float timelineAfterCut = cache.Timing.PlaybackToTimelineTime(playbackAfterCut);

        Assert.IsTrue(playbackAfterCut < cutRegion.EndTime);
        Assert.AreEqual(cutRegion.EndTime + FlybyConstants.TimeStep, timelineAfterCut, 0.001f);
    }

    [TestMethod]
    public void GetSpeedAtTime_ReturnsInvalidInsideCutRegionsAndOutsideRange()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 4,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f),
            new Vector3(0.0f, 0.0f, 3072.0f));

        cameras[1].Flags = FlybyConstants.FlagCameraCut;
        cameras[1].Timer = 3;

        var cache = FlybySequenceCache.Build(cameras, useSmoothPause: false);
        var cutRegion = cache.Timing.CutRegions[0];
        float insideCut = (cutRegion.StartTime + cutRegion.EndTime) * 0.5f;

        Assert.AreEqual(-1.0f, cache.GetSpeedAtTime(-0.1f), 0.001f);
        Assert.AreEqual(-1.0f, cache.GetSpeedAtTime(insideCut), 0.001f);
        Assert.IsTrue(cache.GetSpeedAtTime(0.0f) > 0.0f);
    }

    [TestMethod]
    public void SampleAtTime_AfterCut_ResetsSplineFromSkippedCamera()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 5,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(1024.0f, 0.0f, 2048.0f),
            new Vector3(0.0f, 0.0f, 3072.0f),
            new Vector3(0.0f, 0.0f, 4096.0f));

        cameras[1].Flags = FlybyConstants.FlagCameraCut;
        cameras[1].Timer = 3;

        var cache = FlybySequenceCache.Build(cameras, useSmoothPause: false);
        var cutRegion = cache.Timing.CutRegions[0];
        var firstPostCutFrame = cache.SampleAtTime(cutRegion.EndTime + FlybyConstants.TimeStep);

        Assert.AreEqual(0.0f, firstPostCutFrame.Position.X, 0.01f);
        Assert.IsTrue(firstPostCutFrame.Position.Z > cameras[3].Position.Z);
    }
}
