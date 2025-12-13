using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.LevelData.Compilers;
using TombLib.LevelData.Compilers.TombEngine;
using TombLib.LevelData.IO;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombLib.LevelData.SectorServices;
using TombLib.LevelData.SectorStructs;
using TombLib.LevelData.VisualScripting;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;

namespace TombEditor
{
    public static class EditorActions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Editor _editor = Editor.Instance;

        public static bool ContinueOnFileDrop(IWin32Window owner, string description)
        {
            if (!_editor.HasUnsavedChanges || _editor.Level.Settings.HasUnknownData)
                return true;

            switch (DarkMessageBox.Show(owner,
                "Your unsaved changes will be lost. Do you want to save?",
                description,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2))
            {
                case DialogResult.No:
                    return true;
                case DialogResult.Yes:
                    return SaveLevel(owner, false);
                default:
                    return false;
            }
        }

        public static void SmartBuildGeometry(Room room, RectangleInt2 area)
        {
            _editor.RaiseEvent(new Editor.SuspendRenderingEvent());

            var watch = new Stopwatch();
            watch.Start();
            room.SmartBuildGeometry(area, _editor.Configuration.Rendering3D_HighQualityLightPreview);
            watch.Stop();
            logger.Debug("Edit geometry time: " + watch.ElapsedMilliseconds + "  ms");
            _editor.RoomGeometryChange(room);

            _editor.RaiseEvent(new Editor.ResumeRenderingEvent());
        }

        private enum SmoothGeometryEditingType
        {
            None,
            Floor,
            Wall,
            Any
        }

        public static void EditSectorGeometry(Room room, RectangleInt2 area, ArrowType arrow, SectorVerticalPart vertical, int increment, bool smooth, bool oppositeDiagonalCorner = false, bool autoSwitchDiagonals = false, bool autoUpdateThroughPortal = true, bool disableUndo = false)
        {
            if (!disableUndo)
            {
                if (smooth)
                    _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom.AndAdjoiningRooms);
                else
                {
                    HashSet<Room> affectedRooms = room.GetAdjoiningRoomsFromArea(area);
                    affectedRooms.Add(room);

                    _editor.UndoManager.PushGeometryChanged(affectedRooms);
                }
            }

            if (smooth)
            {
                // Scan selection and decide if the selected zone is wall-only, floor-only, or both.
                // It's needed to force smoothing function to edit either only wall sections or floor sections,
                // in case user wants to smoothly edit only wall splits or actual floor height.

                SmoothGeometryEditingType smoothEditingType = SmoothGeometryEditingType.None;

                for (int x = area.X0; x <= area.X1; x++)
                {
                    for (int z = area.Y0; z <= area.Y1; z++)
                    {
                        if (smoothEditingType != SmoothGeometryEditingType.Wall && room.Sectors[x, z].Type == SectorType.Floor)
                            smoothEditingType = SmoothGeometryEditingType.Floor;
                        else if (smoothEditingType != SmoothGeometryEditingType.Floor && room.Sectors[x, z].Type != SectorType.Floor)
                            smoothEditingType = SmoothGeometryEditingType.Wall;
                        else
                        {
                            smoothEditingType = SmoothGeometryEditingType.Any;
                            break;
                        }
                    }
                    if (smoothEditingType == SmoothGeometryEditingType.Any)
                        break;
                }

                VectorInt2 startCoord = area.Start;
                bool[] corners = new bool[4] { true, true, true, true };

                // Adjust editing area to exclude the side on which the arrow starts
                // This is a superset of the behaviour of the old editor to smooth edit a single edge or side.
                switch (arrow)
                {
                    case ArrowType.EdgeE:
                        area = new RectangleInt2(area.X0 + 1, area.Y0, area.X1, area.Y1);
                        break;
                    case ArrowType.EdgeN:
                        area = new RectangleInt2(area.X0, area.Y0 + 1, area.X1, area.Y1);
                        break;
                    case ArrowType.EdgeW:
                        area = new RectangleInt2(area.X0, area.Y0, area.X1 - 1, area.Y1);
                        break;
                    case ArrowType.EdgeS:
                        area = new RectangleInt2(area.X0, area.Y0, area.X1, area.Y1 - 1);
                        break;
                    case ArrowType.CornerNE:
                        area = new RectangleInt2(area.X0 + 1, area.Y0 + 1, area.X1, area.Y1);
                        break;
                    case ArrowType.CornerNW:
                        area = new RectangleInt2(area.X0, area.Y0 + 1, area.X1 - 1, area.Y1);
                        break;
                    case ArrowType.CornerSW:
                        area = new RectangleInt2(area.X0, area.Y0, area.X1 - 1, area.Y1 - 1);
                        break;
                    case ArrowType.CornerSE:
                        area = new RectangleInt2(area.X0 + 1, area.Y0, area.X1, area.Y1 - 1);
                        break;
                }

                void smoothEdit(RoomSectorPair pair, SectorEdge edge)
                {
                    if (pair.Sector == null) return;

                    if (vertical.IsOnFloor() && (pair.Sector.Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsExtraFloorSplit()) ||
                        vertical.IsOnCeiling() && (pair.Sector.Ceiling.DiagonalSplit == DiagonalSplit.None || vertical.IsExtraCeilingSplit()))
                    {
                        if (smoothEditingType == SmoothGeometryEditingType.Any ||
                           !pair.Sector.IsAnyWall && smoothEditingType == SmoothGeometryEditingType.Floor ||
                            pair.Sector.IsAnyWall && smoothEditingType == SmoothGeometryEditingType.Wall)
                        {
                            pair.Room.ChangeSectorHeight(pair.SectorPosition.X, pair.SectorPosition.Y, vertical, edge, increment);
                            pair.Sector.FixHeights(vertical);
                        }
                    }
                }

                var cornerSectors = new RoomSectorPair[4]
                {
                    room.GetSectorTryThroughPortal(area.X1 + 1, area.Y0 - 1),
                    room.GetSectorTryThroughPortal(area.X0 - 1, area.Y0 - 1),
                    room.GetSectorTryThroughPortal(area.X0 - 1, area.Y1 + 1),
                    room.GetSectorTryThroughPortal(area.X1 + 1, area.Y1 + 1)
                };

                // Unique case of editing single corner
                if(area.Width == -1 && area.Height == -1 && arrow > ArrowType.EdgeW)
                {
                    SectorEdge origin = SectorEdge.XnZn;
                    switch(arrow)
                    {
                        case ArrowType.CornerNE: origin = SectorEdge.XpZp; break;
                        case ArrowType.CornerNW: origin = SectorEdge.XnZp; break;
                        case ArrowType.CornerSE: origin = SectorEdge.XpZn; break;
                    }
                    var originSector = room.GetSectorTryThroughPortal(startCoord);
                    var originHeight = originSector.Sector.GetHeight(vertical, origin) + originSector.Room.Position.Y;
                    for (int i = 0; i < 4; i++)
                        corners[i] = originHeight == cornerSectors[i].Sector.GetHeight(vertical, (SectorEdge)i) + cornerSectors[i].Room.Position.Y;
                }

                // Smoothly change sectors on the corners
                if (corners[0]) smoothEdit(cornerSectors[0], SectorEdge.XnZp);
                if (corners[1]) smoothEdit(cornerSectors[1], SectorEdge.XpZp);
                if (corners[2]) smoothEdit(cornerSectors[2], SectorEdge.XpZn);
                if (corners[3]) smoothEdit(cornerSectors[3], SectorEdge.XnZn);

                // Smoothly change sectors on the sides
                for (int x = area.X0; x <= area.X1; x++)
                {
                    smoothEdit(room.GetSectorTryThroughPortal(x, area.Y0 - 1), SectorEdge.XnZp);
                    smoothEdit(room.GetSectorTryThroughPortal(x, area.Y0 - 1), SectorEdge.XpZp);

                    smoothEdit(room.GetSectorTryThroughPortal(x, area.Y1 + 1), SectorEdge.XnZn);
                    smoothEdit(room.GetSectorTryThroughPortal(x, area.Y1 + 1), SectorEdge.XpZn);
                }

                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    smoothEdit(room.GetSectorTryThroughPortal(area.X0 - 1, z), SectorEdge.XpZp);
                    smoothEdit(room.GetSectorTryThroughPortal(area.X0 - 1, z), SectorEdge.XpZn);

                    smoothEdit(room.GetSectorTryThroughPortal(area.X1 + 1, z), SectorEdge.XnZp);
                    smoothEdit(room.GetSectorTryThroughPortal(area.X1 + 1, z), SectorEdge.XnZn);
                }

