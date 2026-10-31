#nullable enable

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>Colors a node link: orange for an <c>Else</c> branch, light gray for a <c>Next</c> branch.</summary>
    public sealed class LinkBrushConverter : IValueConverter
    {
        private static readonly Brush ElseBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xA0, 0x30));
        private static readonly Brush NextBrush = new SolidColorBrush(Color.FromRgb(0xC0, 0xC0, 0xC0));

        static LinkBrushConverter()
        {
            ElseBrush.Freeze();
            NextBrush.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? ElseBrush : NextBrush;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
