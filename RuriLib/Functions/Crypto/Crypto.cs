using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Numerics;
using System.Text;
using System.Globalization;
using BCrypt.Net;

namespace RuriLib.Functions.Crypto
{
    /// <summary>
    /// The available hashing functions.
    /// </summary>
    public enum HashFunction
    {
        /// <summary>The MD4 hashing function (128 bits digest).</summary>
        MD4,

        /// <summary>The MD5 hashing function (128 bits digest).</summary>
        MD5,

        /// <summary>The SHA-1 hashing function (160 bits digest).</summary>
        SHA1,

        /// <summary>The SHA-256 hashing function (256 bits digest).</summary>
        SHA256,

        /// <summary>The SHA-384 hashing function (384 bits digest).</summary>
        SHA384,

        /// <summary>The SHA-512 hashing function (512 bits digest).</summary>
        SHA512,
    }

    /// <summary>
    /// Provides methods for encrypting, decrypting and generating signatures.
    /// </summary>
    public static class Crypto
    {
        #region XOR
        /// <summary>
        /// XOR operation between byte arrays.
        /// </summary>
        public static byte[] XOR(byte[] bytes, byte[] key)
        {
            var bytesLen = bytes.Length;
            var keyLen = key.Length;
            var result = new byte[key.Length];

            for (var i = 0; i < bytesLen; i++)
            {
                result[i] = (byte)(bytes[i] ^ key[i % keyLen]);
            }

            return result;
        }

        /// <summary>
        /// XOR operation between strings (treated as char arrays).
        /// </summary>
        public static string XORStrings(string text, string key)
        {
            var textLen = text.Length;
            var keyLen = key.Length;
            var buffer = new char[textLen];

            for (var i = 0; i < textLen; ++i)
            {
                buffer[i] = (char)(text[i] ^ key[i % keyLen]);
            }

            return new string(buffer);
        }
        #endregion

        #region Hash and Hmac
        /// <summary>
        /// Hashes a string through NTLM.
        /// </summary>
        /// <param name="input">The string to hash</param>
        /// <returns>The NTLM digest.</returns>
        public static byte[] NTLM(string input)
        {
            // Unicode with little endian
            var bytes = Encoding.Unicode.GetBytes(input);
            return MD4(bytes);
        }

        /// <summary>
        /// Hashes a byte array through MD4.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The MD4 digest.</returns>
        public static byte[] MD4(byte[] input)
        {
            // get padded uints from bytes
            var bytes = input.ToList();
            uint bitCount = (uint)(bytes.Count) * 8;
            bytes.Add(128);
            while (bytes.Count % 64 != 56) bytes.Add(0);
            var uints = new List<uint>();
            for (int i = 0; i + 3 < bytes.Count; i += 4)
                uints.Add(bytes[i] | (uint)bytes[i + 1] << 8 | (uint)bytes[i + 2] << 16 | (uint)bytes[i + 3] << 24);
            uints.Add(bitCount);
            uints.Add(0);

            // run rounds
            uint a = 0x67452301, b = 0xefcdab89, c = 0x98badcfe, d = 0x10325476;
            Func<uint, uint, uint> rol = (x, y) => x << (int)y | x >> 32 - (int)y;
            for (int q = 0; q + 15 < uints.Count; q += 16)
            {
                var chunk = uints.GetRange(q, 16);
                uint aa = a, bb = b, cc = c, dd = d;
                Action<Func<uint, uint, uint, uint>, uint[]> round = (f, y) =>
                {
                    foreach (uint i in new[] { y[0], y[1], y[2], y[3] })
                    {
                        a = rol(a + f(b, c, d) + chunk[(int)(i + y[4])] + y[12], y[8]);
                        d = rol(d + f(a, b, c) + chunk[(int)(i + y[5])] + y[12], y[9]);
                        c = rol(c + f(d, a, b) + chunk[(int)(i + y[6])] + y[12], y[10]);
                        b = rol(b + f(c, d, a) + chunk[(int)(i + y[7])] + y[12], y[11]);
                    }
                };
                round((x, y, z) => (x & y) | (~x & z), new uint[] { 0, 4, 8, 12, 0, 1, 2, 3, 3, 7, 11, 19, 0 });
                round((x, y, z) => (x & y) | (x & z) | (y & z), new uint[] { 0, 1, 2, 3, 0, 4, 8, 12, 3, 5, 9, 13, 0x5a827999 });
                round((x, y, z) => x ^ y ^ z, new uint[] { 0, 2, 1, 3, 0, 8, 4, 12, 3, 9, 11, 15, 0x6ed9eba1 });
                a += aa; b += bb; c += cc; d += dd;
            }

            // return bytes
            return new[] { a, b, c, d }.SelectMany(BitConverter.GetBytes).ToArray();
        }

