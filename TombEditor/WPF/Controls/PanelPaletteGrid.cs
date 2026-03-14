using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.WPF.Controls
{
    public class PanelPaletteGrid : FrameworkElement
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const double CellWidth = 10.0;
        private const double CellHeight = 10.0;

        private static readonly Pen GridPen = CreateFrozenPen(Color.FromArgb(140, 0, 0, 0), 1.0);
        private static readonly Pen BorderPen = CreateFrozenPen(Colors.Black, 1.0);
        private static readonly Pen SelectionPen = CreateFrozenPen(Colors.White, 1.0);

        private readonly Editor _editor;
        private List<ColorC> _palette = new List<ColorC>();
        private int _selectedIndex = -1;
        private Size _oldPaletteSize;

        public bool Editable { get; set; } = true;

        public Color SelectedColor => GetColorAtIndex(_selectedIndex);

        public ColorC SelectedColorC
        {
            get
            {
                if (_selectedIndex < 0 || _selectedIndex >= _palette.Count)
                    return new ColorC(128, 128, 128);
                return _palette[_selectedIndex];
            }
        }

        public List<ColorC> Palette => new List<ColorC>(_palette);

        private int ColumnCount => (int)(ActualWidth / CellWidth);
        private int RowCount => (int)(ActualHeight / CellHeight);

        public PanelPaletteGrid()
        {
            ClipToBounds = true;
            Focusable = true;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            _editor = Editor.Instance;
        }

        public void LoadPalette(List<ColorC> palette)
        {
            if (palette.Count < 1)
                return;

            _palette.Clear();
            foreach (var c in palette)
                _palette.Add(new ColorC(c.R, c.G, c.B));

            PickColor();
            InvalidateVisual();
        }

        public void PickColor()
        {
            if (!_editor.Configuration.Palette_PickColorFromSelectedObject || _editor.SelectedObject == null)
                return;

            if (_editor.SelectedObject is not IColorable instance)
                return;

            var normalizedColor = instance.Color / 2.0f;
            var color = new ColorC(
                (byte)(normalizedColor.X * 255),
                (byte)(normalizedColor.Y * 255),
                (byte)(normalizedColor.Z * 255));

            for (int i = 0; i < _palette.Count; i++)
            {
                if (_palette[i] == color)
                {
                    _selectedIndex = i;
                    _editor.LastUsedPaletteColourChange(SelectedColorC);
                    InvalidateVisual();
                    return;
                }
            }
        }

        public void SetColorAtSelection(ColorC color)
        {
            if (_selectedIndex < 0)
                return;

            if (_selectedIndex >= _palette.Count)
                _palette.Add(color);
            else
                _palette[_selectedIndex] = color;

            InvalidateVisual();
        }

        // Mouse interaction.

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Focus();

            if (e.ClickCount == 2 && Editable)
            {
                PickColourFromDialog();
                return;
            }

            CaptureMouse();

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                PickColourFromDialog(false);
                return;
            }

            if (_editor.SelectedObject is IColorable)
            {
                _editor.UndoManager.PushObjectPropertyChanged(
                    (PositionBasedObjectInstance)_editor.SelectedObject);
            }

            _editor.ToggleHiddenSelection(true);
            ChangeColorByMouse(e.GetPosition(this));
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _editor.ToggleHiddenSelection(false);
            ReleaseMouseCapture();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed)
                ChangeColorByMouse(e.GetPosition(this));
        }

        private void ChangeColorByMouse(Point position)
        {
            int columns = ColumnCount;
            int rows = RowCount;

            if (columns <= 0 || rows <= 0)
                return;

            int x = position.X < 0 ? 0 : (int)(position.X / CellWidth);
            int y = position.Y < 0 ? 0 : (int)(position.Y / CellHeight);

            x = Math.Min(x, columns - 1);
            y = Math.Min(y, rows - 1);

            _selectedIndex = y * columns + x;

            if (_editor.SelectedObject != null && _editor.SelectedObject.CanBeColored())
            {
                _editor.ToggleHiddenSelection(true);

                if (_editor.SelectedObject is IColorable instance)
                {
                    instance.Color = SelectedColor.ToFloat3Color() * 2.0f;

                    if (_editor.SelectedObject is LightInstance)
                        _editor.SelectedObject.Room.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);

                    _editor.ObjectChange(_editor.SelectedObject, ObjectChangeType.Change);
                }
            }

            _editor.LastUsedPaletteColourChange(SelectedColorC);
            InvalidateVisual();
        }

        private void PickColourFromDialog(bool onlyFromPalette = true)
        {
            using (var colorDialog = new TombLib.Controls.RealtimeColorDialog(
                _editor.Configuration.ColorDialog_Position.X,
                _editor.Configuration.ColorDialog_Position.Y,
                null,
                _editor.Configuration.UI_ColorScheme))
            {
                var currentColor = SelectedColor;
                colorDialog.Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);

                if (!onlyFromPalette)
                {
                    var obj = _editor.SelectedObject;
                    if (obj is LightInstance light)
                        colorDialog.Color = (light.Color * 0.5f).ToWinFormsColor();
                    else if (obj is StaticInstance stat)
                        colorDialog.Color = (stat.Color * 0.5f).ToWinFormsColor();
                    else if (_editor.Level.IsTombEngine && obj is MoveableInstance moveable)
                        colorDialog.Color = moveable.Color.ToWinFormsColor();
                }

                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var picked = colorDialog.Color;
                    SetColorAtSelection(new ColorC(picked.R, picked.G, picked.B));
                    _editor.Level.Settings.Palette = Palette;
                }

                _editor.Configuration.ColorDialog_Position = colorDialog.Position;
            }
        }

        // Rendering.

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // Recalculate selected color coordinate on resize.
            if (_selectedIndex >= 0)
            {
                int oldColumns = _oldPaletteSize.Width > 0 ? (int)(_oldPaletteSize.Width / CellWidth) : ColumnCount;
                int newColumns = ColumnCount;

                if (oldColumns > 0 && newColumns > 0 && oldColumns != newColumns)
                {
                    int x = _selectedIndex % oldColumns;
                    int y = _selectedIndex / oldColumns;
                    int flatIndex = y * oldColumns + x;
                    _selectedIndex = flatIndex;
                }
            }

            _oldPaletteSize = sizeInfo.NewSize;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (_editor == null)
            {
                DrawDesignPlaceholder(dc);
                return;
            }

            try
            {
                int columns = ColumnCount;
                int rows = RowCount;

                if (columns <= 0 || rows <= 0)
                    return;

                DrawCells(dc, columns, rows);
                DrawGridLines(dc, columns, rows);
                DrawOuterBorder(dc, columns, rows);
                DrawSelectionRect(dc, columns, rows);
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while drawing the palette grid.");
            }
        }

        private void DrawCells(DrawingContext dc, int columns, int rows)
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int index = y * columns + x;
                    var color = GetColorAtIndex(index);
                    var brush = CreateFrozenBrush(color);
                    dc.DrawRectangle(brush, null, new Rect(x * CellWidth, y * CellHeight, CellWidth, CellHeight));
                }
            }
        }

        private static void DrawGridLines(DrawingContext dc, int columns, int rows)
        {
            double totalWidth = columns * CellWidth;
            double totalHeight = rows * CellHeight;

            for (int x = 1; x < columns; x++)
            {
                double xPos = x * CellWidth;
                dc.DrawLine(GridPen, new Point(xPos, 0), new Point(xPos, totalHeight));
            }

            for (int y = 1; y < rows; y++)
            {
                double yPos = y * CellHeight;
                dc.DrawLine(GridPen, new Point(0, yPos), new Point(totalWidth, yPos));
            }
        }

        private static void DrawOuterBorder(DrawingContext dc, int columns, int rows)
        {
            double totalWidth = columns * CellWidth;
            double totalHeight = rows * CellHeight;
            dc.DrawRectangle(null, BorderPen, new Rect(0, 0, totalWidth, totalHeight));
        }

        private void DrawSelectionRect(DrawingContext dc, int columns, int rows)
        {
            if (_selectedIndex < 0 || columns <= 0)
                return;

            int x = _selectedIndex % columns;
            int y = _selectedIndex / columns;

            if (y >= rows)
                return;

            dc.DrawRectangle(null, SelectionPen, new Rect(x * CellWidth, y * CellHeight, CellWidth, CellHeight));
        }

        private Color GetColorAtIndex(int index)
        {
            if (index < 0 || index >= _palette.Count)
                return Colors.Transparent;

            var c = _palette[index];
            return Color.FromRgb(c.R, c.G, c.B);
        }

        // Helper methods.

        private void DrawDesignPlaceholder(DrawingContext dc)
        {
            dc.DrawRectangle(CreateFrozenBrush(Color.FromRgb(50, 50, 50)), BorderPen,
                new Rect(0, 0, ActualWidth, ActualHeight));

            var text = new FormattedText("Palette",
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
