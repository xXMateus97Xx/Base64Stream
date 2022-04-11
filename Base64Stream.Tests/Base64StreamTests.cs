using System;
using System.IO;
using System.Text;
using Xunit;

namespace Base64Stream.Tests
{
    public class Base64StreamTests
    {
        [Fact]
        public void Base64Stream_1Byte_Success()
        {
            var str = "1";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

            var stream = new Base64Stream(base64);

            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_2Bytes_Success()
        {
            var str = "12";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

            var stream = new Base64Stream(base64);

            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_3Bytes_Success()
        {
            var str = "123";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

            var stream = new Base64Stream(base64);

            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_4Bytes_Success()
        {
            var str = "1234";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

            var stream = new Base64Stream(base64);

            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_MultipleReads_Success()
        {
            var str = "12345678";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

            var stream = new Base64Stream(base64);

            var arr = new byte[9];
            int i = 0, j = 3;
            while (i < str.Length)
            {
                i += stream.Read(arr, i, j);
            }

            var result = Encoding.UTF8.GetString(arr.AsSpan().TrimEnd((byte)0));

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_1ByteTrimNeeded_Success()
        {
            var str = "  1  ";
            var base64 = $"  {Convert.ToBase64String(Encoding.UTF8.GetBytes(str))}  ";

            var stream = new Base64Stream(base64);

            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_2BytesTrimNeeded_Success()
        {
            var str = "  12  \n";
            var base64 = $"  {Convert.ToBase64String(Encoding.UTF8.GetBytes(str))}  \n";

            var stream = new Base64Stream(base64);

            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_3BytesTrimNeeded_Success()
        {
            var str = "\n 123  ";
            var base64 = $"\n {Convert.ToBase64String(Encoding.UTF8.GetBytes(str))}  ";

            var stream = new Base64Stream(base64);

            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_4BytesTrimNeeded_Success()
        {
            var str = "  1234  \r\n";
            var base64 = $"  {Convert.ToBase64String(Encoding.UTF8.GetBytes(str))} \r\n";

            var stream = new Base64Stream(base64);

            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_MultipleReadsTrimNeeded_Success()
        {
            var str = "12345678";
            var base64 = $"\r\n{Convert.ToBase64String(Encoding.UTF8.GetBytes(str))}\r\n";

            var stream = new Base64Stream(base64);

            var arr = new byte[9];
            int i = 0, j = 3;
            while (i < str.Length)
            {
                i += stream.Read(arr, i, j);
            }

            var result = Encoding.UTF8.GetString(arr.AsSpan().TrimEnd((byte)0));

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_SetInitialPosition_Success()
        {
            var str = "Test text";
            var base64 = $"data:text/plain;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(str))}  ";

            var stream = new Base64Stream(base64, base64.IndexOf(',') + 1);

            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal(str, result);
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_BigString_Success()
        {
            var bytes = File.ReadAllBytes("hamlet.txt");

            var base64 = Convert.ToBase64String(bytes);

            var stream = new Base64Stream(base64);

            var ms = new MemoryStream();
            stream.CopyTo(ms);

            var result = ms.ToArray();

            Assert.True(result.AsSpan().SequenceEqual(bytes));
            Assert.Equal(stream.Length, stream.Position);
        }

        [Fact]
        public void Base64Stream_SmallBuffer_ThrowsException()
        {
            var str = "12345678";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

            var stream = new Base64Stream(base64);

            var arr = new byte[2];

            Assert.Throws<ArgumentException>(() => stream.Read(arr, 0, arr.Length));
        }

        [Fact]
        public void Base64Stream_InvalidChar_ThrowsException()
        {
            var base64 = new string(new[] { (char)1234, 'b', '=', '=' });

            var stream = new Base64Stream(base64);

            var reader = new StreamReader(stream);

            Assert.Throws<FormatException>(() => reader.ReadToEnd());
        }

        [Fact]
        public void Base64Stream_Incorrect_ThrowsException()
        {
            var base64 = "abcd==";

            Assert.Throws<FormatException>(() => new Base64Stream(base64));
        }

        [Fact]
        public void Base64Stream_Null_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new Base64Stream(null));
        }

        [Fact]
        public void Base64Stream_Empty_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new Base64Stream(""));
        }
    }
}