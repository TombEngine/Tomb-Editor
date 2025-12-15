using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.Utils;

namespace TombLib.IO
{
    public class BinaryWriterEx : BinaryWriter
    {
        public BinaryWriterEx(Stream output)
            : base(output)
        {}

        public BinaryWriterEx(Stream output, bool leaveOpen)
          : base(output, System.Text.Encoding.UTF8, leaveOpen)
        { }

        public void Write(Vector2 value)
        {
            Write(value.X);
            Write(value.Y);
        }

        public void Write(Vector3 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
        }

        public void Write(Vector4 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        public void Write(Quaternion value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        public void Write(Hash hash)
        {
            Write(hash.HashLow);
            Write(hash.HashHigh);
        }

        public void WriteBlock<T>(T block)
        {
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT);
            try
            {
                var buffer = new byte[sizeOfT];
                Marshal.StructureToPtr(block, unmanaged, false);
                Marshal.Copy(unmanaged, buffer, 0, sizeOfT);
                Write(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(unmanaged);
            }
        }

        public void WriteBlockArray<T>(IEnumerable<T> values)
        {
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT);
            try
            {
                var buffer = new byte[sizeOfT];

                foreach (var s in values)
                {
                    Marshal.StructureToPtr(s, unmanaged, false);
                    Marshal.Copy(unmanaged, buffer, 0, sizeOfT);
                    Write(buffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(unmanaged);
            }
        }
    }
}
