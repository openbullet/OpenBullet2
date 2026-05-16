using ModelContextProtocol.Client;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Dtos.Config.Settings;
using OpenBullet2.Web.Dtos.Settings;
using RuriLib.Services;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Blocks.Parameters;
using System.Collections.Concurrent;
using System.Text.Json;
using Xunit;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class McpIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task McpEndpoint_ExposesSampleTool()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var tools = await client.ListToolsAsync(cancellationToken: TestCancellationToken);

        Assert.Contains(tools, tool => tool.Name == "get_server_info");
        Assert.Contains(tools, tool => tool.Name == "get_environment");
        Assert.Contains(tools, tool => tool.Name == "create_config");
        Assert.Contains(tools, tool => tool.Name == "list_configs");
        Assert.Contains(tools, tool => tool.Name == "get_settings");
        Assert.Contains(tools, tool => tool.Name == "get_rurilib_settings");
        Assert.Contains(tools, tool => tool.Name == "convert_lolicode_to_csharp");
        Assert.Contains(tools, tool => tool.Name == "get_config_making_guide");
        Assert.Contains(tools, tool => tool.Name == "get_config_making_topic");
        Assert.Contains(tools, tool => tool.Name == "debug_config");
        Assert.Contains(tools, tool => tool.Name == "list_blocks");
        Assert.Contains(tools, tool => tool.Name == "get_block_details");
        Assert.Contains(tools, tool => tool.Name == "get_config_lolicode");
        Assert.Contains(tools, tool => tool.Name == "update_config_lolicode");
        Assert.Contains(tools, tool => tool.Name == "get_config_readme");
        Assert.Contains(tools, tool => tool.Name == "update_config_readme");
        Assert.Contains(tools, tool => tool.Name == "get_config_settings");
        Assert.Contains(tools, tool => tool.Name == "update_config_settings");
        Assert.Contains(tools, tool => tool.Name == "get_config_metadata");
        Assert.Contains(tools, tool => tool.Name == "update_config_metadata");
    }

    [Fact]
    public async Task McpEndpoint_CallsSampleTool()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_server_info",
            cancellationToken: TestCancellationToken);

        var version = GetRequiredService<IUpdateService>().CurrentVersion;
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.False(result.IsError ?? false);
        Assert.Contains("OpenBullet 2 server info", text);
        Assert.Contains(version.ToString(), text);
        Assert.Contains(Path.GetFullPath(UserDataFolder), text);
    }

    [Fact]
    public async Task McpEndpoint_CallsEnvironmentTool()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_environment",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.False(result.IsError ?? false);
        Assert.Contains("Environment.ini", text);
        Assert.Contains(Path.GetFullPath(Path.Combine(UserDataFolder, "Environment.ini")), text);
        Assert.Contains("[WORDLIST TYPE]", text);
        Assert.Contains("Name=Default", text);
    }

    [Fact]
    public async Task McpEndpoint_GetsConfigMakingGuide()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_config_making_guide",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = JsonDocument.Parse(text);
        var root = json.RootElement;
        var topics = root.GetProperty("deepDiveTopics").EnumerateArray().ToList();
        var topicIds = topics.Select(t => t.GetProperty("id").GetString()).ToList();
        var guide = root.GetProperty("guide").GetString();

        Assert.False(result.IsError ?? false);
        Assert.Equal("OpenBullet 2 Config Making Guide", root.GetProperty("title").GetString());
        Assert.Contains("create_config", guide);
        Assert.Contains("list_blocks", guide);
        Assert.Contains("get_block_details", guide);
        Assert.Contains("convert_lolicode_to_csharp", guide);
        Assert.Contains("LoliCode and C#", guide);
        Assert.Contains("lolicode_statements_reference", topicIds);
        Assert.Contains("startup_script", topicIds);
        Assert.Contains("data_variable", topicIds);
        Assert.Contains("input_and_custom_inputs", topicIds);
    }

    [Fact]
    public async Task McpEndpoint_GetsConfigMakingTopic()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_config_making_topic",
            new Dictionary<string, object?>
            {
                ["topicId"] = "startup_script"
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = JsonDocument.Parse(text);
        var root = json.RootElement;
        var content = root.GetProperty("content").GetString();

        Assert.False(result.IsError ?? false);
        Assert.Equal("startup_script", root.GetProperty("topicId").GetString());
        Assert.Equal("Startup Script", root.GetProperty("title").GetString());
        Assert.Contains("only `globals` is available", content);
        Assert.Contains("`input` is not available", content);
        Assert.Contains("`data` is not available", content);
    }

    [Fact]
    public async Task McpEndpoint_ListsAvailableConfigs()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Metadata.Name = "MCP Test Config";
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "list_configs",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = System.Text.Json.JsonDocument.Parse(text);
        var configs = json.RootElement.EnumerateArray().ToList();

        Assert.False(result.IsError ?? false);
        Assert.Contains(configs, c => c.GetProperty("id").GetString() == config.Id);
        Assert.Contains(configs, c => c.GetProperty("name").GetString() == config.Metadata.Name);
    }

    [Fact]
    public async Task McpEndpoint_CreatesConfig()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();
        var settingsService = GetRequiredService<OpenBulletSettingsService>();

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "create_config",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
            text, JsonSerializerOptions);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(response);
        Assert.True(response.TryGetValue("id", out var configId));
        Assert.False(string.IsNullOrWhiteSpace(configId));

        var createdConfig = await configRepo.GetAsync(configId!);

        Assert.NotNull(createdConfig);
        Assert.Equal(settingsService.Settings.GeneralSettings.DefaultAuthor, createdConfig.Metadata.Author);
        Assert.Contains(configService.Configs, c => c.Id == configId);
    }

    [Fact]
    public async Task McpEndpoint_GetsOpenBulletSettings()
    {
        var settingsService = GetRequiredService<OpenBulletSettingsService>();
        settingsService.Settings.GeneralSettings.DefaultAuthor = "MCP Settings Test Author";
        settingsService.Settings.GeneralSettings.WarnConfigNotSaved = false;

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_settings",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var dto = System.Text.Json.JsonSerializer.Deserialize<OpenBulletSettingsDto>(
            text, JsonSerializerOptions);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(dto);
        Assert.Equal("MCP Settings Test Author", dto.GeneralSettings.DefaultAuthor);
        Assert.False(dto.GeneralSettings.WarnConfigNotSaved);
    }

    [Fact]
    public async Task McpEndpoint_GetsRuriLibSettings()
    {
        var settingsService = GetRequiredService<RuriLibSettingsService>();
        settingsService.RuriLibSettings.GeneralSettings.VerboseMode = true;
        settingsService.RuriLibSettings.GeneralSettings.RestrictBlocksToCWD = false;

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_rurilib_settings",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = System.Text.Json.JsonDocument.Parse(text);
        var root = json.RootElement;

        Assert.False(result.IsError ?? false);
        Assert.True(root.GetProperty("generalSettings").GetProperty("verboseMode").GetBoolean());
        Assert.False(root.GetProperty("generalSettings").GetProperty("restrictBlocksToCWD").GetBoolean());
    }

    [Fact]
    public async Task McpEndpoint_ConvertsLoliCodeToCSharp()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Mode = ConfigMode.LoliCode;
        config.LoliCodeScript = "LOG \"Hello, world!\"";
        config.StartupLoliCodeScript = "LOG \"Startup\"";
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "convert_lolicode_to_csharp",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = System.Text.Json.JsonDocument.Parse(text);
        var root = json.RootElement;

        Assert.False(result.IsError ?? false);
        Assert.True(root.GetProperty("converted").GetBoolean());
        Assert.Contains("data.Logger.LogObject(\"Hello, world!\");", root.GetProperty("cSharpScript").GetString());
        Assert.Contains("data.Logger.LogObject(\"Startup\");", root.GetProperty("startupCSharpScript").GetString());
    }

    [Fact]
    public async Task McpEndpoint_DebugsConfig_AndStreamsProgressLogs()
    {
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata(),
            Settings = new ConfigSettings
            {
                DataSettings = new DataSettings
                {
                    AllowedWordlistTypes = ["Default"]
                }
            },
            IsRemote = false,
            Mode = ConfigMode.LoliCode,
            LoliCodeScript = "LOG \"Hello, World!\"\nSET VAR \"TEST\" @input.DATA"
        };
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var progressMessages = new ConcurrentQueue<string>();
        var progress = new Progress<ProgressNotificationValue>(value =>
        {
            if (!string.IsNullOrWhiteSpace(value.Message))
            {
                progressMessages.Enqueue(value.Message);
            }
        });

        var result = await client.CallToolAsync(
            "debug_config",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["debug"] = new Dictionary<string, object?>
                {
                    ["testData"] = "Test data",
                    ["wordlistType"] = "Default",
                    ["testProxy"] = null,
                    ["proxyType"] = "Http"
                }
            },
            progress: progress,
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = JsonDocument.Parse(text);
        var root = json.RootElement;
        var variables = root.GetProperty("variables").EnumerateArray().ToList();
        var log = root.GetProperty("log").EnumerateArray().Select(e => e.GetString()).ToList();

        Assert.False(result.IsError ?? false);
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Contains(progressMessages, message => message.Contains("Hello, World!"));
        Assert.Contains(log, entry => entry!.Contains("Hello, World!"));
        Assert.Contains(variables, variable =>
            variable.GetProperty("name").GetString() == "TEST"
            && variable.GetProperty("value").GetString() == "Test data");
        Assert.True(root.GetProperty("error").ValueKind is JsonValueKind.Null);
    }

    [Fact]
    public async Task McpEndpoint_ListsBlocksByCategory()
    {
        var descriptor = RuriLib.Globals.DescriptorsRepository.Descriptors["HttpRequest"];

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "list_blocks",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = JsonDocument.Parse(text);
        var categories = json.RootElement.GetProperty("categories").EnumerateArray().ToList();
        var category = categories.FirstOrDefault(c => c.GetProperty("path").GetString() == descriptor.Category.Path);
        var blocks = category.GetProperty("blocks").EnumerateArray().ToList();

        Assert.False(result.IsError ?? false);
        Assert.Equal(descriptor.Category.Name, category.GetProperty("name").GetString());
        Assert.Equal(descriptor.Category.Description, category.GetProperty("description").GetString());
        Assert.False(category.TryGetProperty("backgroundColor", out _));
        Assert.False(category.TryGetProperty("foregroundColor", out _));
        Assert.False(category.TryGetProperty("namespace", out _));
        Assert.Contains(blocks, b => b.GetProperty("id").GetString() == descriptor.Id
            && b.GetProperty("name").GetString() == descriptor.Name
            && b.GetProperty("description").GetString() == descriptor.Description);
    }

    [Fact]
    public async Task McpEndpoint_GetsBlockDetails()
    {
        var descriptor = RuriLib.Globals.DescriptorsRepository.Descriptors["HttpRequest"];
        var enumParameter = descriptor.Parameters.Values.OfType<EnumParameter>().First();

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_block_details",
            new Dictionary<string, object?>
            {
                ["blockIds"] = new[] { descriptor.Id, "DefinitelyMissingBlock" }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = JsonDocument.Parse(text);
        var root = json.RootElement;
        var blocks = root.GetProperty("blocks").EnumerateArray().ToList();
        var block = blocks.Single();
        var parameters = block.GetProperty("parameters").EnumerateArray().ToList();
        var parameter = parameters.First(p => p.GetProperty("name").GetString() == enumParameter.Name);
        var notFoundBlockIds = root.GetProperty("notFoundBlockIds").EnumerateArray().Select(e => e.GetString()).ToList();

        Assert.False(result.IsError ?? false);
        Assert.Equal(descriptor.Id, block.GetProperty("id").GetString());
        Assert.Equal(descriptor.Name, block.GetProperty("name").GetString());
        Assert.Equal(descriptor.Description, block.GetProperty("description").GetString());
        Assert.Equal(descriptor.ExtraInfo, block.GetProperty("extraInfo").GetString());
        Assert.Equal(descriptor.ReturnType?.ToString(), block.GetProperty("returnType").GetString());
        Assert.Equal(descriptor.Category.Path, block.GetProperty("category").GetProperty("path").GetString());
        Assert.False(block.GetProperty("category").TryGetProperty("backgroundColor", out _));
        Assert.False(block.GetProperty("category").TryGetProperty("foregroundColor", out _));
        Assert.False(block.GetProperty("category").TryGetProperty("namespace", out _));
        Assert.Equal("enumParam", parameter.GetProperty("kind").GetString());
        Assert.Equal(enumParameter.EnumType.Name, parameter.GetProperty("type").GetString());
        Assert.Equal(enumParameter.DefaultValue, parameter.GetProperty("defaultValue").GetString());
        Assert.Equal(enumParameter.Options, parameter.GetProperty("options").EnumerateArray().Select(o => o.GetString()).ToArray());
        Assert.Contains("DefinitelyMissingBlock", notFoundBlockIds);
        Assert.Empty(block.GetProperty("agentNotes").EnumerateArray());
    }

    [Fact]
    public async Task McpEndpoint_GetsBlockDetails_WithAgentNotesForKnownGotchas()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_block_details",
            new Dictionary<string, object?>
            {
                ["blockIds"] = new[] { "Keycheck", "Parse" }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = JsonDocument.Parse(text);
        var blocks = json.RootElement.GetProperty("blocks").EnumerateArray().ToList();
        var keycheckNotes = blocks.Single(b => b.GetProperty("id").GetString() == "Keycheck")
            .GetProperty("agentNotes").EnumerateArray().Select(e => e.GetString()).ToList();
        var parseNotes = blocks.Single(b => b.GetProperty("id").GetString() == "Parse")
            .GetProperty("agentNotes").EnumerateArray().Select(e => e.GetString()).ToList();

        Assert.False(result.IsError ?? false);
        Assert.Contains(keycheckNotes, n => n!.Contains("empty string", StringComparison.Ordinal));
        Assert.Contains(keycheckNotes, n => n!.Contains("Exists", StringComparison.Ordinal));
        Assert.Empty(parseNotes);
    }

    [Fact]
    public async Task McpEndpoint_GetsBlockDetails_WithHelpfulDescriptionsForComplexParameters()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_block_details",
            new Dictionary<string, object?>
            {
                ["blockIds"] = new[] { "Parse", "HttpRequest" }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = JsonDocument.Parse(text);
        var blocks = json.RootElement.GetProperty("blocks").EnumerateArray().ToList();

        var parseOutputFormatDescription = blocks.Single(b => b.GetProperty("id").GetString() == "Parse")
            .GetProperty("parameters").EnumerateArray()
            .Single(p => p.GetProperty("name").GetString() == "outputFormat")
            .GetProperty("description").GetString();

        var httpVersionDescription = blocks.Single(b => b.GetProperty("id").GetString() == "HttpRequest")
            .GetProperty("parameters").EnumerateArray()
            .Single(p => p.GetProperty("name").GetString() == "httpVersion")
            .GetProperty("description").GetString();

        Assert.False(result.IsError ?? false);
        Assert.Contains("[0]", parseOutputFormatDescription);
        Assert.Contains("[1]", parseOutputFormatDescription);
        Assert.Contains("HTTP version string", httpVersionDescription);
    }

    [Fact]
    public async Task McpEndpoint_GetsConfigLoliCode()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Mode = ConfigMode.LoliCode;
        config.LoliCodeScript = "LOG \"Hello, World!\"";
        config.StartupLoliCodeScript = "LOG \"Startup\"";
        config.Settings.ScriptSettings.CustomUsings = ["System.Linq", "System.Net.Http"];
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_config_lolicode",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = System.Text.Json.JsonDocument.Parse(text);
        var root = json.RootElement;

        Assert.False(result.IsError ?? false);
        Assert.Equal("LOG \"Hello, World!\"", root.GetProperty("script").GetString());
        Assert.Equal("LOG \"Startup\"", root.GetProperty("startupScript").GetString());
        Assert.Equal("System.Linq", root.GetProperty("customUsings")[0].GetString());
        Assert.Equal("System.Net.Http", root.GetProperty("customUsings")[1].GetString());
    }

    [Fact]
    public async Task McpEndpoint_UpdatesConfigLoliCode()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.LoliCodeScript = "LOG \"Old script\"";
        config.StartupLoliCodeScript = string.Empty;
        config.Settings.ScriptSettings.CustomUsings = ["System"];
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "update_config_lolicode",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["loliCode"] = new Dictionary<string, object?>
                {
                    ["script"] = "LOG \"Hello, World!\"\nSET VAR \"TEST\" @input.DATA",
                    ["startupScript"] = "LOG \"Startup\"",
                    ["customUsings"] = new[] { "System.Linq", "System.Net.Http" }
                }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(
            text, JsonSerializerOptions);
        var reloadedConfig = await configRepo.GetAsync(config.Id);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(response);
        Assert.True(response["updated"].GetBoolean());
        Assert.Equal("LOG \"Hello, World!\"\nSET VAR \"TEST\" @input.DATA", config.LoliCodeScript);
        Assert.Equal("LOG \"Startup\"", config.StartupLoliCodeScript);
        Assert.Equal(ConfigMode.LoliCode, config.Mode);
        Assert.Equal(["System.Linq", "System.Net.Http"], config.Settings.ScriptSettings.CustomUsings);
        Assert.NotEmpty(config.Stack);
        Assert.Equal("LOG \"Hello, World!\"\nSET VAR \"TEST\" @input.DATA", reloadedConfig.LoliCodeScript);
        Assert.Equal("LOG \"Startup\"", reloadedConfig.StartupLoliCodeScript);
        Assert.Equal(ConfigMode.LoliCode, reloadedConfig.Mode);
        Assert.Equal(["System.Linq", "System.Net.Http"], reloadedConfig.Settings.ScriptSettings.CustomUsings);
    }

    [Fact]
    public async Task McpEndpoint_UpdateConfigLoliCode_InvalidScript_ReturnsParsingError()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "update_config_lolicode",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["loliCode"] = new Dictionary<string, object?>
                {
                    ["script"] = "BLOCK:ThisIsInvalid\n  abc",
                    ["startupScript"] = string.Empty,
                    ["customUsings"] = new[] { "System" }
                }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(
            text, JsonSerializerOptions);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(response);
        Assert.False(response["updated"].GetBoolean());
        Assert.Contains("Invalid block id", response["error"].GetString());
    }

    [Fact]
    public async Task McpEndpoint_GetsConfigReadme()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Readme = "## MCP Test Readme";
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_config_readme",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var dto = System.Text.Json.JsonSerializer.Deserialize<ConfigReadmeDto>(
            text, JsonSerializerOptions);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(dto);
        Assert.Equal(config.Readme, dto.MarkdownText);
    }

    [Fact]
    public async Task McpEndpoint_UpdatesConfigReadme()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Readme = "Old readme";
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "update_config_readme",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["readme"] = new Dictionary<string, object?>
                {
                    ["markdownText"] = "Updated MCP readme"
                }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(
            text, JsonSerializerOptions);
        var reloadedConfig = await configRepo.GetAsync(config.Id);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(response);
        Assert.True(response["updated"]);
        Assert.Equal("Updated MCP readme", config.Readme);
        Assert.Equal("Updated MCP readme", reloadedConfig.Readme);
    }

    [Fact]
    public async Task McpEndpoint_GetsConfigSettings()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Settings.ProxySettings.UseProxies = true;
        config.Settings.GeneralSettings.SuggestedBots = 42;
        config.Settings.DataSettings.AllowedWordlistTypes = ["Credentials"];
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_config_settings",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var dto = System.Text.Json.JsonSerializer.Deserialize<ConfigSettingsDto>(
            text, JsonSerializerOptions);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(dto);
        Assert.True(dto.ProxySettings.UseProxies);
        Assert.Equal(42, dto.GeneralSettings.SuggestedBots);
        Assert.Equal(["Credentials"], dto.DataSettings.AllowedWordlistTypes);
    }

    [Fact]
    public async Task McpEndpoint_UpdatesConfigSettings()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Settings.ProxySettings.UseProxies = false;
        config.Settings.GeneralSettings.SuggestedBots = 10;
        config.Settings.DataSettings.AllowedWordlistTypes = ["Default"];
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "update_config_settings",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["settings"] = new Dictionary<string, object?>
                {
                    ["generalSettings"] = new Dictionary<string, object?>
                    {
                        ["suggestedBots"] = 99
                    },
                    ["proxySettings"] = new Dictionary<string, object?>
                    {
                        ["useProxies"] = true
                    },
                    ["inputSettings"] = new Dictionary<string, object?>(),
                    ["dataSettings"] = new Dictionary<string, object?>
                    {
                        ["allowedWordlistTypes"] = new[] { "Credentials" }
                    },
                    ["browserSettings"] = new Dictionary<string, object?>(),
                    ["scriptSettings"] = new Dictionary<string, object?>()
                }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(
            text, JsonSerializerOptions);
        var reloadedConfig = await configRepo.GetAsync(config.Id);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(response);
        Assert.True(response["updated"]);
        Assert.True(config.Settings.ProxySettings.UseProxies);
        Assert.Equal(99, config.Settings.GeneralSettings.SuggestedBots);
        Assert.Equal(["Credentials"], config.Settings.DataSettings.AllowedWordlistTypes);
        Assert.True(reloadedConfig.Settings.ProxySettings.UseProxies);
        Assert.Equal(99, reloadedConfig.Settings.GeneralSettings.SuggestedBots);
        Assert.Equal(["Credentials"], reloadedConfig.Settings.DataSettings.AllowedWordlistTypes);
    }

    [Fact]
    public async Task McpEndpoint_UpdateConfigSettings_InvalidSettings_ReturnsValidationError()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "update_config_settings",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["settings"] = new Dictionary<string, object?>
                {
                    ["generalSettings"] = new Dictionary<string, object?>
                    {
                        ["suggestedBots"] = -1
                    },
                    ["proxySettings"] = new Dictionary<string, object?>(),
                    ["inputSettings"] = new Dictionary<string, object?>(),
                    ["dataSettings"] = new Dictionary<string, object?>
                    {
                        ["allowedWordlistTypes"] = Array.Empty<string>()
                    },
                    ["browserSettings"] = new Dictionary<string, object?>(),
                    ["scriptSettings"] = new Dictionary<string, object?>()
                }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.True(result.IsError ?? false);
        Assert.Contains("SuggestedBots must be greater than or equal to 0.", text);
        Assert.Contains("AllowedWordlistTypes must contain at least one value.", text);
    }

    [Fact]
    public async Task McpEndpoint_GetsConfigMetadata()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Metadata.Name = "MCP Metadata Test";
        config.Metadata.Author = "Codex";
        config.Metadata.Category = "Tests";
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_config_metadata",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var json = System.Text.Json.JsonDocument.Parse(text);
        var root = json.RootElement;

        Assert.False(result.IsError ?? false);
        Assert.Equal(config.Metadata.Name, root.GetProperty("name").GetString());
        Assert.Equal(config.Metadata.Author, root.GetProperty("author").GetString());
        Assert.Equal(config.Metadata.Category, root.GetProperty("category").GetString());
        Assert.Equal(config.Metadata.LastModified, root.GetProperty("lastModified").GetDateTime());
        Assert.False(root.TryGetProperty("base64Image", out _));
    }

    [Fact]
    public async Task McpEndpoint_UpdatesConfigMetadata_WithoutChangingImage()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Metadata.Name = "Old Metadata Name";
        config.Metadata.Author = "Old Author";
        config.Metadata.Category = "Old Category";
        config.Metadata.Base64Image = "base64-image-value";
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "update_config_metadata",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["metadata"] = new Dictionary<string, object?>
                {
                    ["name"] = "Updated Metadata Name",
                    ["author"] = "Updated Author",
                    ["category"] = "Updated Category"
                }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(
            text, JsonSerializerOptions);
        var reloadedConfig = await configRepo.GetAsync(config.Id);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(response);
        Assert.True(response["updated"]);
        Assert.Equal("Updated Metadata Name", config.Metadata.Name);
        Assert.Equal("Updated Author", config.Metadata.Author);
        Assert.Equal("Updated Category", config.Metadata.Category);
        Assert.Equal("base64-image-value", config.Metadata.Base64Image);
        Assert.Equal("Updated Metadata Name", reloadedConfig.Metadata.Name);
        Assert.Equal("Updated Author", reloadedConfig.Metadata.Author);
        Assert.Equal("Updated Category", reloadedConfig.Metadata.Category);
        Assert.Equal("base64-image-value", reloadedConfig.Metadata.Base64Image);
    }

    [Fact]
    public async Task McpEndpoint_WhenLoginRequired_AnonymousClientIsRejected()
    {
        RequireLogin();

        using var client = Factory.CreateClient();
        var response = await client.GetAsync("/mcp", TestCancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task McpEndpoint_WhenLoginRequired_AdminBearerTokenCanCallTool()
    {
        RequireLogin();

        using var httpClient = Factory.CreateClient();
        ImpersonateAdmin(httpClient);

        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_server_info",
            cancellationToken: TestCancellationToken);

        Assert.False(result.IsError ?? false);
    }

    [Fact]
    public async Task McpEndpoint_WhenLoginRequired_GuestBearerTokenIsRejected()
    {
        RequireLogin();

        using var httpClient = Factory.CreateClient();
        var guestRepo = GetRequiredService<IGuestRepository>();
        var guest = new GuestEntity
        {
            Username = "guest_user",
            AccessExpiration = DateTime.UtcNow.AddDays(1),
            AllowedAddresses = string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest_pass")
        };

        await guestRepo.AddAsync(guest, TestCancellationToken);
        ImpersonateGuest(httpClient, guest);

        var response = await httpClient.GetAsync("/mcp", TestCancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
}
