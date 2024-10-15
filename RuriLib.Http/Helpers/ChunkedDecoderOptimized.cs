using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace RuriLib.Http.Helpers;

internal class ChunkedDecoderOptimized : IDisposable
{
    private long _tempLength;
    private static readonly byte[] _crlfBytes = "\r\n"u8.ToArray();
    private bool _isNewChunk = true;

    public Stream DecodedStream { get; } = new MemoryStream(1024);

    public bool Finished { get; private set; }

    internal void Decode(ref ReadOnlySequence<byte> buff) => ParseNewChunk(ref buff);

    private void ParseNewChunk(ref ReadOnlySequence<byte> buff)
    {
        if (_isNewChunk)
        {
            _tempLength = GetChunkLength(ref buff);
            _isNewChunk = false;
        }
        
        if (_tempLength == 0 && buff.Length >= 2)
        {
            Finished = true;
            buff = buff.Slice(2);//skip last crlf
            return;
        }

        if (_tempLength == -1)
        {
            _isNewChunk = true;
            return;
        }
        
        if (buff.Length > _tempLength + 2)
        {              
            var chunk = buff.Slice(buff.Start, _tempLength);
            WriteToStream(chunk);
            _isNewChunk = true;
            buff = buff.Slice(chunk.End);
            buff = buff.Slice(2); //skip CRLF

            ParseNewChunk(ref buff);
        }
    }

    private static int GetChunkLength(ref ReadOnlySequence<byte> buff)
    {
        if (buff.IsSingleSegment)
        {
            var span = buff.FirstSpan;
            var index = span.IndexOf(_crlfBytes);
            
            if (index == -1)
            {
                // Console.WriteLine($"error payload: {Encoding.ASCII.GetString(buff.FirstSpan)}");
                return -1;
            }
            
            var line = span[..index];
            var pos = line.IndexOf((byte)';');
            if (pos != -1)
            {
                line = line[..pos];
            }
            buff = buff.Slice(index + 2);
            return Convert.ToInt32(Encoding.ASCII.GetString(line), 16);
        }
        else
        {
            var reader = new SequenceReader<byte>(buff);
            
            if (!reader.TryReadTo(out ReadOnlySpan<byte> line, _crlfBytes.AsSpan()))
            {
                // Console.WriteLine($"error payload: {Encoding.ASCII.GetString(buff.FirstSpan)}");
                return -1;
            }
            
            var pos = line.IndexOf((byte)';');
            if (pos > 0)
            {
                line = line[..pos];
            }
            buff = buff.Slice(reader.Position);
            return Convert.ToInt32(Encoding.ASCII.GetString(line), 16);
        }
    }

    private void WriteToStream(ReadOnlySequence<byte> buff)
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

    public void Dispose() => DecodedStream.Dispose();
}
