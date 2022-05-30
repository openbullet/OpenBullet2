using RuriLib.Http.Helpers;
using RuriLib.Http.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace RuriLib.Http
{
    internal class HttpResponseBuilder
    {
        private PipeReader reader;
        private const string newLine = "\r\n";
        private readonly byte[] CRLF = Encoding.UTF8.GetBytes(newLine);
        private static byte[] CRLFCRLF_Bytes = { 13, 10, 13, 10 };
        private HttpResponse response;
    
        private Dictionary<string, List<string>> contentHeaders;
        private int contentLength = -1;

        internal TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(10);

        internal HttpResponseBuilder()
        {
            //  pipe = new Pipe();
        }

        /// <summary>
        /// Builds an HttpResponse by reading a network stream.
        /// </summary>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveOptimization)]
        async internal Task<HttpResponse> GetResponseAsync(HttpRequest request, Stream stream,
            bool readResponseContent = true, CancellationToken cancellationToken = default)
        {
            reader = PipeReader.Create(stream);

            response = new HttpResponse
            {
                Request = request
            };

            contentHeaders = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            try
            {
                await ReceiveFirstLineAsync(cancellationToken).ConfigureAwait(false);
                await ReceiveHeadersAsync(cancellationToken).ConfigureAwait(false);

                if (request.Method != HttpMethod.Head)
                {
                    await ReceiveContentAsync(readResponseContent, cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                response.Dispose();
                throw;
            }

            return response;
        }

        // Parses the first line, for example
        // HTTP/1.1 200 OK
        private async Task ReceiveFirstLineAsync(CancellationToken cancellationToken = default)
        {
            var startingLine = string.Empty;

            // Read the first line from the Network Stream
            while (true)
            {
                var res = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                var buff = res.Buffer;
                int crlfIndex = buff.FirstSpan.IndexOf(CRLF);
                if (crlfIndex > -1)
                {
                    try
                    {
                        startingLine = Encoding.UTF8.GetString(res.Buffer.FirstSpan.Slice(0, crlfIndex));
                        var fields = startingLine.Split(' ');
                        response.Version = Version.Parse(fields[0].Trim()[5..]);
                        response.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), fields[1]);
                        buff = buff.Slice(0, crlfIndex + 2); // add 2 bytes for the CRLF
                        reader.AdvanceTo(buff.End); // advance to the consumed position
                        break;
                    }
                    catch
                    {
                        throw new FormatException($"Invalid first line of the HTTP response: {startingLine}");
                    }
                }
                else
                {
                    // the responce is incomplete ex. (HTTP/1.1 200 O)
                    reader.AdvanceTo(buff.Start, buff.End); // nothing consumed but all the buffer examined loop and read more.
                }
                if (res.IsCanceled || res.IsCompleted)
                {
                    reader.Complete();
                    cancellationToken.ThrowIfCancellationRequested();
                    break;
                }
            }
        }

        // Parses the headers
        private async Task ReceiveHeadersAsync(CancellationToken cancellationToken = default)
        {

            while (true)
            {
                var res = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                var buff = res.Buffer;
                if (buff.IsSingleSegment)
                {
                    if (ReadHeadersFastPath(ref buff))
                    {
                        reader.AdvanceTo(buff.Start);
                        break;
                    }

                }
                else
                {
                    if (ReadHeadersSlowerPath(ref buff))
                    {
                        reader.AdvanceTo(buff.Start);
                        break;
                    }
                }
                reader.AdvanceTo(buff.Start, buff.End); // not adding this line might result in infinit loop.
                if (res.IsCanceled || res.IsCompleted)
                {
                    reader.Complete();
                    cancellationToken.ThrowIfCancellationRequested();
                    break;
                }
            }
        }


        /// <summary>
        /// Reads all Header Lines using <see cref="Span{T}"/> For High Perfromace Parsing.
        /// </summary>
        /// <param name="buff">Buffered Data From Pipe</param>
        private bool ReadHeadersFastPath(ref ReadOnlySequence<byte> buff)
        {
            int endofheadersindex;

            if ((endofheadersindex = buff.FirstSpan.IndexOf(CRLFCRLF_Bytes)) > -1)
            {
                var spanLines = buff.FirstSpan.Slice(0, endofheadersindex + 4);
                var Lines = spanLines.SplitLines();// we use spanHelper class here to make a for each loop.

                foreach (var Line in Lines)
                {
                    ProcessHeaderLine(Line);
                }

                buff = buff.Slice(endofheadersindex + 4); // add 4 bytes for \r\n\r\n and to advance the pipe back in the calling method
                return true;
            }

            return false;
        }
        /// <summary>
        /// Reads all Header Lines using SequenceReader.
        /// </summary>
        /// <param name="buff">Buffered Data From Pipe</param>
        private bool ReadHeadersSlowerPath(ref ReadOnlySequence<byte> buff)
        {
            var reader = new SequenceReader<byte>(buff);

            while (reader.TryReadTo(out ReadOnlySpan<byte> Line, CRLF, true))
            {
                if (Line.Length == 0)// reached last crlf (empty line)
                {
                    buff = buff.Slice(reader.Position);
                    return true;// all headers received
                }
                ProcessHeaderLine(Line);
            }

            buff = buff.Slice(reader.Position);
            return false;// empty line not found need more data
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

            var headerName = Encoding.UTF8.GetString(header.Slice(0, separatorPos));
            var headerValuespan = header.Slice(separatorPos + 1); // skip ':'
            var headerValue = headerValuespan[0] == (byte)' ' ? Encoding.UTF8.GetString(headerValuespan.Slice(1)) : Encoding.UTF8.GetString(headerValuespan); // trim the white space

            // If the header is Set-Cookie, add the cookie
            if (headerName.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase) ||
                headerName.Equals("Set-Cookie2", StringComparison.OrdinalIgnoreCase))
            {
                SetCookie(response, headerValue);
            }
            // If it's a content header
            else if (ContentHelper.IsContentHeader(headerName))
            {
                if (contentHeaders.TryGetValue(headerName, out var values))
                {
                    values.Add(headerValue);
                }
                else
                {
                    values = new List<string>
                    {
                        headerValue
                    };

                    contentHeaders.Add(headerName, values);
                }
            }
            else
            {
                response.Headers[headerName] = headerValue;
            }
        }

        // Sets the value of a cookie
        private static void SetCookie(HttpResponse response, string value)
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
            var cookieName = value.Substring(0, separatorPos);

            if (endCookiePos == -1)
            {
                cookieValue = value[(separatorPos + 1)..];
            }
            else
            {
                cookieValue = value.Substring(separatorPos + 1, (endCookiePos - separatorPos) - 1);
            }

            response.Request.Cookies[cookieName] = cookieValue;
        }

        private async Task ReceiveContentAsync(bool readResponseContent = true, CancellationToken cancellationToken = default)
        {
            // If there are content headers
            if (contentHeaders.Count != 0)
            {
                contentLength = GetContentLength();

                if (readResponseContent)
                {
                    // Try to get the body and write it to a MemoryStream
                    var finaleResponceStream = await GetMessageBodySource(cancellationToken).ConfigureAwait(false);

                    // Rewind the stream and set the content of the response and its headers
                    finaleResponceStream.Seek(0, SeekOrigin.Begin);
                    response.Content = new StreamContent(finaleResponceStream);
                }
                else
                {
                    response.Content = new ByteArrayContent(Array.Empty<byte>());
                }
                
                foreach (var pair in contentHeaders)
                {
                    response.Content.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
                }
            }
        }

        private Task<Stream> GetMessageBodySource(CancellationToken cancellationToken)
        {
            if (response.Headers.ContainsKey("Transfer-Encoding"))
            {
                if (contentHeaders.ContainsKey("Content-Encoding"))
                {
                    return GetChunkedDecompressedStream(cancellationToken);
                }
                else
                {
                    return ReceiveMessageBodyChunked(cancellationToken);
                }
            }
            else if (contentLength > -1)
            {
                if (contentHeaders.ContainsKey("Content-Encoding"))
                {
                    return GetContentLengthDecompressedStream(cancellationToken);
                }
                else
                {
                    return ReciveContentLength(cancellationToken);

                }
            }
            else // handle the case where sever never sent chunked encoding nor content-length headrs (that is not allowed by rfc but whatever)
            {
                if (contentHeaders.ContainsKey("Content-Encoding"))
                {
                    return GetResponcestreamUntilCloseDecompressed(cancellationToken);
                }
                else
                {
                    return GetResponcestreamUntilClose(cancellationToken);
                }
            }
        }

    
        private async Task<Stream> GetResponcestreamUntilClose(CancellationToken cancellationToken)
        {
            var responcestream = new MemoryStream();
            while (true)
            {
                var res = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
             
                if (res.IsCanceled)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                var buff = res.Buffer;

                if (buff.IsSingleSegment)
                {
                    responcestream.Write(buff.FirstSpan);
                }
                else
                {
                    foreach (var seg in buff)
                    {
                        responcestream.Write(seg.Span);
                    }
                }
                reader.AdvanceTo(buff.End);
                if (res.IsCompleted || res.Buffer.Length == 0)// here the pipe will be complete if the server closes the connection or sends 0 length byte array
                {
                    break;
                }
            }
          
            return responcestream;
        }

        private async Task<Stream> GetContentLengthDecompressedStream(CancellationToken cancellationToken)
        {
            using var compressedStream = GetZipStream(await ReciveContentLength(cancellationToken).ConfigureAwait(false));
            var decompressedStream = new MemoryStream();
            await compressedStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);
            return decompressedStream;
        }

        private async Task<Stream> GetChunkedDecompressedStream(CancellationToken cancellationToken)
        {
            using var compressedStream = GetZipStream(await ReceiveMessageBodyChunked(cancellationToken).ConfigureAwait(false));
            var decompressedStream = new MemoryStream();
            await compressedStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);
            return decompressedStream;
        }
        private async Task<Stream> GetResponcestreamUntilCloseDecompressed(CancellationToken cancellationToken)
        {
            using var compressedStream = GetZipStream(await GetResponcestreamUntilClose(cancellationToken).ConfigureAwait(false));
            var decompressedStream = new MemoryStream();
            await compressedStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);
            return decompressedStream;
        }

        private async Task<Stream> ReciveContentLength(CancellationToken cancellationToken)
        {
            var contentlenghtStream = new MemoryStream(contentLength == -1 ? 0 : contentLength);
            if (contentLength == 0)
            {
                return contentlenghtStream;
            }

            while (true)
            {
                var res = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                var buff = res.Buffer;
                if (buff.IsSingleSegment)
                {
                    contentlenghtStream.Write(buff.FirstSpan);
                }
                else
                {
                    foreach (var seg in buff)
                    {
                        contentlenghtStream.Write(seg.Span);
                    }
                }
                reader.AdvanceTo(buff.End);
                if (contentlenghtStream.Length >= contentLength)
                {
                    return contentlenghtStream;
                }
                if (res.IsCanceled || res.IsCompleted)
                {
                    reader.Complete();
                    cancellationToken.ThrowIfCancellationRequested();
                    break;
                }
            }
            return contentlenghtStream;
        }

        private int GetContentLength()
        {
            if (contentHeaders.TryGetValue("Content-Length", out var values))
            {
                if (int.TryParse(values[0], out var length))
                {
                    return length;
                }
            }

            return -1;
        }

        private string GetContentEncoding()
        {
            var encoding = "";

            if (contentHeaders.TryGetValue("Content-Encoding", out var values))
            {
                encoding = values[0];
            }

            return encoding;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private async Task<Stream> ReceiveMessageBodyChunked(CancellationToken cancellationToken)
        {
            var chunkedDecoder = new ChunkedDecoderOptimized();
            while (true)
            {
                var res = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                var buff = res.Buffer;
                chunkedDecoder.Decode(ref buff);
                reader.AdvanceTo(buff.Start, buff.End);
                if (chunkedDecoder.Finished)
                {
                    return chunkedDecoder.DecodedStream;
                }
                if (res.IsCanceled || res.IsCompleted)
                {
                    reader.Complete();
                    cancellationToken.ThrowIfCancellationRequested();
                    break;
                }
            }
            return chunkedDecoder.DecodedStream;
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
                "utf-8" => stream,
                _ => throw new InvalidOperationException($"'{contentEncoding}' not supported encoding format"),
            };
        }
    }
}
