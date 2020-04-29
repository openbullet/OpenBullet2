using OpenBullet2.Models;
using OpenBullet2.Models.BlockParameters;
using RuriLib.Attributes;
using RuriLib.Models.Bots;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers
{
    public static class BlockBuilder
    {
        public static BlockInfo[] FromExposedMethods(Assembly assembly)
        {
            List<BlockInfo> blocks = new List<BlockInfo>();

            // Get all types of the assembly
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                // Check if the type has a BlockCategory attribute
                var category = type.GetCustomAttribute<RuriLib.Attributes.BlockCategory>();
                if (category == null) continue;

                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<Block>();
                    if (attribute == null) continue;

                    blocks.Add(new BlockInfo
                    {
                        MethodName = method.Name,
                        Async = method.CustomAttributes.Any(a => a.AttributeType == typeof(AsyncStateMachineAttribute)),
                        // If the name specified in the attribute is null, use the readable method's name
                        Name = attribute.name ?? ToReadableName(method.Name),
                        Description = attribute.description ?? string.Empty,
                        ExtraInfo = attribute.extraInfo ?? string.Empty,
                        Parameters = method.GetParameters().Where(p => p.ParameterType != typeof(BotData))
                            .Select(p => ToBlockParameter(p)).ToArray(),
                        ReturnType = ToVariableType(method.ReturnType),
                        Category = new Models.BlockCategory
                        {
                            Name = category.name ?? type.Namespace.Split('.')[2],
                            Description = category.description,
                            ForegroundColor = category.foregroundColor,
                            BackgroundColor = category.backgroundColor
                        }
                    });
                }
            }

            return blocks.ToArray();
        }

        /// <summary>
        /// Converts a <paramref name="name"/> from readableName to Readable Name
        /// </summary>
        private static string ToReadableName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            var replaced = Regex.Replace(name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
            return char.ToUpper(replaced[0]) + replaced.Substring(1);
        }

        private static VariableType? ToVariableType(Type type)
        {
            if (type == typeof(void))
                return null;

            var dict = new Dictionary<Type, VariableType>
            {
                { typeof(string), VariableType.String },
                { typeof(int), VariableType.Int },
                { typeof(float), VariableType.Float },
                { typeof(bool), VariableType.Bool },
                { typeof(List<string>), VariableType.ListOfStrings },
                { typeof(Dictionary<string, string>), VariableType.DictionaryOfStrings },
                { typeof(byte[]), VariableType.ByteArray }
            };

            if (dict.ContainsKey(type))
                return dict[type];

            var taskDict = new Dictionary<Type, VariableType>
            {
                { typeof(Task<string>), VariableType.String },
                { typeof(Task<int>), VariableType.Int },
                { typeof(Task<float>), VariableType.Float },
                { typeof(Task<bool>), VariableType.Bool },
                { typeof(Task<List<string>>), VariableType.ListOfStrings },
                { typeof(Task<Dictionary<string, string>>), VariableType.DictionaryOfStrings },
                { typeof(Task<byte[]>), VariableType.ByteArray }
            };

            if (taskDict.ContainsKey(type))
                return taskDict[type];

            throw new InvalidCastException($"The type {type} could not be casted to VariableType");
        }

        private static BlockParameter ToBlockParameter(ParameterInfo parameter)
        {
            var dict = new Dictionary<Type, Func<BlockParameter>>
            {
                { typeof(string), () => new StringParameter
                    { DefaultValue = parameter.HasDefaultValue ? (string)parameter.DefaultValue : "" } },

                { typeof(int), () => new IntParameter
                    { DefaultValue = parameter.HasDefaultValue ? (int)parameter.DefaultValue : 0 } },

                { typeof(float), () => new FloatParameter
                    { DefaultValue = parameter.HasDefaultValue ? (float)parameter.DefaultValue : 0.0f } },

                { typeof(bool), () => new BoolParameter
                    { DefaultValue = parameter.HasDefaultValue ? (bool)parameter.DefaultValue : false } },

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
    }
}
