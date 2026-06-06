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
    public static long Ceil(BotData data, [Variable] double input)
    {
        data.Logger.LogHeader();

        var rounded = Convert.ToInt64(Math.Ceiling(input));
        data.Logger.Log($"Rounded {input.AsString()} to {rounded}", LogColors.YellowGreen);
        return rounded;
    }

    /// <summary>
    /// Rounds the value up to the nearest integer.
    /// </summary>
    public static int Ceil(BotData data, [Variable] float input)
        => Convert.ToInt32(Ceil(data, (double)input));

    /// <summary>
    /// Rounds the value down to the nearest integer.
    /// </summary>
    [Block("Rounds the value down to the nearest integer")]
    public static long Floor(BotData data, [Variable] double input)
    {
        data.Logger.LogHeader();

        var rounded = Convert.ToInt64(Math.Floor(input));
        data.Logger.Log($"Rounded {input.AsString()} to {rounded}", LogColors.YellowGreen);
        return rounded;
    }

    /// <summary>
    /// Rounds the value down to the nearest integer.
    /// </summary>
    public static int Floor(BotData data, [Variable] float input)
        => Convert.ToInt32(Floor(data, (double)input));

    /// <summary>
    /// Rounds the value to the nearest integer.
    /// </summary>
    [Block("Rounds the value to the nearest integer")]
    public static long RoundToInteger(BotData data, [Variable] double input)
    {
        data.Logger.LogHeader();

        var rounded = Convert.ToInt64(Round(data, input, 0));
        data.Logger.Log($"Rounded {input.AsString()} to {rounded}", LogColors.YellowGreen);
        return rounded;
    }

    /// <summary>
    /// Rounds the value to the nearest integer.
    /// </summary>
    public static int RoundToInteger(BotData data, [Variable] float input)
        => Convert.ToInt32(RoundToInteger(data, (double)input));

    /// <summary>
    /// Rounds the value to the given decimal places.
    /// </summary>
    [Block("Rounds the value to the given decimal places")]
    public static double Round(BotData data, [Variable] double input, long decimalPlaces = 2)
    {
        data.Logger.LogHeader();

        var rounded = Math.Round(input, checked((int)decimalPlaces), MidpointRounding.AwayFromZero);
        data.Logger.Log($"Rounded {input.AsString()} to {rounded.AsString()}", LogColors.YellowGreen);
        return rounded;
    }

    /// <summary>
    /// Rounds the value to the given decimal places.
    /// </summary>
    public static float Round(BotData data, [Variable] float input, int decimalPlaces = 2)
        => Convert.ToSingle(Round(data, (double)input, decimalPlaces));

    /// <summary>
    /// Computes the value of a given mathematical expression.
    /// </summary>
    public static float Compute(BotData data, [Variable] string input)
        => Convert.ToSingle(ComputeDouble(data, input));

    /// <summary>
    /// Computes the value of a given mathematical expression.
    /// </summary>
    [Block("Computes the value of a given mathematical expression", id = nameof(Compute), name = "Compute")]
    public static double ComputeDouble(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var result = Convert.ToDouble(new DataTable().Compute(input.Replace(',', '.'), null));
        data.Logger.Log($"Computed {input} with result {result.AsString()}", LogColors.YellowGreen);
        return result;
    }

    /// <summary>
    /// Generates a random float between two values (inclusive).
    /// </summary>
    [Block("Generates a random float between two values (inclusive)")]
    public static double RandomFloat(BotData data, double minimum = 0, double maximum = 1)
    {
        data.Logger.LogHeader();

        var random = data.Random.NextDouble() * (maximum - minimum) + minimum;
        data.Logger.Log($"Generated random value {random.AsString()} in the interval ({minimum.AsString()},{maximum.AsString()})", LogColors.YellowGreen);
        return random;
    }

    /// <summary>
    /// Generates a random float between two values (inclusive).
    /// </summary>
    public static float RandomFloat(BotData data, float minimum = 0, float maximum = 1)
        => Convert.ToSingle(RandomFloat(data, (double)minimum, maximum));

    /// <summary>
    /// Takes the maximum between two floats.
    /// </summary>
    [Block("Takes the maximum between two floats", name = "Maximum Float")]
    public static double TakeMaxFloat(BotData data, double first, double second)
    {
        data.Logger.LogHeader();

        var max = Math.Max(first, second);
        data.Logger.Log($"The maximum between {first} and {second} is {max}", LogColors.YellowGreen);
        return max;
    }

    /// <summary>
    /// Takes the maximum between two floats.
    /// </summary>
    public static float TakeMaxFloat(BotData data, float first, float second)
        => Convert.ToSingle(TakeMaxFloat(data, (double)first, second));

    /// <summary>
    /// Takes the minimum between two floats.
    /// </summary>
    [Block("Takes the minimum between two floats", name = "Minimum Float")]
    public static double TakeMinFloat(BotData data, double first, double second)
    {
        data.Logger.LogHeader();

        var min = Math.Min(first, second);
        data.Logger.Log($"The minimum between {first} and {second} is {min}", LogColors.YellowGreen);
        return min;
    }

    /// <summary>
    /// Takes the minimum between two floats.
    /// </summary>
    public static float TakeMinFloat(BotData data, float first, float second)
        => Convert.ToSingle(TakeMinFloat(data, (double)first, second));
}
