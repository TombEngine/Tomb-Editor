#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TombLib;

namespace TombEditor.WPF
{
	public static class WPFUtils
	{
		public static System.Windows.Forms.IWin32Window? GetWin32WindowFromCaller(INotifyPropertyChanged caller)
			=> System.Windows.Forms.Application.OpenForms.Count > 0 ? System.Windows.Forms.Application.OpenForms[0] : null;

		public static Color ToWPFColor(this Vector3 color) => Color.FromRgb((byte)(color.X * 255.0f), (byte)(color.Y * 255.0f), (byte)(color.Z * 255.0f));
		public static Color ToWPFColor(this Vector4 color) => Color.FromArgb((byte)(color.W * 255.0f), (byte)(color.X * 255.0f), (byte)(color.Y * 255.0f), (byte)(color.Z * 255.0f));

		public static Brush ToWPFBrush(this Vector3 color) => new Vector4(color, 255.0f).ToWPFBrush();
		public static Vector3 ToFloat3Color(this Color color) => new Vector3(color.R, color.G, color.B) / 255.0f;
		public static Vector4 ToFloat4Color(this Color color) => new Vector4(color.R, color.G, color.B, color.A) / 255.0f;

		public static Brush ToWPFBrush(this Vector4 color, float? alpha = null)
		{
			return new SolidColorBrush(Color.FromArgb(
				(byte)Math.Max(0, Math.Min(255, Math.Round((alpha.HasValue ? MathC.Clamp(alpha.Value, 0.0, 1.0) : color.W) * 255.0f))),
				(byte)Math.Max(0, Math.Min(255, Math.Round(color.X * 255.0f))),
				(byte)Math.Max(0, Math.Min(255, Math.Round(color.Y * 255.0f))),
				(byte)Math.Max(0, Math.Min(255, Math.Round(color.Z * 255.0f)))));
		}

		public static Brush ToWPFBrush(this Vector3 color, float alpha)
		{
			return ToWPFBrush(new Vector4(color.X, color.Y, color.Z, alpha));
		}

		public static float GetBrightness(this Brush b)
		{
			if (b is SolidColorBrush scb)
			{
				Color c = scb.Color;
				return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B).GetBrightness();
			}
			else
				return 1.0f;
		}

		public static Pen CreateFrozenPen(Color color, double thickness)
		{
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			var pen = new Pen(brush, thickness);
			pen.Freeze();
			return pen;
		}

		public static SolidColorBrush CreateFrozenBrush(Color color)
		{
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}

		public static IEnumerable<Control> AllSubControls(DependencyObject depObj)
		{
			if (depObj == null)
				yield return (Control)Enumerable.Empty<Control>();

			foreach (var child in LogicalTreeHelper.GetChildren(depObj))
			{
				if (child is DependencyObject dpo)
				{
					if (child is Control t)
						yield return t;

					foreach (Control childOfChild in AllSubControls(dpo))
						yield return childOfChild;
				}
			}
		}
	}
}
