using BCrypt.Net;
using JWT.Algorithms;
using Newtonsoft.Json;
using RuriLib.Attributes;
using RuriLib.Functions.Conversion;
using RuriLib.Functions.Crypto;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using Scrypt;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RuriLib.Blocks.Functions.Crypto
{
    [BlockCategory("Crypto", "Blocks for executing cryptographic functions", "#9acd32")]
    public static class Methods
    {
        [Block("XOR En-/Decryption on byte arrays", name = "XOR")]
        public static byte[] XOR(BotData data, byte[] bytes, byte[] key)
        {
            var xored = RuriLib.Functions.Crypto.Crypto.XOR(bytes, key);
            data.Logger.LogHeader();
            data.Logger.Log($"XORed the two byte arrays and got {HexConverter.ToHexString(xored)}", LogColors.YellowGreen);
            return xored;
        }

        [Block("Does a simple XOR En-/Decryption on strings", name = "XOR Strings")]
        public static string XORStrings(BotData data, string text, string key)
        {
            var xored = RuriLib.Functions.Crypto.Crypto.XORStrings(text, key);
            data.Logger.LogHeader();
            data.Logger.Log($"XORed: {text} with {key} with the outcome {xored}", LogColors.YellowGreen);
            return xored;
        }

        [Block("Hashes data using the specified hashing function")]
        public static byte[] Hash(BotData data, byte[] input, HashFunction hashFunction = HashFunction.MD5)
        {
            var hashed = Hash(input, hashFunction);
            data.Logger.LogHeader();
            data.Logger.Log($"Computed hash: {HexConverter.ToHexString(hashed)}", LogColors.YellowGreen);
            return hashed;
        }

        [Block("Hashes a UTF8 string to a HEX-encoded lowercase string using the specified hashing function")]
        public static string HashString(BotData data, string input, HashFunction hashFunction = HashFunction.MD5)
        {
            var hashed = HexConverter.ToHexString(Hash(Encoding.UTF8.GetBytes(input), hashFunction));
            data.Logger.LogHeader();
            data.Logger.Log($"Computed hash: {hashed}", LogColors.YellowGreen);
            return hashed;
        }

        [Block("Hashes a string using NTLM", name = "NTLM Hash")]
        public static byte[] NTLMHash(BotData data, string input)
        {
            var hashed = RuriLib.Functions.Crypto.Crypto.NTLM(input);
            data.Logger.LogHeader();
            data.Logger.Log($"Computed hash: {HexConverter.ToHexString(hashed)}", LogColors.YellowGreen);
            return hashed;
        }

        [Block("Computes the HMAC signature of some data using the specified secret key and hashing function")]
        public static byte[] Hmac(BotData data, byte[] input, byte[] key, HashFunction hashFunction = HashFunction.MD5)
        {
            var hmac = Hmac(input, key, hashFunction);
            data.Logger.LogHeader();
            data.Logger.Log($"Computed HMAC: {HexConverter.ToHexString(hmac)}", LogColors.YellowGreen);
            return hmac;
        }

        [Block("Computes the HMAC signature as a HEX-encoded lowercase string from a given UTF8 string using the specified key and hashing function")]
        public static string HmacString(BotData data, string input, byte[] key, HashFunction hashFunction = HashFunction.MD5)
        {
            var hmac = HexConverter.ToHexString(Hmac(Encoding.UTF8.GetBytes(input), key, hashFunction));
            data.Logger.LogHeader();
            data.Logger.Log($"Computed HMAC: {hmac}", LogColors.YellowGreen);
            return hmac;
        }

        private static byte[] Hash(byte[] input, HashFunction function)
        {
            return function switch
            {
                HashFunction.MD4 => RuriLib.Functions.Crypto.Crypto.MD4(input),
                HashFunction.MD5 => RuriLib.Functions.Crypto.Crypto.MD5(input),
                HashFunction.SHA1 => RuriLib.Functions.Crypto.Crypto.SHA1(input),
                HashFunction.SHA256 => RuriLib.Functions.Crypto.Crypto.SHA256(input),
                HashFunction.SHA384 => RuriLib.Functions.Crypto.Crypto.SHA384(input),
                HashFunction.SHA512 => RuriLib.Functions.Crypto.Crypto.SHA512(input),
                _ => throw new NotSupportedException()
            };
        }

        private static byte[] Hmac(string input, byte[] key, HashFunction function) => Hmac(Encoding.UTF8.GetBytes(input), key, function);

        private static byte[] Hmac(byte[] input, byte[] key, HashFunction function)
        {
            return function switch
            {
                HashFunction.MD5 => RuriLib.Functions.Crypto.Crypto.HMACMD5(input, key),
                HashFunction.SHA1 => RuriLib.Functions.Crypto.Crypto.HMACSHA1(input, key),
                HashFunction.SHA256 => RuriLib.Functions.Crypto.Crypto.HMACSHA256(input, key),
                HashFunction.SHA384 => RuriLib.Functions.Crypto.Crypto.HMACSHA384(input, key),
                HashFunction.SHA512 => RuriLib.Functions.Crypto.Crypto.HMACSHA512(input, key),
                _ => throw new NotSupportedException()
            };
        }

        [Block("Hashes data using the Scrypt algorithm")]
        public static string ScryptString(BotData data, string password, string salt, int iterationCount = 16384, int blockSize = 8, int threadCount = 1)
        {
            var rng = new FakeRNG(salt);
            var encoder = new ScryptEncoder(iterationCount, blockSize, threadCount, rng);
            var hashed = encoder.Encode(password);
            data.Logger.LogHeader();
            data.Logger.Log($"Computed Scrypt: {hashed}", LogColors.YellowGreen);
            return hashed;
        }

        // Used for Scrypt.NET because it doesn't support a parametrized salt
        private class FakeRNG : RandomNumberGenerator
        {
            private readonly byte[] salt;

            public FakeRNG(string salt)
            {
                this.salt = Encoding.UTF8.GetBytes(salt);
            }

            public override void GetBytes(byte[] data)
            {
                for (var i = 0; i < salt.Length; i++)
                {
                    data[i] = salt[i];
                }
            }
        }

        [Block("Encrypts data using RSA", name = "RSA Encrypt")]
        public static byte[] RSAEncrypt(BotData data, byte[] plainText, byte[] modulus, byte[] exponent, bool useOAEP)
        {
            var cipherText = RuriLib.Functions.Crypto.Crypto.RSAEncrypt(plainText, modulus, exponent, useOAEP);
            data.Logger.LogHeader();
            data.Logger.Log($"Encrypted: {HexConverter.ToHexString(cipherText)}", LogColors.YellowGreen);
            return cipherText;
        }

        [Block("Decrypts data using RSA", name = "RSA Decrypt")]
        public static byte[] RSADecrypt(BotData data, byte[] cipherText, byte[] modulus, byte[] d, bool useOAEP)
        {
            var plainText = RuriLib.Functions.Crypto.Crypto.RSADecrypt(cipherText, modulus, d, useOAEP);
            data.Logger.LogHeader();
            data.Logger.Log($"Decrypted: {HexConverter.ToHexString(plainText)}", LogColors.YellowGreen);
            return plainText;
        }

        [Block("Encrypts data using RSA with PKCS1PAD2", name = "RSA PKCS1PAD2")]
        public static byte[] RSAPkcs1Pad2(BotData data, string plainText, string hexModulus, string hexExponent)
        {
            var encrypted = RuriLib.Functions.Crypto.Crypto.RSAPkcs1Pad2(plainText, hexModulus, hexExponent);
            data.Logger.LogHeader();
            data.Logger.Log($"Encrypted: {HexConverter.ToHexString(encrypted)}", LogColors.YellowGreen);
            return encrypted;
        }

        [Block("Generates a PKCS v5 #2.0 key using a Password-Based Key Derivation Function", name = "PBKDF2PKCS5")]
        public static byte[] PBKDF2PKCS5(BotData data, byte[] password, byte[] salt = null, int saltSize = 8,
            int iterations = 1, int keyLength = 16, HashFunction type = HashFunction.SHA1)
        {
            var derived = RuriLib.Functions.Crypto.Crypto.PBKDF2PKCS5(password, salt, saltSize, iterations, keyLength, type);
            data.Logger.LogHeader();
            data.Logger.Log($"Derived: {HexConverter.ToHexString(derived)}", LogColors.YellowGreen);
            return derived;
        }

        [Block("Encrypts data with AES", name = "AES Encrypt")]
        public static byte[] AESEncrypt(BotData data, byte[] plainText, byte[] key, byte[] iv,
            CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, int keySize = 256)
        {
            var cipherText = RuriLib.Functions.Crypto.Crypto.AESEncrypt(plainText, key, iv, mode, padding, keySize);
            data.Logger.LogHeader();
            data.Logger.Log($"Encrypted: {HexConverter.ToHexString(cipherText)}", LogColors.YellowGreen);
            return cipherText;
        }

        [Block("Encrypts a string with AES", name = "AES Encrypt String")]
        public static byte[] AESEncryptString(BotData data, string plainText, byte[] key, byte[] iv,
            CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, int keySize = 256)
        {
            var cipherText = RuriLib.Functions.Crypto.Crypto.AESEncryptString(plainText, key, iv, mode, padding, keySize);
            data.Logger.LogHeader();
            data.Logger.Log($"Encrypted: {HexConverter.ToHexString(cipherText)}", LogColors.YellowGreen);
            return cipherText;
        }

        [Block("Decrypts data with AES", name = "AES Decrypt")]
        public static byte[] AESDecrypt(BotData data, byte[] cipherText, byte[] key, byte[] iv,
            CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, int keySize = 256)
        {
            var plainText = RuriLib.Functions.Crypto.Crypto.AESDecrypt(cipherText, key, iv, mode, padding, keySize);
            data.Logger.LogHeader();
            data.Logger.Log($"Decrypted: {HexConverter.ToHexString(plainText)}", LogColors.YellowGreen);
            return plainText;
        }

        [Block("Decrypts data with AES to string", name = "AES Decrypt String")]
        public static string AESDecryptString(BotData data, byte[] cipherText, byte[] key, byte[] iv,
            CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, int keySize = 256)
        {
            var plainText = RuriLib.Functions.Crypto.Crypto.AESDecryptString(cipherText, key, iv, mode, padding, keySize);
            data.Logger.LogHeader();
            data.Logger.Log($"Decrypted: {plainText}", LogColors.YellowGreen);
            return plainText;
        }

        [Block("Generates a JSON Web Token using a secret key, payload, optional extra headers and specified algorithm type",
            name = "JWT Encode", extraInfo = "The header already contains the selected algorithm and token type (JWT) by default")]
        public static string JwtEncode(BotData data, JwtAlgorithmName algorithm, string secret, string extraHeaders = "{}", string payload = "{}")
        {
            var extraHeadersDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(extraHeaders);
            var payloadDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload);

            string encoded = RuriLib.Functions.Crypto.Crypto.JwtEncode(algorithm, secret, extraHeadersDictionary, payloadDictionary);

            data.Logger.LogHeader();
            data.Logger.Log($"Encoded: {encoded}", LogColors.YellowGreen);

            return encoded;
        }

        [Block("Generates a BCrypt hash from an input and a salt", name = "BCrypt Hash",
            extraInfo = "If you don't have the salt, use the BCrypt Hash (Gen Salt) block")]
        public static string BCryptHash(BotData data, string input, string salt)
        {
            data.Logger.LogHeader();

            var hashed = RuriLib.Functions.Crypto.Crypto.BCryptWithSalt(input, salt);
            data.Logger.Log($"Hashed: {hashed}", LogColors.YellowGreen);

            return hashed;
        }

        [Block("Generates a BCrypt hash from an input by generating a salt", name = "BCrypt Hash (Gen Salt)",
            extraInfo = "bcryptjs uses salt revision 2X by default currently")]
        public static string BCryptHashGenSalt(BotData data, string input, int rounds = 10, SaltRevision saltRevision = SaltRevision.Revision2X)
        {
            data.Logger.LogHeader();

            var hashed = RuriLib.Functions.Crypto.Crypto.BCryptGenSalt(input, rounds, saltRevision);
            data.Logger.Log($"Hashed: {hashed}", LogColors.YellowGreen);

            return hashed;
        }

        [Block("Verifies that a BCrypt hash is valid", name = "BCrypt Verify")]
        public static bool BCryptVerify(BotData data, string input, string hash)
        {
            data.Logger.LogHeader();

            var isValid = RuriLib.Functions.Crypto.Crypto.BCryptVerify(input, hash);
            data.Logger.Log($"BCrypt hash verification result: {isValid}", LogColors.YellowGreen);

            return isValid;
        }

        [Block("Generates an AWS4 Signature from a key, date, region and service", name = "AWS4 Signature", 
            extraInfo = "It returns a byte array and it expects the date to be in the following format: YYYYMMDD")]
        public static byte[] AWS4Signature(BotData data, string key, string date, string region, string service)
        {
            var signature = RuriLib.Functions.Crypto.Crypto.AWS4Encrypt(key, date, region, service);
            
            data.Logger.LogHeader();
            data.Logger.Log($"Hashed: {HexConverter.ToHexString(signature)}", LogColors.YellowGreen);
            
            return signature;
        }
    }
}
