using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace RuriLib.Http.Helpers
{
    internal class ChunkedDecoderOptimized : IDisposable
    {
        private long templength;
        private static byte[] CRLF_Bytes = { 13, 10 };
        // private long remaningchunklength;
        private bool Isnewchunk = true;
        // private AutoResetEvent manualResetEvent = new AutoResetEvent(true);

        public Stream DecodedStream { get; private set; }

        public bool Finished { get; private set; }

        public ChunkedDecoderOptimized()
        {
            DecodedStream = new MemoryStream(1024);
        }

        internal void Decode(ref ReadOnlySequence<byte> buff) => ParseNewChunk(ref buff);

        private void ParseNewChunk(ref ReadOnlySequence<byte> buff)
        {
            if (Isnewchunk)
            {
                templength = GetChunkLength(ref buff);
                Isnewchunk = false;
            }
            if (templength == 0 && buff.Length >= 2)
            {
                Finished = true;
                buff = buff.Slice(2);//skip last crlf
                return;
            }
            else if (templength == -1)
            {
                Isnewchunk = true;
                return;
            }
            if (buff.Length > templength + 2)
            {              
                var chunk = buff.Slice(buff.Start, templength);
                WritetoStream(chunk);
                Isnewchunk = true;
                buff = buff.Slice(chunk.End);
                buff = buff.Slice(2); //skip CRLF

                ParseNewChunk(ref buff);
            }
        }

        private int GetChunkLength(ref ReadOnlySequence<byte> buff)
        {
            if (buff.IsSingleSegment)
            {
                var index = -1;
                var span = buff.FirstSpan;
                index = span.IndexOf(CRLF_Bytes);
                if (index != -1)
                {
                    var line = span.Slice(0, index);
                    var pos = line.IndexOf((byte)';');
                    if (pos != -1)
                    {
                        line = line.Slice(0, pos);
                    }
                    buff = buff.Slice(index + 2);
                    return Convert.ToInt32(Encoding.ASCII.GetString(line), 16);
                }
                else
                {
                    //  Console.WriteLine($"error payload: {Encoding.ASCII.GetString(buff.FirstSpan)}");
                    return -1;
                }
            }
            else
            {
                SequenceReader<byte> reader = new SequenceReader<byte>(buff);
                if (reader.TryReadTo(out ReadOnlySpan<byte> line, CRLF_Bytes.AsSpan(), true))
                {
                    var pos = line.IndexOf((byte)';');
                    if (pos > 0)
                    {
                        line = line.Slice(0, pos);
                    }
                    buff = buff.Slice(reader.Position);
                    return Convert.ToInt32(Encoding.ASCII.GetString(line), 16);
                }
                else
                {
                    // Console.WriteLine($"error payload: {Encoding.ASCII.GetString(buff.FirstSpan)}");
                    return -1;
                }
            }
        }

        private void WritetoStream(ReadOnlySequence<byte> buff)
        {
            if (buff.IsSingleSegment)
            {
                DecodedStream.Write(buff.FirstSpan);
            }
            else
            {
                foreach (var seg in buff)
                {
                    DecodedStream.Write(seg.Span);
                }
            }
        }

        public void Dispose() => DecodedStream?.Dispose();
    }
}
