using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Http.Helpers
{
    internal sealed class ReceiveHelper
    {
        private const int InitialLineSize = 1000;

        private Stream stream;

        private readonly byte[] buffer;
        private readonly int bufferSize;

        private int linePosition;
        private byte[] lineBuffer = new byte[InitialLineSize];

        public bool HasData => (Length - Position) != 0;
        public int Length { get; private set; }
        public int Position { get; private set; }

        public ReceiveHelper(int bufferSize)
        {
            this.bufferSize = bufferSize;
            buffer = new byte[bufferSize];
        }

        public void Init(Stream stream)
        {
            this.stream = stream;
            linePosition = 0;

            Length = 0;
            Position = 0;
        }

        public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
        {
            linePosition = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Position == Length)
                {
                    Position = 0;
                    Length = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken);

                    if (Length == 0)
                    {
                        break;
                    }
                }

                var b = buffer[Position++];

                lineBuffer[linePosition++] = b;

                // If it's the '\n' symbol, we're done.
                if (b == 10)
                {
                    break;
                }

                // If we reached the maximum buffer dimension
                if (linePosition == lineBuffer.Length)
                {
                    // Double the size of the buffer and copy over the contents
                    var newLineBuffer = new byte[lineBuffer.Length * 2];

                    lineBuffer.CopyTo(newLineBuffer, 0);
                    lineBuffer = newLineBuffer;
                }
            }

            return Encoding.ASCII.GetString(lineBuffer, 0, linePosition);
        }

        public int Read(byte[] buffer, int index, int length)
        {
            var curLength = Length - Position;

            if (curLength > length)
            {
                curLength = length;
            }

            Array.Copy(this.buffer, Position, buffer, index, curLength);

            Position += curLength;

            return curLength;
        }
    }
}
