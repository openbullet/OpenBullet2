using System;
using System.Text;

namespace RuriLib.Functions.Conversion
{
    public static class Base64Converter
    {
        /// <summary>
        /// Removes the dots that sometimes come with base64 strings from the web
        /// to split the different sections of the encoded data.
        /// </summary>
        public static byte[] ToByteArray(string base64String, bool urlEncoded = false)
        {
            return urlEncoded 
                ? UrlDecode(base64String)
                : Convert.FromBase64String(base64String.Replace(".", ""));
        }

        public static string ToBase64String(byte[] bytes, bool urlEncoded = false)
        {
            return urlEncoded
                ? UrlEncode(bytes)
                : Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Encodes data to a Base64 URL-encoded string.
        /// </summary>
        // Source: https://stackoverflow.com/questions/1228701/code-for-decoding-encoding-a-modified-base64-url
        private static string UrlEncode(byte[] bytes)
        {
            string s = Convert.ToBase64String(bytes);
            s = s.Split('=')[0];
            s = s.Replace('+', '-');
            s = s.Replace('/', '_');
            return s;
        }

        /// <summary>
        /// Decodes an URL-encoded Base64 string.
        /// </summary>
        // Source: https://stackoverflow.com/questions/1228701/code-for-decoding-encoding-a-modified-base64-url
        private static byte[] UrlDecode(string base64String)
        {
            string s = new StringBuilder(base64String)
                .Replace('-', '+')
                .Replace('_', '/')
                .ToString();
            
            switch (s.Length % 4)
            {
                case 0: break;
                case 2: s += "=="; break;
                case 3: s += "="; break;
                default:
                    throw new ArgumentException("Illegal base64url string!");
            }
            
            return Convert.FromBase64String(s);
        }
    }
}
