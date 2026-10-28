#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TombLib.Utils;

public static class SingleInstanceManagement
{
    public const int WM_COPYDATA   = 0x004A;
    public const int WM_SHOWWINDOW = 0x7515;

    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, ref NativeCopyData lParam);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

    public static bool Send(Process process, List<string> supportedExtensions, string filename)
    {
        Process[] existingProcesses = Process.GetProcessesByName(process.ProcessName);

        if (existingProcesses.Length <= 1 || !File.Exists(filename))
            return false;

        var extension = Path.GetExtension(filename);
        if (extension is null)
            return false;

        foreach (var ext in supportedExtensions)
        {
            if (extension.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                break;
            return false;
        }

        foreach (Process p in existingProcesses)
        {
            if (p.Id != process.Id)
            {
                NativeCopyData cds;
                cds.Data = IntPtr.Zero;
                cds.Pointer = Marshal.StringToHGlobalAnsi(filename);
                cds.ByteCount = filename.Length;

                try
                {
                    SendMessage(p.MainWindowHandle, WM_COPYDATA, IntPtr.Zero, ref cds);
                }
                finally
                {
                    Marshal.FreeHGlobal(cds.Pointer);
                }

                return true; // Send only to first opened one
            }
        }
        return false; // Only current one is running
    }

    public static void Bump(Process process)
    {
        Process[] existingProcesses = Process.GetProcessesByName(process.ProcessName);

        if (existingProcesses.Length <= 1)
            return;

        foreach (Process p in existingProcesses)
            if (p.Id != process.Id) // Send only to first opened one
                SendMessage(p.MainWindowHandle, WM_SHOWWINDOW, IntPtr.Zero, IntPtr.Zero);
    }

    /// <summary>Reads a file name from a native copy-data message.</summary>
    public static string? Catch(int messageId, IntPtr data)
    {
        if (messageId != WM_COPYDATA || data == IntPtr.Zero)
            return null;

        var payload = Marshal.PtrToStructure<NativeCopyData>(data);
        string? fileName = Marshal.PtrToStringAnsi(payload.Pointer, payload.ByteCount);
        return fileName is not null && File.Exists(fileName) ? fileName : null;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCopyData
    {
        public IntPtr Data;
        public int ByteCount;
        public IntPtr Pointer;
    }
}
