#nullable enable

using System;
using System.Runtime.InteropServices;

namespace TombLib.WPF.Controls.Rendering;

/// <summary>Reports how often rendering panels may present a frame, based on the refresh rate of the primary display.</summary>
internal static class DisplayRefresh
{
    private const int CurrentSettings = -1;
    private const int AttachedToDesktop = 1;
    private const int FallbackRate = 60;
    private const int MaxRate = 360;

    private static readonly Lazy<TimeSpan> FrameInterval = new(QueryFrameInterval);

    /// <summary>Gets the shortest time between two presented frames; slightly under one refresh so no refresh is skipped.</summary>
    public static TimeSpan MinFrameInterval => FrameInterval.Value;

    private static TimeSpan QueryFrameInterval()
    {
        var rate = 0;

        try
        {
            // The fastest attached display wins, so a panel on a high-refresh monitor is never held back by a slower one.
            var device = new DisplayDevice { Size = Marshal.SizeOf<DisplayDevice>() };

            for (var index = 0; EnumDisplayDevices(null, index, ref device, 0); index++)
            {
                if ((device.StateFlags & AttachedToDesktop) == 0)
                    continue;

                var mode = new DeviceMode { Size = (short)Marshal.SizeOf<DeviceMode>() };

                if (EnumDisplaySettings(device.DeviceName, CurrentSettings, ref mode) && mode.DisplayFrequency > 1)
                    rate = Math.Max(rate, Math.Min(mode.DisplayFrequency, MaxRate));
            }
        }
        catch (Exception)
        {
            // Keep the fallback rate when the displays cannot be queried.
        }

        return TimeSpan.FromSeconds(0.9 / (rate > 0 ? rate : FallbackRate));
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(string? device, int deviceNumber, ref DisplayDevice displayDevice, int flags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DisplayDevice
    {
        public int Size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public int StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplaySettings(string? deviceName, int modeNumber, ref DeviceMode mode);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DeviceMode
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        public short SpecVersion;
        public short DriverVersion;
        public short Size;
        public short DriverExtra;
        public int Fields;
        public int PositionX;
        public int PositionY;
        public int DisplayOrientation;
        public int DisplayFixedOutput;
        public short Color;
        public short Duplex;
        public short YResolution;
        public short TrueTypeOption;
        public short Collate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FormName;
        public short LogPixels;
        public int BitsPerPel;
        public int PelsWidth;
        public int PelsHeight;
        public int DisplayFlags;
        public int DisplayFrequency;
        public int IcmMethod;
        public int IcmIntent;
        public int MediaType;
        public int DitherType;
        public int Reserved1;
        public int Reserved2;
        public int PanningWidth;
        public int PanningHeight;
    }
}
