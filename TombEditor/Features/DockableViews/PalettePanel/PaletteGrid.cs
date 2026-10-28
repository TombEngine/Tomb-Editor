#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.PalettePanel;

/// <summary>
/// Displays and edits the active editor palette.
/// </summary>
public class PaletteGrid : FrameworkElement
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const double CellWidth = 10.0;
    private const double CellHeight = 10.0;
    private const double BorderThickness = 1.0;

    private static readonly ColorC FallbackSelectedColor = new(128, 128, 128);
    private static readonly Pen GridPen = BrushHelpers.CreateFrozenPen(Color.FromArgb(140, 0, 0, 0), BorderThickness);
    private static readonly Pen BorderPen = BrushHelpers.CreateFrozenPen(Colors.Black, BorderThickness);
    private static readonly Pen SelectionPen = BrushHelpers.CreateFrozenPen(Colors.White, BorderThickness);
    private static readonly Brush DesignPlaceholderBrush = BrushHelpers.CreateFrozenBrush(Color.FromRgb(50, 50, 50));

    private readonly Editor? _editor;
    private List<ColorC> _palette = [];
    private List<Brush> _paletteBrushes = [];
    private int _selectedIndex = -1;

    /// <summary>
    /// Gets or sets a value indicating whether palette colors can be edited.
    /// </summary>
    public bool Editable { get; set; } = true;

    /// <summary>
    /// Gets the selected color as a WPF color.
    /// </summary>
    public Color SelectedColor => ToWPFColor(SelectedColorC);

    /// <summary>
    /// Gets the selected color as a TombLib color.
    /// </summary>
    public ColorC SelectedColorC => HasSelectedPaletteColor ? _palette[_selectedIndex] : FallbackSelectedColor;

    /// <summary>
    /// Gets a copy of the displayed palette.
    /// </summary>
    public List<ColorC> Palette => [.. _palette];

    private bool HasSelectedPaletteColor => _selectedIndex >= 0 && _selectedIndex < _palette.Count;
    private int ColumnCount => Math.Max(0, (int)((ActualWidth - (BorderThickness * 2)) / CellWidth));
    private int RowCount => Math.Max(0, (int)((ActualHeight - (BorderThickness * 2)) / CellHeight));

    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteGrid"/> class.
    /// </summary>
    public PaletteGrid()
    {
        ClipToBounds = true;
        Focusable = true;
        FocusVisualStyle = null;
        SnapsToDevicePixels = true;
        UseLayoutRounding = true;

        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        _editor = Editor.Instance;
    }

    /// <summary>
    /// Loads the palette displayed by the grid.
    /// </summary>
    public void LoadPalette(IReadOnlyList<ColorC> palette)
    {
        _palette = [.. palette.Select(NormalizePaletteColor)];
        _paletteBrushes = [.. _palette.Select(CreateBrush)];

        if (_palette.Count == 0)
            _selectedIndex = -1;
        else if (_selectedIndex >= _palette.Count)
            _selectedIndex = _palette.Count - 1;

        PickColor();
        InvalidateVisual();
    }

    /// <summary>
    /// Selects the palette color that matches the selected object, when available.
    /// </summary>
    public void PickColor()
    {
        var editor = _editor;

        if (editor is null || !editor.Configuration.Palette_PickColorFromSelectedObject || editor.SelectedObject is null)
            return;

        if (editor.SelectedObject is not IColorable instance)
            return;

        var normalizedColor = instance.Color / 2.0f;
        var color = new ColorC(
            (byte)(normalizedColor.X * 255.0f),
            (byte)(normalizedColor.Y * 255.0f),
            (byte)(normalizedColor.Z * 255.0f));

        for (int i = 0; i < _palette.Count; i++)
        {
            if (_palette[i] == color)
            {
                _selectedIndex = i;
                editor.LastUsedPaletteColourChange(SelectedColorC);

                InvalidateVisual();
                return;
            }
        }
    }

    /// <summary>
    /// Sets the palette color at the selected cell.
    /// </summary>
    public void SetColorAtSelection(ColorC color)
    {
        if (_selectedIndex < 0)
            return;

        SetPaletteColor(_selectedIndex, color);
        InvalidateVisual();
    }

    // Mouse interaction.

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        var editor = _editor;

        if (editor is null)
            return;

        Focus();

        if (e.ClickCount == 2 && Editable)
        {
            PickColourFromDialog();
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            if (Editable)
                PickColourFromDialog(false);

            e.Handled = true;
            return;
        }

        CaptureMouse();

        if (editor.SelectedObject is PositionBasedObjectInstance selectedObject && selectedObject is IColorable)
            editor.UndoManager.PushObjectPropertyChanged(selectedObject);

        editor.ToggleHiddenSelection(true);
        ChangeColorByMouse(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (IsMouseCaptured)
            ReleaseMouseCapture();
        else
            _editor?.ToggleHiddenSelection(false);
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        _editor?.ToggleHiddenSelection(false);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            ChangeColorByMouse(e.GetPosition(this));
    }

    private void ChangeColorByMouse(Point position)
    {
        var editor = _editor;

        if (editor is null)
            return;

        int columns = ColumnCount;
        int rows = RowCount;

        if (columns <= 0 || rows <= 0)
            return;

        int x = GetCellIndex(position.X, columns, CellWidth);
        int y = GetCellIndex(position.Y, rows, CellHeight);

        _selectedIndex = (y * columns) + x;

        if (HasSelectedPaletteColor)
        {
            ApplySelectedColorToObject(editor);
            editor.LastUsedPaletteColourChange(SelectedColorC);
        }

        InvalidateVisual();
    }

    private void ApplySelectedColorToObject(Editor editor)
    {
        var selectedObject = editor.SelectedObject;

        if (selectedObject is null || !selectedObject.CanBeColored())
            return;

        editor.ToggleHiddenSelection(true);

        if (selectedObject is IColorable instance)
            EditorActions.ApplyObjectColor(instance, SelectedColor.ToFloat3Color() * 2.0f);
    }

    private void PickColourFromDialog(bool onlyFromPalette = true)
    {
        var editor = _editor;

        if (editor is null || !Editable)
            return;

        using var colorDialog = new RealtimeColorDialog(
            editor.Configuration.ColorDialog_Position.X,
            editor.Configuration.ColorDialog_Position.Y,
            null,
            editor.Configuration.UI_ColorScheme);

        var currentColor = SelectedColor;
        colorDialog.Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);

        if (!onlyFromPalette)
        {
            var obj = editor.SelectedObject;

            if (obj is LightInstance light)
                colorDialog.Color = (light.Color * 0.5f).ToWinFormsColor();
            else if (obj is StaticInstance stat)
                colorDialog.Color = (stat.Color * 0.5f).ToWinFormsColor();
            else if (editor.Level.IsTombEngine && obj is MoveableInstance moveable)
                colorDialog.Color = moveable.Color.ToWinFormsColor();
        }

        if (colorDialog.ShowDialog(WinFormsDialogHelper.GetOpenFormOwner()) == System.Windows.Forms.DialogResult.OK)
        {
            var picked = colorDialog.Color;
            SetColorAtSelection(new ColorC(picked.R, picked.G, picked.B));
            editor.Level.Settings.Palette = Palette;
        }

        editor.Configuration.ColorDialog_Position = colorDialog.Position;
    }

    // Rendering.

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (_editor is null)
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
            var gridArea = GetGridArea(columns, rows);
            dc.DrawGridLines(GridPen, gridArea, columns, rows);
            dc.DrawRectangleInside(BorderPen, gridArea);
            DrawSelectionRect(dc, columns, rows);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An exception occurred while drawing the palette grid.");
        }
    }

    private void DrawCells(DrawingContext dc, int columns, int rows)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int index = (y * columns) + x;

                var rect = GetCellRect(x, y);

                dc.DrawRectangle(GetBrushAtIndex(index), null, rect);
            }
        }
    }

    private void DrawSelectionRect(DrawingContext dc, int columns, int rows)
    {
        if (_selectedIndex < 0 || columns <= 0)
            return;

        int x = _selectedIndex % columns;
        int y = _selectedIndex / columns;

        if (y >= rows)
            return;

        var selectionRect = GetCellRect(x, y);

        dc.DrawRectangleInside(SelectionPen, selectionRect);
    }

    private Brush GetBrushAtIndex(int index)
    {
        if (index < 0 || index >= _paletteBrushes.Count)
            return Brushes.Transparent;

        return _paletteBrushes[index];
    }

    // Helper methods.

    private void SetPaletteColor(int index, ColorC color)
    {
        while (_palette.Count < index)
        {
            _palette.Add(FallbackSelectedColor);
            _paletteBrushes.Add(CreateBrush(FallbackSelectedColor));
        }

        if (index == _palette.Count)
        {
            _palette.Add(NormalizePaletteColor(color));
            _paletteBrushes.Add(CreateBrush(color));
            return;
        }

        _palette[index] = NormalizePaletteColor(color);
        _paletteBrushes[index] = CreateBrush(color);
    }

    private static int GetCellIndex(double coordinate, int count, double cellSize)
        => Math.Clamp((int)Math.Floor((coordinate - BorderThickness) / cellSize), 0, count - 1);

    private static Rect GetCellRect(int x, int y)
    {
        return new(
            (x * CellWidth) + BorderThickness,
            (y * CellHeight) + BorderThickness,
            CellWidth + BorderThickness,
            CellHeight + BorderThickness);
    }

    private static Rect GetGridArea(int columns, int rows)
    {
        return new(
            BorderThickness,
            BorderThickness,
            (columns * CellWidth) + BorderThickness,
            (rows * CellHeight) + BorderThickness);
    }

    private static ColorC NormalizePaletteColor(ColorC color) => new(color.R, color.G, color.B);

    private static Brush CreateBrush(ColorC color) => BrushHelpers.CreateFrozenBrush(ToWPFColor(color));

    private static Color ToWPFColor(ColorC color) => Color.FromRgb(color.R, color.G, color.B);

    private void DrawDesignPlaceholder(DrawingContext dc)
    {
        dc.DrawRectangle(DesignPlaceholderBrush, BorderPen,
            new Rect(0.0, 0.0, ActualWidth, ActualHeight));

        dc.DrawCenteredText(
            "Palette",
            new Rect(0.0, 0.0, ActualWidth, ActualHeight),
            new Typeface("Segoe UI"),
            12.0,
            Brushes.Gray,
            VisualTreeHelper.GetDpi(this).PixelsPerDip,
            CultureInfo.InvariantCulture);
    }
}
