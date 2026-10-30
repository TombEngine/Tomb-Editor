#nullable enable

using System;
using System.Globalization;
using System.Windows.Data;

namespace WadTool.Features.Dialogs.StateChangesEditor;

/// <summary>
/// Legacy <c>dgvStateChanges_CellValidating</c> parsing: edited text is parsed as Int16 and
/// text that is not a valid Int16 silently keeps the previous value instead of showing a validation error.
/// </summary>
public sealed class Int16TextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => System.Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text && short.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsed))
            return (int)parsed;

        return Binding.DoNothing;
    }
}