                arrow = ArrowType.EntireFace;
            }

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    var targetSector = room.Sectors[x, z];
                    var targetRoom = room;
                    var targetPos = new VectorInt2(x, z);

                    var lookupSector = room.GetSectorTryThroughPortal(x, z);

                    EditSector:
                    {
                        if (arrow == ArrowType.EntireFace)
                        {
                            if (vertical == SectorVerticalPart.QA || vertical == SectorVerticalPart.WS)
                                targetRoom.RaiseSectorStepWise(targetPos.X, targetPos.Y, vertical, oppositeDiagonalCorner, increment, autoSwitchDiagonals);
                            else
                                targetRoom.RaiseSector(targetPos.X, targetPos.Y, vertical, increment, false);
                        }
                        else
                        {
                            var currentSplit = vertical.IsOnFloor() ? targetSector.Floor.DiagonalSplit : targetSector.Ceiling.DiagonalSplit;
                            var incrementInvalid = vertical.IsOnFloor() ? increment < 0 : increment > 0;
                            SectorEdge[] corners = new SectorEdge[2] { SectorEdge.XnZp, SectorEdge.XnZp };
                            DiagonalSplit[] splits = new DiagonalSplit[2] { DiagonalSplit.None, DiagonalSplit.None };

                            switch (arrow)
                            {
                                case ArrowType.EdgeN:
                                case ArrowType.CornerNW:
                                    corners[0] = SectorEdge.XnZp;
                                    corners[1] = SectorEdge.XpZp;
                                    splits[0] = DiagonalSplit.XpZn;
                                    splits[1] = arrow == ArrowType.CornerNW ? DiagonalSplit.XnZp : DiagonalSplit.XnZn;
                                    break;
                                case ArrowType.EdgeE:
                                case ArrowType.CornerNE:
                                    corners[0] = SectorEdge.XpZp;
                                    corners[1] = SectorEdge.XpZn;
                                    splits[0] = DiagonalSplit.XnZn;
                                    splits[1] = arrow == ArrowType.CornerNE ? DiagonalSplit.XpZp : DiagonalSplit.XnZp;
                                    break;
                                case ArrowType.EdgeS:
                                case ArrowType.CornerSE:
                                    corners[0] = SectorEdge.XpZn;
                                    corners[1] = SectorEdge.XnZn;
                                    splits[0] = DiagonalSplit.XnZp;
                                    splits[1] = arrow == ArrowType.CornerSE ? DiagonalSplit.XpZn : DiagonalSplit.XpZp;
                                    break;
                                case ArrowType.EdgeW:
                                case ArrowType.CornerSW:
                                    corners[0] = SectorEdge.XnZn;
                                    corners[1] = SectorEdge.XnZp;
                                    splits[0] = DiagonalSplit.XpZp;
                                    splits[1] = arrow == ArrowType.CornerSW ? DiagonalSplit.XnZn : DiagonalSplit.XpZn;
                                    break;
                            }

                            if (arrow <= ArrowType.EdgeW)
                            {
                                if (targetSector.Type != SectorType.Wall && currentSplit != DiagonalSplit.None && vertical <= SectorVerticalPart.WS)
                                    continue;

                                for (int i = 0; i < 2; i++)
                                    if (currentSplit != splits[i])
                                        targetRoom.ChangeSectorHeight(targetPos.X, targetPos.Y, vertical, corners[i], increment);
                            }
                            else
                            {
                                if (targetSector.Type != SectorType.Wall && currentSplit != DiagonalSplit.None && vertical <= SectorVerticalPart.WS)
                                {
                                    if (currentSplit == splits[1])
                                    {
                                        if (targetSector.GetHeight(vertical, corners[0]) == targetSector.GetHeight(vertical, corners[1]) && incrementInvalid)
                                            continue;
                                    }
                                    else if (autoSwitchDiagonals && currentSplit == splits[0] && targetSector.GetHeight(vertical, corners[0]) == targetSector.GetHeight(vertical, corners[1]) && !incrementInvalid)
                                        targetSector.Transform(new RectTransformation { QuadrantRotation = 2 }, vertical.IsOnFloor());
                                    else
                                        continue;
                                }
                                targetRoom.ChangeSectorHeight(targetPos.X, targetPos.Y, vertical, corners[0], increment);
                            }
                        }
                        targetSector.FixHeights(vertical);
                    }

                    if (autoUpdateThroughPortal && lookupSector.Sector != targetSector)
                    {
                        targetSector = lookupSector.Sector;
                        targetRoom = lookupSector.Room;
                        targetPos = lookupSector.SectorPosition;
                        goto EditSector;
                    }

                    // FIXME: VERY SLOW CODE! Since we need to update geometry in adjoining sector through portal, and each sector may contain portal to different room,
                    // we need to find a way to quickly update geometry in all possible adjoining rooms in area. Until then, this function is used on per-sector basis.

                    if (lookupSector.Room != room)
                        SmartBuildGeometry(lookupSector.Room, new RectangleInt2(lookupSector.SectorPosition, lookupSector.SectorPosition));
                }

            SmartBuildGeometry(room, area);
        }

        public static void ResetObjectRotation(PositionBasedObjectInstance obj, RotationAxis axis = RotationAxis.None)
        {
            if (!(obj is IRotateableY || obj is IRotateableYX || obj is IRotateableYXRoll)) return;
            _editor.UndoManager.PushObjectTransformed(obj);

            if (obj is IRotateableYX)
            {
                if (axis == RotationAxis.X || axis == RotationAxis.None)
                    (obj as IRotateableYX).RotationX = 0;
            }

            if (obj is IRotateableY)
            {
                if (axis == RotationAxis.Y || axis == RotationAxis.None)
                    (obj as IRotateableY).RotationY = 0;
            }

            if (obj is IRotateableYXRoll)
            {
                if (axis == RotationAxis.Roll || axis == RotationAxis.None)
                    (obj as IRotateableYXRoll).Roll = 0;
            }

            _editor.ObjectChange(obj, ObjectChangeType.Change);
        }

        public static void ResetObjectScale(PositionBasedObjectInstance obj)
        {
            if (!(obj is IScaleable || obj is ISizeable)) return;

            _editor.UndoManager.PushObjectTransformed(obj);

            if (obj is IScaleable)
                (obj as IScaleable).Scale = 1;
            else if (obj is ISizeable)
            {
                var o = (ISizeable)obj;
                o.Size = o.DefaultSize;
            }

            _editor.ObjectChange(obj, ObjectChangeType.Change);
        }

        public static void EditColor(IWin32Window owner, IColorable obj, Action<Vector3> newColorCallback = null)
        {
            using (var colorDialog = new RealtimeColorDialog(
                _editor.Configuration.ColorDialog_Position.X,
                _editor.Configuration.ColorDialog_Position.Y,
                c =>
                {
                    obj.Color = c.ToFloat3Color() * 2.0f;
                    _editor.ObjectChange(obj as ObjectInstance, ObjectChangeType.Change);
                }, _editor.Configuration.UI_ColorScheme))
            {
                colorDialog.Color = (obj.Color * 0.5f).ToWinFormsColor();
                var oldLightColor = colorDialog.Color;

                // Temporarily hide selection
                _editor.ToggleHiddenSelection(true);

                // Rollback to previous color if dialog is canceled or push undo if confirmed
                if (colorDialog.ShowDialog(owner) != DialogResult.OK)
                    colorDialog.Color = oldLightColor;
                else if (obj is PositionBasedObjectInstance)
                {
                    obj.Color = oldLightColor.ToFloat3Color() * 2.0f;
                    _editor.UndoManager.PushObjectPropertyChanged(obj as PositionBasedObjectInstance);
                }

                // Unhide selection
                _editor.ToggleHiddenSelection(false);

                obj.Color = colorDialog.Color.ToFloat3Color() * 2.0f;
                _editor.ObjectChange(obj as ObjectInstance, ObjectChangeType.Change);

                _editor.Configuration.ColorDialog_Position = colorDialog.Position;
                newColorCallback?.Invoke(colorDialog.Color.ToFloat3Color());
            }
        }

        public static void SmoothSector(Room room, int x, int z, SectorVerticalPart vertical, int stepHeight, bool disableUndo = false)
        {
            RoomSectorPair currentSectorPair = room.GetSectorTryThroughPortal(x, z);

            if (currentSectorPair.Room != room)
                return;

            if (!disableUndo)
                _editor.UndoManager.PushGeometryChanged(room.AndAdjoiningRooms);

            // Apply smoothing to each corner of the sector
            for (SectorEdge edge = 0; edge < SectorEdge.Count; edge++)
                SmoothSectorCorner(room, x, z, edge, vertical, stepHeight);

            SmartBuildGeometry(room, new RectangleInt2(x, z, x, z));
        }

        public static void SmoothArea(Room room, RectangleInt2 area, SectorVerticalPart vertical, int stepHeight, bool disableUndo = false)
        {
            if (!disableUndo)
            {
                HashSet<Room> affectedRooms = room.GetAdjoiningRoomsFromArea(area.Inflate(1));
                affectedRooms.Add(room);

                _editor.UndoManager.PushGeometryChanged(affectedRooms);
            }

            bool isBorder;

            // Iterate through the selected region (excluding border sectors)
            for (int z = area.Y0; z <= area.Y1; z++)
            {
                isBorder = z <= 0 || z >= room.NumZSectors - 1;

                if (isBorder)
                    continue;

                for (int x = area.X0; x <= area.X1; x++)
                {
                    isBorder = x <= 0 || x >= room.NumXSectors - 1;

                    if (isBorder)
                        continue;

                    // Smooth all four corners of current sector
                    for (SectorEdge edge = 0; edge < SectorEdge.Count; edge++)
                        SmoothSectorCorner(room, x, z, edge, vertical, stepHeight);
                }
            }

            SmartBuildGeometry(room, area);
        }

        private static void SmoothSectorCorner(Room room, int x, int z, SectorEdge edge, SectorVerticalPart vertical, int stepHeight)
        {
            RoomSectorPair currentSectorPair = room.GetSectorTryThroughPortal(x, z);

            if (currentSectorPair.Sector is null || currentSectorPair.Sector.IsFullWall)
                return;

            // Define the neighboring sectors and their corresponding edges based on the corner we're processing
            ReadOnlySpan<SectorEdgeOffset> neighbors = edge switch
            {
                SectorEdge.XnZp => stackalloc SectorEdgeOffset[] { new(-1,  0, SectorEdge.XpZp), new(-1,  1, SectorEdge.XpZn), new(0,  1, SectorEdge.XnZn) }, // North-west corner
                SectorEdge.XpZp => stackalloc SectorEdgeOffset[] { new( 1,  0, SectorEdge.XnZp), new( 1,  1, SectorEdge.XnZn), new(0,  1, SectorEdge.XpZn) }, // North-east corner
                SectorEdge.XpZn => stackalloc SectorEdgeOffset[] { new( 0, -1, SectorEdge.XpZp), new( 1, -1, SectorEdge.XnZp), new(1,  0, SectorEdge.XnZn) }, // South-east corner
                SectorEdge.XnZn => stackalloc SectorEdgeOffset[] { new(-1,  0, SectorEdge.XpZn), new(-1, -1, SectorEdge.XpZp), new(0, -1, SectorEdge.XnZp) }, // South-west corner
                _ => throw new ArgumentException("Invalid edge") // Handle invalid edge
            };

            // Calculate average height from current corner and surrounding neighbors
            int heightSum = currentSectorPair.Sector.GetHeight(vertical, edge);
            int validCorners = 1;

            // Add heights from valid neighboring corners
            foreach ((int neighborX, int neighborZ, SectorEdge neighborEdge) in neighbors)
            {
                RoomSectorPair sectorPair = currentSectorPair.Room.GetSectorTryThroughPortal(x + neighborX, z + neighborZ);

                if (sectorPair.Sector is null || sectorPair.Sector.IsFullWall)
                    continue;

                heightSum += sectorPair.Sector.GetHeight(vertical, neighborEdge);
                validCorners++;
            }

            // If there are not enough valid corners, return without making changes
            if (validCorners < 2)
                return;

            int averageHeight = heightSum / validCorners;

            // Round to the nearest step height
            if (stepHeight > 0)
                averageHeight = (int)Math.Round((float)averageHeight / stepHeight) * stepHeight;

            // Apply the average height to the current corner
            currentSectorPair.Sector.SetHeight(vertical, edge, averageHeight);
            currentSectorPair.Sector.FixHeight(edge, vertical);

            // Apply the same height to neighboring corners
            foreach ((int neighborX, int neighborZ, SectorEdge neighborEdge) in neighbors)
            {
                RoomSectorPair sectorPair = currentSectorPair.Room.GetSectorTryThroughPortal(x + neighborX, z + neighborZ);

                if (sectorPair.Sector is null || sectorPair.Sector.IsFullWall)
                    continue;

                sectorPair.Sector.SetHeight(vertical, neighborEdge, averageHeight);
                sectorPair.Sector.FixHeight(neighborEdge, vertical);
            }
        }

        public static void ShapeGroup(Room room, RectangleInt2 area, ArrowType arrow, EditorToolType type, SectorVerticalPart vertical, double heightScale, bool precise, bool stepped)
        {
            if (precise)
                heightScale /= 4;

            bool linearShape = type <= EditorToolType.HalfPipe;
            bool uniformShape = type >= EditorToolType.HalfPipe;
            bool step90 = arrow <= ArrowType.EdgeW;
            bool turn90 = arrow == ArrowType.EdgeW || arrow == ArrowType.EdgeE;
            bool reverseX = (arrow == ArrowType.EdgeW || arrow == ArrowType.CornerSW || arrow == ArrowType.CornerNW) ^ uniformShape;
            bool reverseZ = (arrow == ArrowType.EdgeS || arrow == ArrowType.CornerSW || arrow == ArrowType.CornerSE) ^ uniformShape;
            bool uniformAlign = arrow != ArrowType.EntireFace && type > EditorToolType.HalfPipe && step90;

            double sizeX = area.Width + (stepped ? 0 : 1);
            double sizeZ = area.Height + (stepped ? 0 : 1);
            double grainBias = uniformShape ? (!step90 ? 0 : 1) : 0;
            double grainX = (1 + grainBias) / sizeX / (uniformAlign && turn90 ? 2 : 1);
            double grainZ = (1 + grainBias) / sizeZ / (uniformAlign && !turn90 ? 2 : 1);

            for (int w = area.X0, x = 0; w < area.X0 + sizeX + 1; w++, x++)
                for (int h = area.Y0, z = 0; h != area.Y0 + sizeZ + 1; h++, z++)
                {
                    double currentHeight;
                    double currX = linearShape && !turn90 && step90 ? 0 : grainX * (reverseX ? sizeX - x : x) - (uniformAlign && turn90 ? 0 : grainBias);
                    double currZ = linearShape && turn90 && step90 ? 0 : grainZ * (reverseZ ? sizeZ - z : z) - (uniformAlign && !turn90 ? 0 : grainBias);

                    switch (type)
                    {
                        case EditorToolType.Ramp:
                            currentHeight = currX + currZ;
                            break;
                        case EditorToolType.Pyramid:
                            currentHeight = 1 - Math.Max(Math.Abs(currX), Math.Abs(currZ));
                            break;
                        default:
                            currentHeight = Math.Sqrt(1 - Math.Pow(currX, 2) - Math.Pow(currZ, 2));
                            currentHeight = double.IsNaN(currentHeight) ? 0 : currentHeight;
                            if (type == EditorToolType.QuarterPipe)
                                currentHeight = 1 - currentHeight;
                            break;
                    }
                    currentHeight = Math.Round(currentHeight * heightScale) * _editor.IncrementReference;

                    if (stepped)
                    {
                        room.RaiseSector(w, h, vertical, (int)currentHeight, false);
                        room.Sectors[w, h].FixHeights();
                    }
                    else
                        room.ModifyHeightThroughPortal(w, h, vertical, (int)currentHeight, area);
                }
            SmartBuildGeometry(room, area);
        }

        public static void ApplyHeightmap(Room room, RectangleInt2 area, ArrowType arrow, SectorVerticalPart vertical, float[,] heightmap, float heightScale, bool precise, bool raw)
        {
            if (precise)
                heightScale /= 4;

            bool allFace = arrow == ArrowType.EntireFace;
            bool step90 = arrow <= ArrowType.EdgeW;
            bool turn90 = arrow == ArrowType.EdgeW || arrow == ArrowType.EdgeE;
            bool reverseX = arrow == ArrowType.EdgeW || arrow == ArrowType.CornerSW || arrow == ArrowType.CornerNW;
            bool reverseZ = arrow == ArrowType.EdgeS || arrow == ArrowType.CornerSW || arrow == ArrowType.CornerSE;

            float smoothGrainX = (float)(allFace || (step90 && !turn90) ? Math.PI : Math.PI * 0.5f) / (area.Width + 1);
            float smoothGrainZ = (float)(allFace || (step90 &&  turn90) ? Math.PI : Math.PI * 0.5f) / (area.Height + 1);

            for (int w = area.X0, x = 0; w < area.X1 + 2; w++, x++)
                for (int h = area.Y0, z = 0; h != area.Y1 + 2; h++, z++)
                {
                    var smoothFactor = raw ? 1 : Math.Sin(smoothGrainX * (reverseX ? area.Width + 1 - x : x)) * Math.Sin(smoothGrainZ * (reverseZ ? area.Height + 1 - z : z));

                    int currX = x * (heightmap.GetLength(0) / (area.Width + 2));
                    int currZ = z * (heightmap.GetLength(1) / (area.Height + 2));
                    room.ModifyHeightThroughPortal(w, h, vertical, (int)Math.Round(heightmap[currX, currZ] * smoothFactor * heightScale) * _editor.IncrementReference, area);
                }
            SmartBuildGeometry(room, area);
        }

        public static void FlipFloorSplit(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (room.Sectors[x, z].Floor.DiagonalSplit == DiagonalSplit.None &&
                        (!room.Sectors[x, z].Floor.IsQuad || !(room.Sectors[x, z].HasGhostBlock && room.Sectors[x, z].GhostBlock.Floor.IsQuad)))
                        room.Sectors[x, z].Floor.SplitDirectionToggled = !room.Sectors[x, z].Floor.SplitDirectionToggled;

            SmartBuildGeometry(room, area);
        }

        public static void FlipCeilingSplit(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (room.Sectors[x, z].Ceiling.DiagonalSplit == DiagonalSplit.None &&
                        (!room.Sectors[x, z].Ceiling.IsQuad || !(room.Sectors[x, z].HasGhostBlock && room.Sectors[x, z].GhostBlock.Ceiling.IsQuad)))
                        room.Sectors[x, z].Ceiling.SplitDirectionToggled = !room.Sectors[x, z].Ceiling.SplitDirectionToggled;

            SmartBuildGeometry(room, area);
        }

        public static void AddTrigger(Room room, RectangleInt2 area, TriggerInstance trigger) =>
            AddTriggers(room, area, new List<TriggerInstance> { trigger });

        public static void AddTriggers(Room room, RectangleInt2 area, List<TriggerInstance> triggers)
        {
            foreach (var trigger in triggers)
            {
                trigger.Area = area;
                room.AddObject(_editor.Level, trigger);
                _editor.ObjectChange(trigger, ObjectChangeType.Add);
            }

            _editor.RoomSectorPropertiesChange(room);

            // Undo
            _editor.UndoManager.PushSectorObjectCreated(triggers.Cast<SectorBasedObjectInstance>().ToList());
        }

        public static void AddTrigger(Room room, RectangleInt2 area, IWin32Window owner)
        {
            if (Control.ModifierKeys.HasFlag(Keys.Shift) || _editor.SelectedObject == null || !(_editor.SelectedObject is PositionBasedObjectInstance))
                AddTrigger(room, area, owner, _editor.BookmarkedObject);
            else
                AddTrigger(room, area, owner, _editor.SelectedObject);
        }

        public static void AddTrigger(Room room, RectangleInt2 area, IWin32Window owner, ObjectInstance @object)
        {
            // Initialize root trigger and object list.
            var trigger = new TriggerInstance(area);
            var objectList = new List<ObjectInstance>();

            // If object group is a group of moveables, batch-trigger it. Otherwise, add single object to list.
            if (@object is ObjectGroup && ((ObjectGroup)@object).Any(o => o is MoveableInstance))
                objectList.AddRange(((ObjectGroup)@object).Where(i => i is MoveableInstance));
            else
                objectList.Add(@object);

            // This flag is set when key/switch/bridge trigger is created to differentiate other object
            // trigger types from root object trigger type.
            bool useDefaultTypeForBatchTriggers = false;

            // Sort object list and prioritize key/switch/bridge objects above all
            if (objectList.First() is MoveableInstance &&
                _editor.Configuration.UI_AutoFillTriggerTypesForSwitchAndKey)
            {
                objectList = objectList.OrderByDescending(o =>
                {
                    var objectName = string.Empty;
					if (o is MoveableInstance)
						objectName = (o as MoveableInstance).WadObjectId.ShortName(_editor.Level.Settings.GameVersion).ToLower();

                    bool isSwitch = objectName.Contains("switch") || objectName.Contains("pulley");
                    bool isHole = objectName.Contains("hole") &&
                        (objectName.Contains("key") || objectName.Contains("puzzle"));
                    bool isBridge = objectName.Contains("bridge") &&
                        (objectName.Contains("flat") || objectName.Contains("tilt") || objectName.Contains("custom"));

                    // Also set trigger type along the way of sorting.

                    if (isHole)
                        trigger.TriggerType = TriggerType.Key;
                    else if (isSwitch)
                        trigger.TriggerType = TriggerType.Switch;
                    else if (isBridge)
                        trigger.TriggerType = TriggerType.Dummy;
                    else
                        return false;

                    useDefaultTypeForBatchTriggers = true;
                    return true;
                }).ToList();
            }

            // Get root object from now-sorted object list.
            var firstObject = objectList.First();

            // Setup root trigger.
            if (firstObject is MoveableInstance)
            {
                trigger.TargetType = TriggerTargetType.Object;
                trigger.Target = firstObject;
            }
            else if (firstObject is FlybyCameraInstance)
            {
                trigger.TargetType = TriggerTargetType.FlyByCamera;
                trigger.Target = firstObject;
            }
            else if (firstObject is CameraInstance)
            {
                trigger.TargetType = TriggerTargetType.Camera;
                trigger.Target = firstObject;
            }
            else if (firstObject is SinkInstance)
            {
                trigger.TargetType = TriggerTargetType.Sink;
                trigger.Target = firstObject;
            }
            else if (firstObject is VolumeInstance volume && volume.EventSet != null && _editor.Level.IsTombEngine)
            {
                trigger.TargetType = TriggerTargetType.VolumeEvent;
                trigger.Target = new TriggerParameterString(volume.EventSet.Name);
            }
            else if (firstObject is StaticInstance && _editor.Level.IsNG)
            {
                trigger.TargetType = TriggerTargetType.FlipEffect;
                trigger.Target = new TriggerParameterUshort(160);
                trigger.Timer = firstObject;
            }

            // Display form for additional root trigger setup.
            using (var formTrigger = GetObjectSetupWindow(trigger, _editor.Level, new Action<ObjectInstance>(obj => _editor.ShowObject(obj)),
                                                     new Action<Room>(r => _editor.SelectRoom(r))))
            {
                if (formTrigger.ShowDialog(owner) != DialogResult.OK)
                    return;
            }

            // Initialize trigger list with root trigger first.
            var triggerList = new List<TriggerInstance>() { trigger };

            // Exclude root object from object list (list will be empty if single object)
            objectList = objectList.Where(o => o != firstObject).ToList();

            // Populate trigger list with additional triggers if object list isn't empty by now.
            foreach (var obj in objectList)
            {
                // If user changed target type, abandon batch triggering.

                if (trigger.Target.GetType() != obj.GetType())
                    break;

                var newTrigger = (TriggerInstance)trigger.Clone(trigger.Area);
                if (useDefaultTypeForBatchTriggers) newTrigger.TriggerType = TriggerType.Trigger;
                newTrigger.Target = obj;
                triggerList.Add(newTrigger);
            }

            AddTriggers(room, area, triggerList);
        }

        public static void AddGhostBlocks(Room room, RectangleInt2 area)
        {
            var ghostList = new List<GhostBlockInstance>();

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (!room.Sectors[x, z].HasGhostBlock && !room.Sectors[x, z].IsAnyWall)
                    {
                        var ghost = new GhostBlockInstance() { SectorPosition = new VectorInt2(x, z) };
                        room.AddObject(_editor.Level, ghost);
                        _editor.ObjectChange(ghost, ObjectChangeType.Add);
                        ghostList.Add(ghost);
                    }
                }

            if (ghostList.Count == 0)
            {
                _editor.SendMessage("No ghost blocks were added. You already have it in specified area.", PopupType.Warning);
                return;
            }

            _editor.RoomSectorPropertiesChange(room);
            _editor.UndoManager.PushGhostBlockCreated(ghostList); // Undo
        }

        public static void AddVolume(VolumeShape shape)
        {
            if (!VersionCheck(_editor.Level.IsTombEngine, "Volume"))
                return;

            switch (shape)
            {
                case VolumeShape.Box:
                    _editor.Action = new EditorActionPlace(false, (l, r) => new BoxVolumeInstance());
                    break;
                case VolumeShape.Sphere:
                    _editor.Action = new EditorActionPlace(false, (l, r) => new SphereVolumeInstance());
                    break;
            }
        }

        public static void AddBoxVolumeInSelectedArea(IWin32Window owner)
        {
            if (!VersionCheck(_editor.Level.IsTombEngine, "Volume"))
                return;

            if (!CheckForRoomAndSectorSelection(owner))
                return;

            var box = new BoxVolumeInstance()
            {
                Size = new Vector3((_editor.SelectedSectors.Area.Size.X + 1) * Level.SectorSizeUnit,
                Level.SectorSizeUnit, (_editor.SelectedSectors.Area.Size.Y + 1) * Level.SectorSizeUnit),
                EventSet = _editor.Level.Settings.VolumeEventSets.Count > 0 ? _editor.Level.Settings.VolumeEventSets[0] : null
            };

            var overallArea = _editor.SelectedSectors.Area.Start + _editor.SelectedSectors.Area.End;
            var localCenter = new Vector2(overallArea.X, overallArea.Y) / 2.0f;
            PlaceObjectWithoutUpdate(_editor.SelectedRoom, localCenter, box);
            box.Position += new Vector3(0, Level.HalfSectorSizeUnit, 0); // Lift it up a bit
            _editor.UndoManager.PushObjectCreated(box);
            AllocateScriptIds(box);

            // Display form
            EditEventSets(owner, false, box);
        }

        public static Vector3 GetMovementPrecision(Keys modifierKeys)
        {
            if (modifierKeys.HasFlag(Keys.Control))
                return new Vector3(0.0f);
            if (modifierKeys.HasFlag(Keys.Shift))
                return new Vector3(64.0f);
            return new Vector3(512.0f, 128.0f, 512.0f);
        }

        public static void ScaleObject(IScaleable instance, float newScale, double quantization)
        {
            if (quantization != 0.0f)
            {
                double logScale = Math.Log(newScale);
                double logQuantization = Math.Log(quantization);
                logScale = Math.Round(logScale / logQuantization) * logQuantization;
                newScale = (float)Math.Exp(logScale);
            }

            // Set some limits to scale
            if (newScale < 1 / 64.0f)
                newScale = 1 / 64.0f;
            if (newScale > 128.0f)
                newScale = 128.0f;
            instance.Scale = newScale;

            _editor.ObjectChange(_editor.SelectedObject, ObjectChangeType.Change);
        }

        public static void ResizeObject(ISizeable instance, Vector3 delta, double quantization)
        {
            if (quantization < 1.0f) quantization = 1.0f;

            var newX = (float)MathC.Clamp(Math.Round((delta.X) / quantization) * quantization, 1 / 64.0f, float.MaxValue);
            var newY = (float)MathC.Clamp(Math.Round((delta.Y) / quantization) * quantization, 1 / 64.0f, float.MaxValue);
            var newZ = (float)MathC.Clamp(Math.Round((delta.Z) / quantization) * quantization, 1 / 64.0f, float.MaxValue);

            instance.Size = new Vector3(newX, newY, newZ);
            _editor.ObjectChange(_editor.SelectedObject, ObjectChangeType.Change);
        }

        public static void MoveObject(PositionBasedObjectInstance instance, Room targetRoom, VectorInt2 sector)
        {
            var r = instance.Room;
            _editor.UndoManager.PushObjectTransformed(instance);
            instance.Room.RemoveObject(_editor.Level, instance);
            _editor.ObjectChange(instance, ObjectChangeType.Remove, r);
            PlaceObjectWithoutUpdate(targetRoom, sector, instance);
        }

        public static void MoveObject(PositionBasedObjectInstance instance, Vector3 pos, Keys modifierKeys)
        {
            MoveObject(instance, pos, GetMovementPrecision(modifierKeys), modifierKeys.HasFlag(Keys.Alt));
        }

        public static void MoveObject(PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            if (instance == null)
                return;

            // Limit movement precision
            if (precision.X > 0.0f && pos.X != instance.Position.X)
                pos.X = (float)Math.Round(pos.X / precision.X) * precision.X;
            if (precision.Y > 0.0f && pos.Y != instance.Position.Y)
                pos.Y = (float)Math.Round(pos.Y / precision.Y) * precision.Y;
            if (precision.Z > 0.0f && pos.Z != instance.Position.Z)
                pos.Z = (float)Math.Round(pos.Z / precision.Z) * precision.Z;

            // Limit movement area
            if (!canGoOutsideRoom)
            {
                float x = (float)Math.Floor(pos.X / Level.SectorSizeUnit);
                float z = (float)Math.Floor(pos.Z / Level.SectorSizeUnit);

                if (x < 0.0f || x > instance.Room.NumXSectors - 1 ||
                    z < 0.0f || z > instance.Room.NumZSectors - 1)
                    return;

                Sector sector = instance.Room.Sectors[(int)x, (int)z];
                if (sector.IsAnyWall)
                    return;
            }

            // Update position
            instance.Position = pos;

            // Update state
            RebuildLightsForObject(instance);
            _editor.ObjectChange(instance, ObjectChangeType.Change);
        }

        public static void MoveObjectRelative(PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            _editor.UndoManager.PushObjectTransformed(instance);
            MoveObject(instance, instance.Position + pos, precision, canGoOutsideRoom);
        }

        public static void MoveObjectToOtherRoom(PositionBasedObjectInstance instance, Room newRoom)
        {
            if (instance == null)
                return;

            if(instance.Room == null || !instance.Room.ExistsInLevel || newRoom == null || !newRoom.ExistsInLevel)
            {
                _editor.SendMessage("Can't move object from or to non-existent room", PopupType.Error);
                return;
            }

            if (instance.Room == newRoom)
            {
                _editor.SendMessage("Object is already in current room", PopupType.Info);
                return;
            }

            _editor.UndoManager.PushObjectTransformed(instance);
            newRoom.MoveObjectFrom(_editor.Level, instance.Room, instance);

            // Update state
            RebuildLightsForObject(instance);
            _editor.ObjectChange(instance, ObjectChangeType.Change);
        }

        public static void RotateObject(ObjectInstance instance, RotationAxis axis, float angleInDegrees, float quantization = 0.0f, bool delta = true, bool disableUndo = false)
        {
            if(!disableUndo && instance is PositionBasedObjectInstance)
                _editor.UndoManager.PushObjectTransformed((PositionBasedObjectInstance)instance);

            if (quantization != 0.0f)
                angleInDegrees = (float)(Math.Round(angleInDegrees / quantization) * quantization);

            switch (axis)
            {
                case RotationAxis.Y:
                    if (Control.ModifierKeys.HasFlag(Keys.Alt) && instance is ObjectGroup)
                    {
                        var og = (ObjectGroup)instance;
                        og.RotateAsGroup(angleInDegrees + (delta ? og.RotationY : 0));
                    }
                    else
                    {
                        var rotateableY = instance as IRotateableY;
                        if (rotateableY != null)
                            rotateableY.RotationY = angleInDegrees + (delta ? rotateableY.RotationY : 0);
                        else
                            return;
                    }

                    break;
                case RotationAxis.X:
                    var rotateableX = instance as IRotateableYX;
                    if (rotateableX == null)
                        return;
                    rotateableX.RotationX = angleInDegrees + (delta ? rotateableX.RotationX : 0);
                    break;
                case RotationAxis.Roll:
                    var rotateableRoll = instance as IRotateableYXRoll;
                    if (rotateableRoll == null)
                        return;
                    rotateableRoll.Roll = angleInDegrees + (delta ? rotateableRoll.Roll : 0);
                    break;
            }

            // Update state
            RebuildLightsForObject(instance);
            _editor.ObjectChange(instance, ObjectChangeType.Change);
        }

        public static void RebuildLightsForObject(ObjectInstance instance)
        {
            if (instance is LightInstance ||
               (instance is ObjectGroup && ((ObjectGroup)instance).Any(o => o is LightInstance)))
                instance.Room.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
        }

        public static DarkForm GetObjectSetupWindow(params object[] args)
        {
            // This function decides on which window version to use based on game version.
            // Right now it is primarily future-proof setup for TEN objects which will feature
            // completely different set of options and using legacy object window layouts won't make any sense.

            // Algorithm:
            // 1. If game version set to TEN, try to find window version from "TombEditor.Forms.TombEngine" namespace
            // 2. If game version set to TEN and no window version is found in that namespace, fall back to "TombEditor.Forms" namespace
            // 3. If game version is NOT TEN, always use "TombEditor.Forms" namespace
            // 4. If no window was found in either namespace, throw an exception, since there's no setup window for such object.

            if (!args.Any() || !(args[0] is ObjectInstance))
                throw new ArgumentException("Object instance was not provided as first argument for this function.");

            var triedAlternateNamespace = false;
            var objectName = (args[0] as ObjectInstance).GetType().Name.Replace("Instance", string.Empty);

            // Additional filter for volume types
            if (objectName.Contains("Volume")) objectName = "Volume";

            // If version is TEN, try to search for new versions of setup window.
            // If no new form version is found, fall back to legacy form.
            // If no legacy form is found, this is clearly user fault and we throw exception.

            while (true)
            {
                var formType = Type.GetType("TombEditor.Forms" + (_editor.Level.IsTombEngine && !triedAlternateNamespace ? ".TombEngine" : "") + ".Form" + objectName);

                if (formType != null)
                {
                    var form = Activator.CreateInstance(formType, args);
                    if (form is DarkForm) return (DarkForm)form;
                }

                if (triedAlternateNamespace)
                    throw new MissingMemberException("Object instance which was called with this function has no associated setup window.");

                triedAlternateNamespace = true;
            }
        }

        public static void RenameObject(ObjectInstance instance, IWin32Window owner)
        {
            if (instance == null)
            {
                _editor.SendMessage("Please select an object.", PopupType.Error);
                return;
            }

            if (!VersionCheck(_editor.Level.IsTombEngine, "Object name"))
                return;

            if (!(instance is PositionAndScriptBasedObjectInstance))
                return;

            var luaInstance = instance as PositionAndScriptBasedObjectInstance;

            using (var form = new FormInputBox("Edit object name", "Enter new Lua name for this object:", luaInstance.LuaName))
            {
                if (form.ShowDialog(owner) == DialogResult.Cancel)
                    return;

                if (!luaInstance.CanSetLuaName(form.Result))
                {
                    MessageBoxes.LuaNameAlreadyTakenError(owner);
                    RenameObject(luaInstance, owner);
                }
                else
                {
                    luaInstance.LuaName = form.Result;
                    _editor.ObjectChange(luaInstance, ObjectChangeType.Change);
                }
            }
        }

        public static void EditObject(ObjectInstance instance, IWin32Window owner)
        {
            if (instance is MoveableInstance)
            {
                if (instance.CanBeColored() && Control.ModifierKeys.HasFlag(Keys.Control))
                    EditColor(owner, (MoveableInstance)instance);
                else
                {
                    using (var formMoveable = GetObjectSetupWindow((MoveableInstance)instance))
                        if (formMoveable.ShowDialog(owner) != DialogResult.OK)
                            return;
                }

                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is StaticInstance)
            {
                // Use static editing dialog only for NG levels for now (bypass it if Ctrl/Alt key is pressed)
                if (instance.CanBeColored() && (!_editor.Level.IsNG || Control.ModifierKeys.HasFlag(Keys.Control)))
                {
                    EditColor(owner, (StaticInstance)instance);
                }
                else if (_editor.Level.IsNG)
                {
                    using (var formStaticMesh = GetObjectSetupWindow((StaticInstance)instance))
                        if (formStaticMesh.ShowDialog(owner) != DialogResult.OK)
                            return;
                }
                else
                    _editor.SendMessage("Light mode for this static mesh was set to dynamic. Color can't be edited.", PopupType.Info);

                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is FlybyCameraInstance)
            {
                using (var formFlyby = GetObjectSetupWindow((FlybyCameraInstance)instance))
                    if (formFlyby.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is CameraInstance)
            {
                using (var formCamera = GetObjectSetupWindow((CameraInstance)instance))
                    if (formCamera.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is SpriteInstance)
            {
                if (!VersionCheck(_editor.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR2, "Room sprite"))
                    return;

                using (var formSprite = GetObjectSetupWindow((SpriteInstance)instance))
                    if (formSprite.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is SinkInstance)
            {
                using (var formSink = GetObjectSetupWindow((SinkInstance)instance))
                    if (formSink.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is SoundSourceInstance)
            {
                using (var formSoundSource = GetObjectSetupWindow((SoundSourceInstance)instance))
                    if (formSoundSource.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is TriggerInstance)
            {
                using (var formTrigger = GetObjectSetupWindow((TriggerInstance)instance, _editor.Level, new Action<ObjectInstance>(obj => _editor.ShowObject(obj)),
                                                         new Action<Room>(r => _editor.SelectRoom(r))))
                    if (formTrigger.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is ImportedGeometryInstance)
            {
                if (Control.ModifierKeys.HasFlag(Keys.Control))
                    EditColor(owner, (ImportedGeometryInstance)instance);
                else
                    using (var formImportedGeometry = new FormImportedGeometry((ImportedGeometryInstance)instance, _editor.Level.Settings))
                    {
                        if (formImportedGeometry.ShowDialog(owner) != DialogResult.OK)
                            return;
                        _editor.UpdateLevelSettings(formImportedGeometry.NewLevelSettings);
                    }
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is LightInstance)
            {
                EditLightColor(owner);
            }
            else if (instance is VolumeInstance)
            {
                if (!VersionCheck(_editor.Level.IsTombEngine, "Trigger volume"))
                    return;

                EditEventSets(owner, false, (VolumeInstance)instance);
            }
            else if (instance is MemoInstance)
            {
                using (var formMemo = new FormMemo((MemoInstance)instance))
                    if (formMemo.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is PortalInstance)
            {
                if (!VersionCheck(_editor.Level.IsTombEngine, "Portal properties"))
                    return;
                using (var formPortal = new FormPortal((PortalInstance)instance))
                    if (formPortal.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
        }

        public static void PasteObject(VectorInt2 pos, Room room)
        {
            ObjectClipboardData data = Clipboard.GetDataObject().GetData(typeof(ObjectClipboardData)) as ObjectClipboardData;
            if (data == null)
                _editor.SendMessage("Clipboard contains no object data.", PopupType.Error);
            else
            {
                ObjectInstance instance = data.MergeGetSingleObject(_editor);

                if (instance is ISpatial)
                {
					// HACK: fix imported geometry reference
					var importedGeometries = new List<ImportedGeometryInstance>();

					if (instance is ImportedGeometryInstance)
						importedGeometries.Add(instance as ImportedGeometryInstance);
					else if (instance is ObjectGroup)
						importedGeometries.AddRange((instance as ObjectGroup).OfType<ImportedGeometryInstance>());

					foreach (var imported in importedGeometries)
					{
                        var pastedPath = _editor.Level.Settings.MakeAbsolute(imported.Model.Info.Path);
                        foreach (var model in _editor.Level.Settings.ImportedGeometries)
                        {
                            var currentPath = _editor.Level.Settings.MakeAbsolute(model.Info.Path);
                            if (currentPath == pastedPath)
                            {
                                imported.Model = model;
                                break;
                            }
                        }
					}

                    PlaceObject(room, pos, instance);
                }
            }
        }

        public static void DeleteObjects(IEnumerable<ObjectInstance> objects, IWin32Window owner = null, bool undo = true)
        {
            if (!objects.Any())
                return;

            bool silent = !_editor.Configuration.UI_WarnBeforeDeletingObjects;
            silent &= (!objects.Any(obj => obj is TriggerInstance) && !objects.Any(obj => obj is PortalInstance));
            silent |= owner == null;

            if (!silent)
            {
                string prompt = "Do you really want to delete ";
                prompt += (objects.Count() > 1 || objects.Any(o => o is ObjectGroup)) ? "specified objects?" : (objects.First() + "?");

                if (DarkMessageBox.Show(owner, prompt, "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            if (undo)
            {
                // Prepare undo
                var undoList = new List<UndoRedoInstance>();
                foreach (var instance in objects)
                {
                    if (instance is ObjectGroup)
                        undoList.AddRange(((ObjectGroup)instance).Select(o => new AddRemoveObjectUndoInstance(_editor.UndoManager, o, false)));
                    else if (instance is PositionBasedObjectInstance )
                        undoList.Add(new AddRemoveObjectUndoInstance(_editor.UndoManager, (PositionBasedObjectInstance)instance, false));
                    else if (instance is GhostBlockInstance)
                        undoList.Add(new AddRemoveGhostBlockUndoInstance(_editor.UndoManager, (GhostBlockInstance)instance, false));
                }

                // Push undo
                _editor.UndoManager.Push(undoList);
            }

            // Delete objects
            foreach (var instance in objects)
            {
                if (instance is ObjectGroup)
                {
                    ((ObjectGroup)instance).ToList().ForEach(o => DeleteObjectWithoutUpdate(o));
                    if (_editor.SelectedObject == instance)
                        _editor.SelectedObject = null;
                }
                else
                    DeleteObjectWithoutUpdate(instance);
            }

            return;
        }

        public static void DeleteObject(ObjectInstance instance, IWin32Window owner = null)
        {
            var objectsToDelete = new List<ObjectInstance>() { instance };
            DeleteObjects(objectsToDelete, owner);
        }

        public static void DeleteObjectWithoutUpdate(ObjectInstance instance)
        {
            var room = instance.Room;
            var adjoiningRoom = (instance as PortalInstance)?.AdjoiningRoom;
            room.RemoveObject(_editor.Level, instance);

            // Delete triggers if is necessary
            var affectedRooms = _editor.Level.DeleteTriggersForObject(instance);
            foreach (var r in affectedRooms)
                _editor.RoomSectorPropertiesChange(r);

            // Avoid having the removed object still selected
            _editor.ObjectChange(instance, ObjectChangeType.Remove, room);

            // Additional updates
            if (instance is SectorBasedObjectInstance)
                _editor.RoomSectorPropertiesChange(room);

            if (instance is LightInstance)
            {
                room.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                _editor.RoomGeometryChange(room);
            }

            if (instance is PortalInstance)
            {
                room.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                if (adjoiningRoom != null)
                {
                    adjoiningRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                    _editor.RoomSectorPropertiesChange(adjoiningRoom);

                    if (adjoiningRoom.AlternateOpposite != null)
                    {
                        adjoiningRoom.AlternateOpposite.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                        _editor.RoomSectorPropertiesChange(adjoiningRoom.AlternateOpposite);
                    }
                }
                if (room.AlternateOpposite != null)
                {
                    room.AlternateOpposite.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                    _editor.RoomSectorPropertiesChange(room.AlternateOpposite);
                }
            }
        }

        public static void EditEventSets(IWin32Window owner, bool global, VolumeInstance targetVolume = null)
        {
            var existingWindow = Application.OpenForms[nameof(FormEventSetEditor)];

            if (existingWindow != null && (existingWindow as FormEventSetEditor).GlobalMode != global)
            {
                existingWindow.Close();
                existingWindow = null;
            }

            if (existingWindow == null)
            {
                var propForm = new FormEventSetEditor(global, targetVolume);
                propForm.Show(owner);
            }
            else
                existingWindow.Focus();
        }

        public static void DeleteEventSet(EventSet eventSet)
        {
            if (eventSet == null)
                return;

            if (_editor.Level.Settings.GlobalEventSets.Contains(eventSet))
            {
                _editor.Level.Settings.GlobalEventSets.Remove(eventSet);
            }
            else if (_editor.Level.Settings.VolumeEventSets.Contains(eventSet))
            {
                _editor.Level.Settings.VolumeEventSets.Remove(eventSet);

                foreach (var vol in _editor.Level.GetAllObjects().OfType<VolumeInstance>())
                {
                    if (vol.EventSet == eventSet)
                    {
                        vol.EventSet = null;
                        _editor.ObjectChange(vol, ObjectChangeType.Change);
                    }
                }
            }
        }

        public static void ReplaceEventSetNames(List<EventSet> list, string oldName, string newName)
        {
            foreach (var set in list)
                foreach (var evt in set.Events)
                    foreach (var node in TriggerNode.LinearizeNodes(evt.Value.Nodes))
                        foreach (bool global in new[] { false, true })
                        {
                            var type = global ? ArgumentType.GlobalEventSets : ArgumentType.VolumeEventSets;

                            var func = ScriptingUtils.NodeFunctions.FirstOrDefault(f => f.Signature == node.Function &&
                                                                                        f.Arguments.Any(a => a.Type == type));
                            if (func == null)
                                continue;

                            for (int i = 0; i < func.Arguments.Count; i++)
                            {
                                if (func.Arguments[i].Type == type &&
                                    node.Arguments.Count > i &&
                                    TextExtensions.Unquote(node.Arguments[i].Value) == oldName)
                                {
                                    node.Arguments[i] = new TriggerNodeArgument() { Name = node.Arguments[i].Name, Value = TextExtensions.Quote(newName) };
                                }
                            }

                        }
        }

        public static void RotateTexture(Room room, VectorInt2 pos, SectorFace face)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            Sector sector = room.GetSector(pos);
            TextureArea newTexture = sector.GetFaceTexture(face);
            bool isTriangle = room.GetFaceShape(pos.X, pos.Y, face) == FaceShape.Triangle;

            newTexture.Rotate(1, isTriangle);
            sector.SetFaceTexture(face, newTexture);

            // Update state
            room.RoomGeometry.UpdateFaceTexture(pos.X, pos.Y, face, newTexture, newTexture.DoubleSided);
            _editor.RoomTextureChange(room);
        }

        public static void MirrorTexture(Room room, VectorInt2 pos, SectorFace face)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            Sector sector = room.GetSector(pos);

            TextureArea newTexture = sector.GetFaceTexture(face);
            newTexture.Mirror(room.GetFaceShape(pos.X, pos.Y, face) == FaceShape.Triangle);
            sector.SetFaceTexture(face, newTexture);

            // Update state
            room.RoomGeometry.UpdateFaceTexture(pos.X, pos.Y, face, newTexture, newTexture.DoubleSided);
            _editor.RoomTextureChange(room);
        }

        public static void PickTexture(Room room, VectorInt2 pos, SectorFace face)
        {
            var area = room.GetSector(pos).GetFaceTexture(face);

            if (area == null || area.TextureIsUnavailable)
                return;
            else if (area.TextureIsInvisible)
            {
                var newInvisibleTexture = TextureArea.Invisible;

                // Preserve current attribs if option is set
                if (_editor.Configuration.TextureMap_PickTextureWithoutAttributes)
                {
                    newInvisibleTexture.BlendMode = _editor.SelectedTexture.BlendMode;
                    newInvisibleTexture.DoubleSided = _editor.SelectedTexture.DoubleSided;
                }

                _editor.SelectedTexture = newInvisibleTexture;
            }
            else
            {
                // Preserve current attribs if option is set
                if (_editor.Configuration.TextureMap_PickTextureWithoutAttributes)
                {
                    area.BlendMode = _editor.SelectedTexture.BlendMode;
                    area.DoubleSided = _editor.SelectedTexture.DoubleSided;
                }

                if (face is SectorFace.Ceiling or SectorFace.Ceiling_Triangle2)
                    area.Mirror(area.TextureIsTriangle);

                _editor.SelectTextureAndCenterView(area.RestoreQuad());
            }
        }

        public static List<KeyValuePair<Room, VectorInt2>> FindTextures(TextureSearchType type, TextureArea texture, bool onlySelectedRooms, uint maxEntries, out bool tooManyEntries)
        {
            var result = new ConcurrentBag<KeyValuePair<Room, VectorInt2>>();
            var roomList = onlySelectedRooms ? _editor.SelectedRooms : _editor.Level.ExistingRooms;
            var _tooManyEntries = false;

            Parallel.ForEach(roomList, (room, state) =>
            {
                for (int x = 0; x < room.NumXSectors; x++)
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        var sector = room.GetSectorTry(x, z);
                        if (sector == null) continue;

                        foreach (var face in Enum.GetValues(typeof(SectorFace)).Cast<SectorFace>())
                        {
                            // Filter out impossible combinations right away
                            if (face.IsNonWall() && sector.IsAnyWall) continue;
                            if (face == SectorFace.Floor_Triangle2 && sector.Floor.IsQuad) continue;
                            if (face == SectorFace.Ceiling_Triangle2 && sector.Ceiling.IsQuad) continue;

                            // Filter out undefined faces
                            if (!room.IsFaceDefined(x, z, face)) continue;

                            var tex = sector.GetFaceTexture(face);
                            var entry = new KeyValuePair<Room, VectorInt2>(room, new VectorInt2(x, z));

                            switch (type)
                            {
                                case TextureSearchType.Empty:
                                    if (tex == TextureArea.None || tex == _editor.Level.Settings.DefaultTexture)
                                        result.Add(entry);
                                    break;

                                case TextureSearchType.Invisible:
                                    if (tex == TextureArea.Invisible)
                                        result.Add(entry);
                                    break;

                                case TextureSearchType.Broken:
                                    if (tex.TriangleCoordsOutOfBounds || tex.QuadCoordsOutOfBounds)
                                        result.Add(entry);

                                    if (!tex.TextureIsInvisible)
                                    {
                                        if (tex.TextureIsUnavailable)
                                            result.Add(entry);

                                        if (!tex.TextureIsTriangle && tex.QuadArea == tex.TriangleArea)
                                            result.Add(entry);

                                        if (tex.TextureIsDegenerate)
                                            result.Add(entry);
                                    }
                                    break;

                                case TextureSearchType.ExactMatch:
                                    if (tex.GetCanonicalTexture(tex.TextureIsTriangle) == texture)
                                        result.Add(entry);
                                    break;

                                case TextureSearchType.PartialMatch:
                                    if (tex.Texture == texture.Texture && tex.GetRect().Intersects(texture.GetRect()))
                                        result.Add(entry);
                                    break;

                                case TextureSearchType.TextureSet:
                                    if (tex.Texture == texture.Texture)
                                        result.Add(entry);
                                    break;
                            }

                            if (result.Count > maxEntries)
                            {
                                state.Break(); // Don't continue if we've reached max entries
                                if (!_tooManyEntries) _tooManyEntries = true;
                                return;
                            }
                        }
                    }
            });

            tooManyEntries = _tooManyEntries;
            return result.Distinct().ToList();
        }

        private static bool FaceIsPortal(Room room, VectorInt2 pos, SectorFace face)
        {
            if (face.IsNonDiagonalWall())
                return room.Sectors[pos.X, pos.Y].WallPortal != null;
            else if (face.IsFloor())
                return room.Sectors[pos.X, pos.Y].FloorPortal != null;
            else if (face.IsCeiling())
                return room.Sectors[pos.X, pos.Y].CeilingPortal != null;
            else if (face.IsDiagonal())
                return false; // TODO: In TombEngine, possibly diagonal portals can be implemented?
            else
                return false;
        }

        private static void CheckTextureAttributes(Room room, VectorInt2 pos, SectorFace face, TextureArea texture)
        {
            if (!_editor.Configuration.TextureMap_WarnAboutIncorrectAttributes)
                return;

            // Identify if double-sided texture is applied to non-double-sided face

            if (texture.DoubleSided && !FaceIsPortal(room, pos, face))
            {
                if (!_textureAtrributeMessageState)
                    _editor.SendMessage("Double-sided texture is applied to a single-sided face.\n" +
                                        "Check if it's intentional.", PopupType.Warning);

                // Increase message count and reset it every 20th time to make sure user is aware of the message.

                _textureAttributeMessageCount++;

                if (_textureAttributeMessageCount > 20)
                {
                    _textureAttributeMessageCount = 0;
                    _textureAtrributeMessageState = false;
                }
                else
                    _textureAtrributeMessageState = true;
            }
            else
                _textureAtrributeMessageState = false;
        }
        private static bool _textureAtrributeMessageState = false;
        private static int  _textureAttributeMessageCount = 0;

        private static bool ApplyTextureToFace(Room room, VectorInt2 pos, SectorFace face, TextureArea texture, bool autocorrectCeiling = true)
        {
            if (_editor.Configuration.UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture &&
                !room.Properties.FlagHorizon && texture.TextureIsInvisible)
            {
                room.Properties.FlagHorizon = true;
                _editor.RoomPropertiesChange(room);
            }

            Sector sector = room.GetSector(pos);
            FaceShape shape = room.GetFaceShape(pos.X, pos.Y, face);
            bool wasDoubleSided = sector.GetFaceTexture(face).DoubleSided;
            bool textureApplied = false;

            // FIXME: Do we really need that now, when TextureOutOfBounds function was fixed?
            texture.ClampToBounds();

            // HACK: Ceiling vertex order is hardly messed up, we need to do some transforms.
            if (autocorrectCeiling && face.IsCeiling()) texture.Mirror();

            if (!_editor.Tool.TextureUVFixer ||
                (shape == FaceShape.Triangle && texture.TextureIsTriangle))
            {
                if (shape == FaceShape.Triangle)
                {
                    if (face.IsCeiling())
                        texture.Rotate(3); // WTF? But it works!
                    texture.TexCoord3 = texture.TexCoord2;
                }

                textureApplied = sector.SetFaceTexture(face, texture);

                if (textureApplied)
                {
                    TextureArea currentTexture = sector.GetFaceTexture(face);
                    CheckTextureAttributes(room, pos, face, currentTexture);
                    room.RoomGeometry.UpdateFaceTexture(pos.X, pos.Y, face, currentTexture, wasDoubleSided);
                }

                return textureApplied;
            }

            TextureArea processedTexture = texture;
            switch (face)
            {
                case SectorFace.Floor:
                case SectorFace.Ceiling:
                    SectorSurface surface = face == SectorFace.Floor ? sector.Floor : sector.Ceiling;
                    if (shape == FaceShape.Quad)
                        break;
                    if (surface.DiagonalSplit != DiagonalSplit.XnZn &&
                        surface.DiagonalSplit != DiagonalSplit.XpZp &&
                        surface.SplitDirectionIsXEqualsZ)
                    {
                        if (surface.DiagonalSplit != DiagonalSplit.XnZp && surface.DiagonalSplit != DiagonalSplit.XpZn)
                        {
                            Swap.Do(ref processedTexture.TexCoord0, ref processedTexture.TexCoord2);
                            processedTexture.TexCoord1 = processedTexture.TexCoord3;
                        }
                    }
                    else
                    {
                        processedTexture.TexCoord0 = processedTexture.TexCoord1;
                        processedTexture.TexCoord1 = processedTexture.TexCoord2;
                        processedTexture.TexCoord2 = processedTexture.TexCoord3;
                    }
                    processedTexture.TexCoord3 = processedTexture.TexCoord2;
                    break;

                case SectorFace.Floor_Triangle2:
                case SectorFace.Ceiling_Triangle2:
                    SectorSurface surface2 = face == SectorFace.Floor_Triangle2 ? sector.Floor : sector.Ceiling;
                    if (shape == FaceShape.Quad)
                        break;
                    if (surface2.DiagonalSplit == DiagonalSplit.XnZn ||
                        surface2.DiagonalSplit == DiagonalSplit.XpZp ||
                        !surface2.SplitDirectionIsXEqualsZ)
                    {
                        processedTexture.TexCoord2 = processedTexture.TexCoord1;
                        processedTexture.TexCoord1 = processedTexture.TexCoord0;
                        processedTexture.TexCoord0 = processedTexture.TexCoord3;
                    }
                    else
                    {
                        if (surface2.DiagonalSplit == DiagonalSplit.XnZp || surface2.DiagonalSplit == DiagonalSplit.XpZn)
                        {
                            Swap.Do(ref processedTexture.TexCoord0, ref processedTexture.TexCoord2);
                            processedTexture.TexCoord1 = processedTexture.TexCoord3;
                        }
                    }
                    processedTexture.TexCoord3 = processedTexture.TexCoord2;
                    break;

                default:
                    if (room.RoomGeometry != null)
                    {
                        // Get current face
                        VertexRange vertexRange = new VertexRange(0, 0);
                        if (!room.RoomGeometry.VertexRangeLookup.TryGetValue(new SectorFaceIdentity(pos.X, pos.Y, face), out vertexRange))
                            return false;

                        if (vertexRange.Count == 6)
                        {
                            Vector3 p0 = room.RoomGeometry.VertexPositions[vertexRange.Start + 2];
                            Vector3 p1 = room.RoomGeometry.VertexPositions[vertexRange.Start + 0];
                            Vector3 p2 = room.RoomGeometry.VertexPositions[vertexRange.Start + 1];
                            Vector3 p3 = room.RoomGeometry.VertexPositions[vertexRange.Start + 3];

                            float maxUp = Math.Max(p0.Y, p1.Y);
                            float minDown = Math.Min(p2.Y, p3.Y);

                            float difference = maxUp - minDown;

                            float delta0 = (minDown - p3.Y) / difference;
                            float delta1 = (maxUp - p0.Y) / difference;
                            float delta2 = (maxUp - p1.Y) / difference;
                            float delta3 = (minDown - p2.Y) / difference;

                            if (texture.TexCoord0.X == texture.TexCoord1.X && texture.TexCoord3.X == texture.TexCoord2.X)
                            {
                                processedTexture.TexCoord0.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta0;
                                processedTexture.TexCoord1.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta1;
                                processedTexture.TexCoord2.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta2;
                                processedTexture.TexCoord3.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta3;
                            }
                            else
                            {
                                processedTexture.TexCoord0.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta0;
                                processedTexture.TexCoord1.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta1;
                                processedTexture.TexCoord2.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta2;
                                processedTexture.TexCoord3.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta3;
                            }
                        }
                        else
                        {
                            Vector3 p0 = room.RoomGeometry.VertexPositions[vertexRange.Start + 0];
                            Vector3 p1 = room.RoomGeometry.VertexPositions[vertexRange.Start + 1];
                            Vector3 p2 = room.RoomGeometry.VertexPositions[vertexRange.Start + 2];

                            float maxUp = Math.Max(Math.Max(p0.Y, p1.Y), p2.Y);
                            float minDown = Math.Min(Math.Min(p0.Y, p1.Y), p2.Y);
                            float difference = maxUp - minDown;

                            if (p0.X == p2.X && p0.Z == p2.Z)
                            {
                                float delta0 = (minDown - p2.Y) / difference;
                                float delta1 = (maxUp - p0.Y) / difference;
                                float delta2 = (maxUp - p1.Y) / difference;
                                float delta3 = (minDown - p1.Y) / difference;

                                if (texture.TexCoord0.X == texture.TexCoord1.X && texture.TexCoord3.X == texture.TexCoord2.X)
                                {
                                    processedTexture.TexCoord0.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta0;
                                    processedTexture.TexCoord1.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta1;
                                    processedTexture.TexCoord2.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta2;
                                    processedTexture.TexCoord3.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta3;
                                }
                                else
                                {
                                    processedTexture.TexCoord0.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta0;
                                    processedTexture.TexCoord1.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta1;
                                    processedTexture.TexCoord2.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta2;
                                    processedTexture.TexCoord3.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta3;
                                }

                                processedTexture.TexCoord3 = processedTexture.TexCoord0;
                                processedTexture.TexCoord0 = processedTexture.TexCoord1;
                                processedTexture.TexCoord1 = processedTexture.TexCoord2;
                                processedTexture.TexCoord2 = processedTexture.TexCoord3;
                            }
                            else
                            {

                                float delta0 = (minDown - p0.Y) / difference;
                                float delta1 = (maxUp - p0.Y) / difference;
                                float delta2 = (maxUp - p1.Y) / difference;
                                float delta3 = (minDown - p2.Y) / difference;

                                if (texture.TexCoord0.X == texture.TexCoord1.X && texture.TexCoord3.X == texture.TexCoord2.X)
                                {
                                    processedTexture.TexCoord0.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta0;
                                    processedTexture.TexCoord1.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta1;
                                    processedTexture.TexCoord2.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta2;
                                    processedTexture.TexCoord3.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta3;
                                }
                                else
                                {
                                    processedTexture.TexCoord0.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta0;
                                    processedTexture.TexCoord1.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta1;
                                    processedTexture.TexCoord2.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta2;
                                    processedTexture.TexCoord3.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta3;
                                }

                                processedTexture.TexCoord0 = processedTexture.TexCoord3;

                                Vector2 tempTexCoord = processedTexture.TexCoord2;
                                processedTexture.TexCoord2 = processedTexture.TexCoord3;
                                processedTexture.TexCoord3 = processedTexture.TexCoord0;
                                processedTexture.TexCoord0 = processedTexture.TexCoord1;
                                processedTexture.TexCoord1 = tempTexCoord;
                            }
                        }
                    }
                    break;
            }

            // FIXME: Do we really need that now, when TextureOutOfBounds function was fixed?
            processedTexture.ClampToBounds();

            // Try to apply texture (returns false if same texture is already applied)
            textureApplied = sector.SetFaceTexture(face, processedTexture);

            // Check if texture attributes are correct
            if (textureApplied)
            {
                TextureArea currentTexture = sector.GetFaceTexture(face);
                CheckTextureAttributes(room, pos, face, currentTexture);
                room.RoomGeometry.UpdateFaceTexture(pos.X, pos.Y, face, currentTexture, wasDoubleSided);
            }

            return textureApplied;
        }

        public static bool ApplyTexture(Room room, VectorInt2 pos, SectorFace face, TextureArea texture, bool disableUndo = false)
        {
            if(!disableUndo)
                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            texture.ParentArea = new Rectangle2();

            bool textureApplied = ApplyTextureToFace(room, pos, face, texture);

            if (textureApplied)
                _editor.RoomTextureChange(room);

            return textureApplied;
        }

        public static Dictionary<SectorFace, float[]> GetFaces(Room room, VectorInt2 pos, Direction direction, SectorFaceType section)
        {
            var sector = room.GetSectorTry(pos.X, pos.Y);
            if (sector == null)
                return null;

            bool sectionIsWall = room.GetSectorTry(pos.X, pos.Y).IsAnyWall;

            var segments = new Dictionary<SectorFace, float[]>();

            switch (direction)
            {
                case Direction.PositiveZ:
                    if (section == SectorFaceType.Ceiling || sectionIsWall)
                    {
                        var positiveZ_WS = SectorFace.Wall_PositiveZ_WS;

                        if (room.IsFaceDefined(pos.X, pos.Y, positiveZ_WS))
                            segments.Add(positiveZ_WS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, positiveZ_WS), room.GetFaceLowestPoint(pos.X, pos.Y, positiveZ_WS) });

                        for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }
                    if (section == SectorFaceType.Floor || sectionIsWall)
                    {
                        var positiveZ_QA = SectorFace.Wall_PositiveZ_QA;

                        if (room.IsFaceDefined(pos.X, pos.Y, positiveZ_QA))
                            segments.Add(positiveZ_QA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, positiveZ_QA), room.GetFaceLowestPoint(pos.X, pos.Y, positiveZ_QA) });

                        for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }

                    var positiveZ_Middle = SectorFace.Wall_PositiveZ_Middle;

                    if (room.IsFaceDefined(pos.X, pos.Y, positiveZ_Middle))
                        segments.Add(positiveZ_Middle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, positiveZ_Middle), room.GetFaceLowestPoint(pos.X, pos.Y, positiveZ_Middle) });
                    break;

                case Direction.NegativeZ:
                    if (section == SectorFaceType.Ceiling || sectionIsWall)
                    {
                        var negativeZ_WS = SectorFace.Wall_NegativeZ_WS;

                        if (room.IsFaceDefined(pos.X, pos.Y, negativeZ_WS))
                            segments.Add(negativeZ_WS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, negativeZ_WS), room.GetFaceLowestPoint(pos.X, pos.Y, negativeZ_WS) });

                        for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }
                    if (section == SectorFaceType.Floor || sectionIsWall)
                    {
                        var negativeZ_QA = SectorFace.Wall_NegativeZ_QA;

                        if (room.IsFaceDefined(pos.X, pos.Y, negativeZ_QA))
                            segments.Add(negativeZ_QA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, negativeZ_QA), room.GetFaceLowestPoint(pos.X, pos.Y, negativeZ_QA) });

                        for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }

                    var negativeZ_Middle = SectorFace.Wall_NegativeZ_Middle;

                    if (room.IsFaceDefined(pos.X, pos.Y, negativeZ_Middle))
                        segments.Add(negativeZ_Middle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, negativeZ_Middle), room.GetFaceLowestPoint(pos.X, pos.Y, negativeZ_Middle) });
                    break;

                case Direction.PositiveX:
                    if (section == SectorFaceType.Ceiling || sectionIsWall)
                    {
                        var positiveX_WS = SectorFace.Wall_PositiveX_WS;

                        if (room.IsFaceDefined(pos.X, pos.Y, positiveX_WS))
                            segments.Add(positiveX_WS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, positiveX_WS), room.GetFaceLowestPoint(pos.X, pos.Y, positiveX_WS) });

                        for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }
                    if (section == SectorFaceType.Floor || sectionIsWall)
                    {
                        var positiveX_QA = SectorFace.Wall_PositiveX_QA;

                        if (room.IsFaceDefined(pos.X, pos.Y, positiveX_QA))
                            segments.Add(positiveX_QA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, positiveX_QA), room.GetFaceLowestPoint(pos.X, pos.Y, positiveX_QA) });

                        for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }

                    var positiveX_Middle = SectorFace.Wall_PositiveX_Middle;

                    if (room.IsFaceDefined(pos.X, pos.Y, positiveX_Middle))
                        segments.Add(positiveX_Middle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, positiveX_Middle), room.GetFaceLowestPoint(pos.X, pos.Y, positiveX_Middle) });
                    break;

                case Direction.NegativeX:
                    if (section == SectorFaceType.Ceiling || sectionIsWall)
                    {
                        var negativeX_WS = SectorFace.Wall_NegativeX_WS;

                        if (room.IsFaceDefined(pos.X, pos.Y, negativeX_WS))
                            segments.Add(negativeX_WS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, negativeX_WS), room.GetFaceLowestPoint(pos.X, pos.Y, negativeX_WS) });

                        for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }
                    if (section == SectorFaceType.Floor || sectionIsWall)
                    {
                        var negativeX_QA = SectorFace.Wall_NegativeX_QA;

                        if (room.IsFaceDefined(pos.X, pos.Y, negativeX_QA))
                            segments.Add(negativeX_QA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, negativeX_QA), room.GetFaceLowestPoint(pos.X, pos.Y, negativeX_QA) });

                        for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }

                    var negativeX_Middle = SectorFace.Wall_NegativeX_Middle;

                    if (room.IsFaceDefined(pos.X, pos.Y, negativeX_Middle))
                        segments.Add(negativeX_Middle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, negativeX_Middle), room.GetFaceLowestPoint(pos.X, pos.Y, negativeX_Middle) });
                    break;

                case Direction.Diagonal:
                    if (section == SectorFaceType.Ceiling || sectionIsWall)
                    {
                        var diagonalWS = SectorFace.Wall_Diagonal_WS;

                        if (room.IsFaceDefined(pos.X, pos.Y, diagonalWS))
                            segments.Add(diagonalWS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, diagonalWS), room.GetFaceLowestPoint(pos.X, pos.Y, diagonalWS) });

                        for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }
                    if (section == SectorFaceType.Floor || sectionIsWall)
                    {
                        var diagonalQA = SectorFace.Wall_Diagonal_QA;

                        if (room.IsFaceDefined(pos.X, pos.Y, diagonalQA))
                            segments.Add(diagonalQA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, diagonalQA), room.GetFaceLowestPoint(pos.X, pos.Y, diagonalQA) });

                        for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
                        {
                            var face = SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, i);

                            if (room.IsFaceDefined(pos.X, pos.Y, face))
                                segments.Add(face, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, face), room.GetFaceLowestPoint(pos.X, pos.Y, face) });
                        }
                    }

                    var diagonalMiddle = SectorFace.Wall_Diagonal_Middle;

                    if (room.IsFaceDefined(pos.X, pos.Y, diagonalMiddle))
                        segments.Add(diagonalMiddle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, diagonalMiddle), room.GetFaceLowestPoint(pos.X, pos.Y, diagonalMiddle) });
                    break;
            }

            return segments;
        }

        private static float[] GetAreaExtremums(Room room, RectangleInt2 area, Direction direction, SectorFaceType type)
        {
            float maxHeight = float.MinValue;
            float minHeight = float.MaxValue;

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    var segments = GetFaces(room, new VectorInt2(x, z), direction, type);
                    if (segments == null)
                        continue;

                    foreach (var segment in segments)
                    {
                        minHeight = Math.Min(minHeight, segment.Value[1]);
                        maxHeight = Math.Max(maxHeight, segment.Value[0]);
                    }
                }

            return new float[2] { minHeight, maxHeight };
        }

        public static void TexturizeWallSection(Room room, VectorInt2 pos, Direction direction, SectorFaceType section, TextureArea texture, int subdivisions = 0, int iteration = 0, float[] overrideHeights = null)
        {
            if (subdivisions < 0 || iteration < 0)
                subdivisions = 0;

            if (overrideHeights?.Count() != 2)
                overrideHeights = null;

            var segments = GetFaces(room, pos, direction, section);

            var processedTexture = texture;
            processedTexture.ParentArea = texture.GetRect();
            float minSectionHeight = float.MaxValue;
            float maxSectionHeight = float.MinValue;

            if (overrideHeights == null)
            {
                foreach (var segment in segments)
                {
                    minSectionHeight = Math.Min(minSectionHeight, segment.Value[1]);
                    maxSectionHeight = Math.Max(maxSectionHeight, segment.Value[0]);
                }
            }
            else
            {
                minSectionHeight = overrideHeights[0];
                maxSectionHeight = overrideHeights[1];
            }

            float sectionHeight = maxSectionHeight - minSectionHeight;
            bool inverted = false;

            foreach (var segment in segments)
            {
                float currentHighestPoint = Math.Abs(segment.Value[0] - maxSectionHeight) / sectionHeight;
                float currentLowestPoint = Math.Abs(maxSectionHeight - segment.Value[1]) / sectionHeight;

                if (texture.TexCoord0.X == texture.TexCoord1.X && texture.TexCoord3.X == texture.TexCoord2.X)
                {
                    float textureHeight = texture.TexCoord0.Y - texture.TexCoord1.Y;

                    processedTexture.TexCoord0.Y = texture.TexCoord1.Y + textureHeight * currentLowestPoint;
                    processedTexture.TexCoord3.Y = texture.TexCoord2.Y + textureHeight * currentLowestPoint;
                    processedTexture.TexCoord1.Y = texture.TexCoord1.Y + textureHeight * currentHighestPoint;
                    processedTexture.TexCoord2.Y = texture.TexCoord2.Y + textureHeight * currentHighestPoint;

                    if (subdivisions > 0)
                    {
                        float stride = (texture.TexCoord2.X - texture.TexCoord1.X) / (subdivisions + 1);

                        if (inverted == false & (direction == Direction.NegativeX || direction == Direction.PositiveZ))
                        {
                            inverted = true;
                            iteration = subdivisions - iteration;
                        }

                        processedTexture.TexCoord0.X = texture.TexCoord0.X + stride * iteration;
                        processedTexture.TexCoord1.X = texture.TexCoord1.X + stride * iteration;
                        processedTexture.TexCoord3.X = texture.TexCoord3.X - stride * (subdivisions - iteration);
                        processedTexture.TexCoord2.X = texture.TexCoord2.X - stride * (subdivisions - iteration);
                    }

                }
                else
                {
                    float textureWidth = texture.TexCoord3.X - texture.TexCoord2.X;

                    processedTexture.TexCoord3.X = texture.TexCoord2.X + textureWidth * currentLowestPoint;
                    processedTexture.TexCoord0.X = texture.TexCoord1.X + textureWidth * currentLowestPoint;
                    processedTexture.TexCoord2.X = texture.TexCoord2.X + textureWidth * currentHighestPoint;
                    processedTexture.TexCoord1.X = texture.TexCoord1.X + textureWidth * currentHighestPoint;

                    if (subdivisions > 0)
                    {
                        float stride = (texture.TexCoord0.Y - texture.TexCoord3.Y) / (subdivisions + 1);

                        if (inverted == false & (direction == Direction.PositiveX || direction == Direction.NegativeZ))
                        {
                            inverted = true;
                            iteration = subdivisions - iteration;
                        }

                        processedTexture.TexCoord2.Y = texture.TexCoord2.Y + stride * iteration;
                        processedTexture.TexCoord3.Y = texture.TexCoord3.Y + stride * iteration;
                        processedTexture.TexCoord1.Y = texture.TexCoord1.Y - stride * (subdivisions - iteration);
                        processedTexture.TexCoord0.Y = texture.TexCoord0.Y - stride * (subdivisions - iteration);
                    }
                }

                ApplyTextureToFace(room, pos, segment.Key, processedTexture);
            }
        }

        public static void TexturizeGroup(Room room, SectorSelection selection, SectorSelection workArea, TextureArea texture, SectorFace pickedFace, bool subdivideWalls, bool unifyHeight, bool disableUndo = false)
        {
            if (!disableUndo)
                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            if (pickedFace.IsCeiling()) texture.Mirror();
            RectangleInt2 area = selection != SectorSelection.None ? selection.Area : _editor.SelectedRoom.LocalArea;

            if (pickedFace.IsWall())
            {
                int xSubs = subdivideWalls ? 0 : area.X1 - area.X0;
                int zSubs = subdivideWalls ? 0 : area.Y1 - area.Y0;

                for (int x = area.X0, iterX = 0; x <= area.X1; x++, iterX++)
                    for (int z = area.Y0, iterZ = 0; z <= area.Y1; z++, iterZ++)
                    {
                        if (room.CoordinateInvalid(x, z) || (workArea != SectorSelection.None && !workArea.Area.Contains(new VectorInt2(x, z))))
                            continue;

                        Direction direction = pickedFace.GetDirection();
                        SectorFaceType faceType = pickedFace.GetFaceType();

                        switch (direction)
                        {
                            case Direction.PositiveZ:
                            case Direction.NegativeZ:
                                TexturizeWallSection(room, new VectorInt2(x, z), direction, faceType, texture, xSubs, iterX, unifyHeight ? GetAreaExtremums(room, area, direction, faceType) : null);
                                break;

                            case Direction.PositiveX:
                            case Direction.NegativeX:
                                TexturizeWallSection(room, new VectorInt2(x, z), direction, faceType, texture, zSubs, iterZ, unifyHeight ? GetAreaExtremums(room, area, direction, faceType) : null);
                                break;

                            case Direction.Diagonal:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.Diagonal, faceType, texture);
                                break;
                        }
                    }
            }
            else
            {
                Vector2 verticalUVStride = (texture.TexCoord3 - texture.TexCoord2) / (area.Y1 - area.Y0 + 1);
                Vector2 horizontalUVStride = (texture.TexCoord2 - texture.TexCoord1) / (area.X1 - area.X0 + 1);

                for (int x = area.X0, x1 = 0; x <= area.X1; x++, x1++)
                {
                    Vector2 currentX = horizontalUVStride * x1;

                    for (int z = area.Y0, z1 = 0; z <= area.Y1; z++, z1++)
                    {
                        if (room.CoordinateInvalid(x, z) || (workArea != SectorSelection.None && !workArea.Area.Contains(new VectorInt2(x, z))))
                            continue;

                        TextureArea currentTexture = texture;
                        Vector2 currentZ = verticalUVStride * z1;

                        currentTexture.ParentArea = texture.GetRect();
                        currentTexture.TexCoord0 -= currentZ - currentX;
                        currentTexture.TexCoord1 = currentTexture.TexCoord0 - verticalUVStride;
                        currentTexture.TexCoord3 = currentTexture.TexCoord0 + horizontalUVStride;
                        currentTexture.TexCoord2 = currentTexture.TexCoord0 + horizontalUVStride - verticalUVStride;

                        // Round textures to 5 decimal digits
                        currentTexture.TexCoord0.X = (float)Math.Round(currentTexture.TexCoord0.X, 5);
                        currentTexture.TexCoord0.Y = (float)Math.Round(currentTexture.TexCoord0.Y, 5);
                        currentTexture.TexCoord1.X = (float)Math.Round(currentTexture.TexCoord1.X, 5);
                        currentTexture.TexCoord1.Y = (float)Math.Round(currentTexture.TexCoord1.Y, 5);
                        currentTexture.TexCoord2.X = (float)Math.Round(currentTexture.TexCoord2.X, 5);
                        currentTexture.TexCoord2.Y = (float)Math.Round(currentTexture.TexCoord2.Y, 5);
                        currentTexture.TexCoord3.X = (float)Math.Round(currentTexture.TexCoord3.X, 5);
                        currentTexture.TexCoord3.Y = (float)Math.Round(currentTexture.TexCoord3.Y, 5);

                        switch (pickedFace)
                        {
                            case SectorFace.Floor:
                            case SectorFace.Floor_Triangle2:
                                ApplyTextureToFace(room, new VectorInt2(x, z), SectorFace.Floor, currentTexture);
                                ApplyTextureToFace(room, new VectorInt2(x, z), SectorFace.Floor_Triangle2, currentTexture);
                                break;

                            case SectorFace.Ceiling:
                            case SectorFace.Ceiling_Triangle2:
                                ApplyTextureToFace(room, new VectorInt2(x, z), SectorFace.Ceiling, currentTexture, false);
                                ApplyTextureToFace(room, new VectorInt2(x, z), SectorFace.Ceiling_Triangle2, currentTexture, false);
                                break;
                        }
                    }
                }
            }

            _editor.RoomTextureChange(room);
        }

        public static void TexturizeAll(Room room, SectorSelection selection, TextureArea texture, SectorFaceType type)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            if (type == SectorFaceType.Ceiling) texture.Mirror();
            RectangleInt2 area = selection.Valid ? selection.Area : _editor.SelectedRoom.LocalArea;

            texture.ParentArea = new Rectangle2();

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    switch (type)
                    {
                        case SectorFaceType.Floor:
                            if (!room.Sectors[x, z].IsFullWall)
                            {
                                ApplyTextureToFace(room, new VectorInt2(x, z), SectorFace.Floor, texture);
                                ApplyTextureToFace(room, new VectorInt2(x, z), SectorFace.Floor_Triangle2, texture);
                            }
                            break;

                        case SectorFaceType.Ceiling:
                            if (!room.Sectors[x, z].IsFullWall)
                            {
                                ApplyTextureToFace(room, new VectorInt2(x, z), SectorFace.Ceiling, texture);
                                ApplyTextureToFace(room, new VectorInt2(x, z), SectorFace.Ceiling_Triangle2, texture);
                            }
                            break;

                        case SectorFaceType.Wall:
                            foreach (SectorFace face in SectorFaceExtensions.GetWalls())
                                if (room.IsFaceDefined(x, z, face))
                                    ApplyTextureToFace(room, new VectorInt2(x, z), face, texture);
                            break;
                    }

                }

            _editor.RoomTextureChange(room);
        }

        private static void AllocateScriptIds(PositionBasedObjectInstance instance)
        {
            if (instance is IHasScriptID &&
                (_editor.Level.Settings.GameVersion == TRVersion.Game.TR4 || _editor.Level.IsNG))
            {
                var si = instance as IHasScriptID;
                if (si.ScriptId == null)
                    si.AllocateNewScriptId();
            }
            else if (instance is IHasLuaName && _editor.Level.IsTombEngine)
            {
                var li = instance as IHasLuaName;
                if (string.IsNullOrEmpty(li.LuaName))
                    li.AllocateNewLuaName();
            }

            if (instance is ObjectGroup)
                foreach (var obj in (ObjectGroup)instance)
                    AllocateScriptIds(obj);
        }

        public static void PlaceLight(LightType type)
        {
            var color = (type == LightType.FogBulb && _editor.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR4) ?
                Vector3.One : (Vector3)_editor.LastUsedPaletteColour * 2.0f;

            _editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(type) { Color = color });
        }

        public static void PlaceObject(Room room, VectorInt2 pos, ObjectInstance instance)
        {
            if (!(instance is ISpatial))
                return;

            if (instance is ObjectGroup)
            {
                PlaceObjectGroupContents(room, pos, (ObjectGroup)instance);
            }
            else if (instance is PositionBasedObjectInstance)
            {
                var posInstance = (PositionBasedObjectInstance)instance;

                PlaceObjectWithoutUpdate(room, pos, posInstance);
                _editor.UndoManager.PushObjectCreated(posInstance);
            }
            else if (instance is GhostBlockInstance)
            {
                var ghost = (GhostBlockInstance)instance;
                if (PlaceGhostBlockWithoutUpdate(room, pos, ghost))
                    _editor.UndoManager.PushGhostBlockCreated(ghost);
                else
                    _editor.SendMessage("You can't place two ghost blocks in one sector.", PopupType.Info);
            }
        }

        public static void PlaceObjectWithoutUpdate(Room room, VectorInt2 pos, PositionBasedObjectInstance instance) =>
            PlaceObjectWithoutUpdate(room, new Vector2(pos.X, pos.Y), instance);

        public static void PlaceObjectWithoutUpdate(Room room, Vector2 pos, PositionBasedObjectInstance instance)
        {
            instance.Position = room.GetFloorMidpointPosition(pos.X, pos.Y);
            room.AddObject(_editor.Level, instance);

            RebuildLightsForObject(instance);
            AllocateScriptIds(instance);

            _editor.ObjectChange(instance, ObjectChangeType.Add);
            _editor.SelectedObject = instance;
        }

        public static bool PlaceGhostBlockWithoutUpdate(Room room, VectorInt2 pos, GhostBlockInstance instance)
        {
            Sector sector = room.GetSector(pos);
            if (sector.HasGhostBlock)
                return false;

            instance.SectorPosition = pos;

            room.AddObject(_editor.Level, instance);
            _editor.ObjectChange(instance, ObjectChangeType.Add);
            _editor.SelectedObject = instance;
            _editor.RoomSectorPropertiesChange(room);

            return true;
        }

        public static void PlaceObjectGroupContents(Room room, VectorInt2 pos, ObjectGroup instance)
        {
            var undoList = new List<UndoRedoInstance>();

            // Update group position
            instance.Position = room.GetFloorMidpointPosition(pos.X, pos.Y);
            instance.SetRoom(room);

            // Place children
            foreach (var child in instance)
            {
                room.AddObject(_editor.Level, child);
                AllocateScriptIds(child);
                undoList.Add(new AddRemoveObjectUndoInstance(_editor.UndoManager, child, true));
            }

            // Update state
            _editor.UndoManager.Push(undoList);
            _editor.SelectedObject = instance;

            // Relight room just once
            RebuildLightsForObject(instance);
        }

        public static void SelectObjectsInArea(IWin32Window owner, SectorSelection area, bool resetCurrentSelection = true)
        {
            if (_editor.SelectedRoom == null || !area.Valid)
            {
                _editor.SendMessage("Please define a valid group of sectors.", PopupType.Error);
                return;
            }

            int objectCount = 0;

            var rootObject = _editor.SelectedObject;
            foreach (var obj in _editor.SelectedRoom.Objects)
            {
                if (obj == rootObject)
                    continue;

                if (area.Area.Contains(obj.SectorPosition))
                {
                    MultiSelect(obj);
                    objectCount++;
                }
            }

            if (objectCount == 0 && rootObject == null)
            {
                _editor.SendMessage("Defined area has no objects. None were selected.", PopupType.Info);
                return;
            }

            if (resetCurrentSelection)
                _editor.SelectedSectors = SectorSelection.None;
        }

        public static void MultiSelect(ObjectInstance instance)
        {
            var objPositionBased = instance as PositionBasedObjectInstance;

            // Ignore ctrl-clicks on things that are not multi-selectable

            if (objPositionBased == null || objPositionBased is ObjectGroup)
                return;

            // We've clicked on something multi-selectable

            var selectedItemInstance = _editor.SelectedObject as PositionBasedObjectInstance;
            if (selectedItemInstance != null)
            {
                // Selected object is also multi-selectable or already an object-group

                var objectGroup = selectedItemInstance as ObjectGroup ?? new ObjectGroup(selectedItemInstance);

                objectGroup.AddOrRemove(objPositionBased);

                if (objectGroup.Count() > 1)
                {
                    // There is more than one object in the group, keep it
                    _editor.SelectedObject = objectGroup;
                }
                else
                {
                    // We're left with one object, there is no reason to keep the object group
                    _editor.SelectedObject = objectGroup.FirstOrDefault();
                }
            }
            else
            {
                // Selected object is not multi-selectable or there's no selected object
                _editor.SelectedObject = instance;
            }
        }

        public static void MakeNewRoom(int index)
        {
            Room newRoom = new Room(_editor.Level,
                        _editor.Configuration.Editor_DefaultNewRoomSize,
                        _editor.Configuration.Editor_DefaultNewRoomSize,
                        _editor.Level.Settings.DefaultAmbientLight,
                        "Room " + index);

            _editor.Level.Rooms[index] = newRoom;
            _editor.RoomListChange();
            _editor.UndoManager.PushRoomCreated(newRoom);
            _editor.SelectRoom(newRoom);

            // Make border wall grids, as in dxtre3d
            if (_editor.Configuration.Editor_GridNewRoom)
                GridWallsSquares(newRoom, newRoom.LocalArea, false, false);
        }

        public static void DeleteRooms(IEnumerable<Room> rooms_, IWin32Window owner = null)
        {
            if (!rooms_.Any())
                return;

            rooms_ = rooms_.SelectMany(room => room.Versions).Distinct();
            HashSet<Room> rooms = new HashSet<Room>(rooms_);

            // Check if is the last room
            int remainingRoomCount = _editor.Level.ExistingRooms.Count(r => !rooms.Contains(r) && !rooms.Contains(r.AlternateOpposite));
            if (remainingRoomCount <= 0)
            {
                _editor.SendMessage("You must have at least one room in your level.", PopupType.Error);
                return;
            }

            // Ask for confirmation. No owner = silent mode!
            if (owner != null && DarkMessageBox.Show(owner,
                    "All objects (including portals) inside rooms will be deleted and \n" +
                    "triggers pointing to them will be removed.",
                    "Delete rooms?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            // Do it finally
            List<Room> adjoiningRooms = rooms.SelectMany(room => room.Portals)
                .Select(portal => portal.AdjoiningRoom)
                .Distinct()
                .Except(rooms)
                .ToList();

            // Delete rooms and collect a list of rooms in which related triggers were removed
            List<Room> affectedRooms = new List<Room>();
            foreach (Room room in rooms)
                affectedRooms = affectedRooms.Concat(_editor.Level.DeleteRoom(room)).ToList();

            // Update sector highlights in rooms where triggers for related objects were removed
            foreach (var r in affectedRooms)
                if (_editor.Level.Rooms.Contains(r)) // Room itself may be removed from level by now if multiselection was used
                    _editor.RoomPropertiesChange(r);

            // Update selection
            foreach (Room adjoiningRoom in adjoiningRooms)
            {
                adjoiningRoom?.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                adjoiningRoom?.AlternateOpposite?.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            }

            // Select last room, if available. Else select first existing room.
            if (rooms.Contains(_editor.SelectedRoom))
            {
                if (_editor.PreviousRoom == null || rooms.Contains(_editor.PreviousRoom))
                    _editor.SelectRoom(_editor.Level.ExistingRooms.FirstOrDefault());
                else
                    _editor.SelectRoom(_editor.PreviousRoom);
            }

            _editor.RoomListChange();
            _editor.RoomGeometryChange(_editor.SelectedRoom);
        }

        public static void CropRoom(Room room, RectangleInt2 newArea, IWin32Window owner)
        {
            // Figure out new room area
            if (newArea.X0 <= 0 || newArea.Y0 <= 0 || newArea.X1 <= 0 || newArea.Y1 <= 0)
                newArea = new RectangleInt2(new VectorInt2(), room.SectorSize - VectorInt2.One);
            else
                newArea = newArea.Inflate(1);

            bool useFloor;
            using (FormResizeRoom form = new FormResizeRoom(_editor, room, newArea))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return;
                newArea = form.NewArea;
                useFloor = form.UseFloor;
            }

            // Estimate wether triggers or portals must be removed
            // Ask the user again if portals or triggers are being removed in process.
            if (room.Portals.Any(portal => portal.Direction != PortalDirection.Floor && portal.Direction != PortalDirection.Ceiling && !newArea.Inflate(-1).Contains(portal.Area)) ||
                room.Triggers.Any(trigger => !newArea.Inflate(-1).Contains(trigger.Area)))
                if (DarkMessageBox.Show(owner, "Warning: if you crop this room, all portals and triggers outside the new area will be deleted." +
                    " Do you want to continue?", "Crop room", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

            // Resize room
            var relevantRooms = new HashSet<Room>(room.Portals.Select(p => p.AdjoiningRoom));
            if (room.Alternated)
                room.AlternateOpposite.Resize(_editor.Level, newArea, room.AlternateOpposite.GetLowestCorner(), room.AlternateOpposite.GetHighestCorner(), useFloor);
            room.Resize(_editor.Level, newArea, room.GetLowestCorner(), room.GetHighestCorner(), useFloor);
            Room.FixupNeighborPortals(_editor.Level, new[] { room }, new[] { room }, ref relevantRooms);
            Parallel.ForEach(relevantRooms, relevantRoom =>
            {
                relevantRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            });

            // Cleanup
            if ((_editor.SelectedRoom == room || _editor.SelectedRoom == room.AlternateOpposite) && _editor.SelectedSectors.Valid)
            {
                var selection = _editor.SelectedSectors;
                selection.Area = selection.Area.Intersect(newArea) - newArea.Start;
                _editor.SelectedSectors = selection;
            }
            foreach (Room relevantRoom in relevantRooms)
            {
                _editor.RoomPropertiesChange(relevantRoom);
                _editor.RoomSectorPropertiesChange(relevantRoom);
            }
        }

        public static void SetDiagonalFloorSplit(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Sectors[x, z].Type == SectorType.BorderWall)
                        continue;

                    if (room.Sectors[x, z].Floor.DiagonalSplit != DiagonalSplit.None)
                    {
                        if (room.Sectors[x, z].Type == SectorType.Floor)
                            room.Sectors[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, true);
                    }
                    else
                    {
                        // Now try to guess the floor split
                        int maxHeight = int.MinValue;
                        byte theCorner = 0;

                        if (room.Sectors[x, z].Floor.XnZp > maxHeight)
                        {
                            maxHeight = room.Sectors[x, z].Floor.XnZp;
                            theCorner = 0;
                        }

                        if (room.Sectors[x, z].Floor.XpZp > maxHeight)
                        {
                            maxHeight = room.Sectors[x, z].Floor.XpZp;
                            theCorner = 1;
                        }

                        if (room.Sectors[x, z].Floor.XpZn > maxHeight)
                        {
                            maxHeight = room.Sectors[x, z].Floor.XpZn;
                            theCorner = 2;
                        }

                        if (room.Sectors[x, z].Floor.XnZn > maxHeight)
                        {
                            maxHeight = room.Sectors[x, z].Floor.XnZn;
                            theCorner = 3;
                        }

                        switch(theCorner)
                        {
                            case 0:
                                room.Sectors[x, z].Floor.XpZp = maxHeight;
                                room.Sectors[x, z].Floor.XnZn = maxHeight;
                                room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZp;
                                break;
                            case 1:
                                room.Sectors[x, z].Floor.XnZp = maxHeight;
                                room.Sectors[x, z].Floor.XpZn = maxHeight;
                                room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZp;
                                break;
                            case 2:
                                room.Sectors[x, z].Floor.XpZp = maxHeight;
                                room.Sectors[x, z].Floor.XnZn = maxHeight;
                                room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZn;
                                break;
                            case 3:
                                room.Sectors[x, z].Floor.XnZp = maxHeight;
                                room.Sectors[x, z].Floor.XpZn = maxHeight;
                                room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZn;
                                break;
                        }
                        room.Sectors[x, z].Floor.SplitDirectionToggled = false;
                        room.Sectors[x, z].FixHeights();
                    }
                    room.Sectors[x, z].Type = SectorType.Floor;
                }
            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalCeilingSplit(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Sectors[x, z].Type == SectorType.BorderWall)
                        continue;

                    if (room.Sectors[x, z].Ceiling.DiagonalSplit != DiagonalSplit.None)
                    {
                        if (room.Sectors[x, z].Type == SectorType.Floor)
                            room.Sectors[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, false);
                    }
                    else
                    {
                        // Now try to guess the floor split
                        int minHeight = int.MaxValue;
                        byte theCorner = 0;

                        if (room.Sectors[x, z].Ceiling.XnZp < minHeight)
                        {
                            minHeight = room.Sectors[x, z].Ceiling.XnZp;
                            theCorner = 0;
                        }

                        if (room.Sectors[x, z].Ceiling.XpZp < minHeight)
                        {
                            minHeight = room.Sectors[x, z].Ceiling.XpZp;
                            theCorner = 1;
                        }

                        if (room.Sectors[x, z].Ceiling.XpZn < minHeight)
                        {
                            minHeight = room.Sectors[x, z].Ceiling.XpZn;
                            theCorner = 2;
                        }

                        if (room.Sectors[x, z].Ceiling.XnZn < minHeight)
                        {
                            minHeight = room.Sectors[x, z].Ceiling.XnZn;
                            theCorner = 3;
                        }

                        switch(theCorner)
                        {
                            case 0:
                                room.Sectors[x, z].Ceiling.XpZp = minHeight;
                                room.Sectors[x, z].Ceiling.XnZn = minHeight;
                                room.Sectors[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XnZp;
                                break;
                            case 1:
                                room.Sectors[x, z].Ceiling.XnZp = minHeight;
                                room.Sectors[x, z].Ceiling.XpZn = minHeight;
                                room.Sectors[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XpZp;
                                break;
                            case 2:
                                room.Sectors[x, z].Ceiling.XpZp = minHeight;
                                room.Sectors[x, z].Ceiling.XnZn = minHeight;
                                room.Sectors[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XpZn;
                                break;
                            case 3:
                                room.Sectors[x, z].Ceiling.XnZp = minHeight;
                                room.Sectors[x, z].Ceiling.XpZn = minHeight;
                                room.Sectors[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XnZn;
                                break;
                        }
                        room.Sectors[x, z].Ceiling.SplitDirectionToggled = false;
                        room.Sectors[x, z].FixHeights();
                    }
                    room.Sectors[x, z].Type = SectorType.Floor;
                }
            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalWall(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Sectors[x, z].Type == SectorType.BorderWall)
                        continue;

                    if (room.Sectors[x, z].Floor.DiagonalSplit != DiagonalSplit.None)
                    {
                        if (room.Sectors[x, z].Type == SectorType.Wall)
                            room.Sectors[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, null);
                        else
                            room.Sectors[x, z].Ceiling.DiagonalSplit = room.Sectors[x, z].Floor.DiagonalSplit;
                    }
                    else
                    {
                        // Now try to guess the floor split
                        int maxHeight = int.MinValue;
                        byte theCorner = 0;

                        if (room.Sectors[x, z].Floor.XnZp > maxHeight)
                        {
                            maxHeight = room.Sectors[x, z].Floor.XnZp;
                            theCorner = 0;
                        }

                        if (room.Sectors[x, z].Floor.XpZp > maxHeight)
                        {
                            maxHeight = room.Sectors[x, z].Floor.XpZp;
                            theCorner = 1;
                        }

                        if (room.Sectors[x, z].Floor.XpZn > maxHeight)
                        {
                            maxHeight = room.Sectors[x, z].Floor.XpZn;
                            theCorner = 2;
                        }

                        if (room.Sectors[x, z].Floor.XnZn > maxHeight)
                        {
                            maxHeight = room.Sectors[x, z].Floor.XnZn;
                            theCorner = 3;
                        }

                        switch(theCorner)
                        {
                            case 0:
                                room.Sectors[x, z].Floor.XpZp = maxHeight;
                                room.Sectors[x, z].Floor.XnZn = maxHeight;
                                room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZp;
                                break;
                            case 1:
                                room.Sectors[x, z].Floor.XnZp = maxHeight;
                                room.Sectors[x, z].Floor.XpZn = maxHeight;
                                room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZp;
                                break;
                            case 2:
                                room.Sectors[x, z].Floor.XpZp = maxHeight;
                                room.Sectors[x, z].Floor.XnZn = maxHeight;
                                room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZn;
                                break;
                            case 3:
                                room.Sectors[x, z].Floor.XnZp = maxHeight;
                                room.Sectors[x, z].Floor.XpZn = maxHeight;
                                room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZn;
                                break;
                        }
                        room.Sectors[x, z].Ceiling.DiagonalSplit = room.Sectors[x, z].Floor.DiagonalSplit;
                    }
                    room.Sectors[x, z].Type = SectorType.Wall;
                }
            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void RotateSectors(Room room, RectangleInt2 area, bool floor)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            bool wallsRotated = false;

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Sectors[x, z].Type == SectorType.BorderWall)
                        continue;
                    room.Sectors[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, room.Sectors[x, z].IsAnyWall ? null : (bool?)floor);

                    if (room.Sectors[x, z].Floor.DiagonalSplit != DiagonalSplit.None && room.Sectors[x, z].IsAnyWall)
                        wallsRotated = true;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomGeometryChange(room);

            if (wallsRotated)
                _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetWall(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Sectors[x, z].Type == SectorType.BorderWall)
                        continue;
                    room.Sectors[x, z].Type = SectorType.Wall;
                    room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.None;
                    room.Sectors[x, z].Ceiling.DiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetFloor(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);
            SetSurfaceWithoutUpdate(room, area, false);
            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetCeiling(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);
            SetSurfaceWithoutUpdate(room, area, true);
            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetSurfaceWithoutUpdate(Room room, RectangleInt2 area, bool ceiling)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Sectors[x, z].Type == SectorType.BorderWall)
                        continue;

                    room.Sectors[x, z].Type = SectorType.Floor;

                    if (ceiling)
                        room.Sectors[x, z].Ceiling.DiagonalSplit = DiagonalSplit.None;
                    else
                        room.Sectors[x, z].Floor.DiagonalSplit = DiagonalSplit.None;
                }
        }

        public static void ToggleSectorFlag(Room room, RectangleInt2 area, SectorFlags flag)
        {
            List<Room> roomsToUpdate = new List<Room>();
            roomsToUpdate.Add(room);

            // Collect all affected rooms for undo
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    var currentSector = room.ProbeLowestSector(x, z, _editor.Configuration.UI_ProbeAttributesThroughPortals);
                    if (!roomsToUpdate.Contains(currentSector.Room))
                        roomsToUpdate.Add(currentSector.Room);
                }

            // Do undo
            _editor.UndoManager.PushGeometryChanged(roomsToUpdate);

            if (_editor.Configuration.UI_SetAttributesAtOnce)
            {
                // Set or unset flag, based on already existing flag prevalence in selected area
                int amount = (area.Width + 1) * (area.Height + 1);
                int prevalence = 0;

                for (int x = area.X0; x <= area.X1; x++)
                    for (int z = area.Y0; z <= area.Y1; z++)
                    {
                        var currentSector = room.ProbeLowestSector(x, z, _editor.Configuration.UI_ProbeAttributesThroughPortals);
                        if ((currentSector.Sector.Flags & flag) != SectorFlags.None) prevalence++;
                    }

                bool toggle = (prevalence == 0 || prevalence <= (amount / 2));

                // Do actual flag editing
                for (int x = area.X0; x <= area.X1; x++)
                    for (int z = area.Y0; z <= area.Y1; z++)
                    {
                        var currentSector = room.ProbeLowestSector(x, z, _editor.Configuration.UI_ProbeAttributesThroughPortals);
                        if (toggle) currentSector.Sector.Flags |= flag;
                        else currentSector.Sector.Flags &= ~flag;
                    }
            }
            else
            {
                // Do actual flag editing
                for (int x = area.X0; x <= area.X1; x++)
                    for (int z = area.Y0; z <= area.Y1; z++)
                    {
                        var currentSector = room.ProbeLowestSector(x, z, _editor.Configuration.UI_ProbeAttributesThroughPortals);
                        currentSector.Sector.Flags ^= flag;
                    }
            }

            foreach (var currentRoom in roomsToUpdate)
                _editor.RoomSectorPropertiesChange(currentRoom);
        }

        public static void ToggleForceFloorSolid(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    room.Sectors[x, z].ForceFloorSolid = !room.Sectors[x, z].ForceFloorSolid;
            SmartBuildGeometry(room, area);
            _editor.RoomGeometryChange(room);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void AddPortal(Room room, RectangleInt2 area, IWin32Window owner)
        {
            if (room.CornerSelected(area))
            {
                _editor.SendMessage("You have selected one of the four room's corners.", PopupType.Error);
                return;
            }

            // Check vertical space
            int floorLevel = int.MaxValue;
            int ceilingLevel = int.MinValue;
            for (int y = area.Y0; y <= area.Y1 + 1; ++y)
                for (int x = area.X0; x <= area.X1 + 1; ++x)
                {
                    floorLevel = room.GetHeightsAtPoint(x, y, SectorVerticalPart.QA).Select(v => v + room.Position.Y).Concat(new int[] { floorLevel }).Min();
                    ceilingLevel = room.GetHeightsAtPoint(x, y, SectorVerticalPart.WS).Select(v => v + room.Position.Y).Concat(new int[] { ceilingLevel }).Max();
                }

            // Check for possible candidates ...
            List<Tuple<PortalDirection, Room>> candidates = new List<Tuple<PortalDirection, Room>>();
            if (floorLevel != int.MaxValue && ceilingLevel != int.MinValue)
            {
                bool couldBeFloorCeilingPortal = false;
                if (new RectangleInt2(1, 1, room.NumXSectors - 2, room.NumZSectors - 2).Contains(area))
                    for (int z = area.Y0; z <= area.Y1; ++z)
                        for (int x = area.X0; x <= area.X1; ++x)
                            if (!room.Sectors[x, z].IsAnyWall)
                                couldBeFloorCeilingPortal = true;

                foreach (var neighborRoom in _editor.Level.ExistingRooms)
                {
                    // Don't make a portal to the room itself
                    // Don't list alternate rooms as seperate candidates
                    if (neighborRoom == room || neighborRoom == room.AlternateOpposite || neighborRoom.AlternateBaseRoom != null)
                        continue;
                    RectangleInt2 neighborArea = area + (room.SectorPos - neighborRoom.SectorPos);
                    if (!new RectangleInt2(0, 0, neighborRoom.NumXSectors - 1, neighborRoom.NumZSectors - 1).Contains(neighborArea))
                        continue;

                    // Check if they vertically touch
                    int neighborFloorLevel = int.MaxValue;
                    int neighborCeilingLevel = int.MinValue;
                    for (int y = neighborArea.Y0; y <= neighborArea.Y1 + 1; ++y)
                        for (int x = neighborArea.X0; x <= neighborArea.X1 + 1; ++x)
                        {
                            neighborFloorLevel = neighborRoom.GetHeightsAtPoint(x, y, SectorVerticalPart.QA).Select(v => v + neighborRoom.Position.Y).Concat(new int[] { neighborFloorLevel }).Min();
                            neighborCeilingLevel = neighborRoom.GetHeightsAtPoint(x, y, SectorVerticalPart.WS).Select(v => v + neighborRoom.Position.Y).Concat(new int[] { neighborCeilingLevel }).Max();
                            if (neighborRoom.AlternateOpposite != null)
                            {
                                neighborFloorLevel = neighborRoom.AlternateOpposite.GetHeightsAtPoint(x, y, SectorVerticalPart.QA).Select(v => v + neighborRoom.AlternateOpposite.Position.Y).Concat(new int[] { neighborFloorLevel }).Min();
                                neighborCeilingLevel = neighborRoom.AlternateOpposite.GetHeightsAtPoint(x, y, SectorVerticalPart.WS).Select(v => v + neighborRoom.AlternateOpposite.Position.Y).Concat(new int[] { neighborCeilingLevel }).Max();
                            }
                        }
                    if (neighborFloorLevel == int.MaxValue || neighborCeilingLevel == int.MinValue)
                        continue;
                    if (!(floorLevel <= neighborCeilingLevel && ceilingLevel >= neighborFloorLevel))
                        continue;

                    // Decide on a direction
                    if (couldBeFloorCeilingPortal &&
                        new RectangleInt2(1, 1, neighborRoom.NumXSectors - 2, neighborRoom.NumZSectors - 2).Contains(neighborArea))
                    {
                        if (Math.Abs(neighborCeilingLevel - floorLevel) <
                            Math.Abs(neighborFloorLevel - ceilingLevel))
                        { // Consider floor portal
                            candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.Floor, neighborRoom));
                        }
                        else
                        { // Consider ceiling portal
                            candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.Ceiling, neighborRoom));
                        }
                    }
                    if (area.Width == 0 && area.X0 == 0)
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallNegativeX, neighborRoom));
                    if (area.Width == 0 && area.X0 == room.NumXSectors - 1)
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallPositiveX, neighborRoom));
                    if (area.Height == 0 && area.Y0 == 0)
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallNegativeZ, neighborRoom));
                    if (area.Height == 0 && area.Y0 == room.NumZSectors - 1)
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallPositiveZ, neighborRoom));
                }
            }

            if (candidates.Count > 1)
            {
                using (var form = new FormChooseRoom("More than one possible room found that can be connected. " +
                    "Please choose one:", candidates.Select(candidate => candidate.Item2), selectedRoom => _editor.SelectedRoom = selectedRoom))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK || form.SelectedRoom == null)
                        return;
                    candidates.RemoveAll(candidate => candidate.Item2 != form.SelectedRoom);
                }
            }
            if (candidates.Count != 1)
            {
                _editor.SendMessage("There are no possible room candidates for a portal.", PopupType.Error);
                return;
            }

            PortalDirection destinationDirection = candidates[0].Item1;
            Room destination = candidates[0].Item2;

            // Create portals
            var mainPortal = new PortalInstance(area, destinationDirection, destination);
            var portals = room.AddObject(_editor.Level, mainPortal).Cast<PortalInstance>();

            if (destinationDirection >= PortalDirection.WallPositiveZ) // If portal is any wall
            {
                // Remove all splits from affected walls to expose geometry of the other room
                for (int z = area.Y0; z <= area.Y1; ++z)
                    for (int x = area.X0; x <= area.X1; ++x)
                    {
                        Sector sector = room.GetSectorTry(x, z);

                        if (sector == null)
                            continue;

                        sector.ExtraFloorSplits.Clear();
                        sector.ExtraCeilingSplits.Clear();
                    }
            }

            // Update
            foreach (Room portalRoom in portals.Select(portal => portal.Room).Distinct())
            {
                portalRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            }

            foreach (PortalInstance portal in portals)
                _editor.ObjectChange(portal, ObjectChangeType.Add);

            // Reset selection
            _editor.Action = null;
            _editor.SelectedSectors = SectorSelection.None;
            _editor.SelectedObject = null;
            _editor.SelectedRooms = new[] { _editor.SelectedRoom };

            _editor.RoomSectorPropertiesChange(room);
            _editor.RoomSectorPropertiesChange(destination);

            // Undo
            _editor.UndoManager.PushSectorObjectCreated(mainPortal);
        }

        public static void AlternateRoomEnable(Room room, short AlternateGroup)
        {
            // Create new room
            var newRoom = room.Clone(_editor.Level, instance => instance.CopyToAlternateRooms);

            // Disable lock flag to prevent deadlocks
            newRoom.Properties.Locked = false;

            newRoom.Name = room + " (Flipped)";
            newRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);

            // Assign room
            _editor.Level.AssignRoomToFree(newRoom);
            _editor.RoomListChange();

            // Update room alternate groups
            room.AlternateGroup = AlternateGroup;
            room.AlternateRoom = newRoom;
            newRoom.AlternateGroup = AlternateGroup;
            newRoom.AlternateBaseRoom = room;

            _editor.RoomPropertiesChange(room);
            _editor.RoomPropertiesChange(newRoom);
        }

        public static void AlternateRoomDisableWithWarning(Room room, IWin32Window owner)
        {
            room = room.AlternateBaseRoom ?? room;

            // Ask for confirmation
            if (DarkMessageBox.Show(owner, "Do you really want to delete the flip room?",
                "Delete flipped room", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            // Change room selection if necessary
            if (_editor.SelectedRoom == room.AlternateRoom)
                _editor.SelectedRoom = room;

            // Delete alternate room
            var affectedRooms = _editor.Level.DeleteRoom(room.AlternateRoom);
            room.AlternateRoom = null;
            room.AlternateGroup = -1;

            // Update sector highlights in rooms where triggers for related objects were removed
            foreach (var r in affectedRooms)
                _editor.RoomPropertiesChange(r);

            _editor.RoomListChange();
            _editor.RoomPropertiesChange(room);
        }

        public static void SmoothRandom(Room room, RectangleInt2 area, float strengthDirection, SectorVerticalPart vertical)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            float[,] changes = new float[area.Width + 2, area.Height + 2];
            Random rng = new Random();
            for (int x = 1; x <= area.Width; x++)
                for (int z = 1; z <= area.Height; z++)
                    changes[x, z] = (float)rng.NextDouble() * strengthDirection;

            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                        room.ChangeSectorHeight(area.X0 + x, area.Y0 + z, vertical, edge,
                            (int)Math.Round(changes[x + edge.DirectionX(), z + edge.DirectionZ()]) * _editor.IncrementReference);

            SmartBuildGeometry(room, area);
        }

        public static void SharpRandom(Room room, RectangleInt2 area, float strengthDirection, SectorVerticalPart vertical)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            Random rng = new Random();
            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                        room.ChangeSectorHeight(area.X0 + x, area.Y0 + z, vertical, edge,
                            (int)Math.Round((float)rng.NextDouble() * strengthDirection) * _editor.IncrementReference);

            SmartBuildGeometry(room, area);
        }

        public static void AverageSectors(Room room, RectangleInt2 area, SectorVerticalPart vertical, int increments)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Sector b = room.Sectors[x, z];
                    int sum = 0;

                    for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                        sum += b.GetHeight(vertical, edge);

                    sum /= increments;

                    for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                        b.SetHeight(vertical, edge, sum / 4 * increments);
                }
            SmartBuildGeometry(room, area);
        }

        public static void GridWalls(Room room, RectangleInt2 area, bool fiveDivisions = false)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Sector sector = room.Sectors[x, z];
                    if (sector.IsAnyWall)
                    {
                        // Figure out corner heights
                        int?[] floorHeights = new int?[(int)SectorEdge.Count];
                        int?[] ceilingHeights = new int?[(int)SectorEdge.Count];
                        for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                        {
                            int testX = x + edge.DirectionX(), testZ = z + edge.DirectionZ();
                            floorHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, SectorVerticalPart.QA).Cast<int?>().Max();
                            ceilingHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, SectorVerticalPart.WS).Cast<int?>().Min();

                            floorHeights[(int)edge] /= _editor.IncrementReference;
                            ceilingHeights[(int)edge] /= _editor.IncrementReference;
                        }

                        if (!floorHeights.Any(floorHeight => floorHeight.HasValue) || !ceilingHeights.Any(floorHeight => floorHeight.HasValue))
                            continue; // We can only do it if there is information available

                        sector.ExtraFloorSplits.Clear();
                        sector.ExtraCeilingSplits.Clear();

                        for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                        {
                            // Skip opposite diagonal step corner
                            switch (sector.Floor.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZn:
                                    if (edge == SectorEdge.XpZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XnZp:
                                    if (edge == SectorEdge.XpZn)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (edge == SectorEdge.XnZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (edge == SectorEdge.XnZn)
                                        continue;
                                    break;
                            }

                            // Use the closest available vertical area information and divide it equally
                            int floor = floorHeights[(int)edge] ?? floorHeights[((int)edge + 1) % 4] ?? floorHeights[((int)edge + 3) % 4] ?? floorHeights[((int)edge + 2) % 4].Value;
                            int ceiling = ceilingHeights[(int)edge] ?? ceilingHeights[((int)edge + 1) % 4] ?? ceilingHeights[((int)edge + 3) % 4] ?? ceilingHeights[((int)edge + 2) % 4].Value;

                            // TODO: Add support for more splits

                            int edHeight = (int)Math.Round(fiveDivisions ? (floor * 4.0f + ceiling * 1.0f) / 5.0f : floor),
                                qaHeight = (int)Math.Round(fiveDivisions ? (floor * 3.0f + ceiling * 2.0f) / 5.0f : (floor * 2.0f + ceiling * 1.0f) / 3.0f),
                                wsHeight = (int)Math.Round(fiveDivisions ? (floor * 2.0f + ceiling * 3.0f) / 5.0f : (floor * 1.0f + ceiling * 2.0f) / 3.0f),
                                rfHeight = (int)Math.Round(fiveDivisions ? (floor * 1.0f + ceiling * 4.0f) / 5.0f : ceiling);

                            edHeight *= _editor.IncrementReference;
                            qaHeight *= _editor.IncrementReference;
                            wsHeight *= _editor.IncrementReference;
                            rfHeight *= _editor.IncrementReference;

                            sector.SetHeight(SectorVerticalPart.Floor2, edge, edHeight);
                            sector.Floor.SetHeight(edge, qaHeight);
                            sector.Ceiling.SetHeight(edge, wsHeight);
                            sector.SetHeight(SectorVerticalPart.Ceiling2, edge, rfHeight);
                        }
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void GridWallsSquares(Room room, RectangleInt2 area, bool fiveDivisions = false, bool fromUI = true)
        {
            // Don't undo if action is called implicitly (e.g. new room/level creation)
            if (fromUI)
                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            int minFloor = int.MaxValue;
            int maxCeiling = int.MinValue;

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Sector sector = room.Sectors[x, z];
                    if (sector.IsAnyWall)
                    {
                        // Figure out corner heights
                        int?[] floorHeights = new int?[(int)SectorEdge.Count];
                        int?[] ceilingHeights = new int?[(int)SectorEdge.Count];
                        for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                        {
                            int testX = x + edge.DirectionX(), testZ = z + edge.DirectionZ();
                            floorHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, SectorVerticalPart.QA).Cast<int?>().Max();
                            ceilingHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, SectorVerticalPart.WS).Cast<int?>().Min();

                            floorHeights[(int)edge] /= _editor.IncrementReference;
                            ceilingHeights[(int)edge] /= _editor.IncrementReference;
                        }

                        if (!floorHeights.Any(floorHeight => floorHeight.HasValue) || !ceilingHeights.Any(floorHeight => floorHeight.HasValue))
                            continue; // We can only do it if there is information available

                        for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                        {
                            // Skip opposite diagonal step corner
                            switch (sector.Floor.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZn:
                                    if (edge == SectorEdge.XpZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XnZp:
                                    if (edge == SectorEdge.XpZn)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (edge == SectorEdge.XnZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (edge == SectorEdge.XnZn)
                                        continue;
                                    break;
                            }

                            // Use the closest available vertical area information and divide it equally
                            int floor = floorHeights[(int)edge] ?? floorHeights[((int)edge + 1) % 4] ?? floorHeights[((int)edge + 3) % 4] ?? floorHeights[((int)edge + 2) % 4].Value;
                            int ceiling = ceilingHeights[(int)edge] ?? ceilingHeights[((int)edge + 1) % 4] ?? ceilingHeights[((int)edge + 3) % 4] ?? ceilingHeights[((int)edge + 2) % 4].Value;

                            if (floor <= minFloor) minFloor = floor;
                            if (ceiling >= maxCeiling) maxCeiling = ceiling;
                        }
                    }
                }

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Sector sector = room.Sectors[x, z];
                    if (sector.IsAnyWall)
                    {
                        for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                        {
                            // Skip opposite diagonal step corner
                            switch (sector.Floor.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZn:
                                    if (edge == SectorEdge.XpZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XnZp:
                                    if (edge == SectorEdge.XpZn)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (edge == SectorEdge.XnZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (edge == SectorEdge.XnZn)
                                        continue;
                                    break;
                            }

                            // TODO: Add support for more splits

                            int edHeight = (int)Math.Round(fiveDivisions ? (minFloor * 4.0f + maxCeiling * 1.0f) / 5.0f : minFloor),
                                qaHeight = (int)Math.Round(fiveDivisions ? (minFloor * 3.0f + maxCeiling * 2.0f) / 5.0f : (minFloor * 2.0f + maxCeiling * 1.0f) / 3.0f),
                                wsHeight = (int)Math.Round(fiveDivisions ? (minFloor * 2.0f + maxCeiling * 3.0f) / 5.0f : (minFloor * 1.0f + maxCeiling * 2.0f) / 3.0f),
                                rfHeight = (int)Math.Round(fiveDivisions ? (minFloor * 1.0f + maxCeiling * 4.0f) / 5.0f : maxCeiling);

                            edHeight *= _editor.IncrementReference;
                            qaHeight *= _editor.IncrementReference;
                            wsHeight *= _editor.IncrementReference;
                            rfHeight *= _editor.IncrementReference;

                            sector.SetHeight(SectorVerticalPart.Floor2, edge, edHeight);
                            sector.Floor.SetHeight(edge, qaHeight);
                            sector.Ceiling.SetHeight(edge, wsHeight);
                            sector.SetHeight(SectorVerticalPart.Ceiling2, edge, rfHeight);
                        }
                    }
                }

            // Explicitly build geometry if action is called from user interface.
            // Otherwise (e.g. new room or level creation), do it implicitly, without calling global editor events.

            if (fromUI)
                SmartBuildGeometry(room, area);
            else
                room.BuildGeometry();
        }

        public static Room CreateAdjoiningRoom(Room room, SectorSelection selection, PortalDirection direction, bool grid, int roomDepth, bool switchRoom = true, bool clearAdjoiningArea = false)
        {
            if (!selection.Empty && !selection.Valid)
            {
                _editor.SendMessage("Selection is invalid. Can't create new room.", PopupType.Error);
                return null;
            }

            var clampedSelection = (selection.Empty ? new SectorSelection { Area = room.LocalArea } : selection).ClampToRoom(room, PortalInstance.GetDirection(direction));
            if (!clampedSelection.HasValue)
            {
                _editor.SendMessage("Can't create adjoining room. \nSelection is inside border walls.", PopupType.Error);
                return null;
            }

            if (room.IsAnyPortal(direction, clampedSelection.Value.Area))
            {
                _editor.SendMessage("Can't create adjoining room. \nSelection contains existing portals.", PopupType.Error);
                return null;
            }

            int roomSizeX, roomSizeY, roomSizeZ;
            VectorInt3 roomPos;
            RectangleInt2 portalArea, roomArea;
            var dirString = "";

            int selectionRefPoint = 0;
            if (direction == PortalDirection.WallPositiveZ)
                selectionRefPoint = room.NumZSectors - 1;
            else if (direction == PortalDirection.WallPositiveX)
                selectionRefPoint = room.NumXSectors - 1;

            switch (direction)
            {
                case PortalDirection.WallNegativeX:
                case PortalDirection.WallPositiveX:
                    if (!(clampedSelection.Value.Area.Start.X == selectionRefPoint || clampedSelection.Value.Area.End.X == selectionRefPoint))
                    {
                        _editor.SendMessage("Can't create horizontally adjoining room. \nSelect room border to attach it.", PopupType.Error);
                        return null; // Don't create room aside if selection doesn't touch border wall
                    }

                    // Inflate small selection to define min/max height later based on neighbor sector heights
                    if (clampedSelection.Value.Area.Size.X == 0)
                    {
                        clampedSelection = new SectorSelection() { Area = clampedSelection.Value.Area.Inflate(1, 0) };
                        clampedSelection = clampedSelection.Value.ClampToRoom(room);
                    }

                    roomSizeX = roomDepth + 2;
                    roomSizeY = room.GetHighestCorner(clampedSelection.Value.Area) - room.GetLowestCorner(clampedSelection.Value.Area);
                    roomSizeZ = clampedSelection.Value.Area.Size.Y + 3;
                    roomPos   = room.Position + new VectorInt3(direction == PortalDirection.WallNegativeX ? -roomDepth : room.NumXSectors - 2,
                                                               room.GetLowestCorner(clampedSelection.Value.Area),
                                                               clampedSelection.Value.Area.Start.Y - 1);
                    // Position portal and assign dir name
                    if (direction == PortalDirection.WallNegativeX)
                    {
                        portalArea = new RectangleInt2(roomSizeX - 1, 1, roomSizeX - 1, roomSizeZ - 2);
                        roomArea = new RectangleInt2(0, 0, roomSizeX - 2, roomSizeZ - 1);
                        dirString = "left";
                    }
                    else
                    {
                        portalArea = new RectangleInt2(0, 1, 0, roomSizeZ - 2);
                        roomArea = new RectangleInt2(1, 0, roomSizeX - 1, roomSizeZ - 1);
                        dirString = "right";
                    }
                    break;

                case PortalDirection.WallNegativeZ:
                case PortalDirection.WallPositiveZ:
                    if (!(clampedSelection.Value.Area.Start.Y == selectionRefPoint || clampedSelection.Value.Area.End.Y == selectionRefPoint))
                    {
                        _editor.SendMessage("Can't create horizontally adjoining room. \nSelect room border to attach it.", PopupType.Error);
                        return null; // Don't create room aside if selection doesn't touch border wall
                    }

                    // Inflate small selection to define min/max height later based on neighbor sector heights
                    if (clampedSelection.Value.Area.Size.Y == 0)
                    {
                        clampedSelection = new SectorSelection() { Area = clampedSelection.Value.Area.Inflate(0, 1) };
                        clampedSelection = clampedSelection.Value.ClampToRoom(room);
                    }

                    roomSizeX = clampedSelection.Value.Area.Size.X + 3;
                    roomSizeY = room.GetHighestCorner(clampedSelection.Value.Area) - room.GetLowestCorner(clampedSelection.Value.Area);
                    roomSizeZ = roomDepth + 2;
                    roomPos   = room.Position + new VectorInt3(clampedSelection.Value.Area.Start.X - 1,
                                                               room.GetLowestCorner(clampedSelection.Value.Area),
                                                               direction == PortalDirection.WallNegativeZ ? -roomDepth : room.NumZSectors - 2);
                    // Position portal and assign dir name
                    if (direction == PortalDirection.WallNegativeZ)
                    {
                        portalArea = new RectangleInt2(1, roomSizeZ - 1, roomSizeX - 2, roomSizeZ - 1);
                        roomArea = new RectangleInt2(0, 0, roomSizeX - 1, roomSizeZ - 2);
                        dirString = "back";
                    }
                    else
                    {
                        portalArea = new RectangleInt2(1, 0, roomSizeX - 2, 0);
                        roomArea = new RectangleInt2(0, 1, roomSizeX - 1, roomSizeZ - 1);
                        dirString = "in front";
                    }
                    break;

                case PortalDirection.Floor:
                case PortalDirection.Ceiling:
                default:
                    roomSizeX = clampedSelection.Value.Area.Size.X + 3;
                    roomSizeY = roomDepth;
                    roomSizeZ = clampedSelection.Value.Area.Size.Y + 3;
                    roomPos = room.Position + new VectorInt3(clampedSelection.Value.Area.Start.X - 1,
                                                             direction == PortalDirection.Floor ? room.GetLowestCorner(selection.Area) - roomDepth : room.GetHighestCorner(selection.Area),
                                                             clampedSelection.Value.Area.Start.Y - 1);
                    portalArea = new RectangleInt2(1, 1, roomSizeX - 2, roomSizeZ - 2);
                    roomArea = new RectangleInt2(0, 0, roomSizeX - 1, roomSizeZ - 1);
                    dirString = direction == PortalDirection.Floor ? "below" : "above";

                    // Reset parent floor or ceiling to adjoin new portal
                    if(clearAdjoiningArea)
                        FlattenRoomArea(room, clampedSelection.Value.Area, null, direction == PortalDirection.Ceiling, false, false);
                    break;
            }

            // Create room and attach portal
            var newRoom = new Room(_editor.Level, roomSizeX, roomSizeZ, _editor.Level.Settings.DefaultAmbientLight,
                                   "", roomSizeY);
            int roomNumber = _editor.Level.AssignRoomToFree(newRoom);
            newRoom.Position = roomPos;
            newRoom.AddObject(_editor.Level, new PortalInstance(portalArea, PortalInstance.GetOppositeDirection(direction), room));
            newRoom.Name = "Room " + roomNumber;

            // Copy main parent room properties
            newRoom.Properties.AmbientLight = room.Properties.AmbientLight;
            newRoom.Properties.Type = room.Properties.Type;
            newRoom.Properties.TypeStrength = room.Properties.TypeStrength;
            newRoom.Properties.FlagCold = room.Properties.FlagCold;
            newRoom.Properties.FlagDamage = room.Properties.FlagDamage;
            newRoom.Properties.FlagOutside = room.Properties.FlagOutside;
            newRoom.Properties.FlagHorizon = room.Properties.FlagHorizon;

            if (_editor.Configuration.UI_GenerateRoomDescriptions)
                newRoom.Name += " (dug " + dirString + ")";

            if (grid && _editor.Configuration.Editor_GridNewRoom && roomArea.Size.X > 0 && roomArea.Size.Y > 0)
                GridWallsSquares(newRoom, roomArea, false, false);

            _editor.RoomListChange();

            // Build the geometry of the new room
            Parallel.Invoke(() =>
            {
                newRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            }, () =>
            {
                room.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            });

            if (switchRoom && (_editor.SelectedRoom == room || _editor.SelectedRoom == room.AlternateOpposite))
                _editor.SelectedRoom = newRoom; // Don't center
            else
            {
                _editor.RoomSectorPropertiesChange(room);
                _editor.RoomSectorPropertiesChange(newRoom);
            }

            // Undo
            _editor.UndoManager.PushAdjoiningRoomCreated(newRoom);

            return newRoom;
        }

        public static void FlattenRoomArea(Room room, RectangleInt2 area, int? height, bool ceiling, bool includeWalls, bool rebuild, bool disableUndo = false)
        {
            if (!disableUndo)
                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            if (!height.HasValue)
                height = ceiling ? room.GetHighestCorner() : room.GetLowestCorner();

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Sectors[x, z].Type == SectorType.Floor || (includeWalls && room.Sectors[x, z].Type != SectorType.Floor))
                    {
                        if (ceiling)
                            room.Sectors[x, z].Ceiling.SetHeight(height.Value);
                        else
                            room.Sectors[x, z].Floor.SetHeight(height.Value);

                        room.Sectors[x, z].FixHeights();
                    }
                }

            if(rebuild)
                SmartBuildGeometry(room, area);
        }

        public static void MergeRoomsHorizontally(IEnumerable<Room> rooms, IWin32Window owner)
        {
            if (rooms.Count() < 2)
            {
                _editor.SendMessage("Select at least 2 rooms to merge them.", PopupType.Info);
                return;
            }

            bool alternated = rooms.Any(room => room.Alternated);
            if (alternated)
                rooms = rooms.Select(room => room.AlternateBaseRoom ?? room);
            rooms = rooms.Distinct();

            // First we need to make sure that no rooms overlap
            // If rooms overlap, it's uncertain which room takes priority.
            Func<Room, Dictionary<VectorInt2, Room>, bool> buildSectorMap = (room, sectorMap) =>
            {
                bool roomOverlaps = false;
                for (int x = 1; x < (room.NumXSectors - 1); ++x)
                    for (int z = 1; z < (room.NumZSectors - 1); ++z)
                        // TODO: maybe we could improve this, for now keep it disabled and test
                        if (!room.Sectors[x, z].IsAnyWall  || true)
                        {
                            VectorInt2 worldPos = room.SectorPos + new VectorInt2(x, z);
                            Room existingRoom;
                            if (sectorMap.TryGetValue(worldPos, out existingRoom))
                                roomOverlaps = true;
                            else
                                sectorMap.Add(worldPos, room);
                        }
                return roomOverlaps;
            };

            bool baseRoomOverlaps = false;
            bool alternateRoomOverlaps = false;
            Dictionary<VectorInt2, Room> baseNewSectorMap = new Dictionary<VectorInt2, Room>();
            Dictionary<VectorInt2, Room> alternateNewSectorMap = new Dictionary<VectorInt2, Room>();
            foreach (Room room in rooms)
            {
                baseRoomOverlaps = buildSectorMap(room, baseNewSectorMap) || baseRoomOverlaps;
                if (alternated)
                    alternateRoomOverlaps = buildSectorMap(room.AlternateOpposite ?? room, alternateNewSectorMap) || alternateRoomOverlaps;
            }

            // If there are overlaps, ask the user about it
            if (baseRoomOverlaps || alternateRoomOverlaps)
                if (DarkMessageBox.Show(owner, "The selected rooms overlap horizontally. To resolve the conflict manually, put down walls on the overlapped sectors. " +
                    "You can continue anyway, the rooms are then taken in this priority: " + string.Join(", ", rooms.Select(room => room.Name)),
                    "Rooms are overlapping", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
                    return;

            // Figure out position and size of the merged room
            VectorInt2 minSectorPos = new VectorInt2(int.MaxValue, int.MaxValue);
            VectorInt2 maxSectorPos = new VectorInt2(int.MinValue, int.MinValue);
            foreach (VectorInt2 position in baseNewSectorMap.Keys.Concat(alternateNewSectorMap.Keys))
            {
                minSectorPos = VectorInt2.Min(minSectorPos, position);
                maxSectorPos = VectorInt2.Max(maxSectorPos, position);
            }
            VectorInt2 size = maxSectorPos - minSectorPos + new VectorInt2(2, 2);
            int maxSize = TrCatalog.GetLimit(_editor.Level.Settings.GameVersion, Limit.RoomDimensions);
            if (size.X >= maxSize || size.Y >= maxSize)
                if (DarkMessageBox.Show(owner, "After merging all rooms, the new room will have size " + size.X + " by " + size.Y + ". It is bigger than " +
                    maxSize + " by " + maxSize + " which is maximum size for the engine. You can continue anyway," +
                    " but in game there will be issues with rendering. Are you sure?", "Room too big", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;

            // Create new room and start merging
            var relevantRooms = new HashSet<Room>(rooms.SelectMany(room => room.Portals).Select(portal => portal.AdjoiningRoom));
            Room newRoom = alternated ? rooms.First(room => room.Alternated) : rooms.First();
            var resizeParameter = RectangleInt2.FromLTRB(minSectorPos - newRoom.SectorPos - VectorInt2.One, size);
            if (alternated)
                newRoom.AlternateOpposite.Resize(_editor.Level, resizeParameter,
                    rooms.Min(room => (room.AlternateOpposite ?? room).GetLowestCorner()),
                    rooms.Max(room => (room.AlternateOpposite ?? room).GetHighestCorner()));
            newRoom.Resize(_editor.Level, resizeParameter,
                rooms.Min(room => room.GetLowestCorner()),
                rooms.Max(room => room.GetHighestCorner()));
            IEnumerable<Room> mergeRooms = rooms.Where(room => room != newRoom);

            Action<Room, IEnumerable<Room>, Dictionary<VectorInt2, Room>, bool> performMergeAction =
                (newRoomToHandle, roomsToHandle, newSectorMap, removeObjectsInNotAlternatedRooms) =>
                {
                    foreach (KeyValuePair<VectorInt2, Room> sector in newSectorMap)
                    {
                        if (sector.Value == newRoomToHandle)
                            continue;
                        Sector oldSector = newRoomToHandle.GetSector(sector.Key - newRoomToHandle.SectorPos);

                        VectorInt2 newSectorVec = sector.Key - sector.Value.SectorPos;
                        Sector newSector = sector.Value.GetSector(newSectorVec).Clone();

                        // Preserve outer wall textures
                        foreach (SectorFace face in oldSector.GetFaceTextures().Keys.Union(newSector.GetFaceTextures().Keys))
                        {
                            var direction = face.GetDirection();
                            if (direction == Direction.NegativeX || direction == Direction.PositiveX || direction == Direction.NegativeZ || direction == Direction.PositiveZ)
                                newSector.SetFaceTexture(face, oldSector.GetFaceTexture(face));
                        }

                        // Transform positions
                        foreach (SectorVerticalPart vertical in oldSector.GetVerticals().Union(newSector.GetVerticals()))
                            for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                                newSector.SetHeight(vertical, edge, newSector.GetHeight(vertical, edge) + (sector.Value.Position.Y - newRoomToHandle.Position.Y));

                        newRoomToHandle.SetSector(sector.Key - newRoomToHandle.SectorPos, newSector);
                    }
                    foreach (KeyValuePair<VectorInt2, Room> sector in newSectorMap)
                    { // Copy all adjacent sectors of walls
                        if (sector.Value == newRoomToHandle)
                            continue;

                        // Copy adjacent sectors
                        Sector thisSectorNegativeX = newRoomToHandle.GetSector(sector.Key - newRoomToHandle.SectorPos + new VectorInt2(-1, 0));
                        Sector otherSectorNegativeX = sector.Value.GetSector(sector.Key - sector.Value.SectorPos + new VectorInt2(-1, 0));
                        Sector thisSectorPositiveX = newRoomToHandle.GetSector(sector.Key - newRoomToHandle.SectorPos + new VectorInt2(1, 0));
                        Sector otherSectorPositiveX = sector.Value.GetSector(sector.Key - sector.Value.SectorPos + new VectorInt2(1, 0));
                        Sector thisSectorNegativeZ = newRoomToHandle.GetSector(sector.Key - newRoomToHandle.SectorPos + new VectorInt2(0, -1));
                        Sector otherSectorNegativeZ = sector.Value.GetSector(sector.Key - sector.Value.SectorPos + new VectorInt2(0, -1));
                        Sector thisSectorPositiveZ = newRoomToHandle.GetSector(sector.Key - newRoomToHandle.SectorPos + new VectorInt2(0, 1));
                        Sector otherSectorPositiveZ = sector.Value.GetSector(sector.Key - sector.Value.SectorPos + new VectorInt2(0, 1));

                        // Copy adjacent outer wall textures
                        // Unfortunately they are always on the adjacent sector, so they need extra handling
                        for (SectorFace face = 0; face < SectorFace.Count; ++face)
                        {
                            var direction = face.GetDirection();
                            switch (direction)
                            {
                                case Direction.NegativeX:
                                    thisSectorPositiveX.SetFaceTexture(face, otherSectorPositiveX.GetFaceTexture(face));
                                    break;
                                case Direction.PositiveX:
                                    thisSectorNegativeX.SetFaceTexture(face, otherSectorNegativeX.GetFaceTexture(face));
                                    break;
                                case Direction.NegativeZ:
                                    thisSectorPositiveZ.SetFaceTexture(face, otherSectorPositiveZ.GetFaceTexture(face));
                                    break;
                                case Direction.PositiveZ:
                                    thisSectorNegativeZ.SetFaceTexture(face, otherSectorNegativeZ.GetFaceTexture(face));
                                    break;
                            }
                        }

                        // Copy vertical splits along edge
                        int heightDifference = sector.Value.Position.Y - newRoomToHandle.Position.Y;

                        if (thisSectorNegativeX.IsAnyWall)
                        {
                            foreach (SectorVerticalPart vertical in otherSectorNegativeX.GetVerticals())
                            {
                                thisSectorNegativeX.SetHeight(vertical, SectorEdge.XpZn, otherSectorNegativeX.GetHeight(vertical, SectorEdge.XpZn) + heightDifference);
                                thisSectorNegativeX.SetHeight(vertical, SectorEdge.XpZp, otherSectorNegativeX.GetHeight(vertical, SectorEdge.XpZp) + heightDifference);
                            }
                        }

                        if (thisSectorPositiveX.IsAnyWall)
                        {
                            foreach (SectorVerticalPart vertical in otherSectorPositiveX.GetVerticals())
                            {
                                thisSectorPositiveX.SetHeight(vertical, SectorEdge.XnZn, otherSectorPositiveX.GetHeight(vertical, SectorEdge.XnZn) + heightDifference);
                                thisSectorPositiveX.SetHeight(vertical, SectorEdge.XnZp, otherSectorPositiveX.GetHeight(vertical, SectorEdge.XnZp) + heightDifference);
                            }
                        }

                        if (thisSectorNegativeZ.IsAnyWall)
                        {
                            foreach (SectorVerticalPart vertical in otherSectorNegativeZ.GetVerticals())
                            {
                                thisSectorNegativeZ.SetHeight(vertical, SectorEdge.XnZp, otherSectorNegativeZ.GetHeight(vertical, SectorEdge.XnZp) + heightDifference);
                                thisSectorNegativeZ.SetHeight(vertical, SectorEdge.XpZp, otherSectorNegativeZ.GetHeight(vertical, SectorEdge.XpZp) + heightDifference);
                            }
                        }

                        if (thisSectorPositiveZ.IsAnyWall)
                        {
                            foreach (SectorVerticalPart vertical in otherSectorPositiveZ.GetVerticals())
                            {
                                thisSectorPositiveZ.SetHeight(vertical, SectorEdge.XnZn, otherSectorPositiveZ.GetHeight(vertical, SectorEdge.XnZn) + heightDifference);
                                thisSectorPositiveZ.SetHeight(vertical, SectorEdge.XpZn, otherSectorPositiveZ.GetHeight(vertical, SectorEdge.XpZn) + heightDifference);
                            }
                        }
                    }
                    foreach (Room room in roomsToHandle)
                        foreach (PositionBasedObjectInstance @object in room.Objects.ToList())
                        {
                            PositionBasedObjectInstance objectToPlace = @object;
                            // Remove the objects or clone it, if we need it for the alternated room. (If it's not 'CopyToAlternateRooms', we can remove it after using it)
                            if (removeObjectsInNotAlternatedRooms || room.Alternated || !@object.CopyToAlternateRooms)
                                room.RemoveObjectAndSingularPortalAndKeepAlive(_editor.Level, @object);
                            else
                                objectToPlace = (PositionBasedObjectInstance)(@object.Clone());
                            objectToPlace.Position += room.WorldPos - newRoomToHandle.WorldPos;
                            newRoomToHandle.AddObjectAndSingularPortal(_editor.Level, objectToPlace);
                        }
                    foreach (Room room in new[] { newRoomToHandle }.Concat(roomsToHandle)) // We also need to handle the base room.
                        foreach (SectorBasedObjectInstance sectorObject in room.SectorObjects.ToList())
                        {
                            // Skip internal portals between the merged rooms
                            if (@sectorObject is PortalInstance)
                            {
                                var adjoiningRoom = ((PortalInstance)@sectorObject).AdjoiningRoom;
                                if (rooms.Contains(adjoiningRoom))
                                    continue;
                                if (adjoiningRoom.AlternateOpposite != null)
                                    if (rooms.Contains(adjoiningRoom.AlternateOpposite))
                                        continue;
                            }

                            // Remove the objects or clone it, if we need it for the alternated room. (If it's not 'CopyToAlternateRooms', we can remove it after using it)
                            SectorBasedObjectInstance sectorObjectToPlace = sectorObject;
                            if (removeObjectsInNotAlternatedRooms || room.Alternated || !sectorObjectToPlace.CopyToAlternateRooms)
                                room.RemoveObjectAndSingularPortalAndKeepAlive(_editor.Level, @sectorObject);
                            else
                                sectorObjectToPlace = (SectorBasedObjectInstance)(sectorObject.Clone());

                            // Split sector based objects if there are overlaps now
                            var portalInstance = sectorObjectToPlace as PortalInstance;
                            Func<VectorInt2, bool> sectorToBeCoveredTest;
                            if (portalInstance == null ||
                                portalInstance.Direction == PortalDirection.Ceiling ||
                                portalInstance.Direction == PortalDirection.Floor)
                            {
                                sectorToBeCoveredTest = pos =>
                                {
                                    Room sectorFromRoom = newSectorMap.TryGetOrDefault(pos + room.SectorPos);
                                    if (sectorFromRoom == room) // Must be from that room.
                                        return true;
                                    if (sectorFromRoom != null)
                                        return false;

                                    // Or it must be the only room with a wall here.
                                    foreach (Room roomToHandle2 in roomsToHandle)
                                        if (roomToHandle2 != room)
                                            if (roomToHandle2.WorldArea.Contains(pos))
                                                return false;
                                    return true;
                                };
                            }
                            else
                            {
                                sectorToBeCoveredTest = pos =>
                                {
                                    if ((pos.X + room.SectorPos.X) != newRoom.SectorPos.X && // It must be at the border
                                        (pos.Y + room.SectorPos.Y) != newRoom.SectorPos.Y &&
                                        (pos.X + room.SectorPos.X) != newRoom.SectorPos.X + newRoom.SectorSize.X - 1 &&
                                        (pos.Y + room.SectorPos.Y) != newRoom.SectorPos.Y + newRoom.SectorSize.Y - 1)
                                        return false;
                                    if (newSectorMap.ContainsKey(pos + room.SectorPos)) // There must be a wall at the wall portal
                                        return false;
                                    VectorInt2 inFrontPos = PortalInstance.GetOppositePortalArea(portalInstance.Direction,
                                        new RectangleInt2(pos, pos)).Start;
                                    return newSectorMap.TryGetOrDefault(inFrontPos + room.SectorPos) == room; // There must be the sector from the corresponding room in front of the portal.
                                };
                            }

                            // Add split portals
                            IEnumerable<SectorBasedObjectInstance> newSectorBasedObjects = sectorObjectToPlace.SplitIntoRectangles(sectorToBeCoveredTest, room.SectorPos - newRoom.SectorPos);
                            foreach (SectorBasedObjectInstance sectorBasedObjectInstance in newSectorBasedObjects)
                                newRoomToHandle.AddObjectAndSingularPortal(_editor.Level, sectorBasedObjectInstance);
                        }
                };
            performMergeAction(newRoom, mergeRooms, baseNewSectorMap, !alternated);
            if (alternated)
                performMergeAction(newRoom.AlternateOpposite, mergeRooms?.Select(room => room.AlternateOpposite ?? room), alternateNewSectorMap, true);
            Room.FixupNeighborPortals(_editor.Level, new[] { newRoom }, new[] { newRoom }.Concat(mergeRooms), ref relevantRooms);
            Parallel.ForEach(relevantRooms, relevantRoom =>
            {
                relevantRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            });

            // Add room and update the editor
            foreach (Room room in mergeRooms)
                _editor.Level.DeleteRoomWithAlternate(room);
            if (_editor.SelectedRooms.Intersect(rooms).Any())
                _editor.SelectedRoom = newRoom;
            foreach (Room relevantRoom in relevantRooms)
            {
                _editor.RoomPropertiesChange(relevantRoom);
                _editor.RoomSectorPropertiesChange(relevantRoom);
            }
            _editor.RoomListChange();
        }

        public static void SplitRoom(IWin32Window owner)
        {
            if (!CheckForRoomAndSectorSelection(owner))
                return;

            var room = _editor.SelectedRoom;
            room = room.AlternateBaseRoom ?? room;
            RectangleInt2 area = _editor.SelectedSectors.Area.Inflate(1).Intersect(room.LocalArea);

            if (area.Width < 2 || area.Height < 2)
            {
                _editor.SendMessage("Selected area is not suitable for splitting.", PopupType.Error);
                return;
            }

            // Split alternate room
            var relevantRooms = new HashSet<Room>(room.Portals.Select(p => p.AdjoiningRoom));

            var splitRoom = room.Split(_editor.Level, area);
            int newRoomIndex = _editor.Level.AssignRoomToFree(splitRoom);
            splitRoom.Name = "Room " + newRoomIndex + " (split from " + room.Name + ")";

            if (room.Alternated)
            {
                var alternateSplitRoom = room.AlternateRoom.Split(_editor.Level, area, splitRoom);
                int newAlternateRoomIndex = _editor.Level.AssignRoomToFree(alternateSplitRoom);
                alternateSplitRoom.Name = "Room " + newRoomIndex + " Split from " + room.AlternateRoom.Name + ")";

            }

            relevantRooms.Add(room);
            relevantRooms.Add(splitRoom);
            Room.FixupNeighborPortals(_editor.Level, new[] { room, splitRoom }, new[] { room, splitRoom }, ref relevantRooms);
            Parallel.ForEach(relevantRooms, relevantRoom =>
            {
                relevantRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            });

            // Cleanup
            if ((_editor.SelectedRoom == room || _editor.SelectedRoom == room.AlternateOpposite) && _editor.SelectedSectors.Valid)
                _editor.SelectedSectors = SectorSelection.None;
            foreach (Room relevantRoom in relevantRooms)
            {
                _editor.RoomPropertiesChange(relevantRoom);
                _editor.RoomSectorPropertiesChange(relevantRoom);
            }
            _editor.RoomListChange();
        }

        public static void SelectConnectedRooms()
        {
            _editor.SelectRooms(_editor.Level.GetConnectedRooms(_editor.SelectedRooms).ToList());
        }

        public static void DuplicateRoom(IWin32Window owner)
        {
            // Prevent "Copy of Copy of Copy" situation
            int foundPostfixPos = _editor.SelectedRoom.Name.IndexOf(" (copy");
            string buffer = string.Empty;
            if (foundPostfixPos != -1)
            {
                int potentialCopyNumber = 0;
                buffer = _editor.SelectedRoom.Name.Substring(foundPostfixPos, _editor.SelectedRoom.Name.Length - foundPostfixPos);
                int enclosingBracketPos = buffer.IndexOf(")");

                if (enclosingBracketPos != -1)
                {
                    var numString = buffer.Substring(6, enclosingBracketPos - 6);
                    bool numberFound = Int32.TryParse(numString, out potentialCopyNumber);
                    if (!numberFound)
                        potentialCopyNumber = 2;
                    else
                        potentialCopyNumber++;
                    buffer = " " + potentialCopyNumber++.ToString();
                }
                else
                    buffer = string.Empty;
            }
            var cutName = _editor.SelectedRoom.Name.Substring(0, foundPostfixPos == -1 ? _editor.SelectedRoom.Name.Length : foundPostfixPos);

            var newRoom = _editor.SelectedRoom.Clone(_editor.Level);
            newRoom.Name = cutName + " (copy" + buffer + ")";
            newRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            _editor.Level.AssignRoomToFree(newRoom);
            _editor.RoomListChange();
            _editor.UndoManager.PushRoomCreated(newRoom);
            _editor.SelectedRoom = newRoom;
        }

        public static bool CheckForRoomAndSectorSelection(IWin32Window owner)
        {
            if (_editor.SelectedRoom == null || !_editor.SelectedSectors.Valid)
            {
                _editor.SendMessage("Please select a valid group of sectors.", PopupType.Error);
                return false;
            }
            return true;
        }

        public static bool VersionCheck(bool supported, string objectType)
        {
            if (!supported)
                _editor.SendMessage(objectType + " is not supported in current game version.", PopupType.Info);
            return supported;
        }

        public static bool CheckForLockedRooms(IWin32Window owner, IEnumerable<Room> rooms)
        {
            if (rooms.All(room => !room.Properties.Locked))
                return false;
            string lockedRoomList = "Locked rooms: " + string.Join(" ,", rooms.Where(room => room.Properties.Locked).Select(s => s.Name));

            if (_editor.Configuration.UI_OnlyShowSmallMessageWhenRoomIsLocked)
            {
                _editor.SendMessage("Can't move rooms because some rooms are locked.\n" + lockedRoomList, PopupType.Info);
                return true;
            }

            // Inform user and offer an option to unlock the room position
            if (DarkMessageBox.Show(owner, "Can't move rooms because some rooms are locked. Unlock and continue?\n" + lockedRoomList, "Locked rooms", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return true;

            // Unlock rooms
            foreach (Room room in rooms)
                if (room.Properties.Locked)
                {
                    room.Properties.Locked = false;
                    _editor.RoomPropertiesChange(room);
                }
            return true;
        }

        public static void UpdateLight<T>(Func<LightInstance, T, bool> compareEquals, Action<LightInstance, T> setLightValue, Func<LightInstance, T?> getGuiValue) where T : struct
        {
            var light = _editor.SelectedObject as LightInstance;
            if (light == null)
                return;

            T? newValue = getGuiValue(light);
            if (!newValue.HasValue || compareEquals(light, newValue.Value))
                return;

            setLightValue(light, newValue.Value);
            light.Room.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            _editor.ObjectChange(light, ObjectChangeType.Change);
        }

        public static void UpdateLightQuality(LightQuality newQuality)
        {
            var light = _editor.SelectedObject as LightInstance;
            if (light == null)
                return;
            light.Quality = newQuality;
            light.Room.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            _editor.ObjectChange(light, ObjectChangeType.Change);
        }

        public static void UpdateLightType(LightType type)
        {
            var light = _editor?.SelectedObject as LightInstance;
            if (light == null)
                return;
            light.Type = type;
            light.Room.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            _editor.ObjectChange(light, ObjectChangeType.Change);
        }

        public static void EditLightColor(IWin32Window owner)
        {
            UpdateLight<Vector3>((light, value) => light.Color == value, (light, value) => light.Color = value,
                light =>
                {
                    // Prompt user that real intensity is now used to define fog bulb intensity
                    if (_editor.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR4 && light.Type == LightType.FogBulb)
                    {
                        _editor.SendMessage("To edit fog bulb intensity, use 'Intensity' field.", PopupType.Info);
                        return light.Color;
                    }

                    _editor.UndoManager.PushObjectPropertyChanged(light);

                    using (var colorDialog = new RealtimeColorDialog(
                        _editor.Configuration.ColorDialog_Position.X,
                        _editor.Configuration.ColorDialog_Position.Y,
                        c =>
                        {
                            UpdateLight<Vector3>((l, v) => l.Color == v, (l, v) => l.Color = v,
                            l => { return c.ToFloat3Color() * 2.0f; });
                        }, _editor.Configuration.UI_ColorScheme))
                    {
                        colorDialog.Color = new Vector4(light.Color * 0.5f, 1.0f).ToWinFormsColor();

                        var oldLightColor = colorDialog.Color;
                        if (colorDialog.ShowDialog(owner) != DialogResult.OK)
                            colorDialog.Color = oldLightColor;

                        _editor.Configuration.ColorDialog_Position = colorDialog.Position;
                        return colorDialog.Color.ToFloat3Color() * 2.0f;
                    }
                });
        }

        public static bool BuildLevel(bool autoCloseWhenDone, IWin32Window owner, bool silent = false, string forceDirectory = null)
        {
            Level level = _editor.Level;

            if (!level.Settings.Wads.All(wad => wad.Wad != null))
            {
                if (!silent)
                    _editor.SendMessage("Wads are missing. Can't compile level without wads.", PopupType.Error);
                return false;
            }

            foreach (var room in level.Rooms.Where(r => r is not null))
            {
                foreach (var obj in room.Objects.Where(o => o is MoveableInstance))
                {
                    var moveableInstance = (MoveableInstance)obj;

                    if (!level.Settings.Wads.Any(w => w.Wad.Moveables.ContainsKey(moveableInstance.WadObjectId)))
                    {
                        if (!silent)
                            _editor.SendMessage("The object " + moveableInstance.ToString() + " was not found in any wad.\nCan't compile level with missing objects.", PopupType.Error);
                        return false;
                    }
                }

                foreach (var obj in room.Objects.Where(o => o is StaticInstance))
                {
                    var staticInstance = (StaticInstance)obj;

                    if (!level.Settings.Wads.Any(w => w.Wad.Statics.ContainsKey(staticInstance.WadObjectId)))
                    {
                        if (!silent)
                            _editor.SendMessage("The object " + staticInstance.ToString() + " was not found in any wad.\nCan't compile level with missing objects.", PopupType.Error);
                        return false;
                    }
                }
            }

                if (!level.Settings.Textures.All(texture => texture.IsAvailable))
            {
                if (!silent)
                    _editor.SendMessage("Textures are missing. Can't compile level without textures.", PopupType.Error);
                return false;
            }

            string fileName = string.IsNullOrEmpty(forceDirectory) ? level.Settings.MakeAbsolute(level.Settings.GameLevelFilePath) : forceDirectory;
            string directory = PathC.GetDirectoryNameTry(fileName);

            if (!Directory.Exists(directory))
            {
                if (!silent)
                {
                    _editor.SendMessage("Specified folder for level file does not exist.\nPlease specify different folder in level settings.", PopupType.Error);
                    return false;
                }
                else
                    Directory.CreateDirectory(directory); // Just silently make dir and put level there
            }

            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                if (!silent)
                    _editor.SendMessage("Specified level file name is incorrect.\nPlease add an extension.", PopupType.Error);
                return false;
            }

            var whatLoaded = string.Empty;

            if (level.Settings.SoundCatalogs.Count == 0)
            {
                if (level.Settings.LoadDefaultSoundCatalog())
                    whatLoaded += "Default sound catalog was preloaded automatically.";
            }

            if (level.Settings.SelectedSounds.Count == 0 && level.Settings.AutoAssignSoundsIfNoSelection)
            {
                AutodetectAndAssignSounds(level.Settings);
                whatLoaded += (string.IsNullOrEmpty(whatLoaded) ? "Sounds" : "\nAlso sounds") + " were auto-assigned because none were selected.";
            }

            if (AutoLoadSamplePath(level.Settings))
                whatLoaded += (string.IsNullOrEmpty(whatLoaded) ? "Stock samples" : "\nAlso stock samples") + " were assigned because some samples were missing.";


			if (level.Settings.ConvertLegacyTombEngineExecutablePath())
				whatLoaded += (string.IsNullOrEmpty(whatLoaded) ? "Executable path" : "\nAlso executable path") + " was upgraded to new TEN directory structure.";

			if (!string.IsNullOrEmpty(whatLoaded))
                _editor.SendMessage(whatLoaded, PopupType.Info);

            using (var form = new FormOperationDialog("Build level", autoCloseWhenDone, false,
                (progressReporter, cancelToken) =>
                {
                    using (var compiler = level.Settings.GameVersion <= TRVersion.Game.TRNG ?
                            (LevelCompiler)(new LevelCompilerClassicTR(level, fileName, progressReporter)) :
                            (LevelCompiler)(new LevelCompilerTombEngine(level, fileName, progressReporter)))
                    {
                        var watch = new Stopwatch();
                        watch.Start();
                        var statistics = compiler.CompileLevel(cancelToken);
                        watch.Stop();

                        progressReporter.ReportProgress(100, $"\nElapsed time: {FormatElapsedSmart(watch.Elapsed.TotalMilliseconds)}");

                        // Raise an event for statistics update
                        _editor.RaiseEvent(new Editor.LevelCompilationCompletedEvent
                        {
                            BoxCount = statistics.BoxCount,
                            OverlapCount = statistics.OverlapCount,
                            TextureCount = statistics.ObjectTextureCount,
                            InfoString = statistics.ToString()
                        });
                    }

                    // Force garbage collector to compact memory
                    GC.Collect();
                }))
            {
                // Make sure form displays correctly if we're running in silent mode without parent window
                if (owner == null)
                {
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.ShowInTaskbar = true;
                }

                form.ShowDialog(owner);
                return form.DialogResult != DialogResult.Cancel;
            }
        }

        private static string FormatElapsedSmart(double elapsedMs) =>
            FormatElapsedSmart(TimeSpan.FromMilliseconds(elapsedMs));

        private static string FormatElapsedSmart(TimeSpan t)
        {
            long totalMs = (long)Math.Round(t.TotalMilliseconds);

            if (totalMs < 1_000)
                return $"{totalMs} ms";

            if (totalMs < 60_000)
            {
                long s = totalMs / 1_000;
                long ms = totalMs % 1_000;
                return $"{s} s {ms} ms";
            }

            if (totalMs < 3_600_000)
            {
                long m = totalMs / 60_000;
                long s = (totalMs % 60_000) / 1_000;
                return $"{m} min {s} s";
            }

            // >= 1 hour
            long h = totalMs / 3_600_000;
            long m2 = (totalMs % 3_600_000) / 60_000;
            long s2 = (totalMs % 60_000) / 1_000;
            return $"{h} h {m2} min {s2} s";
        }

        public static void BuildLevelAndPlay(IWin32Window owner, bool fastMode = false)
        {
            if (IsLaraInLevel())
            {
                // Temporarily enable fast mode if specified
                _editor.Level.Settings.FastMode = fastMode;

                if (BuildLevel(true, owner))
                    TombLauncher.Launch(_editor.Level.Settings, owner);

                // Set fast mode back off
                _editor.Level.Settings.FastMode = false;
            }
            else
                _editor.SendMessage("No Lara found. Place Lara to play level.", PopupType.Error);
        }

        public static void BuildInBatch(Editor editor, BatchCompileList batchList, string startFile)
        {
            int successCounter = 0;
            try
            {
                foreach (var path in batchList.Files)
                {
                    if (!path.EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (OpenLevel(null, path, true))
                    {
                        // If specified, replace build path with custom build path.
                        string customPath = null;
                        if (!string.IsNullOrEmpty(batchList.Location) && Directory.Exists(Path.GetPathRoot(batchList.Location)))
                            customPath = Path.Combine(batchList.Location, Path.GetFileName(editor.Level.Settings.MakeAbsolute(editor.Level.Settings.GameLevelFilePath)));
                        if (BuildLevel(true, null, true, customPath))
                            successCounter++;
                    }
                }

                // Clean up and delete batch XML.
                // It won't happen in case XML was of wrong structure (foolproofing for potentially using wrong XML).
                if (File.Exists(startFile))
                    File.Delete(startFile);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                    return;
                else
                {
                    DarkMessageBox.Show("Batch build failed! \n" + "Exception: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        public static bool IsLaraInLevel()
        {
            return _editor?.Level?.Settings?.WadTryGetMoveable(WadMoveableId.Lara) != null &&
                   _editor.Level.ExistingRooms.SelectMany(room => room.Objects)
                                              .Any(obj => obj is ItemInstance && ((ItemInstance)obj).ItemType == new ItemType(WadMoveableId.Lara));
        }

        public static bool AddAndPlaceImportedGeometry(IWin32Window owner, VectorInt2 position, string file)
        {
            if (!File.Exists(file))
                return false;

            file = _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory);

            ImportedGeometry geometryToPlace = _editor.Level.Settings.ImportedGeometries.Find(
                item => _editor.Level.Settings.MakeRelative(item.Info.Path, VariableType.LevelDirectory).Equals(file, StringComparison.InvariantCultureIgnoreCase));

            if (geometryToPlace == null)
                geometryToPlace = AddImportedGeometry(owner, file);

            if (geometryToPlace != null)
            {
                PlaceObject(_editor.SelectedRoom, position, new ImportedGeometryInstance { Model = geometryToPlace });
                return true;
            }

            return false;
        }

        public static ImportedGeometry AddImportedGeometry(IWin32Window owner, string predefinedPath = null)
        {
            string path = (predefinedPath ?? LevelFileDialog.BrowseFile(owner, _editor.Level.Settings,
                PathC.GetDirectoryNameTry(_editor.Level.Settings.LevelFilePath),
                "Load imported geometry", BaseGeometryImporter.FileExtensions, VariableType.LevelDirectory, false));

            if (string.IsNullOrEmpty(path))
                return null;

            var geometry = new ImportedGeometry();
            var config = Editor.Instance?.Configuration;

            var viewModel = new GeometryIOSettingsWindowViewModel(IOSettingsPresets.GeometryImportSettingsPresets);
            viewModel.SelectPreset(config?.GeometryIO_LastUsedGeometryImportPresetName);

            var settingsDialog = new GeometryIOSettingsWindow { DataContext = viewModel };
            settingsDialog.SetOwner(owner);
            settingsDialog.ShowDialog();

            if (viewModel.DialogResult != true)
                return null;

            if (config is not null)
                config.GeometryIO_LastUsedGeometryImportPresetName = viewModel.SelectedPreset?.Name;

            var settings = viewModel.GetCurrentSettings();
            var info = new ImportedGeometryInfo(path, settings);
            _editor.Level.Settings.ImportedGeometryUpdate(geometry, info);
            _editor.Level.Settings.ImportedGeometries.Add(geometry);
            _editor.LoadedImportedGeometriesChange();

            return geometry;
        }

        public static IEnumerable<LevelTexture> AddTexture(IWin32Window owner, IEnumerable<string> predefinedPaths = null)
        {
            List<string> paths = (predefinedPaths ?? LevelFileDialog.BrowseFiles(owner, _editor.Level.Settings,
                PathC.GetDirectoryNameTry(_editor.Level.Settings.LevelFilePath),
                "Load texture files", ImageC.FileExtensions, VariableType.LevelDirectory)).ToList();

            if (paths.Count == 0) // Fast track to avoid unnecessary updates
                return new LevelTexture[0];

            // Load textures concurrently
            LevelTexture[] results = new LevelTexture[paths.Count];
            Parallel.For(0, paths.Count, i => results[i] = new LevelTexture(_editor.Level.Settings, paths[i]));

            // Open GUI for texture that couldn't be loaded
            for (int i = 0; i < results.Length; ++i)
                while (results[i]?.LoadException != null)
                    switch (DarkMessageBox.Show(owner, "An error occurred while loading texture file '" + paths[i] + "'." +
                        "\nError message: " + results[i].LoadException.GetType(), "Unable to load texture file.",
                        paths.Count == 1 ? MessageBoxButtons.RetryCancel : MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error,
                        paths.Count == 1 ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1))
                    {
                        case DialogResult.Retry:
                            results[i].Reload(_editor.Level.Settings);
                            break;
                        case DialogResult.Ignore:
                            results[i] = null;
                            break;
                        default:
                            return new LevelTexture[0];
                    }

            // Update level
            _editor.Level.Settings.Textures.AddRange(results.Where(result => result != null));
            _editor.LoadedTexturesChange(results.FirstOrDefault(result => result != null));
            return results.Where(result => result != null);
        }

        public static void ClearAllTexturesInLevel(Level level)
        {
            var emptyTexture = new TextureArea() { Texture = null };
            foreach (var room in level.Rooms)
            {
                if (room != null)
                {
                    var selection = new SectorSelection();
                    selection.Area = new RectangleInt2(VectorInt2.Zero, new VectorInt2(room.NumXSectors - 1, room.NumZSectors - 1));

                    TexturizeAll(room, selection, emptyTexture, SectorFaceType.Floor);
                    TexturizeAll(room, selection, emptyTexture, SectorFaceType.Ceiling);
                    TexturizeAll(room, selection, emptyTexture, SectorFaceType.Wall);
                }
            }
        }

        public static void UnloadTextures(IWin32Window owner)
        {
            if (DarkMessageBox.Show(owner, "Are you sure to unload ALL " + _editor.Level.Settings.Textures.Count +
                " texture files loaded?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            foreach (LevelTexture texture in _editor.Level.Settings.Textures)
                texture.SetPath(_editor.Level.Settings, null);
            _editor.LoadedTexturesChange();
        }

        public static void RemoveTextures(IWin32Window owner)
        {
            if (DarkMessageBox.Show(owner, "Are you sure to DELETE ALL " + _editor.Level.Settings.Textures.Count +
                " texture files loaded? Everything will be untextured.", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _editor.SelectedTexture = TextureArea.None;
            _editor.Level.RemoveTextures(texture => true);
            _editor.Level.Settings.Textures.Clear();
            _editor.Level.Settings.AnimatedTextureSets.Clear();
            _editor.LoadedTexturesChange();
        }

        public static void RemoveTexture(IWin32Window owner, LevelTexture textureToDelete)
        {
            if (DarkMessageBox.Show(owner, "Are you sure to DELETE the texture " + textureToDelete +
                "? Everything using the texture will be untextured.", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (_editor.SelectedTexture.Texture == textureToDelete)
                _editor.SelectedTexture = TextureArea.None;
            _editor.Level.Settings.Textures.Remove(textureToDelete);
            _editor.Level.RemoveTextures(texture => texture == textureToDelete);
            _editor.LoadedTexturesChange(_editor.Level.Settings.Textures.FirstOrDefault());
        }

        public static IEnumerable<ReferencedWad> AddWad(IWin32Window owner, IEnumerable<string> predefinedPaths = null)
        {
            List<string> paths = (predefinedPaths ?? LevelFileDialog.BrowseFiles(owner, _editor.Level.Settings,
                PathC.GetDirectoryNameTry(_editor.Level.Settings.LevelFilePath),
                "Load object files (*.wad)", Wad2.FileExtensions, VariableType.LevelDirectory)).ToList();

            if (paths.Count == 0) // Fast track to avoid unnecessary updates
                return new ReferencedWad[0];

            // Load objects (*.wad files) concurrently
            ReferencedWad[] results = new ReferencedWad[paths.Count];
            ReferencedSoundCatalog[] soundsResults = new ReferencedSoundCatalog[paths.Count];

            GraphicalDialogHandler synchronizedDialogHandler = new GraphicalDialogHandler(owner); // Have only one to synchronize the messages.
            using (var loadingTask = Task.Run(() =>
                Parallel.For(0, paths.Count, i => results[i] = new ReferencedWad(_editor.Level.Settings, paths[i], synchronizedDialogHandler))))
                while (!loadingTask.IsCompleted)
                {
                    Thread.Sleep(1);
                    Application.DoEvents(); // Keep dialog handler responsive, otherwise wad loading can deadlock waiting on GUI thread, while GUI thread is waiting for Parallel.For.
                }

            // Open GUI for objects (*.wad files) that couldn't be loaded
            for (int i = 0; i < results.Length; ++i)
                while (results[i]?.LoadException != null)
                    switch (DarkMessageBox.Show(owner, "An error occurred while loading object file '" + paths[i] + "'." +
                        "\nError message: " + results[i].LoadException.GetType(), "Unable to load object file.",
                        paths.Count == 1 ? MessageBoxButtons.RetryCancel : MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error,
                        paths.Count == 1 ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1))
                    {
                        case DialogResult.Retry:
                            results[i].Reload(_editor.Level.Settings);
                            break;
                        case DialogResult.Ignore:
                            results[i] = null;
                            break;
                        default:
                            return new ReferencedWad[0];
                    }

            synchronizedDialogHandler = new GraphicalDialogHandler(owner); // Have only one to synchronize the messages.
            using (var loadingTask = Task.Run(() =>
                Parallel.For(0, paths.Count, i =>
                {
                    string currentPath = paths[i].ToLower();
                    string extension = Path.GetExtension(currentPath);
                    if (extension == ".wad")
                    {
                        string sfxPath = Path.GetDirectoryName(currentPath) + "\\" + Path.GetFileNameWithoutExtension(currentPath) + ".sfx";
                        if (File.Exists(sfxPath))
                        {
                            soundsResults[i] = new ReferencedSoundCatalog(_editor.Level.Settings, sfxPath, synchronizedDialogHandler);
                        }
                    }
                    else if (extension == ".wad2")
                    {
                        string xmlPath = Path.GetDirectoryName(currentPath) + "\\" + Path.GetFileNameWithoutExtension(currentPath) + ".xml";
                        if (File.Exists(xmlPath))
                        {
                            soundsResults[i] = new ReferencedSoundCatalog(_editor.Level.Settings, xmlPath, synchronizedDialogHandler);
                        }
                    }
                })))
                while (!loadingTask.IsCompleted)
                {
                    Thread.Sleep(1);
                    Application.DoEvents(); // Keep dialog handler responsive, otherwise wad loading can deadlock waiting on GUI thread, while GUI thread is waiting for Parallel.For.
                }

            var loadedWads = results.Where(result => result != null);

            // Don't do anything if there's nothing to load
            if (!loadedWads.Any())
                return loadedWads;

            // Check if there's unknown chunks present
            if (loadedWads.Any(result => result?.Wad.HasUnknownData ?? false))
                _editor.SendMessage("Loaded wad2 is of newer version than your editor.\n" +
                    "Some data was lost. Please use newer version of Tomb Editor.", PopupType.Warning);

            // Update level
            _editor.Level.Settings.Wads.InsertRange(0, results.Where(result => result != null));
            _editor.Level.Settings.SoundCatalogs.InsertRange(0, soundsResults.Where(result => result != null));
            _editor.LoadedWadsChange();

            // Autoswitch game version if necessary
            if (AutoswitchGameVersion(_editor.Level.Settings))
            {
                _editor.GameVersionChange();
                _editor.SendMessage("Game version was changed to " + _editor.Level.Settings.GameVersion + " to match loaded wads version.", PopupType.Info);
            }

            return results.Where(result => result != null);
        }

        public static bool AutoswitchGameVersion(LevelSettings settings, IEnumerable<Wad2> wads = null)
        {
            if (settings.Wads.Count != 0)
            {
                if (wads == null)
                    // If no wads specified, use only existing wads
                    wads = settings.Wads.Select(w => w.Wad).ToList();
                else
                    // Compare with already existing wads
                    wads = wads.Concat(settings.Wads.Select(w => w.Wad));
            }

            // Clean up missing wads
            wads = wads.Where(w => w != null);

            // No wads specified, no wads loaded, nothing to do
            if (!wads.Any())
                return false;

            var incomingVersion = wads.First().GameVersion.Native();
            var message = "Loaded wad" + (wads.Count() > 1 ? "s " : " ");
            if (wads.All(w => w.GameVersion.Native() == incomingVersion) &&
                incomingVersion != settings.GameVersion.Native())
            {
                // HACK: We can't tell the difference between TR4 and TRNG wads for sure. Hence, if incoming
                // version is TR4 and default game version is TRNG, we force incoming version as TRNG as well.
                if (incomingVersion == TRVersion.Game.TR4 &&
                    _editor.Configuration.Editor_DefaultProjectGameVersion == TRVersion.Game.TRNG)
                    incomingVersion = TRVersion.Game.TRNG;

                settings.GameVersion = incomingVersion;
                settings.ConvertLevelExtension();
                return true;
            }
            else if (wads.Any(w => w.GameVersion.Native() != incomingVersion))
                _editor.SendMessage(message + "have different game version.\n" +
                    "Remove wrong wads or use WadTool to fix the problem.", PopupType.Warning);

            return false;
        }

        public static void RemoveWads(IWin32Window owner)
        {
            if (DarkMessageBox.Show(owner, "Are you sure to delete ALL " + _editor.Level.Settings.Wads.Count +
                " wad files loaded?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _editor.Level.Settings.Wads.Clear();
            _editor.LoadedWadsChange();
        }

        public static void ReloadWads(IWin32Window owner)
        {
            var dialogHandler = new GraphicalDialogHandler(owner);
            foreach (var wad in _editor.Level.Settings.Wads)
                wad.Reload(_editor.Level.Settings, dialogHandler);
            _editor.LoadedWadsChange();
        }

        public static bool ReloadResource(IWin32Window owner, LevelSettings settings, IReloadableResource toReplace, bool sendEvent = true, bool searchForOthers = true)
        {
            string path = LevelFileDialog.BrowseFile(owner, settings, toReplace.GetPath(),
                "Select a new file for " + toReplace.ResourceType.ToString(), toReplace.FileExtensions, VariableType.LevelDirectory, false);

            if (string.IsNullOrEmpty(path) || (path == toReplace.GetPath() && toReplace?.LoadException == null))
                return false;

            var resourceTypeString = toReplace.ResourceType.ToString().SplitCamelcase().ToLower();
            var message = new Action<string>(s => _editor.SendMessage("Reconnecting " + Path.GetFileName(s) + "...", PopupType.Info));
            var list = new Dictionary<IReloadableResource, string>() { { toReplace, path } };

            if (searchForOthers)
            {
                var resourceListToReplace = toReplace.GetResourceList(settings).Where(i => !string.Equals(i.GetPath(), toReplace.GetPath()) && i != null && i.LoadException != null);

                foreach (var item in resourceListToReplace)
                {
                    // Now recursively search down the folder structure
                    var newPath = PathC.TryFindFile(Path.GetDirectoryName(settings.MakeAbsolute(path)), settings.MakeAbsolute(item.GetPath()), 4, 4);

                    if (!File.Exists(newPath))
                        continue;

                    if (searchForOthers && DarkMessageBox.Show(owner, "Other missing " + resourceTypeString + " was found. Reconnect other resources?",
                                            "Reconnect offline media", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        break;
                    else
                    {
                        searchForOthers = false; // Unset flag so we don't prompt again
                        list.TryAdd(item, settings.MakeRelative(newPath, VariableType.LevelDirectory));
                    }
                }
            }

            if (toReplace.ResourceType == ReloadableResourceType.ImportedGeometry)
            {
                // HACK: Because imp geo uses extremely hacky DX model workflow, we need
                // to sync its update with UI thread, otherwise DX may lock up or throw DEVICE REMOVED exception.

                list.ToList().ForEach(item =>
                {
                    SynchronizationContext.Current.Post(tmp =>
                    {
                        message(item.Value);
                        item.Key.SetPath(settings, item.Value);
                        if (sendEvent) _editor.LoadedImportedGeometriesChange();
                    }, null);
                });
            }
            else
            {
                Task.Run(() =>
                {
                    list.ToList().ForEach(item =>
                    {
                        message(item.Value);
                        item.Key.SetPath(settings, item.Value);

                        if (sendEvent)
                        {
                            if (toReplace.ResourceType == ReloadableResourceType.ImportedGeometry)
                                _editor.LoadedImportedGeometriesChange();
                            else if (toReplace.ResourceType == ReloadableResourceType.Texture)
                                _editor.LoadedTexturesChange();
                            else if (toReplace.ResourceType == ReloadableResourceType.Wad)
                                _editor.LoadedWadsChange();
                            else if (toReplace.ResourceType == ReloadableResourceType.SoundCatalog)
                                _editor.LoadedSoundsCatalogsChange();
                        }
                    });
                });
            }

            return true;
        }

        public static void ReloadSounds(IWin32Window owner)
        {
            var dialogHandler = new GraphicalDialogHandler(owner);
            foreach (var catalog in _editor.Level.Settings.SoundCatalogs)
                catalog.Reload(_editor.Level.Settings, dialogHandler);
            _editor.LoadedSoundsCatalogsChange();
        }

        public static bool EnsureNoOutsidePortalsInSelecton(IWin32Window owner)
        {
            return Room.RemoveOutsidePortals(_editor.Level, _editor.SelectedRooms, list =>
            {
                StringBuilder portalsToRemoveList = list.Aggregate(new StringBuilder(), (str, room) => str.Append(room).Append("\n"), str => str.Remove(str.Length - 1, 1));
                return DarkMessageBox.Show(owner, "The rooms can't have portals to the outside. Do you want to continue by removing all portals to the outside? " +
                    " Portals to remove: " + portalsToRemoveList.ToString(),
                    "Outside portals", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
            });
        }

        public static bool TransformRooms(RectTransformation transformation, IWin32Window owner)
        {
            if (!EnsureNoOutsidePortalsInSelecton(owner))
                return false;
            var newRooms = _editor.Level.TransformRooms(_editor.SelectedRooms, transformation);
            foreach (Room room in newRooms)
            {
                room.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            }

            _editor.SelectRoomsAndResetCamera(newRooms);

            _editor.RoomListChange();
            foreach (Room room in newRooms)
            {
                _editor.RoomGeometryChange(room);
                _editor.RoomSectorPropertiesChange(room);
            }
            return true;
        }

        public static void TryCopyObject(ObjectInstance instance, IWin32Window owner)
        {
            if (!(instance is ICopyable))
            {
                _editor.SendMessage("No suitable object selected. \nYou have to select object before you can cut or copy it.", PopupType.Info);
                return;
            }

            if (_editor.SelectedObject == null && instance == null)
                return;

            // HACK: Thanks to TRTomb, copying empty imported geometry crashes TE.
            // To prevent that, we block copying of such entries.
            if (instance is ImportedGeometryInstance && (instance as ImportedGeometryInstance)?.Model == null)
                return;

            if (instance is ObjectGroup)
            {
                // HACK: Same thing for each object in multi-selection
                foreach (var groupedObject in instance as ObjectGroup)
                {
                    if (groupedObject is ImportedGeometryInstance && (groupedObject as ImportedGeometryInstance)?.Model == null)
                        return;
                }
            }

            if (_editor.SelectedObject == null && instance != null)
            {
                _editor.SelectedObject = instance;
                BookmarkObject(instance);
            }

            Clipboard.SetDataObject(new ObjectClipboardData(_editor));
        }

        public static void TryCopySectors(SectorSelection selection, IWin32Window owner)
        {
            Clipboard.SetDataObject(new SectorsClipboardData(_editor));
        }

        public static void TryStampObject(ObjectInstance instance, IWin32Window owner)
        {
            if (!(instance is ISpatial))
            {
                _editor.SendMessage("No object selected. \nYou have to select position-based object before you can copy it.", PopupType.Info);
                return;
            }

            if (_editor.SelectedObject == null && instance == null)
                return;

            if (_editor.SelectedObject == null && instance != null)
            {
                _editor.SelectedObject = instance;
                EditorActions.BookmarkObject(instance);
            }

            _editor.Action = new EditorActionPlace(true, (level, room) => instance.Clone());
        }

        public static void TryPasteSectors(SectorsClipboardData data, IWin32Window owner)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            int x0 = _editor.SelectedSectors.Area.X0;
            int z0 = _editor.SelectedSectors.Area.Y0;
            int x1 = Math.Min(_editor.SelectedRoom.NumXSectors - 1, x0 + data.Width);
            int z1 = Math.Min(_editor.SelectedRoom.NumZSectors - 1, z0 + data.Height);

            var sectors = data.GetSectors();
            var portals = new List<PortalInstance>();

            for (int x = x0; x < x1; x++)
                for (int z = z0; z < z1; z++)
                {
                    var currentSector = sectors[x - x0, z - z0];
                    if (currentSector.Type == SectorType.BorderWall && _editor.SelectedRoom.Sectors[x, z].Type != SectorType.BorderWall)
                        continue;

                    if (_editor.SelectedSectors.Empty ||
                        _editor.SelectedSectors.Single ||
                        _editor.SelectedSectors.Area.Contains(new VectorInt2(x, z)))
                    {
                        portals.AddRange(_editor.SelectedRoom.Sectors[x, z].Portals);
                        _editor.SelectedRoom.Sectors[x, z].ReplaceGeometry(_editor.Level, currentSector);
                    }
                }

            // Redraw rooms in portals
            portals.Select(p => p.AdjoiningRoom).ToList().ForEach(room => { room.BuildGeometry(); _editor.RoomGeometryChange(room); });

            _editor.SelectedRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            _editor.RoomSectorPropertiesChange(_editor.SelectedRoom);
        }

        public static bool DragDropFileSupported(DragEventArgs e, bool allow3DImport = false)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return false;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                if (Wad2.FileExtensions.Matches(file) ||
                    ImageC.FileExtensions.Matches(file) ||
                    allow3DImport && BaseGeometryImporter.FileExtensions.Matches(file) ||
                    file.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase) ||
                    file.EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
                    return true;
            return false;
        }

        public static void SelectFloorBelowObject(PositionBasedObjectInstance instance)
        {
            VectorInt2 relPos;
            var pair = instance.Room.ProbeLowestSector(instance.SectorPosition, true, out relPos);

            if (pair.Room == null)
            {
                _editor.SendMessage("Current object position is out of room bounds.", PopupType.Error);
                return;
            }

            _editor.SelectedRoom = pair.Room;
            _editor.SelectedSectors = new SectorSelection { Area = new RectangleInt2(relPos) };
        }

		public static void MoveLara(IWin32Window owner, Room targetRoom, VectorInt2 p)
        {
            // Search for first Lara and remove her
            foreach (Room room in _editor.Level.ExistingRooms)
                foreach (var instance in room.Objects)
                {
                    var lara = instance as MoveableInstance;
                    if (lara != null && lara.WadObjectId == WadMoveableId.Lara)
                    {
                        _editor.UndoManager.PushObjectTransformed(lara);

                        room.RemoveObject(_editor.Level, instance);
                        _editor.ObjectChange(lara, ObjectChangeType.Remove, room);

                        // Move lara to current sector
                        PlaceObjectWithoutUpdate(targetRoom, p, lara);
                        return;
                    }
                }

            // Add lara to current sector
            PlaceObject(targetRoom, p, new MoveableInstance { WadObjectId = WadMoveableId.Lara });
        }

        public static int DragDropCommonFiles(DragEventArgs e, IWin32Window owner)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return -1;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Is there a prj file to open?
            string prjFile = files.FirstOrDefault(file => file.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(prjFile))
            {
                OpenLevelPrj(owner, prjFile);
                return files.Length - 1;
            }

            // Is there a prj2 file to open?
            string prj2File = files.FirstOrDefault(file => file.EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(prj2File))
            {
                OpenLevel(owner, prj2File);
                return files.Length - 1;
            }

            // Are there any more specific files to open?
            // (Process the ones of the same type concurrently)
            IEnumerable<string> wadFiles = files.Where(file => Wad2.FileExtensions.Matches(file));
            IEnumerable<string> textureFiles = files.Where(file => ImageC.FileExtensions.Matches(file));
            AddWad(owner, wadFiles.Select(file => _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory)));
            AddTexture(owner, textureFiles.Select(file => _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory)));
            return files.Length - (wadFiles.Count() + textureFiles.Count()); // Unsupported file count
        }

        public static void SetPortalOpacity(PortalOpacity opacity, IWin32Window owner)
        {
            var portal = _editor.SelectedObject as PortalInstance;
            if (portal == null)
            {
                _editor.SendMessage("No portal selected.", PopupType.Error);
                return;
            }

            portal.Opacity = opacity;
            portal.Room.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
            _editor.RoomGeometryChange(portal.Room);
            _editor.ObjectChange(portal, ObjectChangeType.Change);
		}

		public static void ToggleClassicPortalMirror(IWin32Window owner)
		{
			if (!VersionCheck(_editor.Level.IsTombEngine, "Classic mirror effect"))
				return;

			var portal = _editor.SelectedObject as PortalInstance;
			if (portal == null)
			{
				_editor.SendMessage("No portal selected.", PopupType.Error);
				return;
			}

			portal.Effect = (portal.Effect == PortalEffectType.ClassicMirror) ? PortalEffectType.None : PortalEffectType.ClassicMirror;
			_editor.ObjectChange(portal, ObjectChangeType.Change);
		}

		public static bool SaveLevel(IWin32Window owner, bool askForPath)
        {
            // Disable saving if level has unknown data (i.e. new prj2 version opened in old editor version)
            if (_editor.Level.Settings.HasUnknownData)
            {
                _editor.SendMessage("Project is in read-only mode because it was created in newer version of Tomb Editor.\nUse newest Tomb Editor version to edit and save this project.", PopupType.Warning);
                return false;
            }

            string fileName = _editor.Level.Settings.LevelFilePath;

            // Show save dialog if necessary
            if (askForPath || string.IsNullOrEmpty(fileName))
                fileName = LevelFileDialog.BrowseFile(owner, null, fileName, "Save level", LevelSettings.FileFormatsLevel, null, true);
            if (string.IsNullOrEmpty(fileName))
                return false;

            // Save level
            try
            {
                Prj2Writer.SaveToPrj2(fileName, _editor.Level);
                GC.Collect();
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to save to \"" + fileName + "\".");
                _editor.SendMessage("There was an error while saving project file.\nException: " + exc.Message, PopupType.Error);
                return false;
            }

            // Update state
            if (_editor.Level.Settings.LevelFilePath != fileName)
            {
                AddProjectToRecent(fileName);
                _editor.Level.Settings.LevelFilePath = fileName;
                _editor.LevelFileNameChange();
            }
            _editor.HasUnsavedChanges = false;

            return true;
        }

        public static ItemType? GetCurrentItemWithMessage()
        {
            ItemType? result = _editor.ChosenItem;
            if (result == null)
                _editor.SendMessage("Select an item first.", PopupType.Error);
            return result;
        }

        public static void FindItem()
        {
            ItemType? currentItem = GetCurrentItemWithMessage();
            if (currentItem == null)
                return;

            // Search for matching objects after the previous one
            ObjectInstance previousFind = _editor.SelectedObject;
            ObjectInstance instance = _editor.Level.Rooms
                .Where(room => room != null)
                .SelectMany(room => room.Objects)
                .FindFirstAfterWithWrapAround(
                obj => previousFind == obj,
                obj => obj is ItemInstance && ((ItemInstance)obj).ItemType == currentItem.Value);

            // Show result
            if (instance == null)
                _editor.SendMessage("No object of the selected item type found.", PopupType.Info);
            else
                _editor.ShowObject(instance);
        }

        public static void FindImportedGeometry(ImportedGeometry item)
        {
            if (item == null)
                return;

            // Search for matching objects after the previous one
            ObjectInstance previousFind = _editor.SelectedObject;
            ObjectInstance instance = _editor.Level.Rooms
                .Where(room => room != null)
                .SelectMany(room => room.Objects)
                .FindFirstAfterWithWrapAround(
                obj => previousFind == obj,
                obj => obj is ImportedGeometryInstance && ((ImportedGeometryInstance)obj).Model.UniqueID == item.UniqueID);

            // Show result
            if (instance == null)
                _editor.SendMessage("No such imported geometry found.", PopupType.Info);
            else
                _editor.ShowObject(instance);
        }

        public static void ReplaceObject(IWin32Window owner, bool fromContext = false)
        {
            var existingWindow = Application.OpenForms[nameof(FormReplaceObject)];
            if (existingWindow == null)
            {
                var searchAndReplaceForm = new FormReplaceObject(_editor, fromContext);
                searchAndReplaceForm.Show(owner);
            }
            else
                existingWindow.Focus();
        }

        public static void ExportCurrentRoom(IWin32Window owner)
        {
            ExportRooms(new[] { _editor.SelectedRoom }, owner);
        }

        public static void ExportRooms(IEnumerable<Room> rooms, IWin32Window owner)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export current room";
                saveFileDialog.Filter = BaseGeometryExporter.FileExtensions.GetFilter(true);
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "dae";
                saveFileDialog.FileName = _editor.SelectedRoom.Name;

                if (saveFileDialog.ShowDialog(owner) != DialogResult.OK)
                    return;

                if (!saveFileDialog.FileName.IsANSI())
                {
                    MessageBoxes.NonANSIFilePathError(owner);
                    ExportRooms(rooms, owner);
                    return;
                }

                var config = Editor.Instance?.Configuration;

                var viewModel = new GeometryIOSettingsWindowViewModel(
                    IOSettingsPresets.GeometryExportSettingsPresets,
                    new() { Export = true, ExportRoom = true }
                );

                viewModel.SelectPreset(config?.GeometryIO_LastUsedGeometryExportPresetName);

                var settingsDialog = new GeometryIOSettingsWindow { DataContext = viewModel };
                settingsDialog.SetOwner(owner);
                settingsDialog.ShowDialog();

                if (viewModel.DialogResult != true)
                    return;

                if (config is not null)
                    config.GeometryIO_LastUsedGeometryExportPresetName = viewModel.SelectedPreset?.Name;

                var settings = viewModel.GetCurrentSettings();

                BaseGeometryExporter.GetTextureDelegate getTextureCallback = txt =>
                {
                    if (txt is LevelTexture)
                        return _editor.Level.Settings.MakeAbsolute(((LevelTexture)txt).Path);
                    else
                        return "";
                };

                BaseGeometryExporter exporter = BaseGeometryExporter.CreateForFile(saveFileDialog.FileName, settings, getTextureCallback);
                new Thread(() =>
                {
                    bool exportInWorldCoordinates = rooms.Count() > 1;
                    var result = RoomGeometryExporter.ExportRooms(rooms, saveFileDialog.FileName, _editor.Level, exportInWorldCoordinates);

                    if (result.Errors.Count < 1)
                    {
                        IOModel resultModel = result.Model;
                        if (exporter.ExportToFile(resultModel, saveFileDialog.FileName))
                        {
                            if (result.Warnings.Count > 0)
                            {
                                if (result.Warnings.Count < 5)
                                {
                                    string warningmessage = "";
                                    result.Warnings.ForEach(warning => warningmessage += warning + "\n");
                                    _editor.SendMessage("Room export successful with warnings: \n" + warningmessage, PopupType.Warning);
                                }
                                else
                                    _editor.SendMessage("Room export successful with multiple warnings.", PopupType.Warning);
                            }
                            else
                                _editor.SendMessage("Room export successful.", PopupType.Info);
                        }
                    }
                    else
                    {
                        string errorMessage = "";
                        result.Errors.ForEach((error) => { errorMessage += error + "\n"; });
                        _editor.SendMessage("There was an error exporting room(s): \n" + errorMessage, PopupType.Error);
                    }
                }).Start();
            }
        }

        public static void ImportRooms(IWin32Window owner)
        {
            string importedGeometryPath;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Import rooms";
                openFileDialog.Filter = BaseGeometryImporter.FileExtensions.GetFilter();
                openFileDialog.FileName = _editor.SelectedRoom.Name;
                if (openFileDialog.ShowDialog(owner) != DialogResult.OK)
                    return;
                importedGeometryPath = openFileDialog.FileName;
            }

            // Add imported geometries
            try
            {
                string importedGeometryDirectory = Path.GetDirectoryName(importedGeometryPath);

                var info = ImportedGeometryInfo.Default;
                info.Path = importedGeometryPath;
                info.Name = Path.GetFileNameWithoutExtension(importedGeometryPath);

                ImportedGeometry newObject = new ImportedGeometry();
                _editor.Level.Settings.ImportedGeometryUpdate(newObject, info);
                _editor.Level.Settings.ImportedGeometries.Add(newObject);

                // Translate the vertices to room's origin
                foreach (var mesh in newObject.DirectXModel.Meshes)
                {
                    // Find the room
                    int roomIndex = int.Parse(mesh.Name.Split('_')[1]);
                    var room = _editor.Level.Rooms[roomIndex];

                    // Translate vertices
                    for (int j = 0; j < mesh.Vertices.Count; j++)
                    {
                        var vertex = mesh.Vertices[j];
                        vertex.Position.X -= room.WorldPos.X;
                        vertex.Position.Y -= room.WorldPos.Y;
                        vertex.Position.Z -= room.WorldPos.Z;
                        mesh.Vertices[j] = vertex;
                    }
                }

                // Rebuild DirectX buffer
                newObject.DirectXModel.UpdateBuffers();

                // Figure out the relevant rooms
                Dictionary<int, int> roomIndices = new Dictionary<int, int>();
                int meshIndex = 0;
                foreach (ImportedGeometryMesh mesh in newObject.DirectXModel.Meshes)
                {
                    int currentIndex = 0;
                    do
                    {
                        int roomIndexStart = mesh.Name.IndexOf(RoomGeometryExporter.RoomIdentifier, currentIndex, StringComparison.InvariantCultureIgnoreCase);
                        if (roomIndexStart < 0)
                            break;

                        int roomIndexEnd = roomIndexStart + RoomGeometryExporter.RoomIdentifier.Length;
                        while (roomIndexEnd < mesh.Name.Length && !char.IsWhiteSpace(mesh.Name[roomIndexEnd]))
                            ++roomIndexEnd;

                        string roomIndexStr = mesh.Name.Substring(roomIndexStart + RoomGeometryExporter.RoomIdentifier.Length, roomIndexEnd - (roomIndexStart + RoomGeometryExporter.RoomIdentifier.Length));
                        ushort roomIndex;
                        if (ushort.TryParse(roomIndexStr, out roomIndex))
                        {
                            roomIndices.Add(meshIndex, roomIndex);
                            meshIndex++;
                        }

                        currentIndex = roomIndexEnd;
                    } while (currentIndex < mesh.Name.Length);
                }

                // Add rooms
                foreach (var pair in roomIndices)
                {
                    Room room = _editor.Level.Rooms[pair.Value];
                    var newImported = new ImportedGeometryInstance();
                    newImported.Position = Vector3.Zero;
                    newImported.Model = newObject;
                    room.AddObject(_editor.Level, newImported);
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc.Message);
                _editor.SendMessage("Unable to import rooms from geometry.", PopupType.Error);
            }
        }

        public static bool ConvertLevelToTombEngine(IWin32Window owner)
        {
            var fileName = LevelFileDialog.BrowseFile(owner, "Select project to convert",
                LevelSettings.FileFormatsLevelPrj.Concat(LevelSettings.FileFormatsLevel),
                false);

            if (string.IsNullOrEmpty(fileName))
                return false;

            var newLevel = string.Empty;

            using (var form = new FormOperationDialog("TombEngine level converter", false, true, (progressReporter, cancelToken) =>
                newLevel = TombEngineConverter.Start(fileName, owner, progressReporter, cancelToken)))
            {
                if (form.ShowDialog(owner) != DialogResult.OK || string.IsNullOrEmpty(newLevel))
                    return false;
                else
                {
                    OpenLevel(owner, newLevel);
                    return true;
                }
            }
        }

        public static bool OpenLevel(IWin32Window owner, string fileName = null, bool silent = false)
        {
            if (!ContinueOnFileDrop(owner, "Open level"))
                return false;

            if (string.IsNullOrEmpty(fileName))
                fileName = LevelFileDialog.BrowseFile(owner, null, fileName, "Open Tomb Editor level", LevelSettings.FileFormatsLevel, null, false);
            if (string.IsNullOrEmpty(fileName))
                return false;

            Level newLevel = null;
            try
            {
                using (var form = new FormOperationDialog("Open level", true, true, (progressReporter, cancelToken) =>
                    newLevel = Prj2Loader.LoadFromPrj2(fileName, progressReporter, cancelToken, new Prj2Loader.Settings())))
                {
                    // Make sure form displays correctly if we're running in silent mode without parent window
                    if (owner == null)
                    {
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.ShowInTaskbar = true;
                    }

                    if (form.ShowDialog(owner) != DialogResult.OK || newLevel == null)
                        return false;

                    bool hasUnsavedChanges = false;

                    // Check if the level has legacy sound system and should be loaded in early versions of TE
                    if (newLevel.Settings.SoundSystem != SoundSystem.Xml)
                    {
                        DarkMessageBox.Show(owner, "This project is not compatible with this Tomb Editor version.\nUse version 1.3.15 or earlier and re-save this project in it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newLevel = null;
                        return false;
                    }

                    if (!silent)
                    {
                        foreach (Room r in newLevel.ExistingRooms)
                            r.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                        AddProjectToRecent(fileName);
                    }

                    _editor.Level = newLevel;
                    newLevel = null;
                    GC.Collect(); // Clean up memory
                    _editor.HasUnsavedChanges = hasUnsavedChanges;

                    if (!silent && _editor.Level.Settings.HasUnknownData)
                        _editor.SendMessage("This project was created in newer version of Tomb Editor.\nSome data was lost. Project is in read-only mode.", PopupType.Warning);
                    return true;
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to open \"" + fileName + "\"");

                if (exc is FileNotFoundException)
                {
                    RemoveProjectFromRecent(fileName);
                    if (!silent)
                        _editor.SendMessage("Project file not found!", PopupType.Warning);
                    _editor.LevelFileNameChange();  // Updates recent files on the main form
                }
                else if (!silent)
                    _editor.SendMessage("There was an error while opening project file. File may be in use or may be corrupted. \nException: " + exc.Message, PopupType.Error);
                return false;
            }
        }

        private static void AddProjectToRecent(string fileName)
        {
            if (Properties.Settings.Default.RecentProjects == null)
                Properties.Settings.Default.RecentProjects = new List<string>();

            Properties.Settings.Default.RecentProjects.RemoveAll(s => s == fileName);
            Properties.Settings.Default.RecentProjects.Insert(0, fileName);

            if (Properties.Settings.Default.RecentProjects.Count > 10)
                Properties.Settings.Default.RecentProjects.RemoveRange(10, Properties.Settings.Default.RecentProjects.Count - 10);

            Properties.Settings.Default.Save();
        }

        private static void RemoveProjectFromRecent(string fileName)
        {
            Properties.Settings.Default.RecentProjects.RemoveAll(s => s == fileName);
            Properties.Settings.Default.Save();
        }

        public static void OpenLevelPrj(IWin32Window owner, string fileName = null)
        {
            if (!ContinueOnFileDrop(owner, "Open level"))
                return;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = LevelFileDialog.BrowseFile(owner, "Select PRJ to import",
                                                      LevelSettings.FileFormatsLevelPrj,
                                                      false);
                if (string.IsNullOrEmpty(fileName))
                    return;
            }

            using (var formImport = new FormImportPrj(fileName, _editor.Configuration.Editor_RespectFlybyPatchOnPrjImport, _editor.Configuration.Editor_UseHalfPixelCorrectionOnPrjImport))
            {
                if (formImport.ShowDialog(owner) != DialogResult.OK)
                    return;

                Level newLevel = null;
                using (var form = new FormOperationDialog("Import PRJ", false, false, (progressReporter, cancelToken) =>
                    newLevel = PrjLoader.LoadFromPrj(formImport.PrjPath, formImport.SoundsPath,
                    formImport.RespectMousepatchOnFlybyHandling,
                    formImport.UseHalfPixelCorrection,
                    progressReporter, cancelToken)))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK || newLevel == null)
                        return;

                    foreach (Room r in newLevel.ExistingRooms)
                        r.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);

                    _editor.Level = newLevel;
                    newLevel = null;
                    GC.Collect(); // Clean up memory
                }
            }
        }

        public static void MoveSelectedRooms(VectorInt3 positionDelta)
        {
            if (_editor.SelectedRooms == null)
                return;

            // Collect all rooms at once
            var allRooms = _editor.SelectedRooms.SelectMany(r => r.Versions).Distinct().ToList();

            // Don't move rooms if any of the selected rooms is locked
            if (CheckForLockedRooms(null, allRooms))
                return;

            // Check if any selected rooms are connected to non-selected. If this is the case, a potential
            // portal disjointment situation is in effect, and foolproof action must be taken.
            if ((positionDelta.X != 0 || positionDelta.Z != 0) &&
                allRooms.Any(r => r.Portals.Any(p => !allRooms.Contains(p.AdjoiningRoom))))
            {
                _editor.SendMessage("Can't perform room movement because there are other rooms\nconnected to selected. Remove portals and try again.", PopupType.Warning);
                return;
            }

            _editor.UndoManager.PushRoomsMoved(allRooms, positionDelta);
            MoveRooms(positionDelta, allRooms, true);
        }

        public static void MoveRooms(VectorInt3 positionDelta, IEnumerable<Room> rooms, bool disableUndo = false)
        {
            if (!disableUndo)
                _editor.UndoManager.PushRoomsMoved(rooms.ToList(), positionDelta);

            HashSet<Room> roomsToMove = new HashSet<Room>(rooms);

            // Update position of all rooms
            foreach (Room room in roomsToMove)
                room.Position += positionDelta;

            // Make list of potential rooms to update
            HashSet<Room> roomsToUpdate = new HashSet<Room>();
            foreach (Room room in roomsToMove)
            {
                bool anyRoomUpdated = false;
                foreach (PortalInstance portal in room.Portals)
                    if (!roomsToMove.Contains(portal.AdjoiningRoom))
                    {
                        roomsToUpdate.Add(portal.AdjoiningRoom);
                        anyRoomUpdated = true;
                    }

                if (anyRoomUpdated)
                    roomsToUpdate.Add(room);
            }

            // Update
            foreach (Room room in roomsToUpdate)
            {
                room.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                _editor.RoomSectorPropertiesChange(room);
            }

            foreach (Room room in roomsToMove)
                _editor.RoomPositionChange(room);
        }

        public static void SplitSectorObjectOnSelection(SectorBasedObjectInstance @object)
        {
            if (@object == null || !_editor.SelectedSectors.Valid)
            {
                _editor.SendMessage("Please select a sector object as well as some sectors.", PopupType.Info);
                return;
            }
            RectangleInt2 area = _editor.SelectedSectors.Area;
            Room room = @object.Room;
            if (!@object.Area.Intersects(area))
            {
                _editor.SendMessage("Object and sectors don't intersect. No operation performed", PopupType.Info);
                return;
            }
            bool wasSelected = _editor.SelectedObject == @object;

            // Figure out where trigger was placed
            int[,] triggerIndices = null;
            if (@object is TriggerInstance)
            {
                triggerIndices = new int[@object.Area.Width + 1, @object.Area.Height + 1];
                for (int x = 0; x <= @object.Area.Width; ++x)
                    for (int z = 0; z <= @object.Area.Height; ++z)
                        triggerIndices[x, z] = room.Sectors[x + @object.Area.X0, z + @object.Area.Y0].Triggers.IndexOf((TriggerInstance)@object);
            }

            // Handle both base room and alternated room
            var newObjects = new List<ObjectInstance>();
            SectorBasedObjectInstance newObject = null;
            Action<Room, SectorBasedObjectInstance> handleRoomVersion = (roomVersion, objectVersion) =>
            {
                // Remove object
                roomVersion.RemoveObjectAndSingularPortal(_editor.Level, objectVersion);
                _editor.ObjectChange(objectVersion, ObjectChangeType.Remove, roomVersion);

                // Readd in smaller pieces
                foreach (SectorBasedObjectInstance splitInstance in objectVersion.SplitIntoRectangles(pos => !area.Contains(pos), new VectorInt2()))
                    newObjects.Add(roomVersion.AddObjectAndSingularPortal(_editor.Level, splitInstance));
                newObject = objectVersion.Clone(objectVersion.Area.Intersect(area));
                newObjects.Add(roomVersion.AddObjectAndSingularPortal(_editor.Level, newObject));
            };
            if (@object is PortalInstance && room.Alternated)
                handleRoomVersion(room.AlternateOpposite, ((PortalInstance)@object).FindAlternatePortal(room.AlternateOpposite));
            handleRoomVersion(room, @object);

            // Handle special objects
            if (@object is PortalInstance)
            {
                var relevantRooms = new HashSet<Room> { room, ((PortalInstance)@object).AdjoiningRoom };
                Room.FixupNeighborPortals(_editor.Level, new[] { room }, new[] { room }, ref relevantRooms);
                Parallel.ForEach(relevantRooms, relevantRoom =>
                {
                    relevantRoom.BuildGeometry(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                });
                foreach (Room relevantRoom in relevantRooms)
                    _editor.RoomPropertiesChange(relevantRoom);
            }
            else if (@object is TriggerInstance)
            {
                // Make sure that the trigger is in the same place in the trigger list.
                // We don't want to change the order.
                for (int x = 0; x <= @object.Area.Width; ++x)
                    for (int z = 0; z <= @object.Area.Height; ++z)
                    {
                        var triggersOnSector = room.Sectors[x + @object.Area.X0, z + @object.Area.Y0].Triggers;
                        TriggerInstance last = triggersOnSector[triggersOnSector.Count - 1];
                        triggersOnSector.RemoveAt(triggersOnSector.Count - 1);
                        triggersOnSector.Insert(triggerIndices[x, z], last);
                    }
            }

            // Update
            _editor.RoomSectorPropertiesChange(room);
            if (room.AlternateOpposite != null)
                _editor.RoomSectorPropertiesChange(room.AlternateOpposite);
            _editor.ObjectChange(newObjects, ObjectChangeType.Add);
            if (wasSelected)
                _editor.SelectedObject = newObject;
        }

        public static void BookmarkObject(ObjectInstance objectToBookmark)
        {
            _editor.BookmarkedObject = objectToBookmark;
            _editor.SendMessage("Object bookmarked: " + _editor.BookmarkedObject, PopupType.Info);
        }

        public static void SwitchToolOrdered(int toolIndex)
        {
            if (_editor.Mode == EditorMode.Map2D || toolIndex > (int)EditorToolType.Terrain ||
                _editor.Mode != EditorMode.Geometry && toolIndex > 6)
                return;

            EditorTool currentTool = new EditorTool() { Tool = _editor.Tool.Tool, TextureUVFixer = _editor.Tool.TextureUVFixer, GridSize = _editor.Tool.GridSize };

            switch (toolIndex)
            {
                case 0:
                    currentTool.Tool = EditorToolType.Selection;
                    break;
                case 1:
                    currentTool.Tool = EditorToolType.Brush;
                    break;
                case 2:
                    if (_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Shovel;
                    else
                        currentTool.Tool = EditorToolType.Pencil;
                    break;
                case 3:
                    if (_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Pencil;
                    else
                        currentTool.Tool = EditorToolType.Fill;
                    break;
                case 4:
                    if (_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Flatten;
                    else
                        currentTool.Tool = EditorToolType.GridPaint;
                    break;
                case 5:
                    if (_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Smooth;
                    else
                        currentTool.Tool = EditorToolType.Group;
                    break;
                case 6:
                    if(_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Drag;
                    else
                        currentTool.TextureUVFixer = !currentTool.TextureUVFixer;
                    break;
                default:
                    currentTool.Tool = (EditorToolType)(toolIndex + 4);
                    break;
            }
            _editor.Tool = currentTool;
        }

        public static void SetSelectionTileSize(float tileSize)
        {
            _editor.Configuration.TextureMap_TileSelectionSize = tileSize > 256.0f ? 32.0f : tileSize;
            _editor.ConfigurationChange();
        }

        public static void SelectWaterRooms()
        {
            var waterRooms = _editor.Level.ExistingRooms.Where(r => r.Properties.Type == RoomType.Water);
            TrySelectRooms(waterRooms);
        }

        public static void SelectSkyRooms()
        {
            var skyRooms = _editor.Level.ExistingRooms.Where(r => r.Properties.FlagHorizon);
            TrySelectRooms(skyRooms);
        }

        public static void SelectQuicksandRooms()
        {
            var quicksandRooms = _editor.Level.ExistingRooms.Where(r => r.Properties.Type == RoomType.Quicksand);
            TrySelectRooms(quicksandRooms);
        }
        public static void SelectOutsideRooms()
        {
            var outsideRooms = _editor.Level.ExistingRooms.Where(r => r.Properties.FlagOutside);
            TrySelectRooms(outsideRooms);
        }

        public static void SelectRoomsByTags(IWin32Window owner)
        {
            using (var formTags = new FormSelectRoomByTags(_editor))
            {
                if (formTags.ShowDialog(owner) != DialogResult.OK)
                    return;

                string[] tags = formTags.tbTagSearch.Text.Split(' ');
                if (!tags.Any())
                    return;

                bool findAllTags = formTags.findAllTags;
                var matchingRooms = _editor.Level.ExistingRooms.Where(r => {
                    if (findAllTags)
                        return r.Properties.Tags.Intersect(tags).Count() == tags.Count();
                    else
                        return r.Properties.Tags.Intersect(tags).Any();
                });
                TrySelectRooms(matchingRooms);
            }
        }

        private static void TrySelectRooms(IEnumerable<Room> rooms)
        {
            if (rooms.Any())
                _editor.SelectRooms(rooms);
        }

        public static void SetStaticMeshesColorToRoomAmbientLight()
        {
            IEnumerable<Room> SelectedRooms = _editor.SelectedRooms;
            foreach (Room room in SelectedRooms)
            {
                IEnumerable<StaticInstance> staticMeshes = room.Objects.OfType<StaticInstance>();
                foreach (StaticInstance staticMesh in staticMeshes)
                {
                    staticMesh.Color = room.Properties.AmbientLight;
                    _editor.ObjectChange(staticMesh, ObjectChangeType.Change);
                }
            }
        }

        public static void SetStaticMeshesColor(IWin32Window owner)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if(colorDialog.ShowDialog(owner) == DialogResult.OK)
                {
                    IEnumerable<Room> SelectedRooms = _editor.SelectedRooms;
                    foreach (Room room in SelectedRooms)
                    {
                        IEnumerable<StaticInstance> staticMeshes = room.Objects.OfType<StaticInstance>();
                        foreach (StaticInstance staticMesh in staticMeshes)
                        {
                            staticMesh.Color = colorDialog.Color.ToFloat3Color();
                            _editor.ObjectChange(staticMesh, ObjectChangeType.Change);
                        }
                    }
                }
            }

        }

        public static void MakeQuickItemGroup(IWin32Window owner)
        {
            if (_editor.Level.Settings.GameVersion != TRVersion.Game.TRNG)
            {
                _editor.SendMessage("Itemgroup is TRNG-only feature.", PopupType.Info);
                return;
            }

            var items = new List<PositionBasedObjectInstance>();

            if (_editor.SelectedObject is ObjectGroup)
                items.AddRange((_editor.SelectedObject as ObjectGroup).Where(o => o is ItemInstance));

            if (items.Count <= 1)
            {
                using (var form = new FormQuickItemgroup(_editor))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK || form.SelectedValue == null)
                        return;

                    foreach (var item in _editor.Level.GetAllObjects().OfType<ItemInstance>())
                    {
                        if (item is StaticInstance && form.SelectedValue is WadStaticId)
                        {
                            if ((item as StaticInstance).WadObjectId == ((WadStaticId)form.SelectedValue))
                                items.Add(item);
                        }
                        else if (item is MoveableInstance && form.SelectedValue is WadMoveableId)
                        {
                            if ((item as MoveableInstance).WadObjectId == ((WadMoveableId)form.SelectedValue))
                                items.Add(item);
                        }
                    }
                }
            }

            if (items.Count > 0)
            {
                // Create ItemGroup string
                string scriptString = string.Format(";Itemgroup of {0} objects\n", items.Count.ToString());
                scriptString += "ItemGroup= 1";
                foreach (ItemInstance item in items)
                    scriptString += "," + item.ScriptId;
                Clipboard.SetText(scriptString, TextDataFormat.Text);
                _editor.SendMessage("Itemgroup copied into clipboard.", PopupType.Info);
            }
            else
                _editor.SendMessage("No items were selected. Itemgroup was not created.", PopupType.Warning);
        }

        public static bool AutoLoadSamplePath(LevelSettings settings)
        {
            if (settings.GameVersion.UsesMainSfx())
                return false;

            var samplePath = Application.StartupPath + "\\Assets\\Samples\\";

            switch (settings.GameVersion.Native())
            {
                case TRVersion.Game.TR1:
                    samplePath = samplePath + "TR1";
                    break;

                case TRVersion.Game.TR4:
                    samplePath = samplePath + "TR4";
                    break;

                case TRVersion.Game.TR5:
                    samplePath = samplePath + "TR5";
                    break;

                default:
                    return false;
            }

            if (!settings.AllSoundSamplesAvailable && Directory.Exists(samplePath))
                settings.WadSoundPaths.Add(new WadSoundPath(settings.MakeRelative(samplePath, VariableType.EditorDirectory)));
            else
                return false;


            if (!settings.AllSoundSamplesAvailable)
            {
                settings.WadSoundPaths.RemoveAt(settings.WadSoundPaths.Count - 1);
                return false;
            }
            else
                return true;
        }

        public static void AutodetectAndAssignSounds(LevelSettings settings, IWin32Window owner = null) // No owner - no confirmation
        {
            if (owner != null && settings.SelectedSounds.Count > 0 &&
                DarkMessageBox.Show(owner, "Deselect all sounds before autodetection?", "Deselect sounds", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                settings.SelectedSounds.Clear();

            AssignHardcodedSounds(settings);
            AssignWadSounds(settings);
            AssignTriggerSounds(settings);
            AssignSoundSourcesSounds(settings);
        }

        public static void AssignHardcodedSounds(LevelSettings settings)
        {
            var referenceSounds = TrCatalog.GetAllFixedByDefaultSounds(settings.GameVersion);
            foreach (int id in referenceSounds.Keys.ToList())
                if (!settings.SelectedSounds.Contains(id))
                    settings.SelectedSounds.Add(id);

            foreach (var catalog in settings.SoundCatalogs)
                if (catalog.Sounds != null)
                    foreach (var sound in catalog.Sounds.SoundInfos)
                        if (sound.Global)
                            if (!settings.SelectedSounds.Contains(sound.Id))
                                settings.SelectedSounds.Add(sound.Id);

        }

        public static void AssignCatalogSounds(LevelSettings settings, ReferencedSoundCatalog catalog)
        {
            if (catalog.Sounds != null)
                foreach (var soundInfo in catalog.Sounds.SoundInfos)
                    if (!settings.SelectedSounds.Contains(soundInfo.Id))
                        settings.SelectedSounds.Add(soundInfo.Id);
        }

        public static void AssignSoundSourcesSounds(LevelSettings settings)
        {
            foreach (var room in _editor.Level.Rooms)
                if (room != null)
                    foreach (var instance in room.Objects)
                        if (instance is SoundSourceInstance)
                        {
                            var soundSource = instance as SoundSourceInstance;
                            if (!settings.SelectedSounds.Contains(soundSource.SoundId))
                                settings.SelectedSounds.Add(soundSource.SoundId);
                        }
        }

        public static void AssignWadSounds(LevelSettings settings)
        {
            foreach (var wad in _editor.Level.Settings.Wads.Where(w => w.Wad != null))
                foreach (var item in wad.Wad.Moveables)
                    foreach (var anim in item.Value.Animations)
                        foreach (var cmd in anim.AnimCommands)
                            if (cmd.Type == WadAnimCommandType.PlaySound)
                                if (!settings.SelectedSounds.Contains(cmd.Parameter2))
                                    settings.SelectedSounds.Add(cmd.Parameter2);
        }

        public static void AssignTriggerSounds(LevelSettings settings)
        {
            bool isTR4 = _editor.Level.Settings.GameVersion.Native() == TRVersion.Game.TR4;

            foreach (var room in _editor.Level.Rooms)
                if (room != null)
                    foreach (var trigger in room?.Triggers)
                        if (trigger.TargetType == TriggerTargetType.FlipEffect &&
                            trigger.Target is TriggerParameterUshort)
                        {
                            int foundId = -1;
                            var feNum = ((TriggerParameterUshort)trigger.Target).Key;

                            switch (feNum)
                            {
                                case 2: // FLOOD_FX, broken in TR5 onwards
                                    if (isTR4) foundId = 238;
                                    break;

                                case 3: // LARA_BUBBLES
                                    foundId = 37;
                                    break;

                                case 11: // EXPLOSION_FX
                                    foundId = 105;
                                    break;

                                case 10:  // TIMERFIELD_FX
                                case 70:  // NG (legacy 0-255)
                                case 168: // NG extended soundmap
                                    foundId = ((TriggerParameterUshort)trigger.Timer).Key;
                                    break;

                                case 71: // NG (legacy 256-370)
                                    foundId = ((TriggerParameterUshort)trigger.Timer).Key + 256;
                                    break;
                            }

                            if (foundId != -1 && !settings.SelectedSounds.Contains(foundId))
                                settings.SelectedSounds.Add(foundId);
                        }
        }

		public static void GetObjectStatistics(Editor editor, IDictionary<WadMoveableId,uint> resultMoveables, IDictionary<WadStaticId, uint> resultStatics, out int totalMoveables, out int totalStatics)
        {
			totalMoveables = 0;
			totalStatics = 0;
			foreach (var room in editor.Level.ExistingRooms) {
				var roomMoveables = room.Objects.Where(ob => ob is MoveableInstance).Cast<MoveableInstance>().ToList();
				var roomStatics = room.Objects.Where(ob => ob is StaticInstance).Cast<StaticInstance>().ToList();

				foreach (var m in roomMoveables) {
					if (resultMoveables.ContainsKey(m.WadObjectId)) {
						resultMoveables[m.WadObjectId]++;
					} else {
						resultMoveables.Add(m.WadObjectId, 1);
					}
					totalMoveables++;
				}
				foreach (var s in roomStatics) {
					if (resultStatics.ContainsKey(s.WadObjectId)) {
						resultStatics[s.WadObjectId]++;
					} else {
						resultStatics.Add(s.WadObjectId, 1);
					}
					totalStatics++;
				}

			}
		}

        public static void RealignToStepHeight(Room room, RectangleInt2 area, SectorVerticalPart vertical, int stepHeight, bool disableUndo = false)
        {
            if (!disableUndo)
                _editor.UndoManager.PushGeometryChanged(room);

            bool success = AreaUtils.PerformActionOnArea(area, (x, z) =>
            {
                if (!room.GetSectorTry(x, z, out Sector sector))
                    return false;

                sector.FixHeights(vertical, stepHeight);
                return true;
            });

            if (success)
                SmartBuildGeometry(room, area);
        }

        public static void ConvertAreaToQuads(Room room, RectangleInt2 area, SectorVerticalPart vertical, int stepHeight, bool disableUndo = false)
        {
            if (!disableUndo)
                _editor.UndoManager.PushGeometryChanged(room);

            bool success = SurfaceToQuadConverter.ConvertAreaToQuads(room, area, vertical, stepHeight);

            if (success)
                SmartBuildGeometry(room, area);
        }
    }
}
