using System.Numerics;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;

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

    // The "Override individual settings" checkbox in Level Settings has been hidden since 2021
    // because the code behind it went missing, but the flag is still saved in PRJ2 files,
    // so old projects can be stuck with it enabled. Lighting must keep ignoring it,
    // otherwise those levels would silently change in the editor and in the compiled output.
    // Update this test if the feature is ever implemented properly and its checkbox restored.
    [TestMethod]
    public void OverrideIndividualLightQualityDoesNotAffectLighting()
    {
        var level = Level.CreateSimpleLevel();
        var room = level.Rooms[0];

        // A wall slab next to the light creates soft shadow edges, where the number of samples changes the result.
        for (int x = 12; x < 14; x++)
            for (int z = 8; z < 12; z++)
                room.Sectors[x, z].Type = SectorType.Wall;

        var light = new LightInstance(LightType.Point)
        {
            Position = new Vector3(9728.0f, 1536.0f, 10240.0f),
            Quality = LightQuality.High
        };

        level.Settings.DefaultLightQuality = LightQuality.Low;

        var isSceneSensitiveToQuality = false;

        for (float x = 0.0f; x < 20480.0f; x += 256.0f)
            for (float z = 0.0f; z < 20480.0f; z += 256.0f)
            {
                var position = new Vector3(x, 0.0f, z);

                level.Settings.OverrideIndividualLightQualitySettings = false;
                var individualQuality = RoomGeometry.GetRaytraceResult(room, light, position, highQuality: true);
                var singleSample = RoomGeometry.GetRaytraceResult(room, light, position, highQuality: false);

                level.Settings.OverrideIndividualLightQualitySettings = true;
                var overriddenQuality = RoomGeometry.GetRaytraceResult(room, light, position, highQuality: true);

                Assert.AreEqual(individualQuality, overriddenQuality, $"Override changed the result at {position}.");
                isSceneSensitiveToQuality |= individualQuality != singleSample;
            }

        Assert.IsTrue(isSceneSensitiveToQuality, "The test scene must produce different results for different sample counts.");
    }
}
