using System.Collections.Frozen;
using System.Collections.Immutable;
using OpenBullet2.Core.Models.Settings;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Provides autocompletion snippets for the LoliCode editor.
/// </summary>
public class LoliCodeAutocompletionService
{
    /// <summary>
    /// The block snippets.
    /// </summary>
    public FrozenDictionary<string, string> BlockSnippets { get; private set; }
    
    /// <summary></summary>
    public LoliCodeAutocompletionService()
    {
        BlockSnippets = ImmutableDictionary<string, string>.Empty.ToFrozenDictionary();
    }
    
    /// <summary>
    /// Initializes the autocompletion service.
    /// </summary>
    public void Init()
    {
        var blockSnippets = new Dictionary<string, string>();
        
        foreach (var id in RuriLib.Globals.DescriptorsRepository.Descriptors.Keys)
        {
            var block = BlockFactory.GetBlock<BlockInstance>(id);
            blockSnippets[id] = block.ToLC(true);
        }
        
        BlockSnippets = blockSnippets.ToFrozenDictionary();
    }
}
