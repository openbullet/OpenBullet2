using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;

// ReSharper disable once CheckNamespace
namespace RuriLib.Blocks.Functions.Integer;

/// <summary>
/// Blocks for working with integer numbers.
/// </summary>
[BlockCategory("Integer Functions", "Blocks for working with integer numbers", "#9acd32")]
public static class Methods
{
    /// <summary>
    /// Generates a random integer between two values (inclusive).
    /// </summary>
    [Block("Generates a random integer between two values (inclusive)")]
    public static int RandomInteger(BotData data, int minimum = 0, int maximum = 10)
    {
        data.Logger.LogHeader();
        
        var random = data.Random.Next(minimum, maximum + 1);
        data.Logger.Log($"Generated random value {random} in the interval ({minimum},{maximum})", LogColors.YellowGreen);
        return random;
    }

    /// <summary>
    /// Takes the maximum between two integers.
    /// </summary>
    [Block("Takes the maximum between two integers", name = "Maximum Int")]
    public static int TakeMaxInt(BotData data, int first, int second)
    {
        data.Logger.LogHeader();
        
        var max = Math.Max(first, second);
        data.Logger.Log($"The maximum between {first} and {second} is {max}", LogColors.YellowGreen);
        return max;
    }

    /// <summary>
    /// Takes the minimum between two integers.
    /// </summary>
    [Block("Takes the minimum between two integers", name = "Minimum int")]
    public static int TakeMinInt(BotData data, int first, int second)
    {
        data.Logger.LogHeader();
        
        var min = Math.Min(first, second);
        data.Logger.Log($"The minimum between {first} and {second} is {min}", LogColors.YellowGreen);
        return min;
    }
}
