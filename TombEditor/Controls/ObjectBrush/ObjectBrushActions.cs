using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Controls.ObjectBrush
{
    public static class Actions
    {
        private static int _pencilItemIndex;

        private static Vector3 AlignWorldPosToGrid(Vector3 pos)
        {
            float half = Level.SectorSizeUnit * 0.5f;
            return new Vector3(
                (float)Math.Floor(pos.X / Level.SectorSizeUnit) * Level.SectorSizeUnit + half,
                pos.Y,
                (float)Math.Floor(pos.Z / Level.SectorSizeUnit) * Level.SectorSizeUnit + half);
        }

        // Stroke-session state: lives from BeginBrushStroke to EndBrushStroke.
        private static readonly List<UndoRedoInstance> _strokeUndoList = new List<UndoRedoInstance>();
        private static readonly List<PositionBasedObjectInstance> _strokePlacedObjects = new List<PositionBasedObjectInstance>();
        private static readonly HashSet<ObjectInstance> _strokeProcessedObjects = new HashSet<ObjectInstance>();

        // Direction angle for FollowMouseDirection mode. Set by Panel3D before each stroke call.
        internal static float? MouseDirectionAngle;

        private static void ExecuteBrushAction(Editor editor, Room room, float localX, float localZ)
        {
            if (editor.ChosenItems.Count == 0)
                return;

            var sectorConstraint = editor.SelectedSectors.Valid && !editor.SelectedSectors.Empty ? (RectangleInt2?)editor.SelectedSectors.Area : null;

            switch (editor.Tool.Tool)
            {
                case EditorToolType.Brush:
                    var placed = Helper.PlaceObjectsWithBrush(editor, room, localX, localZ, sectorConstraint);
                    _strokeUndoList.AddRange(placed.Select(o => new AddRemoveObjectUndoInstance(editor.UndoManager, o, true)));
                    _strokePlacedObjects.AddRange(placed);
                    break;

                case EditorToolType.Eraser:
                    var removed = Helper.EraseObjectsWithBrush(editor, room, localX, localZ, sectorConstraint);
                    _strokeUndoList.AddRange(removed.Select(r => new AddRemoveObjectUndoInstance(editor.UndoManager, r.Obj, false, r.Room)));
                    break;

                case EditorToolType.ObjectSelection:
                    Helper.SelectObjectsWithBrush(editor, room, localX, localZ, sectorConstraint, _strokeProcessedObjects);
                    break;

                case EditorToolType.ObjectDeselection:
                    Helper.SelectObjectsWithBrush(editor, room, localX, localZ, sectorConstraint, _strokeProcessedObjects, deselect: true);
                    break;

                case EditorToolType.Pencil:
                case EditorToolType.Line:
                    var pencilPlaced = Helper.PlaceObjectWithPencil(editor, room, localX, localZ, ref _pencilItemIndex,
                        sectorConstraint, skipOverlapCheck: editor.Tool.Tool == EditorToolType.Line);
                    _strokeUndoList.AddRange(pencilPlaced.Select(o => new AddRemoveObjectUndoInstance(editor.UndoManager, o, true)));
                    _strokePlacedObjects.AddRange(pencilPlaced);
                    break;
            }
        }

        public static Vector3 BeginBrushStroke(Editor editor, Room room, Vector3 cursorWorldPos)
        {
            _pencilItemIndex = 0;
            _strokeUndoList.Clear();
            _strokePlacedObjects.Clear();
            _strokeProcessedObjects.Clear();

            if (editor.Configuration.ObjectBrush_AlignToGrid && (editor.Tool.Tool == EditorToolType.Pencil || editor.Tool.Tool == EditorToolType.Line))
                cursorWorldPos = AlignWorldPosToGrid(cursorWorldPos);

            float localX = cursorWorldPos.X - room.WorldPos.X;
            float localZ = cursorWorldPos.Z - room.WorldPos.Z;

            ExecuteBrushAction(editor, room, localX, localZ);
            return cursorWorldPos;
        }

        // Returns true if a paint action was performed (moved far enough from last position).

        public static bool ContinueBrushStroke(Editor editor, Room room, Vector3 cursorWorldPos,
            Vector3? lastWorldPosition, float quantizationDistance)
        {
            if (editor.Configuration.ObjectBrush_AlignToGrid && editor.Tool.Tool == EditorToolType.Pencil)
                cursorWorldPos = AlignWorldPosToGrid(cursorWorldPos);

            if (lastWorldPosition.HasValue)
            {
                float dx = cursorWorldPos.X - lastWorldPosition.Value.X;
                float dz = cursorWorldPos.Z - lastWorldPosition.Value.Z;

                if (dx * dx + dz * dz < quantizationDistance * quantizationDistance)
                    return false;
            }

            if (editor.ChosenItems.Count == 0)
                return false;

            float localX = cursorWorldPos.X - room.WorldPos.X;
            float localZ = cursorWorldPos.Z - room.WorldPos.Z;

            ExecuteBrushAction(editor, room, localX, localZ);
            return true;
        }

        public static void ExecuteFill(Editor editor, Room selectedRoom)
        {
            if (editor.ChosenItems.Count == 0)
                return;

            var sectorConstraint = editor.SelectedSectors.Valid && !editor.SelectedSectors.Empty
                ? (RectangleInt2?)editor.SelectedSectors.Area
                : (RectangleInt2?)new RectangleInt2(1, 1, selectedRoom.NumXSectors - 2, selectedRoom.NumZSectors - 2);

            var placed = Helper.FillAreaWithObjects(editor, selectedRoom, editor.ChosenItems, sectorConstraint.Value);
            if (placed.Count == 0)
                return;

            EditorActions.AllocateScriptIds(placed);
            editor.UndoManager.Push(placed.Select(o => (UndoRedoInstance)new AddRemoveObjectUndoInstance(editor.UndoManager, o, true)).ToList());
        }

        public static void EndBrushStroke(Editor editor)
        {
            EditorActions.AllocateScriptIds(_strokePlacedObjects);

            if (_strokeUndoList.Count > 0)
                editor.UndoManager.Push(_strokeUndoList.ToList());

            _strokeUndoList.Clear();
            _strokePlacedObjects.Clear();
        }
    }
}
