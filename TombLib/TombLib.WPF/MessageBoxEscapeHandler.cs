#nullable enable

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace TombLib.WPF;

/// <summary>
/// Makes Escape answer a themed message box with its cancelling button, like the legacy DarkUI message box did:
/// Cancel when there is one, otherwise Ignore, otherwise No.
/// </summary>
internal static class MessageBoxEscapeHandler
{
    private const string MessageBoxTypeName = "CustomMessageBox.WPF.CMessageBox";

    private static readonly string[] CancelCaptions = ["Cancel", "Ignore", "No"];

    private static bool _installed;

    /// <summary>Registers the Escape handler for all message boxes of the application; safe to call more than once.</summary>
    public static void Install()
    {
        if (_installed)
            return;

        _installed = true;
        EventManager.RegisterClassHandler(typeof(Window), Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown));
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || sender is not Window window || window.GetType().FullName != MessageBoxTypeName)
            return;

        var buttons = window.FindDescendants<ButtonBase>().Where(button => button.IsEnabled && button.IsVisible).ToList();

        foreach (var caption in CancelCaptions)
        {
            var button = buttons.FirstOrDefault(candidate => string.Equals(candidate.Content as string, caption, StringComparison.OrdinalIgnoreCase));

            if (button is null)
                continue;

            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            e.Handled = true;
            return;
        }
    }
}