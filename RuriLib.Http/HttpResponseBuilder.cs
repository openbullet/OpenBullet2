using RuriLib.Http.Helpers;
using RuriLib.Http.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Http
{
    internal class HttpResponseBuilder
    {
        private sealed class BytesWrapper
        {
            public int Length { get; set; }

            public byte[] Value { get; set; }
        }

        private static readonly byte[] openHtmlSignature = Encoding.ASCII.GetBytes("<html");
        private static readonly byte[] closeHtmlSignature = Encoding.ASCII.GetBytes("</html>");

        private readonly int bufferSize = 1024;
        private readonly string newLine = "\r\n";

        private readonly ReceiveHelper receiveHelper;
        private HttpResponse response;
        private NetworkStream networkStream;
        private Stream commonStream;
        private Dictionary<string, List<string>> contentHeaders;
        private int contentLength = -1;

        internal TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(10);

        internal HttpResponseBuilder()
        {
            receiveHelper = new ReceiveHelper(bufferSize);
        }

        /// <summary>
        /// Builds an HttpResponse by reading a network stream.
        /// </summary>
        async internal Task<HttpResponse> GetResponseAsync(HttpRequest request, Stream stream,
            CancellationToken cancellationToken = default)
        {
            networkStream = stream as NetworkStream;
            commonStream = stream;

            receiveHelper.Init(stream);

            response = new HttpResponse
            {
                Request = request
            };

            contentHeaders = new Dictionary<string, List<string>>();

            await ReceiveFirstLineAsync(cancellationToken).ConfigureAwait(false);
            await ReceiveHeadersAsync(cancellationToken).ConfigureAwait(false);
            await ReceiveContentAsync(cancellationToken).ConfigureAwait(false);

            return response;
        }

        // Parses the first line, for example
        // HTTP/1.1 200 OK
        private async Task ReceiveFirstLineAsync(CancellationToken cancellationToken = default)
        {
            var startingLine = string.Empty;

            while (string.IsNullOrEmpty(startingLine))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Read the first line from the Network Stream
                startingLine = await receiveHelper.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            }

            try
            {
                var fields = startingLine.Split(' ');
                response.Version = Version.Parse(fields[0].Trim()[5..]);
                response.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), fields[1]);
            }
            catch
            {
                throw new FormatException($"Invalid first line of the HTTP response: {startingLine}");
            }
        }

        // Parses the headers
        private async Task ReceiveHeadersAsync(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var header = await receiveHelper.ReadLineAsync(cancellationToken).ConfigureAwait(false);

                // If we ran out of headers (there is always a blank line after the headers)
                // we're done so we can exit.
                if (header == newLine)
                {
                    break;
                }

                var separatorPos = header.IndexOf(':');

                // If we cannot find the ':' character in a header, skip to the next one
                if (separatorPos == -1)
                {
                    continue;
                }

                var headerName = header.Substring(0, separatorPos);
                var headerValue = header[(separatorPos + 1)..].Trim(' ', '\t', '\r', '\n');

                // If the header is Set-Cookie, add the cookie
                if (headerName.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
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
                throw new FormatException($"Invalid cookie format: {value}");
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

        // TODO: Make this async (need to refactor the mess below)
        private Task ReceiveContentAsync(CancellationToken cancellationToken = default)
        {
            // If there are content headers
            if (contentHeaders.Count != 0)
            {
                contentLength = GetContentLength();

                var memoryStream = new MemoryStream(contentLength == -1 ? 0 : contentLength);

                // Try to get the body and write it to a MemoryStream
                var source = GetMessageBodySource();
                foreach (var bytes in source)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    memoryStream.Write(bytes.Value, 0, bytes.Length);
                }

                // Rewind the stream and set the content of the response and its headers
                memoryStream.Seek(0, SeekOrigin.Begin);
                response.Content = new StreamContent(memoryStream);
                foreach (var pair in contentHeaders)
                {
                    response.Content.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
                }
            }

            return Task.CompletedTask;
        }

        private IEnumerable<BytesWrapper> GetMessageBodySource()
        {
            if (contentHeaders.ContainsKey("Content-Encoding"))
            {
                return GetMessageBodySourceZip();
            }

            return GetMessageBodySourceStd();
        }

        private IEnumerable<BytesWrapper> GetMessageBodySourceZip()
        {
            if (response.Headers.ContainsKey("Transfer-Encoding"))
            {
                return ReceiveMessageBodyChunkedZip();
            }

            if (contentLength != -1)
            {
                return ReceiveMessageBodyZip(contentLength);
            }

            var streamWrapper = new ZipWrapperStream(commonStream, receiveHelper);

            return ReceiveMessageBody(GetZipStream(streamWrapper));
        }

        private IEnumerable<BytesWrapper> GetMessageBodySourceStd()
        {
            if (response.Headers.ContainsKey("Transfer-Encoding"))
            {
                return ReceiveMessageBodyChunked();
            }
            if (contentLength != -1)
            {
                return ReceiveMessageBody(contentLength);
            }

            return ReceiveMessageBody(commonStream);
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

        // TODO: Refactor the mess below

        #region Receive Content (F*cking trash, but works (not sure (really)))
        // Загрузка тела сообщения неизвестной длины.
        private IEnumerable<BytesWrapper> ReceiveMessageBody(Stream stream)
        {
            var bytesWraper = new BytesWrapper();
            var buffer = new byte[bufferSize];
            bytesWraper.Value = buffer;
            var begBytesRead = 0;

            // Считываем начальные данные из тела сообщения.
            if (stream is GZipStream || stream is DeflateStream)
            {
                begBytesRead = stream.Read(buffer, 0, bufferSize);
            }
            else
            {
                if (receiveHelper.HasData)
                {
                    begBytesRead = receiveHelper.Read(buffer, 0, bufferSize);
                }
                if (begBytesRead < bufferSize)
                {
                    begBytesRead += stream.Read(buffer, begBytesRead, bufferSize - begBytesRead);
                }
            }
            // Возвращаем начальные данные.
            bytesWraper.Length = begBytesRead;
            yield return bytesWraper;
            // Проверяем, есть ли открывающий тег '<html'.
            // Если есть, то считываем данные то тех пор, пока не встретим закрывающий тек '</html>'.
            bool isHtml = FindSignature(buffer, begBytesRead, openHtmlSignature);
            if (isHtml)
            {
                bool found = FindSignature(buffer, begBytesRead, closeHtmlSignature);
                // Проверяем, есть ли в начальных данных закрывающий тег.
                if (found)
                {
                    yield break;
                }
            }
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, bufferSize);
                // Если тело сообщения представляет HTML.
                if (isHtml)
                {
                    if (bytesRead == 0)
                    {
                        WaitData();
                        continue;
                    }
                    bool found = FindSignature(buffer, bytesRead, closeHtmlSignature);
                    if (found)
                    {
                        bytesWraper.Length = bytesRead;
                        yield return bytesWraper;
                        yield break;
                    }
                }
                else if (bytesRead == 0)
                {
                    yield break;
                }
                bytesWraper.Length = bytesRead;
                yield return bytesWraper;
            }
        }

        // Загрузка тела сообщения известной длины.
        private IEnumerable<BytesWrapper> ReceiveMessageBody(int contentLength)
        {
            //Stream stream = _request.ClientStream;
            var bytesWraper = new BytesWrapper();
            byte[] buffer = new byte[bufferSize];
            bytesWraper.Value = buffer;

            int totalBytesRead = 0;
            while (totalBytesRead != contentLength)
            {
                int bytesRead;
                if (receiveHelper.HasData)
                {
                    bytesRead = receiveHelper.Read(buffer, 0, bufferSize);
                }
                else
                {
                    bytesRead = commonStream.Read(buffer, 0, bufferSize);
                }
                if (bytesRead == 0)
                {
                    WaitData();
                }
                else
                {
                    totalBytesRead += bytesRead;
                    bytesWraper.Length = bytesRead;
                    yield return bytesWraper;
                }
            }
        }

        // Загрузка тела сообщения частями.
        private IEnumerable<BytesWrapper> ReceiveMessageBodyChunked()
        {
            //Stream stream = _request.ClientStream;
            var bytesWraper = new BytesWrapper();
            byte[] buffer = new byte[this.bufferSize];
            bytesWraper.Value = buffer;
            while (true)
            {
                string line = receiveHelper.ReadLineAsync().Result;
                // Если достигнут конец блока.
                if (line == newLine)
                {
                    continue;
                }

                line = line.Trim(' ', '\r', '\n');
                // Если достигнут конец тела сообщения.
                if (line == string.Empty)
                {
                    yield break;
                }

                int blockLength;
                int totalBytesRead = 0;
                #region Задаём длину блока
                try
                {
                    blockLength = Convert.ToInt32(line, 16);
                }
                catch (Exception ex)
                {
                    if (ex is FormatException || ex is OverflowException)
                    {
                        //throw NewHttpException(string.Format(
                        //Resources.HttpException_WrongChunkedBlockLength, line), ex);
                    }
                    throw;
                }
                #endregion
                // Если достигнут конец тела сообщения.
                if (blockLength == 0)
                {
                    yield break;
                }

                while (totalBytesRead != blockLength)
                {
                    int length = blockLength - totalBytesRead;
                    if (length > bufferSize)
                    {
                        length = bufferSize;
                    }
                    int bytesRead;
                    if (receiveHelper.HasData)
                    {
                        bytesRead = receiveHelper.Read(buffer, 0, length);
                    }
                    else
                    {
                        bytesRead = commonStream.Read(buffer, 0, length);
                    }
                    if (bytesRead == 0)
                    {
                        WaitData();
                    }
                    else
                    {
                        totalBytesRead += bytesRead;
                        bytesWraper.Length = bytesRead;
                        yield return bytesWraper;
                    }
                }
            }
        }

        private IEnumerable<BytesWrapper> ReceiveMessageBodyZip(int contentLength)
        {
            var bytesWraper = new BytesWrapper();
            var streamWrapper = new ZipWrapperStream(commonStream, receiveHelper);
            using (Stream stream = GetZipStream(streamWrapper))
            {
                byte[] buffer = new byte[bufferSize];
                bytesWraper.Value = buffer;

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, bufferSize);
                    if (bytesRead == 0)
                    {
                        if (streamWrapper.TotalBytesRead == contentLength)
                        {
                            yield break;
                        }
                        else
                        {
                            WaitData();
                            continue;
                        }
                    }
                    bytesWraper.Length = bytesRead;
                    yield return bytesWraper;
                }
            }
        }

        private IEnumerable<BytesWrapper> ReceiveMessageBodyChunkedZip()
        {
            var bytesWraper = new BytesWrapper();
            var streamWrapper = new ZipWrapperStream(commonStream, receiveHelper);

            using (Stream stream = GetZipStream(streamWrapper))
            {
                byte[] buffer = new byte[bufferSize];
                bytesWraper.Value = buffer;
                while (true)
                {
                    string line = receiveHelper.ReadLineAsync().Result;
                    // Если достигнут конец блока.
                    if (line == newLine)
                    {
                        continue;
                    }

                    line = line.Trim(' ', '\r', '\n');
                    // Если достигнут конец тела сообщения.
                    if (line == string.Empty)
                    {
                        yield break;
                    }

                    int blockLength;
                    #region Задаём длину блока
                    try
                    {
                        blockLength = Convert.ToInt32(line, 16);
                    }
                    catch (Exception ex)
                    {
                        if (ex is FormatException || ex is OverflowException)
                        {
                            //throw NewHttpException(string.Format(
                            //Resources.HttpException_WrongChunkedBlockLength, line), ex);
                        }
                        throw;
                    }
                    #endregion
                    // Если достигнут конец тела сообщения.
                    if (blockLength == 0)
                    {
                        yield break;
                    }

                    streamWrapper.TotalBytesRead = 0;
                    streamWrapper.LimitBytesRead = blockLength;
                    while (true)
                    {
                        int bytesRead = stream.Read(buffer, 0, bufferSize);
                        if (bytesRead == 0)
                        {
                            if (streamWrapper.TotalBytesRead == blockLength)
                            {
                                break;
                            }
                            else
                            {
                                WaitData();
                                continue;
                            }
                        }
                        bytesWraper.Length = bytesRead;
                        yield return bytesWraper;
                    }
                }
            }
        }

        private Stream GetZipStream(Stream stream)
        {
            var contentEncoding = GetContentEncoding().ToLower();

            return contentEncoding switch
            {
                "gzip" => new GZipStream(stream, CompressionMode.Decompress, true),
                "deflate" => new DeflateStream(stream, CompressionMode.Decompress, true),
                "br" => new BrotliStream(stream, CompressionMode.Decompress, true),
                _ => throw new InvalidOperationException($"'{contentEncoding}' not supported encoding format"),
            };
        }

        private bool FindSignature(byte[] source, int sourceLength, byte[] signature)
        {
            int length = (sourceLength - signature.Length) + 1;
            for (int sourceIndex = 0; sourceIndex < length; ++sourceIndex)
            {
                for (int signatureIndex = 0; signatureIndex < signature.Length; ++signatureIndex)
                {
                    byte sourceByte = source[signatureIndex + sourceIndex];
                    char sourceChar = (char)sourceByte;
                    if (char.IsLetter(sourceChar))
                    {
                        sourceChar = char.ToLower(sourceChar);
                    }
                    sourceByte = (byte)sourceChar;
                    if (sourceByte != signature[signatureIndex])
                    {
                        break;
                    }
                    else if (signatureIndex == (signature.Length - 1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Help Methods
        private void WaitData()
        {
            var sleepTime = 0;
            var delay = Math.Max(10, (int)ReceiveTimeout.TotalMilliseconds);

            var dataAvailable = networkStream?.DataAvailable;
            while (dataAvailable != null && !dataAvailable.Value)
            {
                if (sleepTime >= delay)
                {
                    throw new TimeoutException("Timed out while waiting for the data");
                }

                sleepTime += 10;
                Thread.Sleep(10);
            }
        }

        private string GetTransferEncoding()
        {
            if (ContentHeaderExists("Transfer-Encoding", out var name))
            {
                return contentHeaders[name][0];
            }

            return string.Empty;
        }

        private bool ContentHeaderExists(string name, out string actualName)
        {
            var key = contentHeaders.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
            actualName = key;
            return key != null;
        }
        #endregion
    }
}
