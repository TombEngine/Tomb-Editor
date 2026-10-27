using System.Numerics;
using TombEditor.Controls.FlybyTimeline;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;

namespace TombEditor.Tests.FlybyTimeline;

[TestClass]
public class FlybySequenceHelperTests
{
    [TestMethod]
    public void GetCameras_ReturnsOrderedSequenceSubsetAcrossRooms()
    {
        var level = FlybyTestFactory.CreateLevel();
        var firstRoom = level.Rooms[0];
        var secondRoom = FlybyTestFactory.CreateRoom(level, 1, new VectorInt3(10, 0, 0));

        FlybyTestFactory.AddCamera(firstRoom, 7, 5, new Vector3(0.0f, 0.0f, 0.0f));
        FlybyTestFactory.AddCamera(secondRoom, 7, 2, new Vector3(128.0f, 0.0f, 0.0f));
        FlybyTestFactory.AddCamera(firstRoom, 3, 0, new Vector3(256.0f, 0.0f, 0.0f));
        FlybyTestFactory.AddCamera(secondRoom, 7, 9, new Vector3(384.0f, 0.0f, 0.0f));

        var cameras = FlybySequenceHelper.GetCameras(level, 7);

        CollectionAssert.AreEqual(new ushort[] { 2, 5, 9 }, cameras.Select(camera => camera.Number).ToArray());
        Assert.IsTrue(cameras.All(camera => camera.Sequence == 7));
    }

    [TestMethod]
    public void GetAllSequences_ReturnsDistinctSequenceIds()
    {
        var level = FlybyTestFactory.CreateLevel();
        var firstRoom = level.Rooms[0];
        var secondRoom = FlybyTestFactory.CreateRoom(level, 1);

        FlybyTestFactory.AddCamera(firstRoom, 2, 0, Vector3.Zero);
        FlybyTestFactory.AddCamera(firstRoom, 2, 1, new Vector3(0.0f, 0.0f, 1024.0f));
        FlybyTestFactory.AddCamera(secondRoom, 9, 0, new Vector3(0.0f, 0.0f, 2048.0f));

        var sequences = FlybySequenceHelper.GetAllSequences(level);

        CollectionAssert.AreEquivalent(new ushort[] { 2, 9 }, sequences.ToArray());
    }

    [TestMethod]
    public void GetFreezeDuration_ReturnsSecondsOnlyForNonCutFreezeCameras()
    {
        var freezeCamera = new FlybyCameraInstance
        {
            Flags = FlybyConstants.FlagFreezeCamera,
            Timer = FlybyTestFactory.FreezeFrames(60)
        };

        var cutFreezeCamera = new FlybyCameraInstance
        {
            Flags = FlybyConstants.FlagFreezeCamera | FlybyConstants.FlagCameraCut,
            Timer = FlybyTestFactory.FreezeFrames(60)
        };

        Assert.AreEqual(2.0f, FlybySequenceHelper.GetFreezeDuration(freezeCamera), 0.001f);
        Assert.AreEqual(0.0f, FlybySequenceHelper.GetFreezeDuration(cutFreezeCamera), 0.001f);
    }

