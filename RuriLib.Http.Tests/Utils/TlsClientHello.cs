using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace RuriLib.Http.Tests.Utils;

internal static class TlsClientHello
{
    public static IReadOnlyList<string> GetAlpnProtocols(byte[] handshakeRecord)
    {
        var protocols = new List<string>();

        foreach (var extension in EnumerateExtensions(handshakeRecord))
        {
            if (extension.Type != 16 || extension.Data.Length < 2)
            {
                continue;
            }

            var data = extension.Data.AsSpan();
            var protocolListLength = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
            var offset = 2;
            var end = Math.Min(data.Length, offset + protocolListLength);

            while (offset < end)
            {
                var length = data[offset++];

                if (offset + length > end)
                {
                    break;
                }

                protocols.Add(Encoding.ASCII.GetString(data.Slice(offset, length)));
                offset += length;
            }
        }

        return protocols;
    }

    public static IReadOnlyList<TlsExtension> EnumerateExtensions(byte[] handshakeRecord)
    {
        var span = handshakeRecord.AsSpan();

        if (span.Length < 4 || span[0] != 0x01)
        {
            throw new InvalidOperationException("TLS record is not a ClientHello handshake.");
        }

        var offset = 4;
        offset += 2 + 32;

        var sessionIdLength = span[offset];
        offset += 1 + sessionIdLength;

        var cipherSuitesLength = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset, 2));
        offset += 2 + cipherSuitesLength;

        var compressionMethodsLength = span[offset];
        offset += 1 + compressionMethodsLength;

        var extensions = new List<TlsExtension>();

        if (offset >= span.Length)
        {
            return extensions;
        }

        var extensionsLength = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset, 2));
        offset += 2;
        var end = offset + extensionsLength;

        while (offset + 4 <= end)
        {
            var type = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset, 2));
            var length = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset + 2, 2));
            offset += 4;

            extensions.Add(new TlsExtension(type, span.Slice(offset, length).ToArray()));
            offset += length;
        }

        return extensions;
    }
}

internal sealed record TlsExtension(ushort Type, byte[] Data);
