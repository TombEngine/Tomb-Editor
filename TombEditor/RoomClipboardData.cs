using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using TombLib;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Utils;

namespace TombEditor
{
    [Serializable]
    public class RoomClipboardData
    {
        private const int _magicWord = 0x1f6cf4f5;

        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        public struct ContourLine
        {
            public float StartX; // Vector2 can't be serialized for no good reason.
            public float StartY;
            public float EndX;
            public float EndY;
            public Vector2 Start => new Vector2(StartX, StartY);
            public Vector2 End => new Vector2(EndX, EndY);
        }

        private readonly byte[] _data;
        private readonly List<ContourLine> _contourLines;
        private readonly float _dropPositionX; // Vector2 can't be serialized for no good reason.
        private readonly float _dropPositionY;
        private readonly string _levelPath;

        public RoomClipboardData(Editor editor, Vector2 dropPosition)
        {
            _dropPositionX = dropPosition.X;
            _dropPositionY = dropPosition.Y;

            // Collect contour lines
            _contourLines = new List<ContourLine>();
            foreach (Room room in editor.SelectedRooms)
                for (int z = 1; z < room.NumZSectors; ++z)
                    for (int x = 1; x < room.NumXSectors; ++x)
                    {
                        Sector thisSector = room.Sectors[x, z];
                        Sector aboveSector = room.Sectors[x, z - 1];
                        Sector leftSector = room.Sectors[x - 1, z];
                        if (aboveSector.IsAnyWall != thisSector.IsAnyWall)
                            _contourLines.Add(new ContourLine
                            {
                                StartX = x + room.Position.X, StartY = z + room.Position.Z,
                                EndX = x + room.Position.X + 1, EndY = z + room.Position.Z
                            });
                        if (leftSector.IsAnyWall != thisSector.IsAnyWall)
                            _contourLines.Add(new ContourLine
                            {
                                StartX = x + room.Position.X, StartY = z + room.Position.Z,
                                EndX = x + room.Position.X, EndY = z + room.Position.Z + 1
                            });
                    }

            // Write data
            _levelPath = editor.Level.Settings.LevelFilePath ?? "";
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(stream);
                Prj2Writer.SaveToPrj2(stream, editor.Level, new Prj2Writer.Filter
                {
                    RoomPredicate = room => editor.SelectedRoomsContains(room)
                });
                _data = stream.GetBuffer();
            }
        }

        public RoomClipboardData(Editor editor)
            : this(editor, editor.SelectedRooms.Aggregate(
                RectangleInt2.MaxMin,
                (area, room) => room.WorldArea.Union(area),
                area => area.GetMid()))
        { }

        public IReadOnlyList<ContourLine> ContourLines => _contourLines;

        public Vector2 DropPosition => new Vector2(_dropPositionX, _dropPositionY);

        public Level CreateLevel()
        {
            using (var stream = new MemoryStream(_data, false))
            {
                Level level = Prj2Loader.LoadFromPrj2(_levelPath, stream, new ProgressReporterSimple(),new CancellationToken(false),
                    new Prj2Loader.Settings { IgnoreWads = true });
                return level;
            }
        }

        public void MergeInto(Editor editor, VectorInt2 offset)
        {
            var level = CreateLevel();
            var newRooms = level.ExistingRooms;

            foreach (Room room in newRooms)
            {
                room.Name += " (Copy)";
                room.Position += new VectorInt3(offset.X, 0, offset.Y);
            }

            var newLevelSettings = editor.Level.Settings.Clone();
            editor.Level.MergeFrom(level, true, newSettings => editor.UpdateLevelSettings(newSettings));
            editor.RoomListChange();
            editor.SelectRoomsAndResetCamera(newRooms);

            // Refresh script IDs for all pasted objects
            if (editor.Level.IsTombEngine || editor.Level.IsTRX)
                newRooms.SelectMany(r => r.Objects.OfType<IHasLuaName>().Where(ob => ob.SupportsLuaName())).ToList().ForEach(i => i.AllocateNewLuaName());
            if (editor.Level.IsNG)
                newRooms.ForEach(r => r.Objects.Where(ob => ob is PositionAndScriptBasedObjectInstance).ToList().ForEach(i => ((PositionAndScriptBasedObjectInstance)i).AllocateNewScriptId()));
        }
    }
}