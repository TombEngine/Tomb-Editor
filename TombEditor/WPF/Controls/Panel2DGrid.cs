using NLog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;

namespace TombEditor.WPF.Controls
{
    /// <summary>
    /// Native WPF replacement for the WinForms Panel2DGrid control.
    /// Uses OnRender with DrawingContext for hardware-accelerated rendering.
    /// </summary>
    public class Panel2DGrid : FrameworkElement
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly double OutlineSectorColoringInfoWidth = 3.0;

        private readonly Editor _editor;
        private Room _room;
        private bool _doSectorSelection;

        // Frozen pens for rendering performance.
        private static readonly Pen GridPen = CreateFrozenPen(Color.FromArgb(140, 0, 0, 0), 1.0);
        private static readonly Pen BorderPen = CreateFrozenPen(Colors.Black, 1.0);
        private static readonly Pen SelectedPortalPen = CreateFrozenPen(Colors.YellowGreen, 2.0);
        private static readonly Pen SelectedTriggerPen = CreateFrozenPen(Colors.White, 2.0);

        public Room Room
        {
            get => _room;
            set
            {
                if (_room == value)
                    return;
                _room = value;
                InvalidateVisual();
            }
        }

        public Panel2DGrid()
        {
            ClipToBounds = true;
            Focusable = true;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        public void Dispose()
        {
            _editor.EditorEventRaised -= EditorEventRaised;
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is SectorColoringManager.ChangeSectorColoringInfoEvent ||
                obj is Editor.GameVersionChangedEvent ||
                obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.RoomSectorPropertiesChangedEvent ||
                obj is Editor.ObjectChangedEvent ||
                obj is Editor.RoomGeometryChangedEvent ||
                obj is Editor.ConfigurationChangedEvent ||
                obj is Editor.SelectedObjectChangedEvent e && IsObjectChangeRelevant(e))
            {
                InvalidateVisual();
            }

            if (obj is Editor.ActionChangedEvent actionChanged)
            {
                bool isCameraRelocate = actionChanged.Current is EditorActionRelocateCamera;
                Cursor = isCameraRelocate ? Cursors.Cross : Cursors.Arrow;
            }
        }

        private static bool IsObjectChangeRelevant(Editor.SelectedObjectChangedEvent e)
        {
            return e.Previous is SectorBasedObjectInstance || e.Current is SectorBasedObjectInstance;
        }

        // Grid coordinate calculations.

        private VectorInt2 RoomSize => Room.SectorSize;

        private VectorInt2 GetGridDimensions()
        {
            return VectorInt2.Max(RoomSize, new VectorInt2(Room.DefaultRoomDimensions, Room.DefaultRoomDimensions));
        }

        private double GetGridStep()
        {
            var gridDimensions = GetGridDimensions();
            double w = ActualWidth;
            double h = ActualHeight;

            if (w * gridDimensions.Y < h * gridDimensions.X)
                return w / gridDimensions.X;
            else
                return h / gridDimensions.Y;
        }

        private Rect GetVisualAreaTotal()
        {
            double w = ActualWidth;
            double h = ActualHeight;
            var gridDimensions = GetGridDimensions();
            double gridStep = GetGridStep();
            double gridW = gridDimensions.X * gridStep;
            double gridH = gridDimensions.Y * gridStep;

            return new Rect(
                (w - gridW) * 0.5,
                (h - gridH) * 0.5,
                gridW,
                gridH);
        }

        private Rect GetVisualAreaRoom()
        {
            var totalArea = GetVisualAreaTotal();
            double gridStep = GetGridStep();
            var gridDimensions = GetGridDimensions();
            var roomSize = RoomSize;

            return new Rect(
                totalArea.X + gridStep * ((gridDimensions.X - roomSize.X) / 2),
                totalArea.Y + gridStep * ((gridDimensions.Y - roomSize.Y) / 2),
                gridStep * roomSize.X,
                gridStep * roomSize.Y);
        }

        private Point ToVisualCoord(VectorInt2 sectorCoord)
        {
            var roomArea = GetVisualAreaRoom();
            double gridStep = GetGridStep();
            return new Point(
                sectorCoord.X * gridStep + roomArea.X,
                roomArea.Bottom - (sectorCoord.Y + 1) * gridStep);
        }

