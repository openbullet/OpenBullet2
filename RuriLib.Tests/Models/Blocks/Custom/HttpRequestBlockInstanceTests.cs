using System;
using System.Collections.Generic;
using RuriLib.Exceptions;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.HttpRequest;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using RuriLib.Models.Configs;
using Xunit;

namespace RuriLib.Tests.Models.Blocks.Custom;

public class HttpRequestBlockInstanceTests
{
    private readonly string _nl = Environment.NewLine;

    /*
    [Fact]
    public void ToLC_StandardPost_OutputScript()
    {
        var repo = new DescriptorsRepository();
        var descriptor = repo.GetAs<HttpRequestBlockDescriptor>("HttpRequest");
        var block = new HttpRequestBlockInstance(descriptor);
        ...
    }
    */

    [Fact]
    public void ToLC_StandardPost_OutputScript()
    {
        var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");

        var url = block.Settings["url"];
        url.InputMode = SettingInputMode.Fixed;
        (url.FixedSetting as StringSetting)!.Value = "https://example.com";

        var method = block.Settings["method"];
        method.InputMode = SettingInputMode.Fixed;
        (method.FixedSetting as EnumSetting)!.Value = "POST";

        block.RequestParams = new StandardRequestParams
        {
            Content = BlockSettingFactory.CreateStringSetting(string.Empty, "key1=value1&key2=value2", SettingInputMode.Fixed),
            ContentType = BlockSettingFactory.CreateStringSetting(string.Empty, "application/x-www-form-urlencoded", SettingInputMode.Fixed)
        };

        var output = block.ToLC();

        Assert.Contains($"  url = \"https://example.com\"{_nl}", output);
        Assert.Contains($"  method = POST{_nl}", output);
        Assert.Contains($"  TYPE:STANDARD{_nl}", output);
        Assert.Contains($"  \"key1=value1&key2=value2\"{_nl}", output);
        Assert.Contains($"  \"application/x-www-form-urlencoded\"{_nl}", output);
    }

    [Fact]
    public void ToLC_DefaultCustomCookiesAndHeaders_AreOmitted()
    {
        var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");

        var output = block.ToLC();

        Assert.DoesNotContain("customCookies =", output);
        Assert.DoesNotContain("customHeaders =", output);
    }

    [Fact]
    public void FromLC_MultipartPost_BuildBlock()
    {
        var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");
        var script = $"  url = \"https://example.com\"{_nl}  method = POST{_nl}  TYPE:MULTIPART{_nl}  @myBoundary{_nl}  CONTENT:STRING \"stringName\" \"stringContent\" \"stringContentType\"{_nl}  CONTENT:FILE \"fileName\" \"file.txt\" \"fileContentType\"{_nl}";
        var lineNumber = 0;

        block.FromLC(ref script, ref lineNumber);

        Assert.Equal("https://example.com", Assert.IsType<StringSetting>(block.Settings["url"].FixedSetting).Value);
        Assert.Equal("POST", Assert.IsType<EnumSetting>(block.Settings["method"].FixedSetting).Value);

        var multipart = Assert.IsType<MultipartRequestParams>(block.RequestParams);
        Assert.Equal(SettingInputMode.Variable, multipart.Boundary.InputMode);
        Assert.Equal("myBoundary", multipart.Boundary.InputVariableName);

        var stringContent = Assert.IsType<StringHttpContentSettingsGroup>(multipart.Contents[0]);
        Assert.Equal("stringName", Assert.IsType<StringSetting>(stringContent.Name.FixedSetting).Value);
        Assert.Equal("stringContent", Assert.IsType<StringSetting>(stringContent.Data.FixedSetting).Value);
        Assert.Equal("stringContentType", Assert.IsType<StringSetting>(stringContent.ContentType.FixedSetting).Value);

        var fileContent = Assert.IsType<FileHttpContentSettingsGroup>(multipart.Contents[1]);
        Assert.Equal("fileName", Assert.IsType<StringSetting>(fileContent.Name.FixedSetting).Value);
        Assert.Equal("file.txt", Assert.IsType<StringSetting>(fileContent.FileName.FixedSetting).Value);
        Assert.Equal("fileContentType", Assert.IsType<StringSetting>(fileContent.ContentType.FixedSetting).Value);
    }

    [Fact]
    public void FromLC_MultipartWithoutBoundary_Throws()
    {
        var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");
        var script = $"  TYPE:MULTIPART{_nl}";
        var lineNumber = 0;

        Assert.Throws<LoliCodeParsingException>(() => block.FromLC(ref script, ref lineNumber));
    }

    [Fact]
    public void FromLC_ContentWithoutMultipart_Throws()
    {
        var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");
        var script = $"  CONTENT:STRING \"name\" \"content\" \"contentType\"{_nl}";
        var lineNumber = 0;

        Assert.Throws<LoliCodeParsingException>(() => block.FromLC(ref script, ref lineNumber));
    }

