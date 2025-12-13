using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor
{
    public static class TombLauncher
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void Launch(LevelSettings settings, IWin32Window owner)
        {
            string executablePath = settings.MakeAbsolute(settings.GameExecutableFilePath);
            string levelPath = settings.MakeAbsolute(settings.GameLevelFilePath);

            // Try to launch the game
            try
            {
                var arguments = string.Empty;
                if (settings.GameVersion == TRVersion.Game.TombEngine)
                {
                    string lastDir = new DirectoryInfo(settings.GameLevelFilePath).Parent.Name;
                    arguments = "/debug /level " + lastDir + "\\" + Path.GetFileName(settings.GameLevelFilePath) + " /hash " + Math.Abs(Environment.MachineName.GetHashCode()).ToString();
                }

                var info = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(executablePath),
                    FileName = executablePath,
                    Arguments = arguments,
                    UseShellExecute = true
                };

                // Start the game (i.e. "tomb4.exe")
                Process process = Process.Start(info);
                try
                {
                    // Seperate thread to wait for the options dialog to appear
                    // so it can be suppressed subsequently by sending WM_CLOSE.
                    if (settings.GameVersion.Native() == TRVersion.Game.TR4 &&
                        settings.GameEnableQuickStartFeature && IsWindows)
                    {
                        Process process2 = process;
                        Thread thread = new Thread(() =>
                        {
                            try
                            {
                                Tomb4ConvinienceImprovements.Do(process2, info.WorkingDirectory, levelPath, settings.GameVersion == TRVersion.Game.TRNG);
                            }
                            catch (Exception exc)
                            {
                                logger.Error(exc, "The 'Tomb4ConvinienceImprovements' thread caused issues.");
                            }
                            finally
                            {
                                process2?.Dispose();
                            }
                        });

                        thread.IsBackground = true;
                        thread.Priority = ThreadPriority.BelowNormal;
                        thread.Start();
                        process = null;
                    }
                }
                finally
                {
                    process?.Dispose();
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Trying to launch the game  '" + executablePath + "'.");

                // Show message
                string message = "\nGo to Tools -> Level Settings -> Game Paths to set a valid executable path.";
                if (PathC.IsFileNotFoundException(exc) || !File.Exists(executablePath))
                    message = "Unable to find '" + executablePath + "'. " + message;
                else
                    message = "Unable to start '" + executablePath + "' because a " + exc.GetType().Name + " occurred (" + exc.Message + "). " + message;
                Editor.Instance.SendMessage(message, PopupType.Error);
            }
        }

        private static bool IsWindows
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private static class Tomb4ConvinienceImprovements
        {
            private delegate int EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processID);
            [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int EnumWindows(EnumWindowsProc proc, IntPtr lParam);
            [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
            [DllImport("user32.dll")]
            private static extern int GetWindowRect(IntPtr hWnd, out RECT lpRect);
            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int FindWindow(string className, string windowText);
            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int ShowWindowAsync(int hwnd, int command);
            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public readonly int Left;
                public readonly int Top;
                public readonly int Right;
                public readonly int Bottom;
            }
            private const uint WM_CLOSE = 0x0010;
            private const int SW_HIDE = 0;
            private const int SW_SHOW = 1;

            private const int maxWaitMilliseconds = 4000;
            private const int windowIdenficicationMaxHeight = 100;
            private const int windowIdenficicationMaxWidth = 400;
            private const int maxTaskBarShowDelay = 5000;
            private const int maxRetryDelay = 300;

            public static void Do(Process process, string tomb4Path, string levelOutputPath, bool isNG)
            {
                if (!isNG)
                {
                    // Patch binary to allow quick start. This will work for original tomb4.exe.
                    Tomb4Patcher.ApplyPatches(process, tomb4Path, levelOutputPath);
                }
                else
                {
                    // Avoid the 'Press CTRL window for settings' window of the TRNG
                    // engine, to get into the game quicker.
                    
                    Stopwatch timer = new Stopwatch(); // Wait up to 2 seconds to find a suitable window.
                    timer.Start();
                    int currentWaitMilliseconds = 0;
                    bool closedWindow = false;
                    while (timer.ElapsedMilliseconds < maxWaitMilliseconds && !closedWindow)
                    {
                        // Look for any window of Tomb4.exe
                        EnumWindowsProc onEnumWindowDelegate = (IntPtr hWnd, IntPtr lParam) =>
                        {
                            // Check if the window is owned by Tomb4.exe
                            int processID;
                            ReportError(GetWindowThreadProcessId(hWnd, out processID), "GetWindowThreadProcessId");
                            if (process.Id != processID)
                                return 1;

                            // Check if this is the CTRL window by comparing its size
                            RECT area;
                            ReportError(GetWindowRect(hWnd, out area), "GetWindowRect");
                            int width = area.Right - area.Left;
                            int height = area.Bottom - area.Top;
                            if (width > windowIdenficicationMaxWidth ||
                                height > windowIdenficicationMaxHeight)
                                return 1;

                            // Before closing window, try to patch tomb4 process here, because
                            // new FLEP workflow with patches.bin file does not work with initial patching.
                            Tomb4Patcher.ApplyPatches(process, tomb4Path, levelOutputPath);

                            // Close window
                            logger.Info("Window with handle " + hWnd + " has been identified as TRNG settings window. It is being closed.");
                            ReportError(PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero), "PostMessage");
                            closedWindow = true;
                            return 0;
                        };
                        ReportError(EnumWindows(onEnumWindowDelegate, IntPtr.Zero), "EnumWindows");

                        // Wait
                        Thread.Sleep(currentWaitMilliseconds);
                        currentWaitMilliseconds = (currentWaitMilliseconds + 1) * 4 / 3;
                    }
                }

                // Make sure the task bar stays visible
                // (Workaround super annoying TRNG problem!)
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    int taskbarHandle = ReportError(FindWindow("Shell_TrayWnd", ""), "FindWindow");

                    while (timer.ElapsedMilliseconds < maxTaskBarShowDelay)
                    {
                        ReportError(ShowWindowAsync(taskbarHandle, SW_SHOW), "ShowWindowAsync");
                        Thread.Sleep(maxRetryDelay);
                    }
                }
            }

        }

        private static class Tomb4Patcher
        {
            [DllImport("ntdll.dll", SetLastError = false)]
            public static extern uint NtSuspendProcess(IntPtr processHandle);
            [DllImport("ntdll.dll", SetLastError = false)]
            public static extern uint NtResumeProcess(IntPtr processHandle);
            [DllImport("ntdll.dll")]
            public static extern int RtlNtStatusToDosError(uint status);

            [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int WriteProcessMemory(IntPtr processHandle, IntPtr baseAddress, byte[] buffer, IntPtr bufferSize, IntPtr unused);
            [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int ReadProcessMemory(IntPtr processHandle, IntPtr baseAddress, byte[] buffer, IntPtr bufferSize, IntPtr unused);

            public struct Patch
            {
                public IntPtr Address { get; }
                public byte[] OldCode { get; }
                public byte[] NewCode { get; }
                public Patch(IntPtr address, byte[] oldCode, byte[] newCode)
                {
                    Address = address;
                    OldCode = oldCode;
                    NewCode = newCode;
                    if (oldCode != null && newCode != null && oldCode.Length != NewCode.Length)
                        throw new ArgumentException();
                }

                public IntPtr Length => new IntPtr(OldCode != null ? OldCode.Length : NewCode.Length);
            }

            public static bool ApplyPatches(IntPtr hProc, IEnumerable<Patch> patches)
            {
                try
                {
                    uint ntStatus = NtSuspendProcess(hProc);
                    if (ntStatus != 0)
                    {
                        int errorCode = RtlNtStatusToDosError(ntStatus);
                        logger.Warn("An WinAPI error occurred in 'TombLauncher' at function '" + "NtSuspendProcess" + "': " + errorCode);
                    }

                    try
                    {
                        logger.Info("Sucessfully suspended process for runtime patching.");

                        // Check that the memory to be patched is in the expected state
                        // to not corrupt the process.
                        foreach (Patch patch in patches)
                            if (patch.OldCode != null)
                            {
                                byte[] readCode = new byte[patch.Length.ToInt32()];
                                if (ReportError(ReadProcessMemory(hProc, patch.Address, readCode, patch.Length, IntPtr.Zero), "ReadProcessMemory") == 0)
                                {
                                    logger.Error("Runtime code patching couldn't applied because 'ReadProcessMemory' failed at address " + patch.Address + ".");
                                    return false;
                                }
                                if (!readCode.SequenceEqual(patch.OldCode))
                                {
                                    logger.Error("Runtime code patching couldn't applied because at address " + patch.Address +
                                        " the bytes " + ToStr(patch.OldCode) + " instead of " + ToStr(readCode) + ".");
                                    return false;
                                }
                            }

                        // Apply patches
                        foreach (Patch patch in patches)
                            ReportError(WriteProcessMemory(hProc, patch.Address, patch.NewCode, patch.Length, IntPtr.Zero), "WriteProcessMemory");
                    }
                    finally
                    {
                        uint ntStatus2 = NtResumeProcess(hProc);
                        if (ntStatus2 != 0)
                        {
                            int errorCode = RtlNtStatusToDosError(ntStatus2);
                            logger.Warn("An WinAPI error occurred in 'TombLauncher' at function '" + "NtResumeProcess" + "': " + errorCode);
                        }
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Unable to apply runtime patching.");
                    return false;
                }
                return true;
            }

            public static void ApplyPatches(Process process, string tomb4Path, string levelOutputPath)
            {
                // Setup executable to setup the level index directly to the currently edited level
                // to avoid having to choose it in the title menu.

                int levelIndex = ConvertLevelPathToIndex(tomb4Path, levelOutputPath);
                logger.Info("Level index: " + levelIndex);

                var patches = new List<Patch>();
                patches.Add(new Patch(new IntPtr(0x47E910),
                    new byte[5] { 0x68, 0x14, 0x18, 0x4B, 0x00 },
                    new byte[5] { 0xC3, 0x90, 0x90, 0x90, 0x90 }));
                patches.Add(new Patch(new IntPtr(0x47B600),
                    new byte[5] { 0xa0, 0x70, 0xD1, 0x7F, 0x00 },
                    new byte[5] { 0xC3, 0x90, 0x90, 0x90, 0x90 }));

                if (levelIndex > 0)
                {
                    patches.Add(new Patch(new IntPtr(0x4515DA),
                        null,
                        new byte[1] { checked((byte)levelIndex) }));
                    patches.Add(new Patch(new IntPtr(0x47AE1B),
                        new byte[2] { 0x8B, 0xFE },
                        new byte[2] { 0xEB, 0x45 }));
                    patches.Add(new Patch(new IntPtr(0x47AE62),
                        new byte[11] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 },
                        new byte[11] { 0x89, 0xF7, 0xC6, 0x05, 0xDA, 0x15, 0x45, 0x00, 0x00, 0xEB, 0xB0 }));
                }

                ApplyPatches(process.Handle, patches);
            }
        }

        private static int ConvertLevelPathToIndex(string tomb4Path, string levelOutputPath)
        {
            try
            {
                // Based on publically available documentation from here
                // https://opentomb.earvillage.net/TRosettaStone3/trosettastone.html#_the_script_file
                string scriptPath = Path.Combine(tomb4Path, "SCRIPT.DAT");

                string[] levelPaths;
                using (var strm = new FileStream(scriptPath, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete | FileShare.Inheritable))
                {
                    Encoding encoding = Encoding.ASCII;

                    // Read 'header'
                    byte[] header = new byte[56];
                    strm.Read(header, 0, header.Length);

                    int numTotalLevels = header[9];
                    int levelpathStringLen = header[12] | (header[13] << 8);
                    string pcLevelString = encoding.GetString(header, 36, 4);

                    // Read level paths
                    byte[] offsetsToLevelpathString = new byte[numTotalLevels * 2];
                    strm.Read(offsetsToLevelpathString, 0, offsetsToLevelpathString.Length);
                    byte[] levelpathStringBlock = new byte[levelpathStringLen];
                    strm.Read(levelpathStringBlock, 0, levelpathStringBlock.Length);

                    levelPaths = new string[numTotalLevels];
                    for (int i = 0; i < numTotalLevels; ++i)
                    {
                        int offset = offsetsToLevelpathString[i * 2] | (offsetsToLevelpathString[i * 2 + 1] << 8);
                        int endOffset = offset;
                        while (levelpathStringBlock[endOffset] != 0)
                            ++endOffset;
                        levelPaths[i] = encoding.GetString(levelpathStringBlock, offset, endOffset - offset) + pcLevelString;
                    }
                }

                // Which index is it?
                int foundIndex = -1;
                for (int i = 0; i < levelPaths.Length; ++i)
                    if (levelOutputPath.EndsWith(levelPaths[i], StringComparison.InvariantCultureIgnoreCase))
                        if (foundIndex != -1)
                        {
                            logger.Warn("Parsing 'script.dat' gives an ambigues result for matching level path '" + levelOutputPath + "'." +
                                "It matches with '" + levelPaths[i] + "' (index " + i + ")" +
                                "as well as  '" + levelPaths[foundIndex] + "' (index " + foundIndex + ")");
                            return -1;
                        }
                        else
                            foundIndex = i;
                if (foundIndex == -1)
                    logger.Warn("Parsing 'script.dat' gives no result for matching level path '" + levelOutputPath + "'.");
                return foundIndex;
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occurred while scanning 'script.dat' for the current level.");
                return -1;
            }
        }

        private static int ReportError(int result, string functionName)
        {
            if (result != 0)
                return result;
            int errorCode = Marshal.GetLastWin32Error();
            logger.Warn("An WinAPI error occurred in 'TombLauncher' at function '" + functionName + "': " + errorCode);
            return 0;
        }

        private static string ToStr<T>(T[] arguments)
        {
            return "{" + string.Join(", ", arguments.Select(argument => argument.ToString())) + "}";
        }
    }
}
