using System;
using System.Buffers.Binary;
using System.IO;
using TombLib.Graphics;
using TombLib.Utils;
using Blake3Hasher = Blake3.Hasher;

namespace TombLib.Wad
{
    public class WadTexture : Texture, IRenderableObject, IEquatable<WadTexture>, TextureHashed
    {
        public class AtlasReference
        {
            public WadTexture Texture;
            public int Atlas;
            public VectorInt2 Position;
        }

        public Hash Hash { get; }

        public WadTexture(ImageC image)
        {
            Image = image;

            // Hash image dimensions and pixel data directly without intermediate copies.
            using var hasher = Blake3Hasher.New();
            Span<byte> header = stackalloc byte[8];
            BinaryPrimitives.WriteInt32LittleEndian(header, Image.Size.X);
            BinaryPrimitives.WriteInt32LittleEndian(header.Slice(4), Image.Size.Y);
            hasher.Update(header);
            hasher.Update(Image.ToByteArray());

            Span<byte> digest = stackalloc byte[32];
            hasher.Finalize(digest);

            ulong low = BinaryPrimitives.ReadUInt64LittleEndian(digest.Slice(0, 8));
            ulong high = BinaryPrimitives.ReadUInt64LittleEndian(digest.Slice(8, 8));
            Hash = new Hash { HashLow = low, HashHigh = high };
        }

        public override Texture Clone() => this;

        public static bool operator==(WadTexture first, WadTexture second) => ReferenceEquals(first, null) ? ReferenceEquals(second, null) : (ReferenceEquals(second, null) ? false : first.Hash == second.Hash);
        public static bool operator!=(WadTexture first, WadTexture second) => !(first == second);
        public bool Equals(WadTexture other) => Hash == other.Hash;
        public override bool Equals(object other) => other is WadTexture && Hash == ((WadTexture)other).Hash;
        public override int GetHashCode() => Hash.GetHashCode();

        public override string ToString()
        {
            var hint = string.Empty;

            if (string.IsNullOrEmpty(AbsolutePath))
            {
                if (string.IsNullOrEmpty(Image.FileName))
                    hint = "Untitled (" + Image.Size.X + "x" + Image.Size.Y + ")";
                else
                    hint = Path.GetFileName(Image.FileName);
            }
            else
                hint = "[Ext] " + Path.GetFileName(AbsolutePath);

			return hint;
		}

		public void Dispose()
        {
        }
	}
}