    [Fact]
    public void FromLC_InvalidStandardContent_PreservesLineParsingDetails()
    {
        var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");
        var script = $"  TYPE:STANDARD{_nl}  content{_nl}";
        var lineNumber = 0;

        var ex = Assert.Throws<LoliCodeParsingException>(() => block.FromLC(ref script, ref lineNumber));

        Assert.Equal(2, ex.LineNumber);
        Assert.Equal(1, ex.ColumnNumber);
        Assert.IsType<LineParsingException>(ex.InnerException);
        Assert.Contains("Expected '\"' to start a string literal", ex.Message);
    }

    [Fact]
    public void ToSyntax_MultipartPost_OutputScript()
    {
        var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");
        var script = $"  url = \"https://example.com\"{_nl}  method = POST{_nl}  TYPE:MULTIPART{_nl}  @myBoundary{_nl}  CONTENT:STRING \"stringName\" \"stringContent\" \"stringContentType\"{_nl}  CONTENT:FILE \"fileName\" \"file.txt\" \"fileContentType\"{_nl}";
        var lineNumber = 0;
        block.FromLC(ref script, ref lineNumber);

        var output = RenderSyntax(block);

        Assert.Contains("await HttpRequestMultipart(data, new MultipartHttpRequestOptions {", output);
        Assert.Contains("Boundary = myBoundary.AsString()", output);
        Assert.Contains("new StringHttpContent(\"stringName\", \"stringContent\", \"stringContentType\")", output);
        Assert.Contains("new FileHttpContent(\"fileName\", \"file.txt\", \"fileContentType\")", output);
        Assert.Contains("Url = \"https://example.com\"", output);
        Assert.Contains("Method = RuriLib.Functions.Http.HttpMethod.POST", output);
        Assert.EndsWith("}).ConfigureAwait(false);" + _nl, output);
    }

    [Fact]
    public void ToSyntax_TracksExpectedRequestShapes()
    {
        AssertSyntax(CreateStandardRequestBlock(),
            "await HttpRequestStandard(data, new StandardHttpRequestOptions",
            "Url = $\"https://example.com/{globals.path}\"",
            "Content = $\"name={globals.user}\"",
            "IgnoreCertificateValidation = true",
            "UrlEncodeContent = ObjectExtensions.DynamicAsBool(globals.encodeContent)");

        AssertSyntax(CreateStandardRequestBlock(safe: true),
            "try",
            "await HttpRequestStandard(data, new StandardHttpRequestOptions",
            "catch (Exception safeException)");

        AssertSyntax(CreateRawRequestBlock(),
            "await HttpRequestRaw(data, new RawHttpRequestOptions",
            "Content = new byte[]",
            "ContentType = \"application/octet-stream\"");

        AssertSyntax(CreateBasicAuthRequestBlock(),
            "await HttpRequestBasicAuth(data, new BasicAuthHttpRequestOptions",
            "Username = ObjectExtensions.DynamicAsString(globals.username)",
            "Password = ObjectExtensions.DynamicAsString(globals.password)");

        AssertSyntax(CreateMultipartRequestBlock(safe: true),
            "try",
            "await HttpRequestMultipart(data, new MultipartHttpRequestOptions",
            "Boundary = ObjectExtensions.DynamicAsString(globals.boundary)",
            "new StringHttpContent(\"stringName\", $\"hello {globals.user}\", \"text/plain\")",
            "new RawHttpContent(\"rawName\", new byte[]",
            "new FileHttpContent(\"fileName\", ObjectExtensions.DynamicAsString(globals.uploadPath), \"application/octet-stream\")",
            "catch (Exception safeException)");
    }

    [Fact]
    public void MultipartSettingsGroups_DefaultContentTypes_ArePreserved()
    {
        var stringContent = new StringHttpContentSettingsGroup();
        var fileContent = new FileHttpContentSettingsGroup();
        var rawContent = new RawHttpContentSettingsGroup();

        Assert.Equal("text/plain", Assert.IsType<StringSetting>(stringContent.ContentType.FixedSetting).Value);
        Assert.Equal("application/octet-stream", Assert.IsType<StringSetting>(fileContent.ContentType.FixedSetting).Value);
        Assert.Equal("application/octet-stream", Assert.IsType<StringSetting>(rawContent.ContentType.FixedSetting).Value);
        Assert.IsType<ByteArraySetting>(rawContent.Data.FixedSetting);
    }

    private static HttpRequestBlockInstance CreateStandardRequestBlock(bool safe = false)
    {
        var block = CreateBaseBlock(safe);
        var content = BlockSettingFactory.CreateStringSetting("content", "name=<globals.user>", SettingInputMode.Interpolated);
        content.InterpolatedSetting = new InterpolatedStringSetting { Value = "name=<globals.user>" };

        block.RequestParams = new StandardRequestParams
        {
            Content = content,
            ContentType = BlockSettingFactory.CreateStringSetting("contentType", "application/x-www-form-urlencoded")
        };

        block.Settings["urlEncodeContent"].InputMode = SettingInputMode.Variable;
        block.Settings["urlEncodeContent"].InputVariableName = "globals.encodeContent";

        return block;
    }

