using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace TombLib.Utils
{
    public static class WinFormsUtils
    {
        public static List<Keys> DirectionalCameraKeys = new List<Keys>
        {
            Keys.Up,
            Keys.Down,
            Keys.Left,
            Keys.Right,
            Keys.PageDown,
            Keys.PageUp
        };

        public static Color ToWinFormsColor(this Vector3 color) => new Vector4(color, 255.0f).ToWinFormsColor();

        public static Vector3 ToFloat3Color(this Color color)
        {
            if (color.R == Color.Gray.R && color.G == Color.Gray.G && color.B == Color.Gray.B)
                return new Vector3(0.5f);

            if (color.R == Color.White.R && color.G == Color.White.G && color.B == Color.White.B)
                return Vector3.One;

            return new Vector3(color.R, color.G, color.B) / 255.0f;
        }

        public static Vector4 ToFloat4Color(this Color color)
        {
            if (color.R == Color.Gray.R && color.G == Color.Gray.G && color.B == Color.Gray.B)
                return new Vector4(0.5f);

            if (color.R == Color.White.R && color.G == Color.White.G && color.B == Color.White.B)
                return Vector4.One;

            return new Vector4(color.R, color.G, color.B, color.A) / 255.0f;
        }

        public static Color ToWinFormsColor(this Vector4 color, float? alpha = null)
        {
            return Color.FromArgb(
                    (int)Math.Max(0, Math.Min(255, Math.Round((alpha.HasValue ? MathC.Clamp(alpha.Value, 0.0, 1.0) : color.W) * 255.0f))),
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.X * 255.0f))),
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.Y * 255.0f))),
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.Z * 255.0f))));
        }

        public static Color ToWinFormsColor(this Vector3 color, float alpha)
        {
            return ToWinFormsColor(new Vector4(color.X, color.Y, color.Z, alpha));
        }

        public static Color MixWith(this Color firstColor, Color secondColor, double mixFactor)
        {
            if (mixFactor > 1)
                mixFactor = 1;
            if (!(mixFactor >= 0))
                mixFactor = 0;
            return Color.FromArgb(
                (int)Math.Round(firstColor.A * (1 - mixFactor) + secondColor.A * mixFactor),
                (int)Math.Round(firstColor.R * (1 - mixFactor) + secondColor.R * mixFactor),
                (int)Math.Round(firstColor.G * (1 - mixFactor) + secondColor.G * mixFactor),
                (int)Math.Round(firstColor.B * (1 - mixFactor) + secondColor.B * mixFactor));
        }

        public static bool IsGrayscale(this Color color) => color.R == color.G && color.G == color.B;

        public static void DrawRectangle(this System.Drawing.Graphics g, Pen pen, RectangleF rectangle)
        {
            g.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static Rectangle ToRectangle(this RectangleF area, float margin, Rectangle maxArea)
        {
            double minX = Math.Floor(Math.Min(area.Left, area.Right) - margin);
            double minY = Math.Floor(Math.Min(area.Top, area.Bottom) - margin);
            double maxX = Math.Ceiling(Math.Max(area.Left, area.Right) + margin);
            double maxY = Math.Ceiling(Math.Max(area.Top, area.Bottom) + margin);
            minX = Math.Max(maxArea.X, Math.Min(maxArea.Right, minX));
            minY = Math.Max(maxArea.Y, Math.Min(maxArea.Bottom, minY));
            maxX = Math.Max(minX, Math.Min(maxArea.Right, maxX));
            maxY = Math.Max(minY, Math.Min(maxArea.Bottom, maxY));
            return Rectangle.FromLTRB((int)minX, (int)minY, (int)maxX, (int)maxY);
        }

        public static Rectangle ToRectangle(this RectangleF area, float margin)
        {
            return area.ToRectangle(margin, Rectangle.FromLTRB(short.MinValue, short.MinValue, short.MaxValue, short.MaxValue));
        }

        public static void InvalidateC(this Control control, RectangleF area, float margin)
        {
            control.Invalidate(area.ToRectangle(margin, control.ClientRectangle));
        }

        public static void InvalidateC(this Control control, RectangleF area, float margin, bool invalidateChildren)
        {
            control.Invalidate(area.ToRectangle(margin, control.ClientRectangle), invalidateChildren);
        }

        public static IEnumerable<T> BoolCombine<T>(IEnumerable<T> oldObjects, IEnumerable<T> newObjects, Keys modifierKeys)
        {
            bool control = modifierKeys.HasFlag(Keys.Control);
            bool shift = modifierKeys.HasFlag(Keys.Shift);

            if (control && shift)
                return oldObjects.Except(newObjects).ToList(); // Difference
            else if (control)
                return oldObjects.Union(newObjects).Except(oldObjects.Intersect(newObjects)).ToList(); // Either-or
            else if (shift)
                return oldObjects.Union(newObjects); // Union
            else
                return newObjects;
        }

        public static IEnumerable<Control> AllSubControls(Control control) => Enumerable.Repeat(control, 1).Union(control.Controls.OfType<Control>().SelectMany(AllSubControls));
        public static Control GetFocusedControl(ContainerControl control) => (control.ActiveControl is ContainerControl ? GetFocusedControl((ContainerControl)control.ActiveControl) : control.ActiveControl);

        public static bool CurrentControlSupportsInput(Form form, Keys keyData)
        {
            var activeControlType = GetFocusedControl(form)?.GetType().Name;

            if ((keyData.HasFlag(Keys.Control | Keys.A) ||
                 keyData.HasFlag(Keys.Control | Keys.X) ||
                 keyData.HasFlag(Keys.Control | Keys.C) ||
                 keyData.HasFlag(Keys.Control | Keys.V) ||
                (!keyData.HasFlag(Keys.Control)) && !keyData.HasFlag(Keys.Alt)) &&
                (activeControlType == "DarkTextBox" ||
                 activeControlType == "DarkAutocompleteTextBox" ||
                 activeControlType == "DarkComboBox" ||
                 activeControlType == "DarkListBox" ||
                 activeControlType == "UpDownEdit" ||
                 activeControlType == "ToolStripTextBoxControl"))
                return true;
            else
                return false;
        }

        public static void SetActualSize(this Form form, Size? size = null)
        {
            if (!size.HasValue) size = form.ClientSize;
            form.MinimumSize = size.Value + (form.Size - form.ClientSize - (SystemInformation.Border3DSize + SystemInformation.Border3DSize));
            form.Size = form.MinimumSize;
        }
        public static void SetActualSize(this Form form, int width, int height) => SetActualSize(form, new Size(width, height));

        public static void LockHeight(this Form form) => form.MaximumSize = new Size(int.MaxValue, form.Size.Height);
        public static void LockWidth(this Form form) => form.MaximumSize = new Size(form.Size.Width, int.MaxValue);

        public static IntPtr TryGetMainWindowHandle()
        {
            if (Application.OpenForms.Count == 0)
                return IntPtr.Zero;

            var form = Application.OpenForms[0];
            if (form == null || form.IsDisposed || !form.IsHandleCreated)
                return IntPtr.Zero;

            return form.Handle;
        }
    }
}
