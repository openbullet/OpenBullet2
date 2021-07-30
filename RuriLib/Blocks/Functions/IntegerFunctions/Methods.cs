using RuriLib.Attributes;
using RuriLib.Functions.Parsing;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Blocks.Functions.Integer
{
    [BlockCategory("Integer Functions", "Blocks for working with integer numbers", "#9acd32")]
    public static class Methods
    {
        [Block("Generates a random integer between two values (inclusive)")]
        public static int RandomInteger(BotData data, int minimum = 0, int maximum = 10)
        {
            var random = data.Random.Next(minimum, maximum + 1);
            data.Logger.LogHeader();
            data.Logger.Log($"Generated random value {random} in the interval ({minimum},{maximum})", LogColors.YellowGreen);
            return random;
        }

        [Block("Takes the maximum between two integers", name = "Maximum Int")]
        public static int TakeMaxInt(BotData data, int first, int second)
        {
            var max = Math.Max(first, second);
            data.Logger.LogHeader();
            data.Logger.Log($"The maximum between {first} and {second} is {max}", LogColors.YellowGreen);
            return max;
        }

        [Block("Takes the minimum between two integers", name = "Minimum int")]
        public static int TakeMinInt(BotData data, int first, int second)
        {
            var min = Math.Min(first, second);
            data.Logger.LogHeader();
            data.Logger.Log($"The minimum between {first} and {second} is {min}", LogColors.YellowGreen);
            return min;
        }
    }
}
