using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using OpenBullet2.Web.Exceptions;

namespace OpenBullet2.Web.Mcp;

/// <summary>
/// MCP tools related to config making documentation.
/// </summary>
[McpServerToolType]
public sealed class ConfigMakingDocsMcpTools
{
    /// <summary>
    /// Gets the compact guide for making configs in OpenBullet 2.
    /// </summary>
    [McpServerTool(Name = "get_config_making_guide"),
     Description("Gets the compact OpenBullet 2 config making guide, including the recommended tool flow and the list of deeper topics available through get_config_making_topic.")]
    public string GetConfigMakingGuide()
    {
        var result = new ConfigMakingGuideResult(
            "OpenBullet 2 Config Making Guide",
            ReadDocsFile("config-making-guide.md"),
            Topics.Select(t => new TopicSummary(t.Id, t.Title, t.Description)).ToList());

        return JsonSerializer.Serialize(result, JsonSerializerOptions);
    }

    /// <summary>
    /// Gets a deeper config making topic by id.
    /// </summary>
    [McpServerTool(Name = "get_config_making_topic"),
     Description("Gets an in-depth OpenBullet 2 config making topic by id, for example startup_script, data_variable, input_and_custom_inputs, or wordlists_and_environment.")]
    public string GetConfigMakingTopic(string topicId)
    {
        var topic = Topics.FirstOrDefault(t => string.Equals(t.Id, topicId, StringComparison.Ordinal));

        if (topic is null)
        {
            throw new McpException(
                $"Unknown config making topic '{topicId}'. Valid topic ids: {string.Join(", ", Topics.Select(t => t.Id))}");
        }

        var result = new ConfigMakingTopicResult(topic.Id, topic.Title, ReadDocsFile(topic.RelativePath));
        return JsonSerializer.Serialize(result, JsonSerializerOptions);
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly List<TopicDefinition> Topics =
    [
        new("lolicode_basics", "LoliCode Basics", "How OB2 scripts are structured and how LoliCode and C# coexist.", "topics/lolicode-basics.md"),
        new("blocks_and_values", "Blocks And Values", "Block syntax, setting value forms, outputs, and safe mode.", "topics/blocks-and-values.md"),
        new("statements_and_control_flow", "Statements And Control Flow", "Native LoliCode statements such as IF, TRY, LOCK, and SET.", "topics/statements-and-control-flow.md"),
        new("lolicode_statements_reference", "LoliCode Statements Reference", "Statement-by-statement syntax reference with examples for the supported native LoliCode statements.", "topics/lolicode-statements-reference.md"),
        new("wordlists_and_environment", "Wordlists And Environment", "How Environment.ini defines wordlist types and how slices become input variables.", "topics/wordlists-and-environment.md"),
        new("input_and_custom_inputs", "Input And Custom Inputs", "What the input variable contains and how custom inputs are exposed.", "topics/input-and-custom-inputs.md"),
        new("data_variable", "Data Variable", "The runtime shape of data, including response state, proxy state, and logger access.", "topics/data-variable.md"),
        new("data_rules", "Data Rules", "Config-level validation rules applied after wordlist slicing.", "topics/data-rules.md"),
        new("runtime_outcomes_and_execution_model", "Runtime Outcomes And Execution Model", "How statuses, retries, hits, proxies, debugger behavior, and concurrency affect config design.", "topics/runtime-outcomes-and-execution-model.md"),
        new("globals_variable", "Globals Variable", "Shared cross-bot state and reserved global properties.", "topics/globals-variable.md"),
        new("startup_script", "Startup Script", "When startup runs, what variables exist there, and how to use it safely.", "topics/startup-script.md"),
        new("csharp_interop_and_usings", "Interop And Usings", "When to use inline C#, Script block interpreters like Python or NodeJS, and custom usings effectively.", "topics/csharp-interop-and-usings.md"),
        new("external_libraries", "External Libraries", "How plugin DLLs are loaded and how external C# libraries become available to scripts.", "topics/external-libraries.md"),
        new("proxies_for_config_makers", "Proxies For Config Makers", "Proxy-related settings, statuses, and runtime proxy properties relevant to configs.", "topics/proxies-for-config-makers.md")
    ];

    private static string ReadDocsFile(string relativePath)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "Mcp", "Docs",
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        return File.Exists(fullPath)
            ? File.ReadAllText(fullPath)
            : throw new ApiException(ErrorCode.InternalServerError,
                $"MCP documentation file '{relativePath}' was not found");
    }

    private sealed record ConfigMakingGuideResult(
        string Title,
        string Guide,
        List<TopicSummary> DeepDiveTopics);

    private sealed record TopicSummary(
        string Id,
        string Title,
        string Description);

    private sealed record ConfigMakingTopicResult(
        string TopicId,
        string Title,
        string Content);

    private sealed record TopicDefinition(
        string Id,
        string Title,
        string Description,
        string RelativePath);
}
