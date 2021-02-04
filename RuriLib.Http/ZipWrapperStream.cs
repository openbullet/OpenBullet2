using RuriLib.Http.Helpers;
using System.IO;

namespace RuriLib.Http
{
    internal sealed class ZipWrapperStream : Stream
    {
        private readonly Stream baseStream;
        private readonly ReceiveHelper receiverHelper;

        public int BytesRead { get; private set; }
        public int TotalBytesRead { get; set; }
        public int LimitBytesRead { get; set; }

        public override bool CanRead => baseStream.CanRead;
        public override bool CanSeek => baseStream.CanSeek;
        public override bool CanTimeout => baseStream.CanTimeout;
        public override bool CanWrite => baseStream.CanWrite;
        public override long Length => baseStream.Length;
        public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

        public ZipWrapperStream(Stream baseStream, ReceiveHelper receiverHelper)
        {
            this.baseStream = baseStream;
            this.receiverHelper = receiverHelper;
        }

        public override void Flush() => baseStream.Flush();
        public override void SetLength(long value) => baseStream.SetLength(value);
        public override long Seek(long offset, SeekOrigin origin) => baseStream.Seek(offset, origin);

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (LimitBytesRead != 0)
            {
                var length = LimitBytesRead - TotalBytesRead;

                if (length == 0)
                {
                    return 0;
                }

                if (length > buffer.Length)
                {
                    length = buffer.Length;
                }

                if (receiverHelper.HasData)
                {
                    BytesRead = receiverHelper.Read(buffer, offset, length);
                }
                else
                {
                    BytesRead = baseStream.Read(buffer, offset, length);
                }
            }
            else
            {
                if (receiverHelper.HasData)
                {
                    BytesRead = receiverHelper.Read(buffer, offset, count);
                }
                else
                {
                    BytesRead = baseStream.Read(buffer, offset, count);
                }
            }

            TotalBytesRead += BytesRead;

            return BytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count) => baseStream.Write(buffer, offset, count);
    }
}
