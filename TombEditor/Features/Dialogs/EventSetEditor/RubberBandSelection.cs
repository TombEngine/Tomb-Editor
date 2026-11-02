#nullable enable

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
        private Point _start;

        public RubberBandSelection(Canvas canvas, Rectangle visual)
        {
            _canvas = canvas;
            _visual = visual;
        }

        public bool IsActive { get; private set; }

        /// <summary>Starts a band: the selection is cleared and then always equals the nodes inside the band.</summary>
        public void Begin(NodeEditorViewModel editor, Point start)
        {
            _start = start;
            IsActive = true;

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

            editor.SelectInArea(area);
        }

        public void End()
        {
            IsActive = false;
            _visual.Visibility = Visibility.Collapsed;
            _canvas.ReleaseMouseCapture();
        }
    }
}
