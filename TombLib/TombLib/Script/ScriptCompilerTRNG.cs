using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Script
{
    public class ScriptCompilerTRNG : IScriptCompiler
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int BM_CLICK = 0x00F5;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private string _path;

        public ScriptCompilerTRNG(string path)
        {
            _path = path;
        }

        private static IntPtr FindControlByName(IntPtr parentWindow, string name)
        {
            IntPtr childHandle = FindWindowEx(parentWindow, IntPtr.Zero, null, null);

            while (childHandle != IntPtr.Zero)
            {
                StringBuilder bld = new StringBuilder();
                GetWindowText(childHandle, bld, 2048);

                if (bld.ToString().Equals(name))
                    return childHandle;

                IntPtr found = FindControlByName(childHandle, name);
                if (found != IntPtr.Zero)
                    return found;

                childHandle = FindWindowEx(parentWindow, childHandle, null, null);
            }

            return IntPtr.Zero;
        }

        private static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        private static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return GetWindowText(wnd).Contains(titleText);
            });
        }

        private static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return string.Empty;
        }

        public bool CompileScripts(string srcPath, string dstPath)
        {
            IEnumerable<IntPtr> windows = FindWindowsWithText("NG Center 1.");

            if (windows.Count() == 0)
            {
                if (!File.Exists(_path))
                    return false;

                Process process = Process.Start(_path);
                process.WaitForInputIdle();

                windows = FindWindowsWithText("NG Center 1.");
            }

            if (windows.Count() != 0)
            {
                try
                {
                    IntPtr parentWindow = windows.First();
                    IntPtr child = FindControlByName(parentWindow, "Build");
                    SendMessage(child, BM_CLICK, IntPtr.Zero, IntPtr.Zero);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
