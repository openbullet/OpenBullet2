using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace RuriLib.Http.Tests.Utils;

internal static class Ja3
{
    public static string CalculateHash(byte[] handshakeRecord)
    {
        var ja3 = CalculateString(handshakeRecord);
        var hash = MD5.HashData(Encoding.ASCII.GetBytes(ja3));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string CalculateString(byte[] handshakeRecord)
    {
        var span = handshakeRecord.AsSpan();
        Assert.Equal((byte)0x01, span[0]);

        var offset = 4;
        var version = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset, 2));
        offset += 2 + 32;

        var sessionIdLength = span[offset];
        offset += 1 + sessionIdLength;

        var cipherSuitesLength = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset, 2));
        offset += 2;
        var cipherSuites = ReadUInt16List(span.Slice(offset, cipherSuitesLength));
        offset += cipherSuitesLength;

        var compressionMethodsLength = span[offset];
        offset += 1 + compressionMethodsLength;

        var extensionTypes = new List<ushort>();
        var supportedGroups = new List<ushort>();
        var ecPointFormats = new List<byte>();

        if (offset < span.Length)
        {
            var extensionsLength = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset, 2));
            offset += 2;
            var end = offset + extensionsLength;

            while (offset + 4 <= end)
            {
                var type = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset, 2));
                var length = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset + 2, 2));
                offset += 4;
                var data = span.Slice(offset, length);
                offset += length;

                if (!IsGrease(type))
                {
                    extensionTypes.Add(type);
                }

                if (type == 10 && data.Length >= 2)
                {
                    var groupBytes = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
                    supportedGroups.AddRange(ReadUInt16List(data.Slice(2, groupBytes)));
                }
                else if (type == 11 && data.Length >= 1)
                {
                    var formatLength = data[0];
                    ecPointFormats.AddRange(data.Slice(1, formatLength).ToArray());
                }
            }
        }

        return string.Join(',',
        [
            version.ToString(),
            JoinUInt16(cipherSuites.Where(x => !IsGrease(x))),
            JoinUInt16(extensionTypes),
            JoinUInt16(supportedGroups.Where(x => !IsGrease(x))),
            string.Join('-', ecPointFormats)
        ]);
    }

    private static List<ushort> ReadUInt16List(ReadOnlySpan<byte> span)
    {
        var values = new List<ushort>();

        for (var offset = 0; offset + 1 < span.Length; offset += 2)
        {
            values.Add(BinaryPrimitives.ReadUInt16BigEndian(span.Slice(offset, 2)));
        }

        return values;
    }

    private static string JoinUInt16(IEnumerable<ushort> values)
        => string.Join('-', values);

    private static bool IsGrease(ushort value)
        => (value & 0x0F0F) == 0x0A0A && (value >> 8) == (value & 0xFF);
}
