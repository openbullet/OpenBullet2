using RuriLib.Attributes;
using RuriLib.Extensions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Data;

// ReSharper disable once CheckNamespace
namespace RuriLib.Blocks.Functions.Float;

/// <summary>
/// Blocks for working with floating point numbers.
/// </summary>
[BlockCategory("Float Functions", "Blocks for working with floating point numbers", "#9acd32")]
public static class Methods
{
    /// <summary>
    /// Rounds the value up to the nearest integer.
    /// </summary>
    [Block("Rounds the value up to the nearest integer")]
    public static int Ceil(BotData data, [Variable] float input)
    {
        data.Logger.LogHeader();
        
        var rounded = Convert.ToInt32(Math.Ceiling(input));
        data.Logger.Log($"Rounded {input.AsString()} to {rounded}", LogColors.YellowGreen);
        return rounded;
    }

    /// <summary>
    /// Rounds the value down to the nearest integer.
    /// </summary>
    [Block("Rounds the value down to the nearest integer")]
    public static int Floor(BotData data, [Variable] float input)
    {
        data.Logger.LogHeader();
        
        var rounded = Convert.ToInt32(Math.Floor(input));
        data.Logger.Log($"Rounded {input.AsString()} to {rounded}", LogColors.YellowGreen);
        return rounded;
    }

    /// <summary>
    /// Rounds the value to the nearest integer.
    /// </summary>
    [Block("Rounds the value to the nearest integer")]
    public static int RoundToInteger(BotData data, [Variable] float input)
    {
        data.Logger.LogHeader();
        
        var rounded = Convert.ToInt32(Round(data, input, 0));
        data.Logger.Log($"Rounded {input.AsString()} to {rounded}", LogColors.YellowGreen);
        return rounded;
    }

    /// <summary>
    /// Rounds the value to the given decimal places.
    /// </summary>
    [Block("Rounds the value to the given decimal places")]
    public static float Round(BotData data, [Variable] float input, int decimalPlaces = 2)
    {
        data.Logger.LogHeader();
        
        var rounded = Convert.ToSingle(Math.Round(input, decimalPlaces, MidpointRounding.AwayFromZero));
        data.Logger.Log($"Rounded {input.AsString()} to {rounded.AsString()}", LogColors.YellowGreen);
        return rounded;
    }

    /// <summary>
    /// Computes the value of a given mathematical expression.
    /// </summary>
    [Block("Computes the value of a given mathematical expression")]
    public static float Compute(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        var result = Convert.ToSingle(new DataTable().Compute(input.Replace(',', '.'), null));
        data.Logger.Log($"Computed {input} with result {result.AsString()}", LogColors.YellowGreen);
        return result;
    }

    /// <summary>
    /// Generates a random float between two values (inclusive).
    /// </summary>
    [Block("Generates a random float between two values (inclusive)")]
    public static float RandomFloat(BotData data, float minimum = 0, float maximum = 1)
    {
        data.Logger.LogHeader();
        
        var random = Convert.ToSingle(data.Random.NextDouble()) * (maximum - minimum) + minimum;
        data.Logger.Log($"Generated random value {random.AsString()} in the interval ({minimum.AsString()},{maximum.AsString()})", LogColors.YellowGreen);
        return random;
    }

    /// <summary>
    /// Takes the maximum between two floats.
    /// </summary>
    [Block("Takes the maximum between two floats", name = "Maximum Float")]
    public static float TakeMaxFloat(BotData data, float first, float second)
    {
        data.Logger.LogHeader();
        
        var max = Math.Max(first, second);
        data.Logger.Log($"The maximum between {first} and {second} is {max}", LogColors.YellowGreen);
        return max;
    }

    /// <summary>
    /// Takes the minimum between two floats.
    /// </summary>
    [Block("Takes the minimum between two floats", name = "Minimum Float")]
    public static float TakeMinFloat(BotData data, float first, float second)
    {
        data.Logger.LogHeader();
        
        var min = Math.Min(first, second);
        data.Logger.Log($"The minimum between {first} and {second} is {min}", LogColors.YellowGreen);
        return min;
    }
}
