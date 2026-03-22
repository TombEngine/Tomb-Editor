using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.Utils;
using GlyphIndex = System.UInt16;

namespace TombLib.Rendering
{
    public class RenderingFont : IDisposable
    {
        private class GlyphData
        {
            public VectorInt2 Offset;
            public ImageC Image;
        }

        public RenderingTextureAllocator TextureAllocator { get; private set; }
        private readonly Dictionary<GlyphIndex, WeakReference<GlyphData>> _glyphDictionary = new Dictionary<GlyphIndex, WeakReference<GlyphData>>();

        private IntPtr _gdiHdc;
        private IntPtr _gdiFont;
        private IntPtr _gdiGetCharacterPlacementOrder;
        private IntPtr _gdiGetCharacterPlacementDx;
        private IntPtr _gdiGetCharacterPlacementGlpyhs;
        private const int _gdiGetCharacterPlacementGlyphCount = 512;

        private bool _disposed = false;
        private int _lineAscent;
        private int _lineSpaceing;
        private const int _marginInTexture = 1;

        public class Description
        {
            public RenderingTextureAllocator TextureAllocator;
            public string FontName = "Arial";
            public bool FontIsBold;
            public bool FontIsItalic;
            public bool FontIsUnderline;
            public bool FontIsStrikeout;
            public float FontSize = 20.0f;
            public string PreLoadedCharacters = "\" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~¡";
        }

