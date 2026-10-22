using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Rendering;

namespace TombLib.Controls
{
    public enum ObjectRenderingQuality
    {
        High,
        Medium,
        Low,
        Undefined
    }

    public class RenderingPanel : Panel
    {
        public event EventHandler Draw;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RenderingSwapChain SwapChain { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RenderingDevice Device { get; private set; }

        public bool AllowRendering { get; set; } = true;

        public RenderingPanel()
        {
            BorderStyle = BorderStyle.None;
        }

        public virtual void InitializeRendering(RenderingDevice device, bool antialias = false, ObjectRenderingQuality objectQuality = ObjectRenderingQuality.Undefined)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                Device = device;
                SwapChain = device.CreateSwapChain(new RenderingSwapChain.Description
                {
                    Size = new VectorInt2(ClientSize.Width, ClientSize.Height),
                    WindowHandle = Handle,
                    Antialias = antialias
                });
            }
        }

        public Vector2 Delta(Point currentPosition, Point previousMousePosition)
        {
             return new Vector2((currentPosition.X - previousMousePosition.X) / (float)Height,
                                (currentPosition.Y - previousMousePosition.Y) / (float)Height);
        }

        public Vector2 WarpMouseCursor(Point currentPosition, Point previousMousePosition)
        {
            // Use height for X coordinate because the camera FOV per pixel is defined by the height.
            var coordinate = currentPosition;
            var delta = Delta(currentPosition, previousMousePosition);

            if (coordinate.X <= 0)
                Cursor.Position = new Point(Cursor.Position.X + Width - 2, Cursor.Position.Y);
            else if (coordinate.X >= Width - 1)
                Cursor.Position = new Point(Cursor.Position.X - Width + 2, Cursor.Position.Y);

            if (coordinate.Y <= 0)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + Height - 2);
            else if (coordinate.Y >= Height - 1)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - Height + 2);

            if (coordinate.X - previousMousePosition.X >= (float)Width / 2 ||
                coordinate.X - previousMousePosition.X <= -(float)Width / 2)
                delta.X = 0;

            if (coordinate.Y - previousMousePosition.Y >= (float)Height / 2 ||
                coordinate.Y - previousMousePosition.Y <= -(float)Height / 2)
                delta.Y = 0;

            return delta;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (SwapChain != null && SwapChain.RenderException == null && ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                SwapChain.Resize(new VectorInt2(ClientSize.Width, ClientSize.Height));
                SwapChain.Clear(ClearColor);
                SwapChain.Present();
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime || Device == null || SwapChain == null || SwapChain.RenderException != null)
                e.Graphics.Clear(Parent.BackColor);
            // Don't paint the background if being rendered
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime || Device == null || SwapChain == null)
            {
                e.Graphics.DrawString("Rendering: Not Available in form designer!", Font, Brushes.DarkGray, ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                return;
            }

            if (SwapChain.RenderException != null)
            {
                e.Graphics.DrawString("Rendering: Fatal error has occured." + Environment.NewLine + "Save the log file and report this error to the dev team.", Font, Brushes.DarkGray, ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                OnDraw();
                return;
            }

            SwapChain.Clear(ClearColor);
            OnDraw();
            SwapChain.Present();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_PAINT = 0x000F;

            if (m.Msg == WM_PAINT && !AllowRendering)
                return;

            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                SwapChain.Dispose();
        }

        protected virtual void OnDraw()
        {
            Draw?.Invoke(this, EventArgs.Empty);
        }

        protected virtual Vector4 ClearColor { get; } = new Vector4(0.392f, 0.584f, 0.929f, 1.0f); // "Cornflower blue" by default
    }
}
