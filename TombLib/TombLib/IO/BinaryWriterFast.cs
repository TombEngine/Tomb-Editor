using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.IO
{
    // Similar to BinaryWriter but caches data writes before sending it to the stream
    // and has all methods optimized and inlined.
    // Used to save about 20% when saving a prj2 file, but this can only get more significant after we optimize other things too.
    public unsafe sealed class BinaryWriterFast : IDisposable
    {
        private const int _safetyMargin = 16;

        // In order of importance
        private byte* _ptr;
        private byte* _endPtrWithSafetyMarging;
        private byte* _startPtr;
        private byte* _backupHighPtr;
        private byte[] _buffer;
        private GCHandle _handle;
        private Stream _baseStream;
        private long _baseStreamPosition;

        public BinaryWriterFast(Stream baseStream, int bufferSize = 1024 * 64)
        {
            _baseStream = baseStream;
            _baseStreamPosition = baseStream.Position;
            _buffer = new byte[bufferSize];
            _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            _ptr = _startPtr = _backupHighPtr = (byte*)_handle.AddrOfPinnedObject();
            _endPtrWithSafetyMarging = _ptr + bufferSize - _safetyMargin;
        }

        ~BinaryWriterFast()
        {
            if (_handle.IsAllocated)
                _handle.Free();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Dispose()
        {
            if (_baseStream == null)
                return;
            Flush();
            if (_handle.IsAllocated)
                _handle.Free();
            _buffer = null;
            _ptr = null;
            _startPtr = null;
            _baseStream = null;

            GC.SuppressFinalize(this);
        }

        public Stream BaseStream
        {
            get { return _baseStream; }
            set
            {
                if (_baseStream == value)
                    return;
                Flush();
                _baseStream = value;
                _baseStreamPosition = value.Position;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Flush()
        {
            _baseStream.Write(_buffer, 0, (int)(_ptr - _startPtr));
            _baseStreamPosition += _ptr - _startPtr;
            _ptr = _backupHighPtr = _startPtr;
        }

        public long Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _baseStreamPosition + (_ptr - _startPtr); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                long offset = value - _baseStreamPosition;
                byte* highPtr = _backupHighPtr > _ptr ? _backupHighPtr : _ptr;
                if (offset >= 0 && offset <= (highPtr - _startPtr))
                { // Easy (and common) case, the required data is still buffered
                    _backupHighPtr = highPtr;
                    _ptr = _startPtr + offset;
                    return;
                }

                // Flush current buffer and move stream position
                Flush();
                _baseStream.Position = _baseStreamPosition = value;
            }
        }

        public long Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                byte* highPtr = _backupHighPtr > _ptr ? _backupHighPtr : _ptr;
                return Math.Max(_baseStream.Length, _baseStreamPosition + (highPtr - _startPtr));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value) { unchecked { if (_ptr > _endPtrWithSafetyMarging) Flush(); *(_ptr++) = value ? (byte)1 : (byte)0; } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value) { unchecked { if (_ptr > _endPtrWithSafetyMarging) Flush(); *(_ptr++) = value; } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value) { unchecked { if (_ptr > _endPtrWithSafetyMarging) Flush(); *(_ptr++) = (byte)value; *(_ptr++) = (byte)(value >> 8); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value) { unchecked { if (_ptr > _endPtrWithSafetyMarging) Flush(); *(_ptr++) = (byte)value; *(_ptr++) = (byte)(value >> 8); *(_ptr++) = (byte)(value >> 16); *(_ptr++) = (byte)(value >> 24); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value) { unchecked { if (_ptr > _endPtrWithSafetyMarging) Flush(); *(_ptr++) = (byte)value; *(_ptr++) = (byte)(value >> 8); *(_ptr++) = (byte)(value >> 16); *(_ptr++) = (byte)(value >> 24); *(_ptr++) = (byte)(value >> 32); *(_ptr++) = (byte)(value >> 40); *(_ptr++) = (byte)(value >> 48); *(_ptr++) = (byte)(value >> 56); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value) { unchecked { if (_ptr > _endPtrWithSafetyMarging) Flush(); *(_ptr++) = (byte)value; } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value) { unchecked { if (_ptr > _endPtrWithSafetyMarging) Flush(); *(_ptr++) = (byte)value; *(_ptr++) = (byte)(value >> 8); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value) { unchecked { if (_ptr > _endPtrWithSafetyMarging) Flush(); *(_ptr++) = (byte)value; *(_ptr++) = (byte)(value >> 8); *(_ptr++) = (byte)(value >> 16); *(_ptr++) = (byte)(value >> 24); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value) { unchecked { if (_ptr > _endPtrWithSafetyMarging) Flush(); *(_ptr++) = (byte)value; *(_ptr++) = (byte)(value >> 8); *(_ptr++) = (byte)(value >> 16); *(_ptr++) = (byte)(value >> 24); *(_ptr++) = (byte)(value >> 32); *(_ptr++) = (byte)(value >> 40); *(_ptr++) = (byte)(value >> 48); *(_ptr++) = (byte)(value >> 56); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value) { unchecked { Write(*(uint*)&value); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value) { unchecked { Write(*(ulong*)&value); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value) { unchecked { Write((ushort)value); } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnchecked(int value) { unchecked { *(_ptr++) = (byte)value; *(_ptr++) = (byte)(value >> 8); *(_ptr++) = (byte)(value >> 16); *(_ptr++) = (byte)(value >> 24); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(VectorInt2 value) { if (_ptr > _endPtrWithSafetyMarging) Flush(); WriteUnchecked(value.X); WriteUnchecked(value.Y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(VectorInt3 value) { if (_ptr > _endPtrWithSafetyMarging) Flush(); WriteUnchecked(value.X); WriteUnchecked(value.Y); WriteUnchecked(value.Z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Vector2 value) { if (_ptr > _endPtrWithSafetyMarging) Flush(); WriteUnchecked(*(int*)&value.X); WriteUnchecked(*(int*)&value.Y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Vector3 value) { if (_ptr > _endPtrWithSafetyMarging) Flush(); WriteUnchecked(*(int*)&value.X); WriteUnchecked(*(int*)&value.Y); WriteUnchecked(*(int*)&value.Z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Vector4 value) { if (_ptr > _endPtrWithSafetyMarging) Flush(); WriteUnchecked(*(int*)&value.X); WriteUnchecked(*(int*)&value.Y); WriteUnchecked(*(int*)&value.Z); WriteUnchecked(*(int*)&value.W); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] value, int offset, int length)
        {
            if ((_ptr + length) > _endPtrWithSafetyMarging)
            {
                Flush();
                if ((_ptr + length) > _endPtrWithSafetyMarging || length > 1024)
                { // Data is big, write it out directly
                    _baseStream.Write(value, offset, length);
                    _baseStreamPosition += length;
                    return;
                }
            }
            Marshal.Copy(value, offset, new IntPtr(_ptr), length);
            _ptr += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] value, int offset = 0)
        {
            Write(value, offset, value.Length - offset);
        }

        public void WriteStringUTF8(string value)
        {
            byte[] stringData = Encoding.UTF8.GetBytes(value);
            Write(stringData.Length);
            Write(stringData);
        }
    }
}
