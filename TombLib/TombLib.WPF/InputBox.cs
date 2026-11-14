#nullable enable

using System.Windows;
using TombLib.Services;
using TombLib.WPF.Features.AnimatedTextures;

namespace TombLib.WPF;

/// <summary>
/// WPF replacement for the shared <c>FormInputBox</c>: shows a modal text prompt and returns
/// the entered string, or <see langword="null"/> if the user cancelled.
/// </summary>
public static class InputBox
{
    /// <param name="owner">A WPF <see cref="Window"/>, a native <see cref="IWindowOwner"/>, or null.</param>
    public static string? Show(object? owner, string title, string message, string startValue = "")
    {
        var viewModel = new InputDialogViewModel(title, message, startValue);
        var dialog = new InputDialog { DataContext = viewModel };

        if (owner is Window window)
            dialog.Owner = window;
        else if (owner is IWindowOwner nativeOwner)
            dialog.SetOwner(nativeOwner);

        dialog.ShowDialog();
        return viewModel.DialogResult == true ? viewModel.Value : null;
    }
}
