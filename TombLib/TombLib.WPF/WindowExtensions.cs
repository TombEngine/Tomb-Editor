using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using TombLib.Services;
using TombLib.WPF.Dialogs;

namespace TombLib.WPF;

public static class WindowExtensions
{
    private static readonly DependencyProperty ModalBindingProperty = DependencyProperty.RegisterAttached(
        "ModalBinding", typeof(ModalDialogBinding), typeof(WindowExtensions));

    private sealed class Win32WindowWrapper : IWindowOwner
    {
        public IntPtr Handle { get; }

        public Win32WindowWrapper(IntPtr handle)
        {
            Handle = handle;
        }
    }

    /// <summary>Assigns native ownership to a WPF window.</summary>
    public static WindowInteropHelper SetOwner(this Window window, IWindowOwner owner)
        => new(window) { Owner = owner.Handle };

    /// <summary>Gets the native owner contract for a WPF window, creating its handle if necessary.</summary>
    public static IWindowOwner GetWin32Window(this Window window)
    {
        var helper = new WindowInteropHelper(window);

        if (helper.Handle == IntPtr.Zero)
            helper.EnsureHandle();

        return new Win32WindowWrapper(helper.Handle);
    }

    /// <summary>Validates a native window owner before invoking an editor command.</summary>
    public static bool IsValid(this IWindowOwner? owner) => owner is not null && owner.Handle != IntPtr.Zero && IsWindow(owner.Handle);

    /// <summary>Connects modal result notifications once and releases them when the window closes.</summary>
    public static void HookModalAutoClose(this Window window)
    {
        if (window.GetValue(ModalBindingProperty) is not ModalDialogBinding)
            window.SetValue(ModalBindingProperty, new ModalDialogBinding(window));
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr window);
}
