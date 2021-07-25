using RuriLib.Attributes;
using RuriLib.Functions.Conversion;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Numerics;
using System.Text;

namespace RuriLib.Blocks.Utility.Conversion
{
    [BlockCategory("Conversion", "Blocks for converting between different encodings", "#fad6a5")]
    public static class Methods
    {
        [Block("Converts a hex string to a byte array", name = "Hex => Bytes")]
        public static byte[] HexStringToByteArray(BotData data, [Variable] string hexString, bool addPadding = true)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Converting {hexString} to a byte array", LogColors.Flavescent);
            return HexConverter.ToByteArray(hexString, addPadding);
        }

        [Block("Converts a byte array to a hex string", name = "Bytes => Hex")]
        public static string ByteArrayToHexString(BotData data, [Variable] byte[] bytes)
        {
            var hex = HexConverter.ToHexString(bytes);
            data.Logger.LogHeader();
            data.Logger.Log($"Converted the byte array to {hex}", LogColors.Flavescent);
            return hex;
        }

        [Block("Converts a base64 string to a byte array", name = "Base64 => Bytes")]
        public static byte[] Base64StringToByteArray(BotData data, [Variable] string base64String, bool urlEncoded = false)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Converting {base64String} to a byte array", LogColors.Flavescent);
            return Base64Converter.ToByteArray(base64String, urlEncoded);
        }

        [Block("Converts a byte array to a base64 string", name = "Bytes => Base64")]
        public static string ByteArrayToBase64String(BotData data, [Variable] byte[] bytes, bool urlEncoded = false)
        {
            var b64 = Base64Converter.ToBase64String(bytes, urlEncoded);
            data.Logger.LogHeader();
            data.Logger.Log($"Converted the byte array to {b64}", LogColors.Flavescent);
            return b64;
        }

        [Block("Converts a (big) integer to a byte array", name = "Big Integer => Bytes")]
        public static byte[] BigIntegerToByteArray(BotData data, [Variable] string bigInteger)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Converting {bigInteger} to a byte array", LogColors.Flavescent);
            return BigInteger.Parse(bigInteger).ToByteArray();
        }

        [Block("Converts a byte array to a (big) integer", name = "Bytes => Big Integer")]
        public static string ByteArrayToBigInteger(BotData data, [Variable] byte[] bytes)
        {
            var bi = new BigInteger(bytes);
            data.Logger.LogHeader();
            data.Logger.Log($"Converted the byte array to {bi}", LogColors.Flavescent);
            return bi.ToString();
        }

        [Block("Converts a binary string to a byte array", name = "Binary String => Bytes")]
        public static byte[] BinaryStringToByteArray(BotData data, [Variable] string binaryString, bool addPadding = true)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Converting {binaryString} to a byte array", LogColors.Flavescent);
            return BinaryConverter.ToByteArray(binaryString, addPadding);
        }

        [Block("Converts a byte array to a binary string", name = "Bytes => Binary String")]
        public static string ByteArrayToBinaryString(BotData data, [Variable] byte[] bytes)
        {
            var bin = BinaryConverter.ToBinaryString(bytes);
            data.Logger.LogHeader();
            data.Logger.Log($"Converted the byte array to {bin}", LogColors.Flavescent);
            return bin;
        }

        [Block("Converts a UTF8 string to a base64 string", name = "UTF8 => Base64")]
        public static string UTF8ToBase64(BotData data, [Variable] string input)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            data.Logger.LogHeader();
            data.Logger.Log($"Encoded as base64: {base64}", LogColors.Flavescent);
            return base64;
        }

        [Block("Converts a base64 string to a UTF8 string", name = "Base64 => UTF8")]
        public static string Base64ToUTF8(BotData data, [Variable] string input)
        {
            // Pad the input if the length is not a multiple of 4
            var toDecode = input.Replace(".", "");
            var remainder = toDecode.Length % 4;
            if (remainder != 0)
            {
                toDecode = toDecode.PadRight(toDecode.Length + (4 - remainder), '=');
            }

            var utf8 = Encoding.UTF8.GetString(Convert.FromBase64String(toDecode));
            data.Logger.LogHeader();
            data.Logger.Log($"Encoded as UTF8: {utf8}", LogColors.Flavescent);
            return utf8;
        }

        [Block("Converts an encoded string to a byte array", name = "String => Bytes")]
        public static byte[] StringToBytes(BotData data, [Variable] string input, StringEncoding encoding = StringEncoding.UTF8)
        {
            data.Logger.LogHeader();
            var bytes = MapEncoding(encoding).GetBytes(input);
            data.Logger.Log($"Got bytes from {encoding} string", LogColors.Flavescent);
            return bytes;
        }

        [Block("Converts a byte array to an encoded string", name = "Bytes => String")]
        public static string BytesToString(BotData data, [Variable] byte[] input, StringEncoding encoding = StringEncoding.UTF8)
        {
            data.Logger.LogHeader();
            var str = MapEncoding(encoding).GetString(input);
            data.Logger.Log($"Decoded {encoding} string from byte array: {str}", LogColors.Flavescent);
            return str;
        }

        [Block("Converts a long number representing bytes into a readable string like 4.5 Gbit or 2.14 KiB")]
        public static string ReadableSize(BotData data, [Variable] string input,
            bool outputBits = false, bool binaryUnit = false, int decimalPlaces = 2)
        {
            var size = SizeConverter.ToReadableSize(long.Parse(input), outputBits, binaryUnit, decimalPlaces);
            data.Logger.LogHeader();
            data.Logger.Log($"Converted {input} bytes into the string {size}", LogColors.Flavescent);
            return size;
        }

        private static Encoding MapEncoding(StringEncoding encoding)
            => encoding switch
            {
                StringEncoding.UTF8 => Encoding.UTF8,
                StringEncoding.ASCII => Encoding.ASCII,
                StringEncoding.Unicode => Encoding.Unicode,
                StringEncoding.BigEndianUnicode => Encoding.BigEndianUnicode,
                StringEncoding.UTF32 => Encoding.UTF32,
                StringEncoding.Latin1 => Encoding.Latin1,
                _ => throw new NotImplementedException()
            };
    }

    public enum StringEncoding
    {
        UTF8,
        ASCII,
        Unicode,
        BigEndianUnicode,
        UTF32,
        Latin1
    }
}
