using System.Numerics;
using System.Reflection;
using TombLib.LevelData;

namespace TombLib.Test;

[TestClass]
public class RoomLightingInvalidationTests
{
    [TestMethod]
    public void AddLightInvalidatesLighting()
    {
        var level = Level.CreateSimpleLevel();
        var room = level.Rooms[0];

        Assert.IsFalse(room.PendingRelight);

        room.AddObject(level, new LightInstance(LightType.Point));

        Assert.IsTrue(room.PendingRelight);
    }

    [TestMethod]
    public void AddNonLightDoesNotInvalidateLighting()
    {
        var level = Level.CreateSimpleLevel();
        var room = level.Rooms[0];

        room.AddObject(level, new CameraInstance());

        Assert.IsFalse(room.PendingRelight);
    }

    [TestMethod]
    public void RemoveLightInvalidatesLighting()
    {
        var level = Level.CreateSimpleLevel();
        var room = level.Rooms[0];
        var light = new LightInstance(LightType.Point);

        room.AddObject(level, light);
        room.RebuildLighting(highQualityLighting: false);
        room.RemoveObject(level, light);

        Assert.IsTrue(room.PendingRelight);
    }

    [TestMethod]
    public void RemoveNonLightDoesNotInvalidateLighting()
    {
        var level = Level.CreateSimpleLevel();
        var room = level.Rooms[0];
        var camera = new CameraInstance();

        room.AddObject(level, camera);
        room.RebuildLighting(highQualityLighting: false);
        room.RemoveObject(level, camera);

        Assert.IsFalse(room.PendingRelight);
    }

    [TestMethod]
    public void MoveLightInvalidatesBothRooms()
    {
        var level = Level.CreateSimpleLevel();
        var sourceRoom = level.Rooms[0];
        var destinationRoom = new Room(level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, Vector3.Zero, "Room 1");
        var light = new LightInstance(LightType.Point);

        sourceRoom.AddObject(level, light);
        sourceRoom.RebuildLighting(highQualityLighting: false);

        destinationRoom.MoveObjectFrom(level, sourceRoom, light);

        Assert.IsTrue(sourceRoom.PendingRelight);
        Assert.IsTrue(destinationRoom.PendingRelight);
    }

    [TestMethod]
    public void BuildGeometryInvalidatesLighting()
    {
        var level = Level.CreateSimpleLevel();
        var room = level.Rooms[0];

        room.RebuildLighting(highQualityLighting: false);
        room.BuildGeometry();

        Assert.IsTrue(room.PendingRelight);
    }

    [TestMethod]
    public void RebuildLightingClearsPendingState()
    {
        var level = Level.CreateSimpleLevel();
        var room = level.Rooms[0];

        room.InvalidateLighting();
        room.RebuildLighting(highQualityLighting: false);

        Assert.IsFalse(room.PendingRelight);
    }

    [TestMethod]
    public void OverrideIndividualLightQualityUsesDefaultQuality()
    {
        var light = new LightInstance(LightType.Point)
        {
            Quality = LightQuality.High
        };

        var getLightSampleCount = typeof(RoomGeometry).GetMethod("GetLightSampleCount", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(getLightSampleCount);

        var explicitQualitySampleCount = (int)getLightSampleCount!.Invoke(null, [light, LightQuality.Low, false])!;
        var overriddenQualitySampleCount = (int)getLightSampleCount.Invoke(null, [light, LightQuality.Low, true])!;

        Assert.AreEqual(5, explicitQualitySampleCount);
        Assert.AreEqual(1, overriddenQualitySampleCount);
    }
}
