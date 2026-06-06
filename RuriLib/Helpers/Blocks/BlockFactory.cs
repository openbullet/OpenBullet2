using System;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;

namespace RuriLib.Helpers.Blocks;

/// <summary>
/// In charge of creating new blocks.
/// </summary>
public class BlockFactory
{
    /// <summary>
    /// The label of the block, useful to specify the purpose of the block.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Whether the block is disabled and will not be executed.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets a block by <paramref name="id"/> and casts it to the requested type.
    /// </summary>
    /// <typeparam name="T">The expected block instance type.</typeparam>
    /// <param name="id">The unique block identifier.</param>
    /// <returns>The created block instance cast to <typeparamref name="T"/>.</returns>
    public static T GetBlock<T>(string id) where T : BlockInstance
    {
        if (!Globals.DescriptorsRepository.Descriptors.TryGetValue(id, out var descriptor))
        {
            throw new Exception($"Invalid block id: {id}");
        }

        BlockInstance instance = descriptor switch
        {
            AutoBlockDescriptor autoBlockDescriptor => new AutoBlockInstance(autoBlockDescriptor),
            KeycheckBlockDescriptor keycheckBlockDescriptor => new KeycheckBlockInstance(keycheckBlockDescriptor),
            HttpRequestBlockDescriptor httpRequestBlockDescriptor => new HttpRequestBlockInstance(httpRequestBlockDescriptor),
            ParseBlockDescriptor parseBlockDescriptor => new ParseBlockInstance(parseBlockDescriptor),
            ScriptBlockDescriptor scriptBlockDescriptor => new ScriptBlockInstance(scriptBlockDescriptor),
            _ => throw new NotImplementedException()
        };

        if (instance is T typedInstance)
        {
            return typedInstance;
        }

        throw new InvalidCastException($"Block {id} cannot be cast to {typeof(T).Name}");
    }
}
