using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace RuriLib.Http.Helpers;

internal static class ContentEncodingHelper
{
    private static readonly HashSet<string> IgnoredEncodings = new(StringComparer.OrdinalIgnoreCase)
    {
        "",
        "identity",
        "none",
        "utf-8",
        "true",
        "false"
    };

    internal static Stream GetDecodedStream(Stream stream, IEnumerable<string> headerValues)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(headerValues);

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        var encodings = GetEncodings(headerValues);
        if (encodings.Count == 0)
        {
            return stream;
        }

        Stream current = stream;

        for (var i = encodings.Count - 1; i >= 0; i--)
        {
            current = WrapStream(current, encodings[i]);
        }

        return current;
    }

    private static List<string> GetEncodings(IEnumerable<string> headerValues)
    {
        var encodings = new List<string>();

        foreach (var headerValue in headerValues)
        {
            foreach (var rawEncoding in headerValue.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var encoding = rawEncoding.ToLowerInvariant();

                if (IgnoredEncodings.Contains(encoding))
                {
                    continue;
                }

                if (!IsSupported(encoding))
                {
                    throw new InvalidOperationException($"'{encoding}' not supported encoding format");
                }

                encodings.Add(encoding);
            }
        }

        return encodings;
    }

    private static bool IsSupported(string encoding)
        => encoding is "gzip" or "deflate" or "br" or "zstd";

    private static Stream WrapStream(Stream stream, string encoding)
        => encoding switch
        {
            "gzip" => new GZipStream(stream, CompressionMode.Decompress, false),
            "deflate" => new DeflateStream(stream, CompressionMode.Decompress, false),
            "br" => new BrotliStream(stream, CompressionMode.Decompress, false),
            "zstd" => new ZstdSharp.DecompressionStream(stream, leaveOpen: false),
            _ => throw new InvalidOperationException($"'{encoding}' not supported encoding format")
        };
}
