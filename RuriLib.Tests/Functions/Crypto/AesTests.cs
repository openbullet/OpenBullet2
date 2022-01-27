using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace RuriLib.Tests.Functions.Crypto
{
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
    }
}
