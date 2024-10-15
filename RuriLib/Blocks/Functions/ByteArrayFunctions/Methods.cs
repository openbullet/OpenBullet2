using RuriLib.Attributes;
using RuriLib.Functions.Conversion;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;

// ReSharper disable once CheckNamespace
namespace RuriLib.Blocks.Functions.ByteArray;

[BlockCategory("Byte Array Functions", "Blocks for working with byte arrays", "#9acd32")]
public static class Methods
{
    [Block("Merges two byte arrays together to form a longer one")]
    public static byte[] MergeByteArrays(BotData data, byte[] first, byte[] second)
    {
        data.Logger.LogHeader();
        
        var merged = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, merged, 0, first.Length);
        Buffer.BlockCopy(second, 0, merged, second.Length, second.Length);
            
        data.Logger.Log($"Merged {HexConverter.ToHexString(first)} and {HexConverter.ToHexString(second)} into {HexConverter.ToHexString(merged)}", LogColors.YellowGreen);
        return merged;
    }
}
