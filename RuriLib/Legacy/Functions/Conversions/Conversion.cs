using System;
using System.Linq;
using System.Text;

namespace RuriLib.Legacy.Functions.Conversions
{
    /// <summary>
    /// The available conversion formats.
    /// </summary>
    public enum Encoding
    {
        /// <summary>A hexadecimal representation of a byte array.</summary>
        HEX,

        /// <summary>A binary representation of a byte array, containing a multiple of 8 binary digits.</summary>
        BIN,

        /// <summary>A base64 representation of a byte array.</summary>
        BASE64,

        /// <summary>An ASCII string representation of a byte array.</summary>
        ASCII,

        /// <summary>A UTF8 string representation of a byte array.</summary>
        UTF8,

        /// <summary>A UTF16 Unicode string representation of a byte array.</summary>
        UNICODE
    }

    /// <summary>
    /// Provides methods to convert between different representations of binary data.
    /// </summary>
    public static class Conversion
    {
        /// <summary>
        /// Converts an encoded input to a byte array.
        /// </summary>
        /// <param name="input">The encoded input</param>
        /// <param name="encoding">The encoding</param>
        /// <returns>The converted byte array</returns>
        public static byte[] ConvertFrom(this string input, Encoding encoding)
        {
            switch (encoding)
            {
                case Encoding.BASE64:
                    return Convert.FromBase64String(input);

                case Encoding.HEX:
                    input = new string(input.ToCharArray()
                                .Where(c => !char.IsWhiteSpace(c))
                                .ToArray()).Replace("0x", "");
                    return Enumerable.Range(0, input.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(input.Substring(x, 2), 16))
                     .ToArray();

                case Encoding.BIN:
                    var numOfBytes = input.Length / 8;
                    var bytes = new byte[numOfBytes];
                    for (var i = 0; i < numOfBytes; ++i)
                    {
                        bytes[i] = Convert.ToByte(input.Substring(8 * i, 8), 2);
                    }
                    return bytes;

                case Encoding.ASCII:
                    return System.Text.Encoding.ASCII.GetBytes(input);

                case Encoding.UTF8:
                    return System.Text.Encoding.UTF8.GetBytes(input);

                case Encoding.UNICODE:
                    return System.Text.Encoding.Unicode.GetBytes(input);

                default:
                    return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Converts a byte array to an encoded string.
        /// </summary>
        /// <param name="input">The byte array to encode</param>
        /// <param name="encoding">The encoding</param>
        /// <returns>The encoded string</returns>
        public static string ConvertTo(this byte[] input, Encoding encoding)
        {
            var sb = new StringBuilder();

            switch (encoding)
            {
                case Encoding.BASE64:
                    return Convert.ToBase64String(input);

                case Encoding.HEX:
                    foreach (byte b in input)
                        sb.AppendFormat("{0:x2}", b);
                    return sb.ToString().ToUpper();

                case Encoding.BIN:
                    return string.Concat(input.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

                case Encoding.ASCII:
                    return System.Text.Encoding.ASCII.GetString(input);

                case Encoding.UTF8:
                    return System.Text.Encoding.UTF8.GetString(input);

                case Encoding.UNICODE:
                    return System.Text.Encoding.Unicode.GetString(input);

                default:
                    return "";
            }
        }
    }
}
