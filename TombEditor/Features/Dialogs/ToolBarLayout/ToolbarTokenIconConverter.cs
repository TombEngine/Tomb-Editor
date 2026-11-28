#nullable enable

using System;
using System.Globalization;
using System.Windows.Data;
using TombEditor.Features.Toolbar;
using TombLib.Icons;

namespace TombEditor.Features.Dialogs.ToolBarLayout;

/// <summary>Turns a toolbar button token into the icon the button shows on the toolbar.</summary>
internal sealed class ToolbarTokenIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string token && ToolbarButtonRegistry.TryGet(token, out var spec) ? IconSources.Load(spec.Icon) : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}