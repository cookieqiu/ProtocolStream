﻿using CommunityToolkit.HighPerformance.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProtocolStream
{
    public partial class ProtocolWriter : IDisposable
    {
        private readonly Memory<byte> _buffer;

        private readonly MemoryOwner<byte>? _memoryOwner;

        private readonly int _length;

        private int _bytePointer = 0;

        private int _bitPointer = 0;

        private int _maxByteCount = 0;

        public ProtocolWriter(byte[] buffer)
        {
            _length = buffer.Length;
            _buffer = buffer.AsMemory();
        }

        public ProtocolWriter(int byteSize)
        {
            _length = byteSize;
            _memoryOwner = MemoryOwner<byte>.Allocate(byteSize);
            _buffer = _memoryOwner.Memory;

            Unsafe.InitBlock(ref _memoryOwner.Span[0], 0, (uint)byteSize);
        }

        public void Seek(int byteOffset)
        {
            _maxByteCount = Math.Max(_maxByteCount, _bytePointer);
            _bytePointer = byteOffset;
            _bitPointer = 0;
        }

        public ReadOnlySpan<byte> AsSpan()
        {
            return this.AsSpan(0);
        }

        public ReadOnlySpan<byte> AsSpan(int start)
        {
            var length = Math.Max(_maxByteCount, _bytePointer + (_bitPointer + 7 >> 3));
            return _buffer[start..length].Span;
        }

        public ReadOnlySpan<byte> AsSpan(int start, int length)
        {
            return _buffer.Slice(start, length).Span;
        }

        public ReadOnlyMemory<byte> AsMemory()
        {
            return this.AsMemory(0);
        }

        public ReadOnlyMemory<byte> AsMemory(int start)
        {
            var length = Math.Max(_maxByteCount, _bytePointer + (_bitPointer + 7 >> 3));
            return _buffer[start..length];
        }

        public ReadOnlyMemory<byte> AsMemory(int start, int length)
        {
            return _buffer.Slice(start, length);
        }

        public void Write(byte value)
        {
            var p = _bytePointer;
            _bytePointer += 1;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(sbyte value)
        {
            var p = _bytePointer;
            _bytePointer += 1;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(ushort value)
        {
            var p = _bytePointer;
            _bytePointer += 2;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(short value)
        {
            var p = _bytePointer;
            _bytePointer += 2;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(uint value)
        {
            var p = _bytePointer;
            _bytePointer += 4;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(int value)
        {
            var p = _bytePointer;
            _bytePointer += 4;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(ulong value)
        {
            var p = _bytePointer;
            _bytePointer += 8;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(long value)
        {
            var p = _bytePointer;
            _bytePointer += 8;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(UInt128 value)
        {
            var p = _bytePointer;
            _bytePointer += 16;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(Int128 value)
        {
            var p = _bytePointer;
            _bytePointer += 16;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(Half value)
        {
            var p = _bytePointer;
            _bytePointer += 2;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(float value)
        {
            var p = _bytePointer;
            _bytePointer += 4;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(double value)
        {
            var p = _bytePointer;
            _bytePointer += 8;
            this.ValidatePointer();
            Unsafe.WriteUnaligned(ref _buffer.Span[p], value);
        }

        public void Write(string value)
        {
            this.Write(value, Encoding.ASCII);
        }

        public void Write(string value, int length)
        {
            this.Write(value, length, Encoding.ASCII);
        }

        public void Write(string value, Encoding encoding)
        {
            var p = _bytePointer;
            var length = value.Length;

            _bytePointer += length;
            this.ValidatePointer();
            encoding.GetBytes(value.AsSpan(), _buffer[p..].Span);
        }

        public void Write(string value, int length, Encoding encoding)
        {
            var p = _bytePointer;

            _bytePointer += length;
            this.ValidatePointer();

            var strLen = value.Length;
            if (length <= strLen)
            {
                encoding.GetBytes(value.AsSpan(0, length), _buffer[p..].Span);
            }
            else
            {
                encoding.GetBytes(value.AsSpan(0, strLen), _buffer[p..].Span);
                Unsafe.InitBlock(ref _buffer.Span[p + strLen], 0, (uint)(length - strLen));
            }
        }

        public void Write(byte[] value)
        {
            var p = _bytePointer;
            var length = value.Length;

            _bytePointer += length;
            this.ValidatePointer();
            Unsafe.CopyBlock(ref _buffer.Span[p], ref value[0], (uint)length);
        }

        public void Write(byte[] value, int start, int count)
        {
            var p = _bytePointer;

            _bytePointer += count;
            this.ValidatePointer();
            Unsafe.CopyBlock(ref _buffer.Span[p], ref value[start], (uint)count);
        }

        public void Write(bool value)
        {
            var p = _bytePointer;
            var q = _bitPointer;

            _bitPointer += 1;
            _bytePointer += _bitPointer >> 3;
            _bitPointer &= 7;
            this.ValidatePointer();
            if (value)
            {
                _buffer.Span[p] |= (byte)(1 << q);
            }
            else
            {
                _buffer.Span[p] &= (byte)~(1 << q);
            }
        }

        private void ValidatePointer()
        {
            if (_bytePointer > _length || _bytePointer == _length && _bitPointer > 0)
            {
                throw new IndexOutOfRangeException(nameof(_bytePointer));
            }
        }

        public void Dispose()
        {
            _memoryOwner?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
