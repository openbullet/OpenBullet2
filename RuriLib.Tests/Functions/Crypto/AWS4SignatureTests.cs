using RuriLib.Functions.Conversion;
using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace RuriLib.Tests.Functions.Crypto
{
    public class AWS4SignatureTests
    {
        [Fact]
        public void Aws4SignatureGenerator_ValidInputs_ValidOutput()
        {
            const string key = "wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY";
            const string dateStamp = "20120215";
            const string regionName = "us-east-1";
            const string serviceName = "iam";

            var signature = RuriLib.Functions.Crypto.Crypto.AWS4Encrypt(key, dateStamp, regionName, serviceName);
            Assert.Equal("f4780e2d9f65fa895f9c67b32ce1baf0b0d8a43505a000a1a9e090d414db404d", HexConverter.ToHexString(signature));
        }
    }
}