using System.Collections.Generic;
using TombLib.Utils;

namespace TombLib.Wad
{
    internal class WadVideos
    {
        public enum VideoFormat
        {
            Mp4,
            Avi,
            Mov,
            Mkv
        }

        public static IReadOnlyList<FileFormat> FileExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("MP4 video", "mp4"),
            new FileFormat("AVI video", "avi"),
            new FileFormat("QuickTime video", "mov"),
            new FileFormat("Matroska video", "mkv"),
        };

        public static readonly string VideoFolder = "FMV";
    }
}