    [TestMethod]
    public void FindCameraIndexAtTimeAndFindInsertionIndex_UsePrecomputedTiming()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 4,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f));

        var timing = FlybySequenceTiming.Build(cameras, useSmoothPause: false);
        float midpoint = (timing.GetCameraTime(0) + timing.GetCameraTime(1)) * 0.5f;
        float slightlyAfterMidpoint = midpoint + 0.01f;

        Assert.AreEqual(0, FlybySequenceHelper.FindCameraIndexAtTime(cameras, midpoint * 0.5f, timing));
        Assert.AreEqual(0, FlybySequenceHelper.FindCameraIndexAtTime(cameras, midpoint, timing));
        Assert.AreEqual(1, FlybySequenceHelper.FindCameraIndexAtTime(cameras, slightlyAfterMidpoint, timing));
        Assert.AreEqual(2, FlybySequenceHelper.FindCameraIndexAtTime(cameras, timing.GetCameraTime(2), timing));
        Assert.AreEqual(1, FlybySequenceHelper.FindInsertionIndex(cameras, midpoint, timing));
        Assert.AreEqual(cameras.Count, FlybySequenceHelper.FindInsertionIndex(cameras, -0.01f, timing));
        Assert.AreEqual(cameras.Count, FlybySequenceHelper.FindInsertionIndex(cameras, timing.GetCameraTime(cameras.Count - 1), timing));
        Assert.AreEqual(cameras.Count, FlybySequenceHelper.FindInsertionIndex(cameras, float.NaN, timing));
    }

    [TestMethod]
    public void FormattersAndFlagHelpers_HandleEdgeCases()
    {
        Assert.AreEqual("00:01.23", FlybySequenceHelper.FormatTimecode(1.239f));
        Assert.AreEqual("1.24", FlybySequenceHelper.FormatRulerLabel(1.239f));
        Assert.AreEqual("00:00.00", FlybySequenceHelper.FormatTimecode(float.PositiveInfinity));
        Assert.AreEqual("0.00", FlybySequenceHelper.FormatRulerLabel(float.NaN));

        Assert.IsTrue(FlybySequenceHelper.GetFlagBit(1 << 15, 15));
        Assert.IsFalse(FlybySequenceHelper.GetFlagBit(ushort.MaxValue, 16));
        Assert.IsTrue(FlybySequenceHelper.GetFlagBit(1 << 7, 7));
        Assert.IsFalse(FlybySequenceHelper.GetFlagBit(0, 20));
        Assert.AreEqual((ushort)(1 << 3), FlybySequenceHelper.SetFlagBit(0, 3, true));
        Assert.AreEqual((ushort)0, FlybySequenceHelper.SetFlagBit(1 << 3, 3, false));
    }

    [TestMethod]
    public void CameraListsMatchByReference_RequiresSameInstancesInSameOrder()
    {
        var first = new FlybyCameraInstance();
        var second = new FlybyCameraInstance();
        IReadOnlyList<FlybyCameraInstance> original = [first, second];
        IReadOnlyList<FlybyCameraInstance> sameOrder = [first, second];
        IReadOnlyList<FlybyCameraInstance> reversed = [second, first];
        IReadOnlyList<FlybyCameraInstance> differentInstance = [first, new FlybyCameraInstance()];

        Assert.IsTrue(FlybySequenceHelper.CameraListsMatchByReference(original, sameOrder));
        Assert.IsFalse(FlybySequenceHelper.CameraListsMatchByReference(original, reversed));
        Assert.IsFalse(FlybySequenceHelper.CameraListsMatchByReference(original, differentInstance));
        Assert.IsFalse(FlybySequenceHelper.CameraListsMatchByReference(original, null));
    }

    [TestMethod]
    public void SolveSegmentSpeedForTargetTime_AdjustsTimingTowardRequestedTarget()
    {
        var level = FlybyTestFactory.CreateLevel();
        var cameras = FlybyTestFactory.CreateLinearSequence(level.Rooms[0], 5,
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1024.0f),
            new Vector3(0.0f, 0.0f, 2048.0f));

        var originalTiming = FlybySequenceTiming.Build(cameras, useSmoothPause: false);
        float targetTime = originalTiming.GetCameraTime(1) * 0.6f;

        float solvedSpeed = FlybySequenceHelper.SolveSegmentSpeedForTargetTime(cameras, 0, 1, targetTime, useSmoothPause: false);
        cameras[0].Speed = solvedSpeed;

        var adjustedTiming = FlybySequenceTiming.Build(cameras, useSmoothPause: false);

        Assert.IsTrue(solvedSpeed >= FlybyConstants.MinSpeed);
        Assert.AreEqual(targetTime, adjustedTiming.GetCameraTime(1), 0.05f);
    }

    [TestMethod]
    public void SnapSpeedToStep_RoundsToNearestIncrement()
    {
        float snappedDown = FlybySequenceHelper.SnapSpeedToStep(1.124f, FlybyConstants.TimelineDragSpeedStep);
        float snappedUp = FlybySequenceHelper.SnapSpeedToStep(1.126f, FlybyConstants.TimelineDragSpeedStep);

        Assert.AreEqual(1.12f, snappedDown, 0.001f);
        Assert.AreEqual(1.13f, snappedUp, 0.001f);
    }

    [TestMethod]
    public void ApplyEditorCameraRotation_CopiesOrientationAndFov()
    {
        var editorCamera = new FreeCamera(new Vector3(10.0f, 20.0f, 30.0f), 0.0f, 0.0f,
            -MathF.PI * 0.5f, MathF.PI * 0.5f, MathC.DegToRad(70.0f))
        {
            Target = new Vector3(1010.0f, -480.0f, 530.0f)
        };

        var flyby = new FlybyCameraInstance();

        FlybySequenceHelper.ApplyEditorCameraRotation(editorCamera, flyby);

        Assert.AreEqual(63.4349f, flyby.RotationY, 0.01f);
        Assert.AreEqual(-24.0948f, flyby.RotationX, 0.01f);
        Assert.AreEqual(70.0f, flyby.Fov, 0.001f);
    }
}
