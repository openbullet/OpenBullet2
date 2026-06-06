using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace RuriLib.Tests.Functions.Crypto;

public class AesTests
{
    [Fact]
    public void AesEncrypt_ValidInputs_ValidOutput()
    {
        var input = Encoding.UTF8.GetBytes("test");
        var key = Convert.FromBase64String("WGxReE43YTUxODFiMDdjalhsUXhON2E1MTgxYjA3Y2o=");
        var iv = Convert.FromBase64String("bjVKSk0wNEZONGtRMTc4ZA==");

        var encrypted = RuriLib.Functions.Crypto.Crypto.AESEncrypt(input, key, iv, CipherMode.CBC, PaddingMode.PKCS7, 256);
        Assert.Equal("Yu5IU/XZS3tMjw2m5p4Pgw==", Convert.ToBase64String(encrypted));
    }

    [Fact]
    public void AesDecrypt_EncryptedInput_RoundTrips()
    {
        var input = Encoding.UTF8.GetBytes("test");
        var key = Convert.FromBase64String("WGxReE43YTUxODFiMDdjalhsUXhON2E1MTgxYjA3Y2o=");
        var iv = Convert.FromBase64String("bjVKSk0wNEZONGtRMTc4ZA==");

        var encrypted = RuriLib.Functions.Crypto.Crypto.AESEncrypt(input, key, iv, CipherMode.CBC, PaddingMode.PKCS7, 256);
        var decrypted = RuriLib.Functions.Crypto.Crypto.AESDecrypt(encrypted, key, iv, CipherMode.CBC, PaddingMode.PKCS7, 256);

        Assert.Equal(input, decrypted);
    }

    [Fact]
    public void AesEncryptAndDecrypt_WithNullIv_UsesKeyPrefix()
    {
        const string input = "test";
        var key = Convert.FromBase64String("WGxReE43YTUxODFiMDdjalhsUXhON2E1MTgxYjA3Y2o=");

        var encrypted = RuriLib.Functions.Crypto.Crypto.AESEncryptString(input, key, null, CipherMode.CBC, PaddingMode.PKCS7, 256);
        var decrypted = RuriLib.Functions.Crypto.Crypto.AESDecryptString(encrypted, key, null, CipherMode.CBC, PaddingMode.PKCS7, 256);

        Assert.Equal(input, decrypted);
    }

    [Fact]
    public void Pbkdf2Pkcs5_NullSalt_ReturnsRequestedKeyLength()
    {
        var password = Encoding.UTF8.GetBytes("password");

        var derived = RuriLib.Functions.Crypto.Crypto.PBKDF2PKCS5(password, null, saltSize: 8, iterations: 2, keyLength: 32);

        Assert.Equal(32, derived.Length);
    }
}
