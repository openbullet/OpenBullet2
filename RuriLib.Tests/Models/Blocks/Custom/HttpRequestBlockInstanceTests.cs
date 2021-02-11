using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.HttpRequest;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RuriLib.Tests.Models.Blocks.Custom
{
    public class HttpRequestBlockInstanceTests
    {
        /*
        [Fact]
        public void ToLC_StandardPost_OutputScript()
        {
            var repo = new DescriptorsRepository();
            var descriptor = repo.GetAs<HttpRequestBlockDescriptor>("HttpRequest");
            var block = new HttpRequestBlockInstance(descriptor);

            var url = block.Settings["url"];
            url.InputMode = SettingInputMode.Fixed;
            (url.FixedSetting as StringSetting).Value = "https://example.com";

            var method = block.Settings["method"];
            method.InputMode = SettingInputMode.Fixed;
            (method.FixedSetting as EnumSetting).Value = "POST";

            block.RequestParams = new StandardRequestParams
            {
                Content = new BlockSetting { FixedSetting = new StringSetting { Value = "key1=value1&key2=value2" } },
                ContentType = new BlockSetting { FixedSetting = new StringSetting { Value = "application/x-www-form-urlencoded" } }
            };

            var expected = "  url = \"https://example.com\"\r\n  method = POST\r\n  TYPE:STANDARD\r\n  \"key1=value1&key2=value2\"\r\n  \"application/x-www-form-urlencoded\"\r\n";
            Assert.Equal(expected, block.ToLC());
        }

        [Fact]
        public void ToLC_MultipartPost_OutputScript()
        {
            var repo = new DescriptorsRepository();
            var descriptor = repo.GetAs<HttpRequestBlockDescriptor>("HttpRequest");
            var block = new HttpRequestBlockInstance(descriptor);

            var url = block.Settings["url"];
            url.InputMode = SettingInputMode.Fixed;
            (url.FixedSetting as StringSetting).Value = "https://example.com";

            var method = block.Settings["method"];
            method.InputMode = SettingInputMode.Fixed;
            (method.FixedSetting as EnumSetting).Value = "POST";

            block.RequestParams = new MultipartRequestParams
            {
                Boundary = new BlockSetting { InputMode = SettingInputMode.Variable, InputVariableName = "myBoundary" },
                Contents = new List<HttpContentSettingsGroup>
                {
                    new StringHttpContentSettingsGroup
                    {
                        Name = new BlockSetting { FixedSetting = new StringSetting { Value = "stringName" } },
                        Data = new BlockSetting { FixedSetting = new StringSetting { Value = "stringContent" } },
                        ContentType = new BlockSetting { FixedSetting = new StringSetting { Value = "stringContentType" } }
                    },
                    new FileHttpContentSettingsGroup
                    {
                        Name = new BlockSetting { FixedSetting = new StringSetting { Value = "fileName" } },
                        FileName = new BlockSetting { FixedSetting = new StringSetting { Value = "file.txt" } },
                        ContentType = new BlockSetting { FixedSetting = new StringSetting { Value = "fileContentType" } }
                    },
                }
            };

            var expected = "  url = \"https://example.com\"\r\n  method = POST\r\n  TYPE:MULTIPART\r\n  @myBoundary\r\n  CONTENT:STRING \"stringName\" \"stringContent\" \"stringContentType\"\r\n  CONTENT:FILE \"fileName\" \"file.txt\" \"fileContentType\"\r\n";
            Assert.Equal(expected, block.ToLC());
        }

        [Fact]
        public void FromLC_MultipartPost_BuildBlock()
        {
            var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");
            var script = "  url = \"https://example.com\"\r\n  method = POST\r\n  TYPE:MULTIPART\r\n  @myBoundary\r\n  CONTENT:STRING \"stringName\" \"stringContent\" \"stringContentType\"\r\n  CONTENT:FILE \"fileName\" \"file.txt\" \"fileContentType\"\r\n";
            int lineNumber = 0;
            block.FromLC(ref script, ref lineNumber);

            var url = block.Settings["url"];
            Assert.Equal("https://example.com", (url.FixedSetting as StringSetting).Value);

            var method = block.Settings["method"];
            Assert.Equal("POST", (method.FixedSetting as EnumSetting).Value);

            Assert.IsType<MultipartRequestParams>(block.RequestParams);

            var multipart = (MultipartRequestParams)block.RequestParams;
            Assert.Equal(SettingInputMode.Variable, multipart.Boundary.InputMode);
            Assert.Equal("myBoundary", multipart.Boundary.InputVariableName);

            var content = multipart.Contents[0];
            Assert.IsType<StringHttpContentSettingsGroup>(content);
            var stringContent = (StringHttpContentSettingsGroup)content;
            Assert.Equal("stringName", (stringContent.Name.FixedSetting as StringSetting).Value);
            Assert.Equal("stringContent", (stringContent.Data.FixedSetting as StringSetting).Value);
            Assert.Equal("stringContentType", (stringContent.ContentType.FixedSetting as StringSetting).Value);

            content = multipart.Contents[1];
            Assert.IsType<FileHttpContentSettingsGroup>(content);
            var fileContent = (FileHttpContentSettingsGroup)content;
            Assert.Equal("fileName", (fileContent.Name.FixedSetting as StringSetting).Value);
            Assert.Equal("file.txt", (fileContent.FileName.FixedSetting as StringSetting).Value);
            Assert.Equal("fileContentType", (fileContent.ContentType.FixedSetting as StringSetting).Value);
        }

        [Fact]
        public void ToCSharp_MultipartPost_OutputScript()
        {
            var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");
            var script = "  url = \"https://example.com\"\r\n  method = POST\r\n  TYPE:MULTIPART\r\n  @myBoundary\r\n  CONTENT:STRING \"stringName\" \"stringContent\" \"stringContentType\"\r\n  CONTENT:FILE \"fileName\" \"file.txt\" \"fileContentType\"\r\n";
            int lineNumber = 0;
            block.FromLC(ref script, ref lineNumber);
            var headers = block.Settings["customHeaders"];
            (headers.FixedSetting as DictionaryOfStringsSetting).Value.Clear(); 

            string expected = "await HttpRequestMultipart(data, \"https://example.com\", RuriLib.Functions.Http.HttpMethod.POST, true, RuriLib.Functions.Http.SecurityProtocol.SystemDefault, myBoundary.AsString(), new List<MyHttpContent> { new StringHttpContent(\"stringName\", \"stringContent\", \"stringContentType\"), new FileHttpContent(\"fileName\", \"file.txt\", \"fileContentType\") }, new Dictionary<string, string> {}, new Dictionary<string, string> {}, 10000, \"1.1\");\r\n";
            Assert.Equal(expected, block.ToCSharp(new List<string>(), new ConfigSettings()));
        }
        */
    }
}
