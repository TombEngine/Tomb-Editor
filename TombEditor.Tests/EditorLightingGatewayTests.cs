using System.Numerics;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.Tests;

[TestClass]
public class EditorLightingGatewayTests
{
    [TestMethod]
    public void DeferredLightingUpdateInvalidatesWithoutRaisingEvent()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        room.RebuildLighting(highQualityLighting: false);

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        editor.UpdateRoomLighting(room);

        Assert.IsTrue(room.PendingRelight);
        Assert.AreEqual(0, lightingEventCount);
        Assert.IsFalse(editor.HasUnsavedChanges);
    }

    [TestMethod]
    public void ImmediateLightingUpdateRelightsAndRaisesNonDirtyEvent()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        room.InvalidateLighting();
        editor.Mode = EditorMode.Lighting;

        var lightingEvents = new List<Editor.RoomLightingChangedEvent>();

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent lightingEvent)
                lightingEvents.Add(lightingEvent);
        };

        room.InvalidateLighting();
        editor.UpdateRoomLighting(room);

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(1, lightingEvents.Count);
        Assert.AreSame(room, lightingEvents[0].Room);
        Assert.IsFalse(editor.HasUnsavedChanges);
    }

    [TestMethod]
    public void EnteringLightingModeRelightsOnlyPendingRooms()
    {
        using var editor = CreateEditor();
        var cleanRoom = editor.Level.Rooms[0];

        var pendingRoom = new Room(editor.Level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, Vector3.Zero, "Room 1");
        editor.Level.Rooms[1] = pendingRoom;

        cleanRoom.RebuildLighting(highQualityLighting: false);
        pendingRoom.InvalidateLighting();

        var lightingEvents = new List<Editor.RoomLightingChangedEvent>();

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent lightingEvent)
                lightingEvents.Add(lightingEvent);
        };

        editor.Mode = EditorMode.Lighting;

        Assert.IsFalse(cleanRoom.PendingRelight);
        Assert.IsFalse(pendingRoom.PendingRelight);
        Assert.AreEqual(1, lightingEvents.Count);
        Assert.AreSame(pendingRoom, lightingEvents[0].Room);
    }

    [TestMethod]
    public void SameRoomLightTransformUndoRelightsOneRoom()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];
        var light = new LightInstance(LightType.Point);

        room.AddObject(editor.Level, light);

        editor.Mode = EditorMode.Lighting;
        editor.UpdateRoomLighting(room);

        var lightingEvents = new List<Editor.RoomLightingChangedEvent>();

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent lightingEvent)
                lightingEvents.Add(lightingEvent);
        };

        var undo = new TransformObjectUndoInstance(editor.UndoManager, light);

        light.Position += new Vector3(1024.0f, 0.0f, 0.0f);
        editor.UpdateRoomLighting(room);
        lightingEvents.Clear();

        undo.UndoAction();

        Assert.AreEqual(Vector3.Zero, light.Position);
        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(1, lightingEvents.Count);
        Assert.AreSame(room, lightingEvents[0].Room);
    }

    [TestMethod]
    public void BulkLightingUpdateDeduplicatesRooms()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        editor.Mode = EditorMode.Lighting;
        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        room.InvalidateLighting();
        editor.UpdateRoomsLighting([room, room]);

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(1, lightingEventCount);
    }

    [TestMethod]
    public void SingleLightColorChangeDefersOutsideLightingMode()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];
        var light = new LightInstance(LightType.Point);

        room.AddObject(editor.Level, light);
        room.RebuildLighting(highQualityLighting: false);

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        EditorActions.ApplyObjectColor(light, new Vector3(1.0f, 0.5f, 0.25f));

        Assert.IsTrue(room.PendingRelight);
        Assert.AreEqual(0, lightingEventCount);
    }

    [TestMethod]
    public void LightGroupColorChangeRelightsEachAffectedRoomOnce()
    {
        using var editor = CreateEditor();
        var sourceRoom = editor.Level.Rooms[0];

        var destinationRoom = new Room(editor.Level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, Vector3.Zero, "Room 1");
        editor.Level.Rooms[1] = destinationRoom;

        var sourceLight = new LightInstance(LightType.Point);
        var destinationLight = new LightInstance(LightType.Point);

        sourceRoom.AddObject(editor.Level, sourceLight);
        destinationRoom.AddObject(editor.Level, destinationLight);

        sourceRoom.RebuildLighting(highQualityLighting: false);
        destinationRoom.RebuildLighting(highQualityLighting: false);

        editor.Mode = EditorMode.Lighting;

        var lightingEvents = new List<Editor.RoomLightingChangedEvent>();

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent lightingEvent)
                lightingEvents.Add(lightingEvent);
        };

        var group = new ObjectGroup(sourceLight)
        {
            destinationLight
        };

        var color = new Vector3(1.0f, 0.5f, 0.25f);
        EditorActions.ApplyObjectColor(group, color);

        Assert.AreEqual(color, sourceLight.Color);
        Assert.AreEqual(color, destinationLight.Color);

        Assert.AreEqual(1, lightingEvents.Count(eventObject => eventObject.Room == sourceRoom));
        Assert.AreEqual(1, lightingEvents.Count(eventObject => eventObject.Room == destinationRoom));
    }

    [TestMethod]
    public void LightGroupTransformUndoRelightsRoomOnce()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        var firstLight = new LightInstance(LightType.Point);
        var secondLight = new LightInstance(LightType.Point);

        room.AddObject(editor.Level, firstLight);
        room.AddObject(editor.Level, secondLight);
        room.RebuildLighting(highQualityLighting: false);

        editor.Mode = EditorMode.Lighting;

        var group = new ObjectGroup(firstLight)
        {
            secondLight
        };

        editor.UndoManager.PushObjectTransformed(group);
        group.Position += new Vector3(1024.0f, 0.0f, 0.0f);
        editor.UpdateRoomLighting(room);

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        editor.UndoManager.Undo();

        Assert.AreEqual(Vector3.Zero, firstLight.Position);
        Assert.AreEqual(Vector3.Zero, secondLight.Position);
        Assert.AreEqual(1, lightingEventCount);

        lightingEventCount = 0;
        editor.UndoManager.Redo();

        Assert.AreEqual(new Vector3(1024.0f, 0.0f, 0.0f), firstLight.Position);
        Assert.AreEqual(new Vector3(1024.0f, 0.0f, 0.0f), secondLight.Position);
        Assert.AreEqual(1, lightingEventCount);
    }

    [TestMethod]
    public void LightGroupPropertyUndoRelightsRoomOnce()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        var firstLight = new LightInstance(LightType.Point)
        {
            Color = new Vector3(0.25f, 0.5f, 0.75f)
        };

        var secondLight = new LightInstance(LightType.Point)
        {
            Color = new Vector3(0.75f, 0.5f, 0.25f)
        };

        room.AddObject(editor.Level, firstLight);
        room.AddObject(editor.Level, secondLight);
        room.RebuildLighting(highQualityLighting: false);

        editor.Mode = EditorMode.Lighting;

        var group = new ObjectGroup(firstLight)
        {
            secondLight
        };

        editor.UndoManager.PushObjectPropertyChanged(group);
        group.Color = new Vector3(1.0f, 1.0f, 1.0f);
        editor.UpdateRoomLighting(room);

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        editor.UndoManager.Undo();

        Assert.AreEqual(new Vector3(0.25f, 0.5f, 0.75f), firstLight.Color);
        Assert.AreEqual(new Vector3(0.75f, 0.5f, 0.25f), secondLight.Color);
        Assert.AreEqual(1, lightingEventCount);

        lightingEventCount = 0;
        editor.UndoManager.Redo();

        Assert.AreEqual(Vector3.One, firstLight.Color);
        Assert.AreEqual(Vector3.One, secondLight.Color);
        Assert.AreEqual(1, lightingEventCount);
    }

    [TestMethod]
    public void LightingUpdateRejectsNullRoom()
    {
        using var editor = CreateEditor();
        Assert.ThrowsException<ArgumentNullException>(() => editor.UpdateRoomLighting(null));
    }

    [TestMethod]
    public void CrossRoomLightTransformUndoRelightsBothRooms()
    {
        using var editor = CreateEditor();
        var sourceRoom = editor.Level.Rooms[0];

        var destinationRoom = new Room(editor.Level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, Vector3.Zero, "Room 1");
        editor.Level.Rooms[1] = destinationRoom;

        var light = new LightInstance(LightType.Point);
        editor.Mode = EditorMode.Lighting;

        sourceRoom.AddObject(editor.Level, light);
        editor.UpdateRoomLighting(sourceRoom);

        var undo = new TransformObjectUndoInstance(editor.UndoManager, light);

        destinationRoom.MoveObjectFrom(editor.Level, sourceRoom, light);
        editor.UpdateRoomsLighting([sourceRoom, destinationRoom]);

        undo.UndoAction();

        Assert.AreSame(sourceRoom, light.Room);
        Assert.IsFalse(sourceRoom.PendingRelight);
        Assert.IsFalse(destinationRoom.PendingRelight);
    }

    [TestMethod]
    public void NormalLightMovesRelightBothRoomsAndMixedRoomGroupsOnce()
    {
        using var editor = CreateEditor();
        var sourceRoom = editor.Level.Rooms[0];

        var destinationRoom = new Room(editor.Level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, Vector3.Zero, "Room 1");
        editor.Level.Rooms[1] = destinationRoom;

        var firstLight = new LightInstance(LightType.Point);
        var secondLight = new LightInstance(LightType.Point);

        sourceRoom.AddObject(editor.Level, firstLight);
        destinationRoom.AddObject(editor.Level, secondLight);

        editor.Mode = EditorMode.Lighting;

        var lightingEvents = new List<Editor.RoomLightingChangedEvent>();

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent lightingEvent)
                lightingEvents.Add(lightingEvent);
        };

        EditorActions.MoveObject(firstLight, destinationRoom, new VectorInt2(1, 1));

        Assert.AreSame(destinationRoom, firstLight.Room);
        Assert.IsFalse(sourceRoom.PendingRelight);
        Assert.IsFalse(destinationRoom.PendingRelight);

        Assert.AreEqual(1, lightingEvents.Count(lightingEvent => lightingEvent.Room == sourceRoom));
        Assert.AreEqual(1, lightingEvents.Count(lightingEvent => lightingEvent.Room == destinationRoom));

        lightingEvents.Clear();
        editor.UpdateRoomLighting(destinationRoom);
        lightingEvents.Clear();

        EditorActions.MoveObjectToOtherRoom(secondLight, sourceRoom);

        Assert.AreSame(sourceRoom, secondLight.Room);
        Assert.IsFalse(sourceRoom.PendingRelight);
        Assert.IsFalse(destinationRoom.PendingRelight);

        Assert.AreEqual(1, lightingEvents.Count(lightingEvent => lightingEvent.Room == sourceRoom));
        Assert.AreEqual(1, lightingEvents.Count(lightingEvent => lightingEvent.Room == destinationRoom));

        lightingEvents.Clear();

        var mixedRoomGroup = new ObjectGroup(firstLight)
        {
            secondLight
        };

        EditorActions.RebuildLightsForObject(mixedRoomGroup);

        Assert.AreEqual(1, lightingEvents.Count(lightingEvent => lightingEvent.Room == sourceRoom));
        Assert.AreEqual(1, lightingEvents.Count(lightingEvent => lightingEvent.Room == destinationRoom));
    }

    [TestMethod]
    public void NormalNonLightMoveDoesNotRelightRooms()
    {
        using var editor = CreateEditor();
        var sourceRoom = editor.Level.Rooms[0];

        var destinationRoom = new Room(editor.Level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, Vector3.Zero, "Room 1");
        editor.Level.Rooms[1] = destinationRoom;

        var camera = new CameraInstance();
        sourceRoom.AddObject(editor.Level, camera);
        sourceRoom.RebuildLighting(highQualityLighting: false);
        destinationRoom.RebuildLighting(highQualityLighting: false);

        editor.Mode = EditorMode.Lighting;

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        EditorActions.MoveObject(camera, destinationRoom, new VectorInt2(1, 1));

        Assert.AreSame(destinationRoom, camera.Room);
        Assert.IsFalse(sourceRoom.PendingRelight);
        Assert.IsFalse(destinationRoom.PendingRelight);
        Assert.AreEqual(0, lightingEventCount);
    }

    [TestMethod]
    public void PreviewQualityChangeDefersOutsideLightingAndRelightsInLightingMode()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        room.RebuildLighting(highQualityLighting: false);

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        editor.Configuration.Rendering3D_HighQualityLightPreview = true;
        editor.ConfigurationChange();

        Assert.IsTrue(room.PendingRelight);
        Assert.AreEqual(0, lightingEventCount);

        editor.Mode = EditorMode.Lighting;

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(1, lightingEventCount);

        editor.Configuration.Rendering3D_HighQualityLightPreview = false;
        editor.ConfigurationChange();

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(2, lightingEventCount);
    }

    [TestMethod]
    public void LevelLightQualityChangeWaitsForHighQualityPreviewAndRelightsWhenActive()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        room.RebuildLighting(highQualityLighting: false);

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        var settings = editor.Level.Settings.Clone();
        settings.DefaultLightQuality = LightQuality.High;
        editor.UpdateLevelSettings(settings);

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(0, lightingEventCount);

        editor.Configuration.Rendering3D_HighQualityLightPreview = true;
        editor.ConfigurationChange();

        Assert.IsTrue(room.PendingRelight);
        Assert.AreEqual(0, lightingEventCount);

        editor.Mode = EditorMode.Lighting;

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(1, lightingEventCount);

        settings = editor.Level.Settings.Clone();
        settings.OverrideIndividualLightQualitySettings = true;
        editor.UpdateLevelSettings(settings);

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(2, lightingEventCount);
    }

    [TestMethod]
    public void DeletingLightRaisesLightingEventWithoutGeometryEvent()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];
        var light = new LightInstance(LightType.Point);

        room.AddObject(editor.Level, light);
        editor.Mode = EditorMode.Lighting;

        var lightingEventCount = 0;
        var geometryEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
            else if (eventObject is Editor.RoomGeometryChangedEvent)
                geometryEventCount++;
        };

        EditorActions.DeleteObjectWithoutUpdate(light);

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(1, lightingEventCount);
        Assert.AreEqual(0, geometryEventCount);
    }

    [TestMethod]
    public void UndoDeletedLightRelightsRoomOnce()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];
        var light = new LightInstance(LightType.Point);

        room.AddObject(editor.Level, light);
        editor.Mode = EditorMode.Lighting;

        var undo = new AddRemoveObjectUndoInstance(editor.UndoManager, light, created: false);
        EditorActions.DeleteObjectWithoutUpdate(light);

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        undo.UndoAction();

        Assert.AreSame(room, light.Room);
        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(1, lightingEventCount);
    }

    [TestMethod]
    public void DynamicOnlyLightChangeRaisesObjectChangeWithoutRelighting()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];
        var light = new LightInstance(LightType.Point);

        room.AddObject(editor.Level, light);
        room.RebuildLighting(highQualityLighting: false);

        editor.SelectedObject = light;
        editor.HasUnsavedChanges = false;

        var objectChangeCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.ObjectChangedEvent)
                objectChangeCount++;
        };

        EditorActions.UpdateLight<bool>((currentLight, value) => currentLight.IsDynamicallyUsed == value,
            (currentLight, value) => currentLight.IsDynamicallyUsed = value, _ => false, updateLighting: false);

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(1, objectChangeCount);
        Assert.IsTrue(editor.HasUnsavedChanges);
    }

    [TestMethod]
    public void StaticLightChangeStillRelights()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];
        var light = new LightInstance(LightType.Point);

        room.AddObject(editor.Level, light);
        room.RebuildLighting(highQualityLighting: false);

        editor.SelectedObject = light;

        EditorActions.UpdateLight<bool>((currentLight, value) => currentLight.IsStaticallyUsed == value,
            (currentLight, value) => currentLight.IsStaticallyUsed = value, _ => false);

        Assert.IsTrue(room.PendingRelight);
    }

    [TestMethod]
    public void ResetLightRotationRelightsAffectedRoom()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        var light = new LightInstance(LightType.Point)
        {
            RotationX = 30.0f,
            RotationY = 45.0f
        };

        room.AddObject(editor.Level, light);

        editor.Mode = EditorMode.Lighting;
        editor.UpdateRoomLighting(room);

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        EditorActions.ResetObjectRotation(light);

        Assert.AreEqual(0.0f, light.RotationX);
        Assert.AreEqual(0.0f, light.RotationY);

        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(1, lightingEventCount);
    }

    [TestMethod]
    public void ResetLightRotationDefersOutsideLightingMode()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        var light = new LightInstance(LightType.Point)
        {
            RotationY = 45.0f
        };

        room.AddObject(editor.Level, light);
        room.RebuildLighting(highQualityLighting: false);

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        EditorActions.ResetObjectRotation(light);

        Assert.AreEqual(0.0f, light.RotationY);
        Assert.IsTrue(room.PendingRelight);
        Assert.AreEqual(0, lightingEventCount);
    }

    [TestMethod]
    public void ResetNonLightRotationDoesNotRelightRoom()
    {
        using var editor = CreateEditor();
        var room = editor.Level.Rooms[0];

        var staticInstance = new StaticInstance
        {
            RotationY = 45.0f
        };

        room.AddObject(editor.Level, staticInstance);
        room.RebuildLighting(highQualityLighting: false);

        editor.Mode = EditorMode.Lighting;

        var lightingEventCount = 0;

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEventCount++;
        };

        EditorActions.ResetObjectRotation(staticInstance);

        Assert.AreEqual(0.0f, staticInstance.RotationY);
        Assert.IsFalse(room.PendingRelight);
        Assert.AreEqual(0, lightingEventCount);
    }

    [TestMethod]
    public void ResetLightGroupRotationRelightsEachAffectedRoomOnce()
    {
        using var editor = CreateEditor();
        var sourceRoom = editor.Level.Rooms[0];

        var destinationRoom = new Room(editor.Level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, Vector3.Zero, "Room 1");
        editor.Level.Rooms[1] = destinationRoom;

        var sourceLight = new LightInstance(LightType.Point)
        {
            RotationY = 30.0f
        };

        var destinationLight = new LightInstance(LightType.Point)
        {
            RotationY = 60.0f
        };

        sourceRoom.AddObject(editor.Level, sourceLight);
        destinationRoom.AddObject(editor.Level, destinationLight);

        editor.Mode = EditorMode.Lighting;
        editor.UpdateRoomsLighting([sourceRoom, destinationRoom]);

        var lightingEvents = new List<Editor.RoomLightingChangedEvent>();

        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent lightingEvent)
                lightingEvents.Add(lightingEvent);
        };

        var group = new ObjectGroup(sourceLight)
        {
            destinationLight
        };

        group.RotationY = 45.0f;

        EditorActions.ResetObjectRotation(group);

        Assert.AreEqual(0.0f, group.RotationY);
        Assert.AreEqual(30.0f, sourceLight.RotationY);
        Assert.AreEqual(60.0f, destinationLight.RotationY);

        Assert.IsFalse(sourceRoom.PendingRelight);
        Assert.IsFalse(destinationRoom.PendingRelight);

        Assert.AreEqual(1, lightingEvents.Count(eventObject => eventObject.Room == sourceRoom));
        Assert.AreEqual(1, lightingEvents.Count(eventObject => eventObject.Room == destinationRoom));
    }

    private static Editor CreateEditor()
    {
        return new Editor(new ImmediateSynchronizationContext(), new Configuration(), Level.CreateSimpleLevel());
    }

    private sealed class ImmediateSynchronizationContext : SynchronizationContext
    {
        public override void Send(SendOrPostCallback callback, object? state)
        {
            callback(state);
        }
    }
}