        public RenderingFont(Description description)
        {
            try
            {
                try
                {
                    // Load specified font
                    Create(description);
                }  
                catch (Exception)
                {
                    // Fall back to default font
                    GDI.DeleteObject(_gdiFont);
                    GDI.DeleteDC(_gdiHdc);
                    Marshal.FreeHGlobal(_gdiGetCharacterPlacementOrder);
                    Marshal.FreeHGlobal(_gdiGetCharacterPlacementDx);
                    Marshal.FreeHGlobal(_gdiGetCharacterPlacementGlpyhs);
                    Create(new Description() { TextureAllocator = TextureAllocator } );
                }  
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        private void Create(Description description)
        {
            TextureAllocator = description.TextureAllocator;
            TextureAllocator.GarbageCollectionCollectEvent.Add(delegate (RenderingTextureAllocator allocator, RenderingTextureAllocator.Map map, HashSet<RenderingTextureAllocator.Map.Entry> inOutUsedTextures)
            {
                // Clean dictionary for weak references that died.
                List<GlyphIndex> GlyphKeysToRemove = new List<GlyphIndex>();
                GlyphData unused;
                foreach (KeyValuePair<GlyphIndex, WeakReference<GlyphData>> glyph in _glyphDictionary)
                    if (!glyph.Value.TryGetTarget(out unused))
                        GlyphKeysToRemove.Add(glyph.Key);
                foreach (GlyphIndex GlyphKeyToRemove in GlyphKeysToRemove)
                    _glyphDictionary.Remove(GlyphKeyToRemove);
                return null;
            });

            // Setup GDI device and font
            _gdiHdc = GDI.CreateCompatibleDC(IntPtr.Zero);
            if (_gdiHdc == IntPtr.Zero)
                throw new GDI.GDIException("CreateCompatibleDC");
            _gdiFont = GDI.CreateFontW((int)description.FontSize, 0, 0, 0, description.FontIsBold ? 700 : 400,
                description.FontIsItalic ? 1u : 0u, description.FontIsUnderline ? 1u : 0u, description.FontIsStrikeout ? 1u : 0u, 0, 0, 0,
                GDI.FontQuality.CLEARTYPE_QUALITY, 0, description.FontName);
            if (_gdiFont == IntPtr.Zero)
                throw new GDI.GDIException("CreateFont");
            IntPtr selectObjectResult = GDI.SelectObject(_gdiHdc, _gdiFont);
            if (selectObjectResult == IntPtr.Zero || selectObjectResult == new IntPtr(65535))
                throw new GDI.GDIException("SelectObject");
            _gdiGetCharacterPlacementOrder = Marshal.AllocHGlobal(sizeof(uint) * _gdiGetCharacterPlacementGlyphCount);
            _gdiGetCharacterPlacementDx = Marshal.AllocHGlobal(sizeof(int) * _gdiGetCharacterPlacementGlyphCount);
            _gdiGetCharacterPlacementGlpyhs = Marshal.AllocHGlobal(sizeof(GlyphIndex) * _gdiGetCharacterPlacementGlyphCount);
            GDI.TEXTMETRICW textMetric;
            if (!GDI.GetTextMetricsW(_gdiHdc, out textMetric))
                throw new GDI.GDIException("GetTextMetricsW");
            _lineAscent = textMetric.tmAscent;
            _lineSpaceing = textMetric.tmAscent + textMetric.tmDescent + textMetric.tmExternalLeading;
            if (GDI.SetBkColor(_gdiHdc, 0x00ffffff) == 0xffffffff) // White background (GDI does not support alpha though)
                throw new GDI.GDIException("SetBkColor");

            // Preload characters
            ParseString(description.PreLoadedCharacters, false, new List<GlyphRenderInfo>());
        }

        private unsafe GlyphData GdiCreateGlyph(GlyphIndex glyphIndex)
        {
            // Measure the size
            GDI.GLYPHMETRICS glyphMetrics;
            GDI.MAT2 matrix = new GDI.MAT2 { eM11 = 65536, eM12 = 0, eM21 = 0, eM22 = 65536 };
            if (GDI.GetGlyphOutline(_gdiHdc, glyphIndex, GDI.GGO.GGO_METRICS | GDI.GGO.GGO_GLYPH_INDEX, out glyphMetrics, 0, IntPtr.Zero, ref matrix) == 0xffffffff)
                throw new GDI.GDIException("GetGlyphOutline");

            // Create bitmap
            GDI.BITMAPINFO bitmapInfo;
            bitmapInfo.biSize = Marshal.SizeOf(typeof(GDI.BITMAPINFO));
            bitmapInfo.biWidth = glyphMetrics.width + _marginInTexture * 2;
            bitmapInfo.biHeight = glyphMetrics.height + _marginInTexture * 2;
            bitmapInfo.biPlanes = 1;
            bitmapInfo.biBitCount = 32;
            bitmapInfo.biCompression = 0; //BI_RGB
            bitmapInfo.biSizeImage = 0;
            bitmapInfo.biXPelsPerMeter = 1024;
            bitmapInfo.biYPelsPerMeter = 1024;
            bitmapInfo.biClrUsed = 0;
            bitmapInfo.biClrImportant = 0;
            IntPtr bitmapPtr;
            IntPtr hbitmap = GDI.CreateDIBSection(_gdiHdc, ref bitmapInfo, GDI.DIB_RGB_COLORS, out bitmapPtr, IntPtr.Zero, 0);
            if (hbitmap == IntPtr.Zero)
                throw new GDI.GDIException("CreateDIBSection");
            try
            {
                // Render gylph
                IntPtr selectObjectResult = GDI.SelectObject(_gdiHdc, hbitmap);
                if (selectObjectResult == IntPtr.Zero || selectObjectResult == new IntPtr(65535))
                    throw new GDI.GDIException("SelectObject");
                GDI.RECT rect = new GDI.RECT { Left = 0, Top = 0, Right = bitmapInfo.biWidth, Bottom = bitmapInfo.biHeight };
                if (!GDI.ExtTextOutW(_gdiHdc, _marginInTexture - glyphMetrics.x, _marginInTexture + glyphMetrics.y - _lineAscent, GDI.ETO.ETO_OPAQUE | GDI.ETO.ETO_CLIPPED | GDI.ETO.ETO_GLYPH_INDEX, ref rect, new IntPtr(&glyphIndex), 1, IntPtr.Zero))
                    throw new GDI.GDIException("ExtTextOut");
                if (!GDI.GdiFlush())
                    throw new GDI.GDIException("GdiFlush");

                // Convert RGB GDI image to RGBA byte array
                // White color is mapped to full transparency
                byte[] imageData = new byte[bitmapInfo.biWidth * bitmapInfo.biHeight * ImageC.PixelSize];
                fixed (byte* destination2 = imageData)
                {
                    uint* source = (uint*)bitmapPtr;
                    uint* destination = (uint*)destination2;
                    int count = bitmapInfo.biWidth * bitmapInfo.biHeight;
                    for (int i = 0; i < count; ++i)
                    {
                        // Optimized version of:
                        // https://stackoverflow.com/a/40862635

                        // White text
                        uint pixel = source[i];
                        uint r = 255 - (pixel & 0xff);
                        uint g = 255 - ((pixel >> 8) & 0xff);
                        uint b = 255 - ((pixel >> 16) & 0xff);
                        uint a = 255;
                        uint factor = a == 0 ? 0xff0000 : (0xff0000 / a);
                        r = Math.Min((r * factor + 0x8000) >> 16, 255);
                        g = Math.Min((g * factor + 0x8000) >> 16, 255);
                        b = Math.Min((b * factor + 0x8000) >> 16, 255);
                        destination[i] = (a << 24) | (b << 16) | (g << 8) | r;

                        /* // Black text
                        uint pixel = source[i];
                        uint r = pixel & 0xff;
                        uint g = (pixel >> 8) & 0xff;
                        uint b = (pixel >> 16) & 0xff;
                        uint a = 0xff - Math.Min(r, Math.Min(g, b));
                        uint factor = a == 0 ? 0xff0000 : (0xff0000 / a);
                        r = (0xff8000 - Math.Min(0xff8000, (255 - r) * factor)) >> 16;
                        g = (0xff8000 - Math.Min(0xff8000, (255 - g) * factor)) >> 16;
                        b = (0xff8000 - Math.Min(0xff8000, (255 - b) * factor)) >> 16;
                        destination[i] = (a << 24) | (b << 16) | (g << 8) | r;
                         */
                    }
                }

                // TEST
                //ImageC.FromByteArray(imageData, bitmapInfo.biWidth, bitmapInfo.biHeight).Save("T:\\Out.png");

                // Generate output
                return new GlyphData
                {
                    Image = ImageC.FromByteArray(imageData, bitmapInfo.biWidth, bitmapInfo.biHeight),
                    Offset = new VectorInt2(glyphMetrics.x - _marginInTexture, glyphMetrics.y + _marginInTexture)
                };
            }
            finally
            {
                GDI.DeleteObject(hbitmap);
            }
        }

        private GlyphData GetOrCreateGlyph(GlyphIndex glyphIndex)
        {
            // Try to find glyph in dictionary
            WeakReference<GlyphData> glyphDataRef;
            if (_glyphDictionary.TryGetValue(glyphIndex, out glyphDataRef))
            {
                GlyphData glyphData;
                if (glyphDataRef.TryGetTarget(out glyphData))
                    return glyphData;
                _glyphDictionary.Remove(glyphIndex);
            }

            // Add and return
            {
                GlyphData glyphData = GdiCreateGlyph(glyphIndex);
                _glyphDictionary.Add(glyphIndex, new WeakReference<GlyphData>(glyphData));
                return glyphData;
            }
        }

        public class GlyphRenderInfo
        {
            public VectorInt2 PosStart;
            public VectorInt2 PosEnd;
            public VectorInt3 TexStart;
            public VectorInt2 TexSize;
        };

        public unsafe RectangleInt2 ParseString(string str, bool buildOverlay, List<GlyphRenderInfo> outGlyphRenderInfos, VectorInt2 offsetedPos = new VectorInt2(), Vector2 alignment = new Vector2())
        {
            RectangleInt2? result = null;

            // Do line breaking
            List<string> lines = new List<string>();
            int lineStart = 0;
            for (int i = 0; i < str.Length; ++i)
                if (str[i] == '\n')
                {
                    bool hadR = i != 0 && str[i - 1] == '\r';
                    int lineEnd = i - (hadR ? 1 : 0);
                    lines.Add(str.Substring(lineStart, lineEnd - lineStart));
                    lineStart = i + 1;
                }
            if (lineStart < str.Length)
                lines.Add(str.Substring(lineStart));

            // Handle line by line
            int totalHeight = (lines.Count - 1) * _lineSpaceing + _lineAscent;
            int alignmentOffsetY = (int)(totalHeight * alignment.Y + 0.5f);
            for (int i = 0; i < lines.Count; ++i)
            {
                // Figure out glyph placement
                GDI.GCP_RESULTS gcp;
                gcp.StructSize = Marshal.SizeOf(typeof(GDI.GCP_RESULTS));
                gcp.OutString = IntPtr.Zero;
                gcp.Order = _gdiGetCharacterPlacementOrder;
                gcp.Dx = _gdiGetCharacterPlacementDx;
                gcp.CaretPos = IntPtr.Zero;
                gcp.Class = IntPtr.Zero;
                gcp.Glyphs = _gdiGetCharacterPlacementGlpyhs;
                gcp.GlyphCount = _gdiGetCharacterPlacementGlyphCount;
                gcp.MaxFit = int.MaxValue;
                uint dimensions = GDI.GetCharacterPlacementW(_gdiHdc, lines[i], lines[i].Length, int.MaxValue, ref gcp, GDI.GCPFlags.GCP_LIGATE | GDI.GCPFlags.GCP_REORDER | GDI.GCPFlags.GCP_DIACRITIC | GDI.GCPFlags.GCP_USEKERNING);
                if (dimensions == 0)
                    continue;
                int width = (int)(dimensions & 0xffff);
                int height = (int)(dimensions >> 16);

                // Make each glyph available
                uint* orderPtr = (uint*)gcp.Order;
                int* dxPtr = (int*)gcp.Dx;
                GlyphIndex* gylphPtr = (GlyphIndex*)gcp.Glyphs;
                int alignmentOffsetX = (int)(alignment.X * width + 0.5f);
                VectorInt2 pos = offsetedPos + new VectorInt2(-alignmentOffsetX, alignmentOffsetY - _lineAscent - i * _lineSpaceing);
                for (int j = 0; j < gcp.GlyphCount; ++j)
                {
                    ushort glyphIndex = gylphPtr[orderPtr[j]];
                    int dx = dxPtr[orderPtr[j]];

                    GlyphData glyphData = GetOrCreateGlyph(glyphIndex);
                    VectorInt3 texPosition = TextureAllocator.Get(new RenderingTexture(glyphData.Image) { Tag = glyphData }); // Tag the glyph data to keep the weak reference alive

                    var posStart = pos + glyphData.Offset + new VectorInt2(0, -glyphData.Image.Size.Y);
                    var posEnd = pos + glyphData.Offset + new VectorInt2(glyphData.Image.Size.X, 0);

                    // Generate rectangular font overlay
                    if(buildOverlay)
                    {
                        var rect = new RectangleInt2(posStart, posEnd);
                        if (!result.HasValue) result = rect;
                        else result = result.Value.Union(rect);
                    }

                    outGlyphRenderInfos.Add(new GlyphRenderInfo
                    {
                        PosStart = posStart,
                        PosEnd = posEnd,
                        TexStart = texPosition,
                        TexSize = glyphData.Image.Size
                    });

                    pos.X += dx;
                }
            }

            return result.HasValue ? result.Value.Inflate(2) : RectangleInt2.Zero;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            TextureAllocator?.Dispose();
            GDI.DeleteObject(_gdiFont);
            GDI.DeleteDC(_gdiHdc);
            Marshal.FreeHGlobal(_gdiGetCharacterPlacementOrder);
            Marshal.FreeHGlobal(_gdiGetCharacterPlacementDx);
            Marshal.FreeHGlobal(_gdiGetCharacterPlacementGlpyhs);

            GC.SuppressFinalize(this);
        }

        ~RenderingFont()
        {
            Dispose();
        }

        private static class GDI
        {
            public class GDIException : Exception
            {
                public GDIException(string functionName)
                    : base("GDI function '" + functionName + "' failed with error code " + Marshal.GetLastWin32Error())
                { }
            }

            [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll", EntryPoint = "DeleteDC", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll", EntryPoint = "GetTextMetricsW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetTextMetricsW(IntPtr hdc, out TEXTMETRICW lptm);

            [DllImport("gdi32.dll", EntryPoint = "SetBkColor", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern uint SetBkColor(IntPtr hdc, int crColor);

            [DllImport("gdi32.dll", EntryPoint = "CreateDIBSection", SetLastError = true)]
            public static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, uint pila, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

            [DllImport("gdi32.dll", EntryPoint = "CreateFontW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr CreateFontW(int nHeight, int nWidth, int nEscapement,
               int nOrientation, int fnWeight, uint fdwItalic, uint fdwUnderline,
               uint fdwStrikeOut, uint fdwCharSet, uint fdwOutputPrecision,
               uint fdwClipPrecision, FontQuality fdwQuality, uint fdwPitchAndFamily, string lpszFace);

            [DllImport("gdi32.dll", EntryPoint = "DeleteObject", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll", EntryPoint = "SelectObject", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

            [DllImport("gdi32.dll", EntryPoint = "GdiFlush", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GdiFlush();

            [DllImport("gdi32.dll", EntryPoint = "GetCharacterPlacementW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern uint GetCharacterPlacementW(IntPtr hdc, [MarshalAs(UnmanagedType.LPWStr)] string lpString, int nCount, int nMaxExtent, ref GCP_RESULTS lpResults, GCPFlags dwFlags);

            [DllImport("gdi32.dll", EntryPoint = "GetGlyphOutline", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern uint GetGlyphOutline(IntPtr hdc, uint uChar, GGO uFormat, out GLYPHMETRICS lpgm, uint cbBuffer, IntPtr lpvBuffer, ref MAT2 lpmat2);

            [DllImport("gdi32.dll", EntryPoint = "ExtTextOutW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ExtTextOutW(IntPtr hdc, int X, int Y, ETO fuOptions, ref RECT lprc, IntPtr lpString, int cbCount, [In] IntPtr lpDx);

            [StructLayout(LayoutKind.Sequential)]
            public struct GCP_RESULTS
            {
                public int StructSize;
                public IntPtr OutString;
                public IntPtr Order;
                public IntPtr Dx;
                public IntPtr CaretPos;
                public IntPtr Class;
                public IntPtr Glyphs;
                public int GlyphCount;
                public int MaxFit;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct BITMAPINFO
            {
                public int biSize;
                public int biWidth;
                public int biHeight;
                public ushort biPlanes;
                public ushort biBitCount;
                public uint biCompression;
                public uint biSizeImage;
                public int biXPelsPerMeter;
                public int biYPelsPerMeter;
                public uint biClrUsed;
                public uint biClrImportant;
            }

            [Flags]
            public enum GCPFlags : uint
            {
                GCP_DBCS = 0x0001,
                GCP_REORDER = 0x0002,
                GCP_USEKERNING = 0x0008,
                GCP_GLYPHSHAPE = 0x0010,
                GCP_LIGATE = 0x0020,
                GCP_DIACRITIC = 0x0100,
                GCP_KASHIDA = 0x0400,
                GCP_ERROR = 0x8000,
                GCP_JUSTIFY = 0x00010000,
                GCP_CLASSIN = 0x00080000,
                GCP_MAXEXTENT = 0x00100000,
                GCP_JUSTIFYIN = 0x00200000,
                GCP_DISPLAYZWG = 0x00400000,
                GCP_SYMSWAPOFF = 0x00800000,
                GCP_NUMERICOVERRIDE = 0x01000000,
                GCP_NEUTRALOVERRIDE = 0x02000000,
                GCP_NUMERICSLATIN = 0x04000000,
                GCP_NUMERICSLOCAL = 0x08000000,
            }

            [Flags]
            public enum ETO : uint
            {
                ETO_CLIPPED = 0x4,
                ETO_GLYPH_INDEX = 0x10,
                ETO_IGNORELANGUAGE = 0x1000,
                ETO_NUMERICSLATIN = 0x800,
                ETO_NUMERICSLOCAL = 0x400,
                ETO_OPAQUE = 0x2,
                ETO_PDY = 0x2000,
                ETO_RTLREADING = 0x800,
            }

            [Flags]
            public enum GGO : uint
            {
                GGO_METRICS = 0,
                GGO_BITMAP = 1,
                GGO_NATIVE = 2,
                GGO_BEZIER = 3,
                GGO_GRAY2_BITMAP = 4,
                GGO_GRAY4_BITMAP = 5,
                GGO_GRAY8_BITMAP = 6,
                GGO_GLYPH_INDEX = 0x80,
                GGO_UNHINTED = 0x100
            }

            public enum FontQuality : uint
            {
                DEFAULT_QUALITY = 0,
                DRAFT_QUALITY = 1,
                PROOF_QUALITY = 2,
                NONANTIALIASED_QUALITY = 3,
                ANTIALIASED_QUALITY = 4,
                CLEARTYPE_QUALITY = 5,
                CLEARTYPE_NATURAL_QUALITY = 6,
            }

            public const uint CLEARTYPE_QUALITY = 5;
            public const uint DIB_RGB_COLORS = 0;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct TEXTMETRICW
            {
                public int tmHeight;
                public int tmAscent;
                public int tmDescent;
                public int tmInternalLeading;
                public int tmExternalLeading;
                public int tmAveCharWidth;
                public int tmMaxCharWidth;
                public int tmWeight;
                public int tmOverhang;
                public int tmDigitizedAspectX;
                public int tmDigitizedAspectY;
                public ushort tmFirstChar;
                public ushort tmLastChar;
                public ushort tmDefaultChar;
                public ushort tmBreakChar;
                public byte tmItalic;
                public byte tmUnderlined;
                public byte tmStruckOut;
                public byte tmPitchAndFamily;
                public byte tmCharSet;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct GLYPHMETRICS
            {
                public int width;
                public int height;
                public int x;
                public int y;
                public short gmCellIncX;
                public short gmCellIncY;

            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MAT2
            {
                public uint eM11;
                public uint eM12;
                public uint eM21;
                public uint eM22;
            }
        }
    }
}
