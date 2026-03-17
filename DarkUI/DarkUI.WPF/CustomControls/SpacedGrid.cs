using System;
using System.Windows;
using System.Windows.Controls;

namespace DarkUI.WPF.CustomControls;

/// <summary>
/// A Grid that supports RowSpacing and ColumnSpacing between rows and columns
/// without injecting dummy rows or columns.
/// </summary>
public class SpacedGrid : Grid
{
	public static readonly DependencyProperty RowSpacingProperty =
		DependencyProperty.Register(
			nameof(RowSpacing),
			typeof(double),
			typeof(SpacedGrid),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

	public static readonly DependencyProperty ColumnSpacingProperty =
		DependencyProperty.Register(
			nameof(ColumnSpacing),
			typeof(double),
			typeof(SpacedGrid),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

	/// <summary>
	/// Gets or sets the vertical spacing between rows.
	/// </summary>
	public double RowSpacing
	{
		get => (double)GetValue(RowSpacingProperty);
		set => SetValue(RowSpacingProperty, value);
	}

	/// <summary>
	/// Gets or sets the horizontal spacing between columns.
	/// </summary>
	public double ColumnSpacing
	{
		get => (double)GetValue(ColumnSpacingProperty);
		set => SetValue(ColumnSpacingProperty, value);
	}

	protected override Size MeasureOverride(Size constraint)
	{
		double totalRowSpacing = GetTotalRowSpacing();
		double totalColSpacing = GetTotalColumnSpacing();

		var reducedConstraint = new Size(
			double.IsInfinity(constraint.Width) ? constraint.Width : Math.Max(0, constraint.Width - totalColSpacing),
			double.IsInfinity(constraint.Height) ? constraint.Height : Math.Max(0, constraint.Height - totalRowSpacing));

		var baseSize = base.MeasureOverride(reducedConstraint);

		return new Size(baseSize.Width + totalColSpacing, baseSize.Height + totalRowSpacing);
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		double rowSpacing = RowSpacing;
		double colSpacing = ColumnSpacing;
		double totalRowSpacing = GetTotalRowSpacing();
		double totalColSpacing = GetTotalColumnSpacing();

		// Let the base Grid arrange children within a reduced area.
		var reducedSize = new Size(
			Math.Max(0, arrangeSize.Width - totalColSpacing),
			Math.Max(0, arrangeSize.Height - totalRowSpacing));

		base.ArrangeOverride(reducedSize);

		int rowCount = Math.Max(1, RowDefinitions.Count);
		int colCount = Math.Max(1, ColumnDefinitions.Count);

		// Build cumulative row offsets from ActualHeight after base layout.
		double[] rowStarts = new double[rowCount + 1];

		for (int i = 0; i < rowCount; i++)
		{
			double height = RowDefinitions.Count > 0 ? RowDefinitions[i].ActualHeight : reducedSize.Height;
			rowStarts[i + 1] = rowStarts[i] + height;
		}

		// Build cumulative column offsets from ActualWidth after base layout.
		double[] colStarts = new double[colCount + 1];

		for (int i = 0; i < colCount; i++)
		{
			double width = ColumnDefinitions.Count > 0 ? ColumnDefinitions[i].ActualWidth : reducedSize.Width;
			colStarts[i + 1] = colStarts[i] + width;
		}

		// Re-arrange each child, injecting spacing offsets.
		foreach (UIElement child in InternalChildren)
		{
			if (child == null || child.Visibility == Visibility.Collapsed)
				continue;

			int row = Math.Min(GetRow(child), rowCount - 1);
			int col = Math.Min(GetColumn(child), colCount - 1);
			int rowSpan = Math.Min(GetRowSpan(child), rowCount - row);
			int colSpan = Math.Min(GetColumnSpan(child), colCount - col);

			// Position = base offset + cumulative spacing for preceding rows/columns.
			double x = colStarts[col] + col * colSpacing;
			double y = rowStarts[row] + row * rowSpacing;

			// Size = base span size + spacing gaps within the span.
			double width = colStarts[col + colSpan] - colStarts[col] + (colSpan - 1) * colSpacing;
			double height = rowStarts[row + rowSpan] - rowStarts[row] + (rowSpan - 1) * rowSpacing;

			child.Arrange(new Rect(x, y, Math.Max(0, width), Math.Max(0, height)));
		}

		return arrangeSize;
	}

	private double GetTotalRowSpacing()
	{
		int gaps = Math.Max(0, (RowDefinitions.Count > 0 ? RowDefinitions.Count : 1) - 1);
		return gaps * RowSpacing;
	}

	private double GetTotalColumnSpacing()
	{
		int gaps = Math.Max(0, (ColumnDefinitions.Count > 0 ? ColumnDefinitions.Count : 1) - 1);
		return gaps * ColumnSpacing;
	}
}
