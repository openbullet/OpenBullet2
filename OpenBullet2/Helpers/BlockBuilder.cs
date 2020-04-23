using OpenBullet2.Models;
using OpenBullet2.Models.BlockParameters;
using RuriLib.Attributes;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OpenBullet2.Helpers
{
    public static class BlockBuilder
    {
        public static BlockInfo[] FromExposedMethods(Assembly assembly)
        {
            List<BlockInfo> blocks = new List<BlockInfo>();

            // Get all exposed methods
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<Block>();
                    if (attribute == null) continue;

                    blocks.Add(new BlockInfo
                    {
                        // If the name specified in the attribute is null, use the method's name
                        Name = attribute.name ?? ToReadableName(method.Name),
                        Description = attribute.description ?? string.Empty,
                        ExtraInfo = attribute.extraInfo ?? string.Empty,
                        Category = type.Namespace.Split('.')[2],
                        Parameters = method.GetParameters().Select(p => ToBlockParameter(p)).ToArray()
                    });
                }
            }

            return blocks.ToArray();
        }

        private static string ToReadableName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            var replaced = Regex.Replace(name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
            return char.ToUpper(replaced[0]) + replaced.Substring(1);
        }

        private static BlockParameter ToBlockParameter(ParameterInfo parameter)
        {
            var dict = new Dictionary<Type, Func<BlockParameter>>
            {
                { typeof(string), () => new StringParameter
                    { DefaultValue = parameter.HasDefaultValue ? (string)parameter.DefaultValue : default } },

                { typeof(int), () => new IntParameter
                    { DefaultValue = parameter.HasDefaultValue ? (int)parameter.DefaultValue : default } },

                { typeof(float), () => new FloatParameter
                    { DefaultValue = parameter.HasDefaultValue ? (float)parameter.DefaultValue : default } },

                { typeof(bool), () => new BoolParameter
                    { DefaultValue = parameter.HasDefaultValue ? (bool)parameter.DefaultValue : default } },

                // TODO: Add defaults for these through parameter attributes
                { typeof(List<string>), () => new ListOfStringsParameter() },
                { typeof(Dictionary<string, string>), () => new DictionaryOfStringsParameter() },
                { typeof(byte[]), () => new ByteArrayParameter() }
            };

            // If it's one of the standard types
            if (dict.ContainsKey(parameter.ParameterType))
            {
                var blockParam = dict[parameter.ParameterType].Invoke();
                blockParam.Name = ToReadableName(parameter.Name);
                return blockParam;
            }

            // If it's an enum type
            if (parameter.ParameterType.IsEnum)
            {
                return new EnumParameter
                {
                    Name = ToReadableName(parameter.Name),
                    EnumType = parameter.ParameterType,
                    DefaultValue = parameter.HasDefaultValue 
                        ? parameter.DefaultValue.ToString() 
                        : Enum.GetNames(parameter.ParameterType).First()
                };
            }

            throw new ArgumentException($"Parameter {parameter.Name} has an invalid type ({parameter.ParameterType})");
        }

        public static string GetColor(string category)
        {
            var dict = new Dictionary<string, string>
            {
                { "Parsing", "#f4ff9e" },
                { "Conversion", "#e2ff8c" }
            };

            if (dict.ContainsKey(category))
                return dict[category];

            return "#fff";
        }
    }
}