    private static HttpRequestBlockInstance CreateRawRequestBlock()
    {
        var block = CreateBaseBlock();
        block.RequestParams = new RawRequestParams
        {
            Content = BlockSettingFactory.CreateByteArraySetting("content", [1, 2, 3]),
            ContentType = BlockSettingFactory.CreateStringSetting("contentType", "application/octet-stream")
        };

        return block;
    }

    private static HttpRequestBlockInstance CreateBasicAuthRequestBlock()
    {
        var block = CreateBaseBlock();
        block.RequestParams = new BasicAuthRequestParams
        {
            Username = BlockSettingFactory.CreateStringSetting("username", "globals.username", SettingInputMode.Variable),
            Password = BlockSettingFactory.CreateStringSetting("password", "globals.password", SettingInputMode.Variable)
        };

        return block;
    }

    private static HttpRequestBlockInstance CreateMultipartRequestBlock(bool safe = false)
    {
        var block = CreateBaseBlock(safe);
        block.RequestParams = new MultipartRequestParams
        {
            Boundary = BlockSettingFactory.CreateStringSetting("boundary", "globals.boundary", SettingInputMode.Variable),
            Contents =
            [
                new StringHttpContentSettingsGroup
                {
                    Name = BlockSettingFactory.CreateStringSetting("name", "stringName"),
                    Data = BlockSettingFactory.CreateStringSetting("data", "hello <globals.user>", SettingInputMode.Interpolated),
                    ContentType = BlockSettingFactory.CreateStringSetting("contentType", "text/plain")
                },
                new RawHttpContentSettingsGroup
                {
                    Name = BlockSettingFactory.CreateStringSetting("name", "rawName"),
                    Data = BlockSettingFactory.CreateByteArraySetting("data", [4, 5, 6]),
                    ContentType = BlockSettingFactory.CreateStringSetting("contentType", "application/octet-stream")
                },
                new FileHttpContentSettingsGroup
                {
                    Name = BlockSettingFactory.CreateStringSetting("name", "fileName"),
                    FileName = BlockSettingFactory.CreateStringSetting("fileName", "globals.uploadPath", SettingInputMode.Variable),
                    ContentType = BlockSettingFactory.CreateStringSetting("contentType", "application/octet-stream")
                }
            ]
        };

        return block;
    }

    private static HttpRequestBlockInstance CreateBaseBlock(bool safe = false)
    {
        var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");
        block.Safe = safe;

        block.Settings["url"].InputMode = SettingInputMode.Interpolated;
        block.Settings["url"].InterpolatedSetting = new InterpolatedStringSetting
        {
            Value = "https://example.com/<globals.path>"
        };

        (block.Settings["method"].FixedSetting as EnumSetting)!.Value = "POST";
        block.Settings["autoRedirect"].InputMode = SettingInputMode.Variable;
        block.Settings["autoRedirect"].InputVariableName = "input.autoRedirect";
        (block.Settings["maxNumberOfRedirects"].FixedSetting as IntSetting)!.Value = 5;
        (block.Settings["readResponseContent"].FixedSetting as BoolSetting)!.Value = false;
        (block.Settings["absoluteUriInFirstLine"].FixedSetting as BoolSetting)!.Value = true;
        (block.Settings["ignoreCertificateValidation"].FixedSetting as BoolSetting)!.Value = true;
        block.Settings["customCookies"].InputMode = SettingInputMode.Variable;
        block.Settings["customCookies"].InputVariableName = "globals.cookies";
        block.Settings["customHeaders"].InputMode = SettingInputMode.Interpolated;
        block.Settings["customHeaders"].InterpolatedSetting = new InterpolatedDictionaryOfStringsSetting
        {
            Value = new Dictionary<string, string>
            {
                ["X-Test"] = "<globals.headerValue>"
            }
        };
        block.Settings["timeoutMilliseconds"].InputMode = SettingInputMode.Variable;
        block.Settings["timeoutMilliseconds"].InputVariableName = "input.timeout";
        block.Settings["codePagesEncoding"].InputMode = SettingInputMode.Variable;
        block.Settings["codePagesEncoding"].InputVariableName = "globals.encoding";
        (block.Settings["alwaysSendContent"].FixedSetting as BoolSetting)!.Value = true;
        (block.Settings["decodeHtml"].FixedSetting as BoolSetting)!.Value = true;
        block.Settings["customCipherSuites"].InputMode = SettingInputMode.Variable;
        block.Settings["customCipherSuites"].InputVariableName = "globals.cipherSuites";

        return block;
    }

    private static void AssertSyntax(HttpRequestBlockInstance block, params string[] expectedFragments)
    {
        var syntax = RenderSyntax(block);

        foreach (var expectedFragment in expectedFragments)
        {
            Assert.Contains(expectedFragment, syntax);
        }
    }

    private static string RenderSyntax(HttpRequestBlockInstance block)
        => block.ToSyntax(new BlockSyntaxGenerationContext([], new ConfigSettings())).ToSnippet();
}
