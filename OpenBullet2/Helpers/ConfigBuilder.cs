using OpenBullet2.Models.Configs;
using OpenBullet2.Models.Settings;
using RuriLib.Models.Variables;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

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
        public static void CheckVariables(Config config)
        {
            // TODO: Initialize this list with the default variables like SOURCE etc.
            List<(string, VariableType)> variables = new List<(string, VariableType)>();

            // Add custom inputs
            config.Settings.InputSettings.CustomInputs
                .ForEach(i => variables.Add((i.VariableName, VariableType.String)));

            foreach (var block in config.Blocks)
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

            // TEMPORARY: Finally check if all the variable names are allowed
            // TODO: Avoid this restriction and automatically transform invalid variable names into valid ones
            var provider = CodeDomProvider.CreateProvider("C#");
            foreach (var variable in variables)
            {
                if (!provider.IsValidIdentifier(variable.Item1))
                {
                    throw new Exception($"The name {variable.Item1} is not a valid variable name in the C# language. Some valid names are myvar, MyVar1, myVar_1. Please modify the invalid name and try again.");
                }
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