        /// <summary>
        /// Hashes a byte array through MD5.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The MD5 digest.</returns>
        public static byte[] MD5(byte[] input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            return md5.ComputeHash(input);
        }

        /// <summary>
        /// Calculates an MD5 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACMD5(byte[] input, byte[] key)
        {
            using var hmac = new HMACMD5(key);
            return hmac.ComputeHash(input);
        }

        /// <summary>
        /// Hashes a byte array through SHA-1.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA-1 digest.</returns>
        public static byte[] SHA1(byte[] input)
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            return sha1.ComputeHash(input);
        }

        /// <summary>
        /// Calculates a SHA-1 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACSHA1(byte[] input, byte[] key)
        {
            using var hmac = new HMACSHA1(key);
            return hmac.ComputeHash(input);
        }

        /// <summary>
        /// Hashes a byte array through SHA-256.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA-256 digest.</returns>
        public static byte[] SHA256(byte[] input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(input);
        }
        
        /// <summary>
        /// Overload for method below that calculates a SHA-256 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        private static byte[] Hmac(string input, byte[] key) => HMACSHA256(Encoding.UTF8.GetBytes(input), key);

        /// <summary>
        /// Calculates a SHA-256 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACSHA256(byte[] input, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(input);
        }

        /// <summary>
        /// Hashes a byte array through SHA-384.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA-384 digest.</returns>
        public static byte[] SHA384(byte[] input)
        {
            using var sha384 = System.Security.Cryptography.SHA384.Create();
            return sha384.ComputeHash(input);
        }

        /// <summary>
        /// Calculates a SHA-384 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACSHA384(byte[] input, byte[] key)
        {
            using var hmac = new HMACSHA384(key);
            return hmac.ComputeHash(input);
        }

        /// <summary>
        /// Hashes a byte array through SHA-512.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA-512 digest.</returns>
        public static byte[] SHA512(byte[] input)
        {
            using var sha512 = System.Security.Cryptography.SHA512.Create();
            return sha512.ComputeHash(input);
        }

        /// <summary>
        /// Calculates a SHA-512 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACSHA512(byte[] input, byte[] key)
        {
            using var hmac = new HMACSHA512(key);
            return hmac.ComputeHash(input);
        }

        /// <summary>
        /// Converts from the Hash enum to the HashAlgorithmName default struct.
        /// </summary>
        /// <param name="type">The hash type as a Hash enum</param>
        /// <returns>The HashAlgorithmName equivalent.</returns>
        public static HashAlgorithmName ToHashAlgorithmName(this HashFunction type)
        {
            switch (type)
            {
                case HashFunction.MD5:
                    return HashAlgorithmName.MD5;

                case HashFunction.SHA1:
                    return HashAlgorithmName.SHA1;

                case HashFunction.SHA256:
                    return HashAlgorithmName.SHA256;

                case HashFunction.SHA384:
                    return HashAlgorithmName.SHA384;

                case HashFunction.SHA512:
                    return HashAlgorithmName.SHA512;

                default:
                    throw new NotSupportedException("No such algorithm name");
            }
        }
        #endregion

        #region RSA
        /// <summary>
        /// Encrypts data using RSA.
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <param name="n">The public key's modulus</param>
        /// <param name="e">The public key's exponent</param>
        /// <param name="oaep">Whether to use OAEP-SHA1 padding mode instead of PKCS1</param>
        public static byte[] RSAEncrypt(byte[] data, byte[] n, byte[] e, bool oaep)
        {
            using RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(new RSAParameters
            {
                Modulus = n,
                Exponent = e
            });
            return RSA.Encrypt(data, oaep);
        }

        /// <summary>
        /// Decrypts data using RSA.
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <param name="n">The public key's modulus</param>
        /// <param name="d">The private key's exponent</param>
        /// <param name="oaep">Whether to use OAEP-SHA1 padding mode instead of PKCS1</param>
        public static byte[] RSADecrypt(byte[] data, byte[] n, byte[] d, bool oaep)
        {
            using RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(new RSAParameters
            {
                Modulus = n,
                D = d
            });
            return RSA.Decrypt(data, oaep);
        }

        /// <summary>
        /// Encrypts a message using RSA with PKCS1PAD2 padding.
        /// </summary>
        /// <param name="message">The message as a UTF-8 string</param>
        /// <param name="hexModulus">The public key's modulus as a Hexadecimal string</param>
        /// <param name="hexExponent">The public key's exponent as a Hexadecimal string</param>
        // Thanks to TheLittleTrain for this implementation.
        public static byte[] RSAPkcs1Pad2(string message, string hexModulus, string hexExponent)
        {
            // Convert the public key components to numbers
            // NOTE: A BigInteger is parsed in two's complement and little endian, so we add
            // a 0x00 byte at the start to make the number positive
            var n = BigInteger.Parse("00" + hexModulus, NumberStyles.AllowHexSpecifier);
            var e = BigInteger.Parse("00" + hexExponent, NumberStyles.AllowHexSpecifier);

            // (modulus.ToByteArray().Length - 1) * 8
            // modulus has 256 bits multiplied by 8 bits equals 2048
            var encryptedNumber = Pkcs1Pad2(message, (2048 + 7) >> 3);

            // And now, the RSA encryption
            encryptedNumber = BigInteger.ModPow(encryptedNumber, e, n);
            var outputArray = encryptedNumber.ToByteArray();

            // Reverse the array since it's stored as little endian in the BigInteger
            Array.Reverse(outputArray);
            return outputArray;
        }

        private static BigInteger Pkcs1Pad2(string data, int keySize)
        {
            if (keySize < data.Length + 11)
                return new BigInteger();

            var buffer = new byte[256];
            var i = data.Length - 1;

            while (i >= 0 && keySize > 0)
            {
                buffer[--keySize] = (byte)data[i--];
            }

            // Padding, I think
            var random = new Random();
            buffer[--keySize] = 0;
            while (keySize > 2)
            {
                buffer[--keySize] = (byte)random.Next(1, 256);
                //buffer[--keySize] = 5;
            }

            buffer[--keySize] = 2;
            buffer[--keySize] = 0;

            Array.Reverse(buffer);

            return new BigInteger(buffer);
        }
        #endregion

        #region KDF
        /// <summary>
        /// Generates a PKCS v5 #2.0 key using a Password-Based Key Derivation Function.
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <param name="salt">The salt to use. If null, a random salt will be generated</param>
        /// <param name="saltSize">The random salt size that gets generated in case no salt is provided</param>
        /// <param name="iterations">The number of times the algorithm should be executed</param>
        /// <param name="type">The hashing algorithm to use</param>
        /// <param name="keyLength">The generated key length in bytes</param>
        public static byte[] PBKDF2PKCS5(byte[] password, byte[] salt = null, int saltSize = 8, int iterations = 1, int keyLength = 16, HashFunction type = HashFunction.SHA1)
        {
            if (salt.Length > 0)
            {
                using var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, type.ToHashAlgorithmName());
                return deriveBytes.GetBytes(keyLength);
            }
            else
            {
                // Generate a random salt
                var randomSalt = new byte[saltSize];
                RandomNumberGenerator.Create().GetBytes(randomSalt);
                using var deriveBytes = new Rfc2898DeriveBytes(password, randomSalt, iterations, type.ToHashAlgorithmName());
                return deriveBytes.GetBytes(keyLength);
            }
        }
        #endregion

        #region AES
        /// <summary>
        /// Encrypts data with AES.
        /// </summary>
        /// <param name="plainText">The plaintext data</param>
        /// <param name="key">The encryption key</param>
        /// <param name="iv">The initial value</param>
        /// <param name="mode">The cipher mode</param>
        /// <param name="padding">The padding mode</param>
        public static byte[] AESEncrypt(byte[] plainText, byte[] key, byte[] iv = null,
            CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, int keySize = 256)
        {
            // If no IV was provided, use the first 16 bytes of the key
            if (iv == null)
            {
                iv = new byte[16];
                Array.Copy(key, iv, 16);
            }

            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }

            using var aes = Aes.Create();
            aes.KeySize = keySize;
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = mode;
            aes.Padding = padding;

            using var decryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return PerformCryptography(plainText, decryptor);
        }

        /// <summary>
        /// Encrypts a string with AES.
        /// </summary>
        /// <param name="plainText">The plaintext data</param>
        /// <param name="key">The encryption key</param>
        /// <param name="iv">The initial value</param>
        /// <param name="mode">The cipher mode</param>
        /// <param name="padding">The padding mode</param>
        public static byte[] AESEncryptString(string plainText, byte[] key, byte[] iv = null,
            CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, int keySize = 256)
        {
            // If no IV was provided, use the first 16 bytes of the key
            if (iv == null)
            {
                iv = new byte[16];
                Array.Copy(key, iv, 16);
            }

            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }

            // Create an Aes object
            // with the specified key and IV.
            using var aes = Aes.Create();
            aes.KeySize = keySize;
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = mode;
            aes.Padding = padding;

            // Create an encryptor to perform the stream transform.
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            // Create the streams used for encryption.
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                //Write all data to the stream.
                swEncrypt.Write(plainText);
            }

            return msEncrypt.ToArray();
        }

        /// <summary>
        /// Decrypts AES-encrypted data.
        /// </summary>
        /// <param name="cipherText">The AES-encrypted data</param>
        /// <param name="key">The decryption key</param>
        /// <param name="iv">The initial value</param>
        /// <param name="mode">The cipher mode</param>
        /// <param name="padding">The padding mode</param>
        public static byte[] AESDecrypt(byte[] cipherText, byte[] key, byte[] iv = null,
            CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, int keySize = 256)
        {
            // If no IV was provided, use the first 16 bytes of the key
            if (iv == null)
            {
                iv = new byte[16];
                Array.Copy(key, iv, 16);
            }

            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
                
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }

            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.KeySize = keySize;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = mode;
            aes.Padding = padding;

            using var decryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return PerformCryptography(cipherText, decryptor);
        }

        /// <summary>
        /// Decrypts AES-encrypted data.
        /// </summary>
        /// <param name="cipherText">The AES-encrypted data</param>
        /// <param name="key">The decryption key</param>
        /// <param name="iv">The initial value</param>
        /// <param name="mode">The cipher mode</param>
        /// <param name="padding">The padding mode</param>
        public static string AESDecryptString(byte[] cipherText, byte[] key, byte[] iv = null,
            CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, int keySize = 256)
        {
            // If no IV was provided, use the first 16 bytes of the key
            if (iv == null)
            {
                iv = new byte[16];
                Array.Copy(key, iv, 16);
            }

            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            using var aes = Aes.Create();
            aes.KeySize = keySize;
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = mode;
            aes.Padding = padding;

            // Create a decryptor to perform the stream transform.
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            // Create the streams used for decryption.
            using (var msDecrypt = new MemoryStream(cipherText))
            {
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                // Read the decrypted bytes from the decrypting stream
                // and place them in a string.
                plaintext = srDecrypt.ReadToEnd();
            }

            return plaintext;
        }

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using var ms = new MemoryStream();
            using var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return ms.ToArray();
        }
        #endregion

        #region JWT
        public static string JwtEncode(JwtAlgorithmName algorithmName, string secret, IDictionary<string, object> extraHeaders, IDictionary<string, object> payload)
        {
            IJwtAlgorithm algorithm = algorithmName switch
            {
                JwtAlgorithmName.HS256 => new HMACSHA256Algorithm(),
                JwtAlgorithmName.HS384 => new HMACSHA384Algorithm(),
                JwtAlgorithmName.HS512 => new HMACSHA512Algorithm(),
                _ => throw new NotSupportedException("This algorithm is not supported at the moment")
            };

            var jsonSerializer = new JsonNetSerializer();
            var urlEncoder = new JwtBase64UrlEncoder();
            var jwtEncoder = new JwtEncoder(algorithm, jsonSerializer, urlEncoder);

            return jwtEncoder.Encode(extraHeaders, payload, secret);
        }
        #endregion

        #region Bcrypt
        /// <summary>
        /// Hashes an <paramref name="input"/> with BCrypt using the provided <paramref name="salt"/>.
        /// </summary>
        public static string BCryptWithSalt(string input, string salt = "")
            => BCrypt.Net.BCrypt.HashPassword(input, salt);

        /// <summary>
        /// Hashes an <paramref name="input"/> with BCrypt after generating the salt with the given number of
        /// <paramref name="rounds"/> and <paramref name="saltRevision"/>.
        /// </summary>
        public static string BCryptGenSalt(string input, int rounds = 10, SaltRevision saltRevision = SaltRevision.Revision2X)
            => BCrypt.Net.BCrypt.HashPassword(input, rounds, saltRevision);

        /// <summary>
        /// Verifies that a BCrypt <paramref name="hash"/> is valid with respect to a given <paramref name="input"/>.
        /// </summary>
        public static bool BCryptVerify(string input, string hash)
            => BCrypt.Net.BCrypt.Verify(input, hash);
        #endregion

        #region AWS4
        /// <summary>
        /// Generates a SHA-256 hash that is the AWS4 Signature.
        /// </summary>
        /// <param name="key">The secret key</param>
        /// <param name="date">The date stamp</param>
        /// <param name="region">The name of the AWS instance region</param>
        /// <param name="service">The name of the AWS instance service</param>
        /// <returns>The AWS4 signature</returns>
        public static byte[] AWS4Encrypt(string key, string date, string region, string service)
        {
            var keyChars = $"AWS4{key}".ToCharArray();
            
            var secret = Encoding.UTF8.GetBytes(keyChars);
            var dateKey = Hmac(date, secret);
            var regionKey = Hmac(region, dateKey);
            var serviceKey = Hmac(service, regionKey);

            return Hmac("aws4_request", serviceKey);
        }
        #endregion
    }
}
