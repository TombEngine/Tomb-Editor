#nullable enable

using System;
using System.Globalization;
using System.Windows.Data;
using TombLib.Utils;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>Displays an <c>EventType</c> enum value as a spaced, human-readable name.</summary>
    public sealed class EventTypeDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString()?.SplitCamelcase() ?? string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
