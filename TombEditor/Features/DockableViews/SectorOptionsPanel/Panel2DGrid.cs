#nullable enable

using NLog;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib.LevelData;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

/// <summary>
/// Displays the selected room as a compact 2D sector grid.
/// </summary>
public partial class Panel2DGrid : FrameworkElement, IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const double BorderThickness = 1.0;
    private const double MinZoom = 1.0;
    private const double MaxZoom = 12.0;
    private const double MouseWheelZoomBase = 1.15;
    private const double KeyboardZoomBase = 1.2;
    private const double KeyboardPanStep = 48.0;
    private const double DragPanEdgeMargin = 8.0;
    private const double DragPanMaxStep = 4.0;
    private static readonly TimeSpan DragPanInterval = TimeSpan.FromMilliseconds(16.0);

    private static readonly Pen GridPen = BrushHelpers.CreateFrozenPen(Color.FromArgb(140, 0, 0, 0), BorderThickness);
    private static readonly Pen BorderPen = BrushHelpers.CreateFrozenPen(Colors.Black, BorderThickness);
    private static readonly Pen SelectedPortalPen = BrushHelpers.CreateFrozenPen(Colors.YellowGreen, 2.0);
    private static readonly Pen SelectedTriggerPen = BrushHelpers.CreateFrozenPen(Colors.White, 2.0);
    private static readonly Brush DesignPlaceholderBrush = BrushHelpers.CreateFrozenBrush(Color.FromRgb(50, 50, 50));

    private readonly Editor? _editor;
    private readonly DispatcherTimer _dragPanTimer;
    private Room? _room;
    private bool _doSectorSelection;
    private bool _disposed;
    private double _viewOffsetX;
    private double _viewOffsetY;
    private double _viewScale = 1.0;
    private MouseButton? _panButton;
    private bool _panThresholdPending;
    private bool _panWarpPending;
    private CursorWarpResult _panWarpTarget;
    private Point _lastDragPosition;
    private Point _lastPanPosition;
    private ToolTip? _selectionToolTip;

    /// <summary>
    /// Gets or sets the room displayed by the grid.
    /// </summary>
    public Room? Room
    {
        get => _room;
        set
        {
            if (_room == value)
                return;

            _room = value;
            CoerceViewOffset();
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Panel2DGrid"/> class.
    /// </summary>
    public Panel2DGrid()
    {
        ClipToBounds = true;
        Focusable = true;
        FocusVisualStyle = null;
        SnapsToDevicePixels = true;
        UseLayoutRounding = true;

        _dragPanTimer = new DispatcherTimer { Interval = DragPanInterval };
        _dragPanTimer.Tick += DragPanTimerTick;

        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        _editor = Editor.Instance;
        _editor.EditorEventRaised += EditorEventRaised;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        if (_editor is not null)
            _editor.EditorEventRaised -= EditorEventRaised;

        _dragPanTimer.Stop();
        _dragPanTimer.Tick -= DragPanTimerTick;
        CloseSelectionToolTip();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void EditorEventRaised(IEditorEvent obj)
    {
        if (_disposed)
            return;

        if (obj is SectorColoringManager.ChangeSectorColoringInfoEvent
            or Editor.GameVersionChangedEvent
            or Editor.SelectedSectorsChangedEvent
            or Editor.RoomSectorPropertiesChangedEvent
            or Editor.ObjectChangedEvent
            or Editor.RoomGeometryChangedEvent
            or Editor.ConfigurationChangedEvent
            || (obj is Editor.SelectedObjectChangedEvent e && IsObjectChangeRelevant(e)))
        {
            InvalidateVisual();
        }

        if (obj is Editor.ActionChangedEvent actionChanged)
        {
            bool isCameraRelocate = actionChanged.Current is EditorActionRelocateCamera;
            UpdateCursor(isCameraRelocate);
        }
    }

    private static bool IsObjectChangeRelevant(Editor.SelectedObjectChangedEvent e)
        => e.Previous is SectorBasedObjectInstance || e.Current is SectorBasedObjectInstance;
}
