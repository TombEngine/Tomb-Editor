#nullable enable

using DarkUI.WPF.CustomControls;
using DarkUI.WPF.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TombLib.WPF.Input;

/// <summary>Identifies controls that own text editing and their keyboard shortcuts.</summary>
public static class KeyboardInput
{
    /// <summary>Gets whether the event source belongs to an editable text or numeric control.</summary>
    public static bool IsEditingText(DependencyObject? source)
    {
        while (source is FrameworkContentElement content)
            source = content.Parent;

        if (source is null)
            return false;

        return source.FindVisualAncestorOrSelf<TextBoxBase>() is not null ||
               source.FindVisualAncestorOrSelf<PasswordBox>() is not null ||
               source.FindVisualAncestorOrSelf<NumericUpDown>() is not null ||
               source.FindVisualAncestorOrSelf<ComboBox>() is { IsEditable: true };
    }
}
