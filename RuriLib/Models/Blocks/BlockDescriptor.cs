using System.Collections.Generic;
using Newtonsoft.Json;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Variables;

namespace RuriLib.Models.Blocks;

/// <summary>
/// Describes the metadata and UI configuration of a block.
/// </summary>
public class BlockDescriptor
{
    /// <summary>
    /// The unique block id.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the block.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Additional ids that resolve to this block when parsing LoliCode.
    /// </summary>
    public List<string> Aliases { get; set; } = [];

    /// <summary>
    /// The description shown to the user.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Extra information shown in the UI.
    /// </summary>
    public string ExtraInfo { get; set; } = string.Empty;

    /// <summary>
    /// The full name of the assembly that owns the block.
    /// </summary>
    public string AssemblyFullName { get; set; } = string.Empty;

    /// <summary>
    /// The optional return type of the block.
    /// </summary>
    public VariableType? ReturnType { get; set; }

    /// <summary>
    /// The category metadata for the block.
    /// </summary>
    public BlockCategory Category { get; set; } = new();

    /// <summary>
    /// The settings parameters keyed by parameter name.
    /// </summary>
    public Dictionary<string, BlockParameter> Parameters { get; set; } = [];

    /// <summary>
    /// The actions available for the block.
    /// </summary>
    [JsonIgnore]
    public List<BlockActionInfo> Actions { get; set; } = [];

    /// <summary>
    /// The images available for the block.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, BlockImageInfo> Images { get; set; } = [];
}
