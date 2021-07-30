using RuriLib.Attributes;
using RuriLib.Extensions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Data;

namespace RuriLib.Blocks.Functions.Float
{
    [BlockCategory("Float Functions", "Blocks for working with floating point numbers", "#9acd32")]
    public static class Methods
    {
        [Block("Rounds the value up to the nearest integer")]
        public static int Ceil(BotData data, [Variable] float input)
        {
            var rounded = Convert.ToInt32(Math.Ceiling(input));
            data.Logger.LogHeader();
            data.Logger.Log($"Rounded {input.AsString()} to {rounded}", LogColors.YellowGreen);
            return rounded;
        }

        [Block("Rounds the value down to the nearest integer")]
        public static int Floor(BotData data, [Variable] float input)
        {
            var rounded = Convert.ToInt32(Math.Floor(input));
            data.Logger.LogHeader();
            data.Logger.Log($"Rounded {input.AsString()} to {rounded}", LogColors.YellowGreen);
            return rounded;
        }

        [Block("Rounds the value to the nearest integer")]
        public static int RoundToInteger(BotData data, [Variable] float input)
        {
            var rounded = Convert.ToInt32(Round(data, input, 0));
            data.Logger.LogHeader();
            data.Logger.Log($"Rounded {input.AsString()} to {rounded}", LogColors.YellowGreen);
            return rounded;
        }

        [Block("Rounds the value to the given decimal places")]
        public static float Round(BotData data, [Variable] float input, int decimalPlaces = 2)
        {
            var rounded = Convert.ToSingle(Math.Round(input, decimalPlaces, MidpointRounding.AwayFromZero));
            data.Logger.LogHeader();
            data.Logger.Log($"Rounded {input.AsString()} to {rounded.AsString()}", LogColors.YellowGreen);
            return rounded;
        }

        [Block("Computes the value of a given mathematical expression")]
        public static float Compute(BotData data, [Variable] string input)
        {
            var result = Convert.ToSingle(new DataTable().Compute(input.Replace(',', '.'), null));
            data.Logger.LogHeader();
            data.Logger.Log($"Computed {input} with result {result.AsString()}", LogColors.YellowGreen);
            return result;
        }

        [Block("Generates a random float between two values (inclusive)")]
        public static float RandomFloat(BotData data, float minimum = 0, float maximum = 1)
        {
            var random = Convert.ToSingle(data.Random.NextDouble()) * (maximum - minimum) + minimum;
            data.Logger.LogHeader();
            data.Logger.Log($"Generated random value {random.AsString()} in the interval ({minimum.AsString()},{maximum.AsString()})", LogColors.YellowGreen);
            return random;
        }

        [Block("Takes the maximum between two floats", name = "Maximum Float")]
        public static float TakeMaxFloat(BotData data, float first, float second)
        {
            var max = Math.Max(first, second);
            data.Logger.LogHeader();
            data.Logger.Log($"The maximum between {first} and {second} is {max}", LogColors.YellowGreen);
            return max;
        }

        [Block("Takes the minimum between two floats", name = "Minimum Float")]
        public static float TakeMinFloat(BotData data, float first, float second)
        {
            var min = Math.Min(first, second);
            data.Logger.LogHeader();
            data.Logger.Log($"The minimum between {first} and {second} is {min}", LogColors.YellowGreen);
            return min;
        }
    }
}
