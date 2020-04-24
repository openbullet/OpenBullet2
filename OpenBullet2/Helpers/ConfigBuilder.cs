using OpenBullet2.Models;
using OpenBullet2.Models.Settings;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers
{
    public static class ConfigBuilder
    {
        /// <summary>
        /// This method checks things like variable declaration, precedence, invalid type casting...
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void CheckVariables(IEnumerable<BlockInstance> stack)
        {
            // TODO: Initialize this list with the default variables like SOURCE etc.
            List<(string, VariableType)> variables = new List<(string, VariableType)>();

            foreach (var block in stack)
            {
                // Check that all settings in variable mode use a variable that has already been
                // declared and that is of the correct type
                foreach (var setting in block.Settings.Settings.Where(s => s.InputMode == Enums.InputMode.Variable))
                {
                    // Throw if not declared
                    if (!variables.Any(v => v.Item1 == setting.InputVariableName))
                        throw new NullReferenceException
                            ($"The variable {setting.InputVariableName} in setting {setting.FixedSetting.Name} in block {block.Settings.Label} has not been previously declared");

                    var variable = variables.First(v => v.Item1 == setting.InputVariableName);

                    // Throw if type not supported (e.g. enum variable)
                    if (!settingTypeToVariableType.ContainsKey(setting.FixedSetting.GetType()))
                        throw new ArgumentException($"The setting {setting.FixedSetting.Name} of type {setting.FixedSetting.GetType()} in block {block.Settings.Label} does not support variables");

                    var requiredType = settingTypeToVariableType[setting.FixedSetting.GetType()];

                    // Throw if type mismatch
                    if (variable.Item2 != requiredType)
                        throw new InvalidCastException($"The setting {setting.FixedSetting.Name} in block {block.Settings.Label} expected a variable of type {requiredType} but got {variable.Item2}");
                }

                // If the method returns a value, add it to the available variables
                if (block.Info.ReturnType.HasValue)
                    variables.Add((block.OutputVariable, block.Info.ReturnType.Value));
            }
        }

        private static readonly Dictionary<Type, VariableType> settingTypeToVariableType = new Dictionary<Type, VariableType>
        {
            { typeof(StringSetting), VariableType.String },
            { typeof(BoolSetting), VariableType.Bool },
            { typeof(IntSetting), VariableType.Int },
            { typeof(FloatSetting), VariableType.Float },
            { typeof(ListOfStringsSetting), VariableType.ListOfStrings },
            { typeof(DictionaryOfStringsSetting), VariableType.DictionaryOfStrings },
            { typeof(ByteArraySetting), VariableType.ByteArray }
        };
    }
}
