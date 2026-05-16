using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OpenBullet2.Web.Exceptions;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Parameters;

namespace OpenBullet2.Web.Mcp;

/// <summary>
/// MCP tools related to block discovery and reference.
/// </summary>
[McpServerToolType]
public sealed class BlockReferenceMcpTools
{
    /// <summary>
    /// Lists the available blocks grouped by category without including API reference details.
    /// </summary>
    [McpServerTool(Name = "list_blocks"),
     Description("Lists the available blocks grouped by category with each block's id, name, and description, without including detailed API reference information.")]
    public string ListBlocks()
    {
        var categories = RuriLib.Globals.DescriptorsRepository.Descriptors.Values
            .GroupBy(d => d.Category.Path)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new BlockCategorySummary(
                g.First().Category.Name,
                g.Key,
                g.First().Category.Description,
                g.OrderBy(d => d.Name, StringComparer.Ordinal)
                    .Select(d => new BlockSummary(
                        d.Id,
                        d.Name,
                        d.Description))
                    .ToList()))
            .ToList();

        return JsonSerializer.Serialize(new ListBlocksResult(categories), JsonSerializerOptions);
    }

    /// <summary>
    /// Gets detailed reference information for the requested blocks.
    /// </summary>
    [McpServerTool(Name = "get_block_details"),
     Description("Gets detailed reference information for the requested blocks, including parameters, types, descriptions, extra block info, and return type when present.")]
    public string GetBlockDetails(List<string>? blockIds)
    {
        blockIds ??= [];

        var descriptors = RuriLib.Globals.DescriptorsRepository.Descriptors;
        var blocks = new List<BlockDetailsResult>();
        var notFoundBlockIds = new List<string>();

        foreach (var blockId in blockIds)
        {
            if (!descriptors.TryGetValue(blockId, out var descriptor))
            {
                notFoundBlockIds.Add(blockId);
                continue;
            }

            blocks.Add(new BlockDetailsResult(
                descriptor.Id,
                descriptor.Name,
                descriptor.Description,
                descriptor.ExtraInfo,
                descriptor.ReturnType?.ToString(),
                MapCategory(descriptor.Category),
                descriptor.Parameters.Values.Select(MapParameter).ToList(),
                GetAgentNotes(descriptor.Id)));
        }

        return JsonSerializer.Serialize(new GetBlockDetailsResult(blocks, notFoundBlockIds), JsonSerializerOptions);
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static BlockCategorySummaryInfo MapCategory(BlockCategory category)
        => new(category.Name, category.Path, category.Description);

    private static List<string> GetAgentNotes(string blockId)
        => blockId switch
        {
            "Keycheck" =>
            [
                "When using the Exists or DoesNotExist comparer, still provide an empty string on the right-hand side instead of omitting it."
            ],

            "Parse" =>
            [
                "In Regex mode, captured groups are referenced as [1], [2], and so on, not $1, $2, etc."
            ],

            _ => []
        };

    private static BlockParameterInfo MapParameter(BlockParameter parameter)
        => parameter switch
        {
            StringParameter p => new BlockParameterInfo(
                "stringParam",
                "string",
                p.Name,
                p.AssignedName ?? string.Empty,
                p.PrettyName,
                p.Description,
                p.InputMode.ToString(),
                p.DefaultVariableName,
                p.DefaultValue ?? string.Empty,
                p.MultiLine),

            IntParameter p => new BlockParameterInfo(
                "intParam",
                "int",
                p.Name,
                p.AssignedName ?? string.Empty,
                p.PrettyName,
                p.Description,
                p.InputMode.ToString(),
                p.DefaultVariableName,
                p.DefaultValue),

            FloatParameter p => new BlockParameterInfo(
                "floatParam",
                "float",
                p.Name,
                p.AssignedName ?? string.Empty,
                p.PrettyName,
                p.Description,
                p.InputMode.ToString(),
                p.DefaultVariableName,
                p.DefaultValue),

            BoolParameter p => new BlockParameterInfo(
                "boolParam",
                "bool",
                p.Name,
                p.AssignedName ?? string.Empty,
                p.PrettyName,
                p.Description,
                p.InputMode.ToString(),
                p.DefaultVariableName,
                p.DefaultValue),

            EnumParameter p => new BlockParameterInfo(
                "enumParam",
                p.EnumType.Name,
                p.Name,
                p.AssignedName ?? string.Empty,
                p.PrettyName,
                p.Description,
                p.InputMode.ToString(),
                p.DefaultVariableName,
                p.DefaultValue,
                Options: p.Options),

            ListOfStringsParameter p => new BlockParameterInfo(
                "listOfStringsParam",
                "string[]",
                p.Name,
                p.AssignedName ?? string.Empty,
                p.PrettyName,
                p.Description,
                p.InputMode.ToString(),
                p.DefaultVariableName,
                p.DefaultValue),

            DictionaryOfStringsParameter p => new BlockParameterInfo(
                "dictionaryOfStringsParam",
                "dictionary<string,string>",
                p.Name,
                p.AssignedName ?? string.Empty,
                p.PrettyName,
                p.Description,
                p.InputMode.ToString(),
                p.DefaultVariableName,
                p.DefaultValue),

            ByteArrayParameter p => new BlockParameterInfo(
                "byteArrayParam",
                "byte[]",
                p.Name,
                p.AssignedName ?? string.Empty,
                p.PrettyName,
                p.Description,
                p.InputMode.ToString(),
                p.DefaultVariableName,
                DefaultValueBase64: Convert.ToBase64String(p.DefaultValue)),

            _ => throw new ApiException(ErrorCode.InternalServerError,
                $"Unsupported block parameter type {parameter.GetType().Name}")
        };

    private sealed record ListBlocksResult(List<BlockCategorySummary> Categories);

    private sealed record BlockCategorySummary(
        string Name,
        string Path,
        string Description,
        List<BlockSummary> Blocks);

    private sealed record BlockSummary(
        string Id,
        string Name,
        string Description);

    private sealed record GetBlockDetailsResult(
        List<BlockDetailsResult> Blocks,
        List<string> NotFoundBlockIds);

    private sealed record BlockDetailsResult(
        string Id,
        string Name,
        string Description,
        string ExtraInfo,
        string? ReturnType,
        BlockCategorySummaryInfo Category,
        List<BlockParameterInfo> Parameters,
        List<string> AgentNotes);

    private sealed record BlockCategorySummaryInfo(
        string Name,
        string Path,
        string Description);

    private sealed record BlockParameterInfo(
        string Kind,
        string Type,
        string Name,
        string AssignedName,
        string PrettyName,
        string? Description,
        string InputMode,
        string DefaultVariableName,
        object? DefaultValue = null,
        bool? MultiLine = null,
        string[]? Options = null,
        string? DefaultValueBase64 = null);
}
