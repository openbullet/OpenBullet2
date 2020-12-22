using RuriLib.Attributes;
using RuriLib.Functions.Parsing;
using RuriLib.Logging;
using RuriLib.Models.Bots;
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
    }
}
