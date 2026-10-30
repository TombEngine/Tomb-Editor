#nullable enable

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using TombEditor.Features.Toolbar;
using TombLib.Input;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Input;

namespace TombEditor;

/// <summary>Window shell behavior: file drops, second-instance messages and activation.</summary>
public partial class MainWindow
{
    private const int WmClipboardUpdate = 0x031D;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    private void InitializeShell()
    {
        AllowDrop = true;
        SourceInitialized += (_, _) =>
        {
            var handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowMessage);
            AddClipboardFormatListener(handle);
        };
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _editor.Focus();
    }

    protected override void OnDragOver(System.Windows.DragEventArgs e)
    {
        base.OnDragOver(e);
        e.Effects = EditorActions.DragDropFileSupported(CreateDragArgs(e)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    protected override void OnDrop(System.Windows.DragEventArgs e)
    {
        base.OnDrop(e);
        EditorActions.DragDropCommonFiles(CreateDragArgs(e), this.GetWin32Window());
        e.Handled = true;
    }

    private static PointerDragEventArgs CreateDragArgs(System.Windows.DragEventArgs e)
    {
        var data = new WpfDataTransfer();
        foreach (var format in e.Data.GetFormats())
        {
            var value = e.Data.GetData(format);
            if (value is not null)
                data.SetData(format, value);
        }

        return new PointerDragEventArgs(data, (int)e.KeyStates, 0, 0, (TransferEffects)e.AllowedEffects, (TransferEffects)e.Effects);
    }

    private IntPtr WindowMessage(IntPtr handle, int messageId, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (messageId == WmClipboardUpdate)
        {
            _menuState.UpdatePaste();
            ToolbarEnableRules.NotifyClipboardChanged();
            return IntPtr.Zero;
        }

        if (messageId != SingleInstanceManagement.WM_COPYDATA && messageId != SingleInstanceManagement.WM_SHOWWINDOW)
            return IntPtr.Zero;

        handled = true;

        if (messageId == SingleInstanceManagement.WM_COPYDATA)
        {
            var fileName = SingleInstanceManagement.Catch(messageId, lParam);
            if (fileName is not null && Path.GetExtension(fileName) == ".prj2")
            {
                RestoreWindowState();
                Dispatcher.BeginInvoke(new Action(() => OpenLevelFromOtherInstance(fileName)));
            }
        }
        else
        {
            RestoreWindowState();
        }

        return IntPtr.Zero;
    }

    private void OpenLevelFromOtherInstance(string fileName)
    {
        if (_editor.Level.Settings.LevelFilePath == fileName)
            return;

        // Open the file only while the main window is the only window, otherwise close the others first.
        foreach (Window window in Application.Current.Windows)
        {
            if (!ReferenceEquals(window, this) && window.IsLoaded)
                window.Close();
        }

        EditorActions.OpenLevel(this.GetWin32Window(), fileName);
    }

    private void RestoreWindowState()
    {
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;

        Activate();
    }
}