        private Rect ToVisualCoord(RectangleInt2 sectorArea)
        {
            var p0 = ToVisualCoord(sectorArea.Start);
            var p1 = ToVisualCoord(sectorArea.End);
            double gridStep = GetGridStep();
            return new Rect(
                Math.Min(p0.X, p1.X), Math.Min(p0.Y, p1.Y),
                Math.Abs(p1.X - p0.X) + gridStep, Math.Abs(p1.Y - p0.Y) + gridStep);
        }

        private VectorInt2 FromVisualCoord(Point point)
        {
            var roomArea = GetVisualAreaRoom();
            double gridStep = GetGridStep();
            var roomSize = RoomSize;
            return new VectorInt2(
                (int)Math.Max(0, Math.Min(roomSize.X - 1, (point.X - roomArea.X) / gridStep)),
                (int)Math.Max(0, Math.Min(roomSize.Y - 1, (roomArea.Bottom - point.Y) / gridStep)));
        }

        // Mouse interaction.

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            CaptureMouse();
            HandleMouseDown(e.GetPosition(this), isRightButton: false);
            e.Handled = true;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            HandleMouseDown(e.GetPosition(this), isRightButton: true);
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _doSectorSelection = false;
            ReleaseMouseCapture();
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            _doSectorSelection = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_editor?.SelectedRoom == null || _editor.Action is EditorActionRelocateCamera)
                return;

            if (e.LeftButton == MouseButtonState.Pressed && _doSectorSelection)
                _editor.SelectedSectors = new SectorSelection { Start = _editor.SelectedSectors.Start, End = FromVisualCoord(e.GetPosition(this)) };
        }

        private void HandleMouseDown(Point position, bool isRightButton)
        {
            if (_editor == null || Room == null)
                return;

            // Move camera to selected sector.
            if (_editor.Action is EditorActionRelocateCamera)
            {
                _editor.MoveCameraToSector(FromVisualCoord(position));
                return;
            }

            var sectorPos = FromVisualCoord(position);
            var selectedSectorObject = _editor.SelectedObject as SectorBasedObjectInstance;
            bool isAltDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

            if (!isRightButton && !isAltDown)
            {
                if (selectedSectorObject != null &&
                    selectedSectorObject.Room == Room &&
                    selectedSectorObject.Area.Contains(sectorPos))
                {
                    HandleSectorObjectClick(selectedSectorObject);
                }
                else
                {
                    _editor.SelectedSectors = new SectorSelection { Start = sectorPos, End = sectorPos };

                    if (selectedSectorObject != null)
                        _editor.SelectedObject = null;
                    _doSectorSelection = true;
                }
            }
            else
            {
                SelectNextObjectAtSector(sectorPos, selectedSectorObject, position);
            }
        }

        private void HandleSectorObjectClick(SectorBasedObjectInstance selectedSectorObject)
        {
            if (selectedSectorObject is PortalInstance portal)
            {
                var room = Room;

                if (room.AlternateBaseRoom != null && portal.AdjoiningRoom.Alternated)
                {
                    _editor.SelectRoom(portal.AdjoiningRoom.AlternateRoom);
                    _editor.SelectedObject = portal.FindOppositePortal(room).FindAlternatePortal(portal.AdjoiningRoom.AlternateRoom);
                }
                else
                {
                    _editor.SelectRoom(portal.AdjoiningRoom);
                    _editor.SelectedObject = portal.FindOppositePortal(room);
                }
            }
            else if (selectedSectorObject is TriggerInstance)
            {
                EditorActions.EditObject(selectedSectorObject, null);
            }
        }

        private void SelectNextObjectAtSector(VectorInt2 sectorPos, SectorBasedObjectInstance selectedSectorObject, Point mousePosition)
        {
            var portalsInRoom = Room.Portals.Cast<SectorBasedObjectInstance>();
            var triggersInRoom = Room.Triggers.Cast<SectorBasedObjectInstance>();
            var relevantObjects = portalsInRoom.Concat(triggersInRoom)
                .Where(obj => obj.Area.Contains(sectorPos));

            var nextObject = relevantObjects
                .FindFirstAfterWithWrapAround(obj => obj == selectedSectorObject, obj => true);

            if (nextObject != null)
            {
                _editor.SelectedObject = nextObject;

                // Show tooltip near the cursor.
                var toolTip = new ToolTip
                {
                    Content = nextObject.ToString(),
                    IsOpen = true,
                    StaysOpen = false
                };
                ToolTip = toolTip;
            }
        }

        // Rendering.

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (_editor == null || Room == null)
            {
                DrawDesignPlaceholder(dc);
                return;
            }

            try
            {
                var totalArea = GetVisualAreaTotal();
                var roomArea = GetVisualAreaRoom();
                var gridDimensions = GetGridDimensions();
                double gridStep = GetGridStep();
                var roomSize = RoomSize;

                // Draw background.
                var bgBrush = CreateFrozenBrush(_editor.Configuration.UI_ColorScheme.Color2DBackground.ToWPFColor());
                dc.DrawRectangle(bgBrush, null, totalArea);

                // Draw sector tiles.
                for (int x = 0; x < roomSize.X; x++)
                {
                    for (int z = 0; z < roomSize.Y; z++)
                    {
                        var tileRect = new Rect(
                            roomArea.X + x * gridStep,
                            roomArea.Y + (roomSize.Y - 1 - z) * gridStep,
                            gridStep,
                            gridStep);
                        PaintSectorTile(dc, tileRect, x, z);
                    }
                }

                // Draw grid lines.
                DrawGridLines(dc, totalArea, gridDimensions, gridStep);

                // Draw selection.
                DrawSelection(dc);
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while drawing the 2D grid.");
            }
        }

        private void DrawGridLines(DrawingContext dc, Rect totalArea, VectorInt2 gridDimensions, double gridStep)
        {
            for (int x = 0; x <= gridDimensions.X; ++x)
            {
                var pen = (x == 0 || x == gridDimensions.X) ? BorderPen : GridPen;
                double xPos = totalArea.X + x * gridStep;
                dc.DrawLine(pen, new Point(xPos, totalArea.Y), new Point(xPos, totalArea.Y + gridStep * gridDimensions.Y));
            }

            for (int y = 0; y <= gridDimensions.Y; ++y)
            {
                var pen = (y == 0 || y == gridDimensions.Y) ? BorderPen : GridPen;
                double yPos = totalArea.Y + y * gridStep;
                dc.DrawLine(pen, new Point(totalArea.X, yPos), new Point(totalArea.X + gridStep * gridDimensions.X, yPos));
            }
        }

        private void DrawSelection(DrawingContext dc)
        {
            if (_editor.SelectedSectors.Valid)
            {
                var selectionPen = CreateFrozenPen(_editor.Configuration.UI_ColorScheme.ColorSelection.ToWPFColor(), 2.0);
                dc.DrawRectangle(null, selectionPen, ToVisualCoord(_editor.SelectedSectors.Area));
            }

            var instance = _editor.SelectedObject as SectorBasedObjectInstance;
            if (instance != null && instance.Room == Room)
            {
                var pen = instance is PortalInstance ? SelectedPortalPen : SelectedTriggerPen;
                dc.DrawRectangle(null, pen, ToVisualCoord(instance.Area));
            }
        }

        private void PaintSectorTile(DrawingContext dc, Rect sectorArea, int x, int z)
        {
            var coloringInfos = _editor.SectorColoringManager.ColoringInfo.GetColors(
                _editor.Configuration.UI_ColorScheme, Room, x, z,
                _editor.Configuration.UI_ProbeAttributesThroughPortals);

            if (coloringInfos == null)
                return;

            for (int i = 0; i < coloringInfos.Count; i++)
            {
                var info = coloringInfos[i];
                var brush = CreateFrozenBrush(info.Color.ToWPFColor());

                switch (info.Shape)
                {
                    case SectorColoringShape.Rectangle:
                        dc.DrawRectangle(brush, null, sectorArea);
                        break;

                    case SectorColoringShape.Frame:
                        DrawFrame(dc, sectorArea, info.Color);
                        break;

                    case SectorColoringShape.Hatch:
                        DrawHatch(dc, sectorArea, info.Color);
                        break;

                    case SectorColoringShape.EdgeZp:
                    case SectorColoringShape.EdgeZn:
                    case SectorColoringShape.EdgeXp:
                    case SectorColoringShape.EdgeXn:
                        DrawEdge(dc, sectorArea, info.Shape, brush);
                        break;

                    case SectorColoringShape.TriangleXnZn:
                    case SectorColoringShape.TriangleXnZp:
                    case SectorColoringShape.TriangleXpZn:
                    case SectorColoringShape.TriangleXpZp:
                        DrawTriangle(dc, sectorArea, info.Shape, brush);
                        break;
                }
            }
        }

        private void DrawFrame(DrawingContext dc, Rect sectorArea, Vector4 color)
        {
            var pen = CreateFrozenPen(color.ToWPFColor(), OutlineSectorColoringInfoWidth);
            double half = OutlineSectorColoringInfoWidth / 2.0;
            var frameRect = new Rect(
                sectorArea.X + half,
                sectorArea.Y + half,
                sectorArea.Width - OutlineSectorColoringInfoWidth,
                sectorArea.Height - OutlineSectorColoringInfoWidth);
            dc.DrawRectangle(null, pen, frameRect);
        }

        private void DrawHatch(DrawingContext dc, Rect sectorArea, Vector4 color)
        {
            // Simulate a hatch pattern using diagonal lines.
            var pen = CreateFrozenPen(color.ToWPFColor(), 1.0);
            double spacing = 6.0;

            dc.PushClip(new RectangleGeometry(sectorArea));

            double totalRange = sectorArea.Width + sectorArea.Height;
            for (double offset = 0; offset < totalRange; offset += spacing)
            {
                dc.DrawLine(pen,
                    new Point(sectorArea.X + offset, sectorArea.Bottom),
                    new Point(sectorArea.X + offset - sectorArea.Height, sectorArea.Top));
            }

            dc.Pop();
        }

        private static void DrawEdge(DrawingContext dc, Rect sectorArea, SectorColoringShape shape, Brush brush)
        {
            double w = OutlineSectorColoringInfoWidth;
            Rect edgeRect;

            if (shape == SectorColoringShape.EdgeZp)
                edgeRect = new Rect(sectorArea.X, sectorArea.Y, sectorArea.Width, w);
            else if (shape == SectorColoringShape.EdgeZn)
                edgeRect = new Rect(sectorArea.X, sectorArea.Bottom - w, sectorArea.Width, w);
            else if (shape == SectorColoringShape.EdgeXp)
                edgeRect = new Rect(sectorArea.Right - w, sectorArea.Y, w, sectorArea.Height);
            else
                edgeRect = new Rect(sectorArea.X, sectorArea.Y, w, sectorArea.Height);

            dc.DrawRectangle(brush, null, edgeRect);
        }

        private static void DrawTriangle(DrawingContext dc, Rect sectorArea, SectorColoringShape shape, Brush brush)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                Point p0, p1, p2;

                if (shape == SectorColoringShape.TriangleXnZn)
                {
                    p0 = new Point(sectorArea.Left, sectorArea.Top);
                    p1 = new Point(sectorArea.Left, sectorArea.Bottom);
                    p2 = new Point(sectorArea.Right, sectorArea.Bottom);
                }
                else if (shape == SectorColoringShape.TriangleXnZp)
                {
                    p0 = new Point(sectorArea.Left, sectorArea.Bottom);
                    p1 = new Point(sectorArea.Left, sectorArea.Top);
                    p2 = new Point(sectorArea.Right, sectorArea.Top);
                }
                else if (shape == SectorColoringShape.TriangleXpZn)
                {
                    p0 = new Point(sectorArea.Left, sectorArea.Bottom);
                    p1 = new Point(sectorArea.Right, sectorArea.Top);
                    p2 = new Point(sectorArea.Right, sectorArea.Bottom);
                }
                else
                {
                    p0 = new Point(sectorArea.Left, sectorArea.Top);
                    p1 = new Point(sectorArea.Right, sectorArea.Top);
                    p2 = new Point(sectorArea.Right, sectorArea.Bottom);
                }

                ctx.BeginFigure(p0, true, true);
                ctx.LineTo(p1, false, false);
                ctx.LineTo(p2, false, false);
            }

            geometry.Freeze();
            dc.DrawGeometry(brush, null, geometry);
        }

        // Helper methods.

        private void DrawDesignPlaceholder(DrawingContext dc)
        {
            dc.DrawRectangle(CreateFrozenBrush(Color.FromRgb(50, 50, 50)), BorderPen,
                new Rect(0, 0, ActualWidth, ActualHeight));

            var text = new FormattedText("2D Grid",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), 12.0, Brushes.Gray,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(text, new Point(
                (ActualWidth - text.Width) / 2.0,
                (ActualHeight - text.Height) / 2.0));
        }

        private static Pen CreateFrozenPen(Color color, double thickness)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            var pen = new Pen(brush, thickness);
            pen.Freeze();
            return pen;
        }

        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
