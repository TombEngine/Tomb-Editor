using System.Numerics;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.Tests.FlybyTimeline;

internal static class FlybyTestFactory
{
    public static Level CreateLevel(TRVersion.Game version = TRVersion.Game.TR4)
        => Level.CreateSimpleLevel(version);

    public static Room CreateRoom(Level level, int roomIndex, VectorInt3? worldPos = null)
    {
        var room = new Room(level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, level.Settings.DefaultAmbientLight, $"Room {roomIndex}");

        if (worldPos is not null)
            room.WorldPos = worldPos.Value;

        level.Rooms[roomIndex] = room;
        return room;
    }

    public static FlybyCameraInstance AddCamera(Room room, ushort sequence, ushort number, Vector3 position,
        float speed = 1.0f, short timer = 0, ushort flags = 0, float rotationX = 0.0f, float rotationY = 0.0f,
        float roll = 0.0f, float fov = 80.0f)
    {
        var camera = new FlybyCameraInstance
        {
            Sequence = sequence,
            Number = number,
            Position = position,
            Speed = speed,
            Timer = timer,
            Flags = flags,
            RotationX = rotationX,
            RotationY = rotationY,
            Roll = roll,
            Fov = fov
        };

        room.AddObject(room.Level, camera);
        return camera;
    }

    public static IReadOnlyList<FlybyCameraInstance> CreateLinearSequence(Room room, ushort sequence,
        params Vector3[] positions)
    {
        var cameras = new List<FlybyCameraInstance>(positions.Length);

        for (ushort i = 0; i < positions.Length; i++)
            cameras.Add(AddCamera(room, sequence, i, positions[i]));

        return cameras;
    }

    public static short FreezeFrames(int frames)
        => (short)(frames << 4);
}
