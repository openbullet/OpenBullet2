using RuriLib.Attributes;
using RuriLib.Functions.Conversion;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RuriLib.Blocks.Functions.Constants
{
    [BlockCategory("Constants", "Blocks that allow to assign constant values to variables", "#9acd32")]
    public static class Methods
    {
        [Block("Creates a constant string")]
        public static string ConstantString(BotData data, [MultiLine] string value)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Set constant value {value}", LogColors.YellowGreen);
            return value;
        }

        [Block("Creates a constant integer")]
        public static int ConstantInteger(BotData data, int value)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Set constant value {value}", LogColors.YellowGreen);
            return value;
        }

        [Block("Creates a constant float")]
        public static float ConstantFloat(BotData data, float value)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Set constant value {value.ToString(CultureInfo.InvariantCulture)}", LogColors.YellowGreen);
            return value;
        }

        [Block("Creates a constant bool")]
        public static bool ConstantBool(BotData data, bool value)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Set constant value {value}", LogColors.YellowGreen);
            return value;
        }

        [Block("Creates a constant byte array")]
        public static byte[] ConstantByteArray(BotData data, byte[] value)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Set constant value {HexConverter.ToHexString(value)}", LogColors.YellowGreen);
            return value;
        }

        [Block("Creates a constant list")]
        public static List<string> ConstantList(BotData data, List<string> value)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Set constant value {string.Join(", ", value)}", LogColors.YellowGreen);
            return value.Select(i => i).ToList(); // Clone the list
        }

        [Block("Creates a constant dictionary")]
        public static Dictionary<string, string> ConstantDictionary(BotData data, Dictionary<string, string> value)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Set constant value {string.Join(", ", value.Select(kvp => $"({kvp.Key}, {kvp.Value})"))}", LogColors.YellowGreen);
            return value.Select(i => i).ToDictionary(i => i.Key, i => i.Value);
        }
    }
}
