#nullable enable

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>Rubber-band (marquee) selection on the node canvas; the rectangle element is owned by the view.</summary>
    public sealed class RubberBandSelection
    {
        private readonly Canvas _canvas;
        private readonly Rectangle _visual;
        private IReadOnlyCollection<NodeViewModel> _baseline = [];
        private Point _start;

        public RubberBandSelection(Canvas canvas, Rectangle visual)
        {
            _canvas = canvas;
            _visual = visual;
        }

        public bool IsActive { get; private set; }

        /// <summary>Starts a band; with <paramref name="additive"/> the current selection is kept as the baseline.</summary>
        public void Begin(NodeEditorViewModel editor, Point start, bool additive)
        {
            _start = start;
            _baseline = additive ? [.. editor.SelectedNodes] : [];
            IsActive = true;

            if (!additive)
                editor.ClearSelection();

            Update(editor, start);
            _visual.Visibility = Visibility.Visible;
            _canvas.CaptureMouse();
        }

        public void Update(NodeEditorViewModel editor, Point current)
        {
            var area = new Rect(_start, current);

            Canvas.SetLeft(_visual, area.X);
            Canvas.SetTop(_visual, area.Y);
            _visual.Width = area.Width;
            _visual.Height = area.Height;

            editor.SelectInArea(area, _baseline);
        }

        public void End()
        {
            IsActive = false;
            _baseline = [];
            _visual.Visibility = Visibility.Collapsed;
            _canvas.ReleaseMouseCapture();
        }
    }
}
