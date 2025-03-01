using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Collections.Generic;
using RuriLib.Http.Helpers;
using System.IO.Pipelines;
using System.Buffers;

namespace RuriLib.Http;

internal class HttpResponseMessageBuilder
{
    private PipeReader? _reader;
    private const string _newLine = "\r\n";
    private readonly byte[] _crlf = Encoding.UTF8.GetBytes(_newLine);
    private static readonly byte[] _doubleCrlfBytes = "\r\n\r\n"u8.ToArray();

    private int _contentLength = -1;

    private HttpResponseMessage? _response;
    private Dictionary<string, List<string>>? _contentHeaders;

    private readonly CookieContainer _cookies;
    private readonly Uri? _uri;

    public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public HttpResponseMessageBuilder(int bufferSize, CookieContainer? cookies = null, Uri? uri = null)
    {
        // this.bufferSize = bufferSize;
        this._cookies = cookies ?? new CookieContainer();
        this._uri = uri;

        // receiveHelper = new ReceiveHelper(bufferSize);
    }

    public async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request, Stream stream,
        bool readResponseContent = true, CancellationToken cancellationToken = default)       {           
           
        _reader = PipeReader.Create(stream);
            
        _response = new HttpResponseMessage();
        _contentHeaders = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        _response.RequestMessage = request;

        try
        {
            await ReceiveFirstLineAsync(cancellationToken).ConfigureAwait(false);
            await ReceiveHeadersAsync(cancellationToken).ConfigureAwait(false);
            await ReceiveContentAsync(readResponseContent, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            _response.Dispose();
            throw;
        }

        return _response;
    }

    // Parses the first line, for example
    // HTTP/1.1 200 OK
    private async Task ReceiveFirstLineAsync(CancellationToken cancellationToken = default)
    {
        var startingLine = string.Empty;

        // Read the first line from the Network Stream
        while (true)
        {
            var res = await _reader!.ReadAsync(cancellationToken).ConfigureAwait(false);
                
            var buff = res.Buffer;
            var crlfIndex = buff.FirstSpan.IndexOf(_crlf);
            if (crlfIndex > -1)
            {
                try
                {
                    startingLine = Encoding.UTF8.GetString(buff.FirstSpan[..crlfIndex]);
                    var fields = startingLine.Split(' ');
                    _response!.Version = Version.Parse(fields[0].Trim()[5..]);
                    _response.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), fields[1]);
                    buff = buff.Slice(0, crlfIndex + 2); // add 2 bytes for the CRLF
                    _reader.AdvanceTo(buff.End); // advance to the consumed position
                    break;
                }
                catch
                {
                    throw new FormatException($"Invalid first line of the HTTP response: {startingLine}");
                }
            }

            // the response is incomplete ex. (HTTP/1.1 200 O)
            _reader.AdvanceTo(buff.Start, buff.End); // nothing consumed but all the buffer examined loop and read more.

            if (res is { IsCanceled: false, IsCompleted: false })
            {
                continue;
            }
            
            await _reader.CompleteAsync();
            cancellationToken.ThrowIfCancellationRequested();
        }


    }

    // Parses the headers
    private async Task ReceiveHeadersAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var res = await _reader!.ReadAsync(cancellationToken).ConfigureAwait(false);

            var buff = res.Buffer;
            if (buff.IsSingleSegment)
            {
                if (ReadHeadersFastPath(ref buff))
                {
                    _reader.AdvanceTo(buff.Start);
                    break;
                }

            }
            else
            {
                if (ReadHeadersSlowerPath(ref buff))
                {
                    _reader.AdvanceTo(buff.Start);
                    break;
                }
            }
            _reader.AdvanceTo(buff.Start, buff.End); // not adding this line might result in an infinite loop
            
            if (res is { IsCanceled: false, IsCompleted: false })
            {
                continue;
            }
            
            await _reader.CompleteAsync();
            cancellationToken.ThrowIfCancellationRequested();

        }
    }
    /// <summary>
    /// Reads all Header Lines using <see cref="Span{T}"/> For High Perfromace Parsing.
    /// </summary>
    /// <param name="buff">Buffered Data From Pipe</param>
    private bool ReadHeadersFastPath(ref ReadOnlySequence<byte> buff)
    {
        int endOfHeadersIndex;
        
        if ((endOfHeadersIndex = buff.FirstSpan.IndexOf(_doubleCrlfBytes)) <= -1)
        {
            return false;
        }
        
        var spanLines = buff.FirstSpan[..(endOfHeadersIndex + 4)];
        var lines = spanLines.SplitLines(); // we use spanHelper class here to make a for each loop.
        foreach (var line in lines)
        {
                   
            ProcessHeaderLine(line);
        }

        buff = buff.Slice(endOfHeadersIndex + 4); // add 4 bytes for \r\n\r\n and to advance the pipe back in the calling method
        return true;
    }
    /// <summary>
    /// Reads all Header Lines using SequenceReader.
    /// </summary>
    /// <param name="buff">Buffered Data From Pipe</param>
    private bool ReadHeadersSlowerPath(ref ReadOnlySequence<byte> buff)
    {
        var reader = new SequenceReader<byte>(buff);

        while (reader.TryReadTo(out ReadOnlySpan<byte> line, _crlf))
        {
            if (line.Length == 0) // reached last crlf (empty line)
            {
                buff = buff.Slice(reader.Position);
                return true; // all headers received
            }
            ProcessHeaderLine(line);
        }
        buff = buff.Slice(reader.Position);
        return false; // empty line not found need more data
    }

    private void ProcessHeaderLine(ReadOnlySpan<byte> header)
    {
        if (header.Length == 0)
        {
            return;
        }

        // changed to use span directly to decrease the number of strings allocated (less GC activity)
        var separatorPos = header.IndexOf((byte)':');

        // If not found, don't do anything because the header is not valid
        // Sometimes it can happen that the first line e.g. HTTP/1.1 200 OK is read as a header (maybe the buffer
        // is not advanced properly) so it can cause an exception.
        if (separatorPos == -1)
        {
            return;
        }

        var headerName = Encoding.UTF8.GetString(header[..separatorPos]);
        var headerValueSpan = header[(separatorPos + 1)..]; // skip ':'
        var headerValue = headerValueSpan[0] == (byte)' ' 
            ? Encoding.UTF8.GetString(headerValueSpan[1..])
            : Encoding.UTF8.GetString(headerValueSpan); // trim the white space

        // If the header is Set-Cookie, add the cookie
        if (headerName.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase) ||
            headerName.Equals("Set-Cookie2", StringComparison.OrdinalIgnoreCase))
        {
            SetCookies(headerValue, _cookies, _uri!);
        }
        // If it's a content header
        else if (ContentHelper.IsContentHeader(headerName))
        {
            if (_contentHeaders!.TryGetValue(headerName, out var values))
            {
                values.Add(headerValue);
            }
            else
            {
                values = new List<string>
                {
                    headerValue
                };

                _contentHeaders.Add(headerName, values);
            }
        }
        else
        {
            _response!.Headers.TryAddWithoutValidation(headerName, headerValue);
        }
    }

    /// <summary>
    /// Sets a list of comma-separated cookies.
    /// </summary>
    internal static void SetCookies(string value, CookieContainer cookies, Uri uri)
    {
        // Cookie values, as per the RFC, cannot contain commas. A comma is used
        // to separate multiple cookies in the same Set-Cookie header. So, we split
        // the header by commas and set each cookie individually.
        foreach (var cookie in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            SetCookie(cookie, cookies, uri);
        }
    }
        
    /// <summary>
    /// Sets a single cookie.
    /// </summary>
    internal static void SetCookie(string value, CookieContainer cookies, Uri uri)
    {
        if (value.Length == 0)
        {
            return;
        }

        var endCookiePos = value.IndexOf(';');
        var separatorPos = value.IndexOf('=');

        if (separatorPos == -1)
        {
            // Invalid cookie, simply don't add it
            return;
        }

        string cookieValue;
        var cookieName = value[..separatorPos];

        if (endCookiePos == -1)
        {
            cookieValue = value[(separatorPos + 1)..];
        }
        else
        {
            cookieValue = value.Substring(separatorPos + 1, (endCookiePos - separatorPos) - 1);
            
            var expiresPos = value.IndexOf("expires=", StringComparison.OrdinalIgnoreCase);

            if (expiresPos != -1)
            {
                var endExpiresPos = value.IndexOf(';', expiresPos);

                expiresPos += 8;

                var expiresStr = endExpiresPos == -1 ? value[expiresPos..] : value[expiresPos..endExpiresPos];

                if (DateTime.TryParse(expiresStr, out var expires) &&
                    expires < DateTime.Now)
                {
                    var collection = cookies.GetCookies(uri);
                    
                    if (collection[cookieName] is not null)
                    {
                        collection[cookieName]!.Expired = true;
                    }
                }
            }
        }

        if (cookieValue.Length == 0 ||
            cookieValue.Equals("deleted", StringComparison.OrdinalIgnoreCase))
        {
            var collection = cookies.GetCookies(uri);
            
            if (collection[cookieName] is not null)
            {
                collection[cookieName]!.Expired = true;
            }
        }
        else
        {
            cookies.Add(new Cookie(cookieName, cookieValue, "/", uri.Host));
        }
    }

    private async Task ReceiveContentAsync(bool readResponseContent = true, CancellationToken cancellationToken = default)
    {
        // If there are content headers
        if (_contentHeaders!.Count != 0)
        {
            _contentLength = GetContentLength();

            if (readResponseContent)
            {
                // Try to get the body and write it to a MemoryStream
                var finaleResponseStream = await GetMessageBodySource(cancellationToken).ConfigureAwait(false);

                // Rewind the stream and set the content of the response and its headers
                finaleResponseStream.Seek(0, SeekOrigin.Begin);
                _response!.Content = new StreamContent(finaleResponseStream);
            }
            else
            {
                _response!.Content = new ByteArrayContent([]);
            }

            foreach (var pair in _contentHeaders)
            {
                _response.Content.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
        }
    }

    private Task<Stream> GetMessageBodySource(CancellationToken cancellationToken)
    {
        if (_response!.Headers.Contains("Transfer-Encoding"))
        {
            return _contentHeaders!.ContainsKey("Content-Encoding")
                ? GetChunkedDecompressedStream(cancellationToken)
                : ReceiveMessageBodyChunked(cancellationToken);
        }

        if (_contentLength > -1)
        {
            return _contentHeaders!.ContainsKey("Content-Encoding")
                ? GetContentLengthDecompressedStream(cancellationToken)
                : ReceiveContentLength(cancellationToken);
        }

        // handle the case where sever never sent chunked encoding nor content-length headrs (that is not allowed by rfc but whatever)
        return _contentHeaders!.ContainsKey("Content-Encoding")
            ? GetResponseStreamUntilCloseDecompressed(cancellationToken) 
            : GetResponseStreamUntilClose(cancellationToken);

    }
    private async Task<Stream> GetResponseStreamUntilClose(CancellationToken cancellationToken)
    {
        var responseStream = new MemoryStream();
        while (true)
        {
            var res = await _reader!.ReadAsync(cancellationToken).ConfigureAwait(false);
          
            if (res.IsCanceled)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            var buff = res.Buffer;

            if (buff.IsSingleSegment)
            {
                responseStream.Write(buff.FirstSpan);
            }
            else
            {
                foreach (var seg in buff)
                {
                    responseStream.Write(seg.Span);
                }
            }
            _reader.AdvanceTo(buff.End);
            if (res.IsCompleted || res.Buffer.Length == 0)// here the pipe will be complete if the server closes the connection or sends 0 length byte array
            {
                break;
            }
        }
        return responseStream;
    }
    private async Task<Stream> GetContentLengthDecompressedStream(CancellationToken cancellationToken)
    {
        await using var compressedStream = GetZipStream(await ReceiveContentLength(cancellationToken).ConfigureAwait(false));
        var decompressedStream = new MemoryStream();
        await compressedStream.CopyToAsync(decompressedStream, cancellationToken);
        return decompressedStream;
    }

    private async Task<Stream> GetChunkedDecompressedStream(CancellationToken cancellationToken)
    {
        await using var compressedStream = GetZipStream(await ReceiveMessageBodyChunked(cancellationToken).ConfigureAwait(false));
        var decompressedStream = new MemoryStream();
        await compressedStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);
        return decompressedStream;
    }
    private async Task<Stream> GetResponseStreamUntilCloseDecompressed(CancellationToken cancellationToken)
    {
        await using var compressedStream = GetZipStream(await GetResponseStreamUntilClose(cancellationToken).ConfigureAwait(false));
        var decompressedStream = new MemoryStream();
        await compressedStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);
        return decompressedStream;
    }
    private async Task<Stream> ReceiveContentLength(CancellationToken cancellationToken)
    {
        var contentLengthStream = new MemoryStream(_contentLength);
        
        if (_contentLength == 0)
        {
            return contentLengthStream;
        }

        while (true)
        {
            var res = await _reader!.ReadAsync(cancellationToken).ConfigureAwait(false);

            var buff = res.Buffer;
            if (buff.IsSingleSegment)
            {
                contentLengthStream.Write(buff.FirstSpan);
            }
            else
            {
                foreach (var seg in buff)
                {
                    contentLengthStream.Write(seg.Span);
                }
            }
            _reader.AdvanceTo(buff.End);

            if (contentLengthStream.Length >= _contentLength)
            {
                return contentLengthStream;
            }

            if (res is { IsCanceled: false, IsCompleted: false })
            {
                continue;
            }
            
            await _reader.CompleteAsync();
            cancellationToken.ThrowIfCancellationRequested();

        }
    }
    
    private int GetContentLength()
    {
        if (!_contentHeaders!.TryGetValue("Content-Length", out var values))
        {
            return -1;
        }
        
        if (int.TryParse(values[0], out var length))
        {
            return length;
        }

        return -1;
    }

    private string GetContentEncoding()
    {
        var encoding = "";

        if (_contentHeaders!.TryGetValue("Content-Encoding", out var values))
        {
            encoding = values[0];
        }

        return encoding;
    }
    
    private async Task<Stream> ReceiveMessageBodyChunked(CancellationToken cancellationToken)
    {
        var chunkedDecoder = new ChunkedDecoderOptimized();
        while (true)
        {
            var res = await _reader!.ReadAsync(cancellationToken).ConfigureAwait(false);

            var buff = res.Buffer;
            chunkedDecoder.Decode(ref buff);
            _reader.AdvanceTo(buff.Start, buff.End);
            if (chunkedDecoder.Finished)
            {
                return chunkedDecoder.DecodedStream;
            }

            if (res is { IsCanceled: false, IsCompleted: false })
            {
                continue;
            }
            
            await _reader.CompleteAsync();
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private Stream GetZipStream(Stream stream)
    {
        var contentEncoding = GetContentEncoding().ToLower();
        stream.Seek(0, SeekOrigin.Begin);
        return contentEncoding switch
        {
            "gzip" => new GZipStream(stream, CompressionMode.Decompress, false),
            "deflate" => new DeflateStream(stream, CompressionMode.Decompress, false),
            "br" => new BrotliStream(stream, CompressionMode.Decompress, false),
            "zstd" => new ZstdSharp.DecompressionStream(stream, leaveOpen: false),
            _ => throw new InvalidOperationException($"'{contentEncoding}' not supported encoding format"),
        };
    }
}
