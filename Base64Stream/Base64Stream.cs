using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Base64Stream
{
    public class Base64Stream : Stream
    {
        private static ReadOnlySpan<sbyte> DecodingMap => new sbyte[]
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, -1, -1, 63,
            52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,
            -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
            15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,
            -1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
            41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        };

        private readonly string _base64;
        private int _charPosition;

        public Base64Stream(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                throw new ArgumentNullException(nameof(base64));

            if (base64.Length % 4 != 0)
                throw new FormatException("String is not a valid base64");

            _base64 = base64;

            var strSpan = base64.AsSpan();
            ref char srcChars = ref MemoryMarshal.GetReference(strSpan);

            var numberOfPaddingCharacters = 0L;
            if (Unsafe.Add(ref srcChars, base64.Length - 2) == '=')
                numberOfPaddingCharacters = 2;
            else if (Unsafe.Add(ref srcChars, base64.Length - 1) == '=')
                numberOfPaddingCharacters = 1;

            _length = (3L * (base64.Length / 4L)) - numberOfPaddingCharacters;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        private readonly long _length;
        public override long Length => _length;

        private long _position;
        public override long Position
        {
            get => _position;
            set
            {
                if (value != 0)
                    throw new NotSupportedException();

                _position = 0;
                _charPosition = 0;
            }
        }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan().Slice(offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            var i = 0;
            var strSpan = _base64.AsSpan();
            ref char srcChars = ref MemoryMarshal.GetReference(strSpan);
            ref byte dstbytes = ref MemoryMarshal.GetReference(buffer);
            ref sbyte decodingMap = ref MemoryMarshal.GetReference(DecodingMap);

            var nearestMultiple = buffer.Length - buffer.Length % 3;
            if (nearestMultiple == 0)
                throw new ArgumentException("buffer size is less then 3");

            for (; i < nearestMultiple && _position < _length; _charPosition += 4, i += 3, _position += 3)
            {
                var c = Unsafe.Add(ref srcChars, _charPosition);
                var c2 = Unsafe.Add(ref srcChars, _charPosition + 1);
                var c3 = Unsafe.Add(ref srcChars, _charPosition + 2);
                var c4 = Unsafe.Add(ref srcChars, _charPosition + 3);

                if (((c | c2 | c3 | c4) & 0xffffff00) != 0)
                    throw new FormatException("Invalid character on Base64 string");

                var b = Unsafe.Add(ref decodingMap, c);
                var b2 = Unsafe.Add(ref decodingMap, c2);
                var b3 = Unsafe.Add(ref decodingMap, c3);
                var b4 = Unsafe.Add(ref decodingMap, c4);

                var r = (byte)(((b << 2) & 0xFF) | (b2 >> 4));
                Unsafe.Add(ref dstbytes, i) = r;

                if (b3 != -1)
                {
                    var r2 = (byte)(((b2 << 4) & 0xFF) | (b3 >> 2));
                    Unsafe.Add(ref dstbytes, i + 1) = r2;

                    if (b4 != -1)
                    {
                        var r3 = (byte)(((b3 << 6) & 0xFF) | (byte)b4);
                        Unsafe.Add(ref dstbytes, i + 2) = r3;
                    }
                    else
                    {
                        i -= 1;
                        _position -= 1;
                    }
                }
                else
                {
                    i -= 2;
                    _position -= 2;
                }
            }

            return i;
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}