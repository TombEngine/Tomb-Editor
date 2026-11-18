#nullable enable

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// Lays out a node's function selector and arguments like the previous editor: a flow of fixed-height rows where
    /// each argument takes a percentage of its row (<see cref="ArgumentViewModel.FieldWidth"/>), a new row starts at
    /// every argument flagged with <see cref="ArgumentViewModel.NewLine"/> and the function selector (first child)
    /// shares the first row, taking whatever the first-row arguments leave over.
    /// </summary>
    public sealed class NodeArgumentPanel : Panel
    {
        public const double RowHeight = 24.0;
        public const double Spacing = 8.0;

        private const double MinimumFieldWidth = 24.0;
        private const double FallbackWidth = 400.0;

        protected override Size MeasureOverride(Size availableSize)
        {
            double width = double.IsInfinity(availableSize.Width) ? FallbackWidth : availableSize.Width;
            var rows = BuildRows();

            foreach (var row in rows)
            {
                var widths = GetWidths(row, width);
                for (int i = 0; i < row.Count; i++)
                    row[i].Measure(new Size(widths[i], RowHeight));
            }

            return new Size(width, GetHeight(rows.Count));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var rows = BuildRows();
            double y = 0.0;

            foreach (var row in rows)
            {
                var widths = GetWidths(row, finalSize.Width);
                double x = 0.0;

                for (int i = 0; i < row.Count; i++)
                {
                    row[i].Arrange(new Rect(x, y, widths[i], RowHeight));
                    x += widths[i] + Spacing;
                }

                y += RowHeight + Spacing;
            }

            return new Size(finalSize.Width, GetHeight(rows.Count));
        }

        private static double GetHeight(int rowCount) => rowCount == 0 ? 0.0 : rowCount * RowHeight + (rowCount - 1) * Spacing;

        private static ArgumentViewModel? GetArgument(UIElement child) => (child as ContentPresenter)?.Content as ArgumentViewModel;

        private List<List<UIElement>> BuildRows()
        {
            var rows = new List<List<UIElement>>();

            foreach (UIElement child in InternalChildren)
            {
                // The first child is the function selector and always opens the first row.
                if (rows.Count == 0 || GetArgument(child) is { NewLine: true })
                    rows.Add([]);

                rows[^1].Add(child);
            }

            return rows;
        }

        // Arguments take their percentage of the row without spacings; the function selector gets the rest of the first row.
        private static double[] GetWidths(List<UIElement> row, double totalWidth)
        {
            var widths = new double[row.Count];
            double workWidth = Math.Max(0.0, totalWidth - (row.Count - 1) * Spacing);
            double used = 0.0;

            for (int i = 0; i < row.Count; i++)
            {
                if (GetArgument(row[i]) is not { } argument)
                    continue;

                widths[i] = Math.Max(MinimumFieldWidth, Math.Round(workWidth * argument.FieldWidth / 100.0));
                used += widths[i];
            }

            for (int i = 0; i < row.Count; i++)
            {
                if (GetArgument(row[i]) is null)
                    widths[i] = Math.Max(MinimumFieldWidth, workWidth - used);
            }

            return widths;
        }
    }
}
