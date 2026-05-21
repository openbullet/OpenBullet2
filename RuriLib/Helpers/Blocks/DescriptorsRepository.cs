using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RuriLib.Extensions;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Bots;
using RuriLib.Models.Trees;
using RuriLib.Models.Variables;

namespace RuriLib.Helpers.Blocks;

/// <summary>
/// Repository of all block descriptors that are available to create new blocks.
/// </summary>
public class DescriptorsRepository
{
    private static readonly Dictionary<Type, VariableType?> _variableTypes = new()
    {
        [typeof(void)] = null,
        [typeof(string)] = VariableType.String,
        [typeof(int)] = VariableType.Int,
        [typeof(float)] = VariableType.Float,
        [typeof(bool)] = VariableType.Bool,
        [typeof(List<string>)] = VariableType.ListOfStrings,
        [typeof(Dictionary<string, string>)] = VariableType.DictionaryOfStrings,
        [typeof(byte[])] = VariableType.ByteArray,
        [typeof(Task)] = null,
        [typeof(Task<string>)] = VariableType.String,
        [typeof(Task<int>)] = VariableType.Int,
        [typeof(Task<float>)] = VariableType.Float,
        [typeof(Task<bool>)] = VariableType.Bool,
        [typeof(Task<List<string>>)] = VariableType.ListOfStrings,
        [typeof(Task<Dictionary<string, string>>)] = VariableType.DictionaryOfStrings,
        [typeof(Task<byte[]>)] = VariableType.ByteArray
    };

    private readonly Dictionary<string, BlockCategory> categoryDefinitions = [];
    private readonly Dictionary<string, string> aliasMappings = [];

    /// <summary>
    /// The descriptors keyed by their unique id.
    /// </summary>
    public Dictionary<string, BlockDescriptor> Descriptors { get; set; } = [];

    /// <summary>
    /// The canonical block id keyed by alias.
    /// </summary>
    public IReadOnlyDictionary<string, string> Aliases => aliasMappings;

    /// <summary>
    /// Initializes a <see cref="DescriptorsRepository"/> and imports blocks from the executing assembly.
    /// </summary>
    public DescriptorsRepository()
    {
        Recreate();
    }

    /// <summary>
    /// Recreates the repository with only the descriptors in the executing assembly (no plugins).
    /// </summary>
    public void Recreate()
    {
        Descriptors.Clear();
        categoryDefinitions.Clear();
        aliasMappings.Clear();

        Descriptors["Keycheck"] = new KeycheckBlockDescriptor();
        Descriptors["HttpRequest"] = new HttpRequestBlockDescriptor();
        Descriptors["Parse"] = new ParseBlockDescriptor();
        Descriptors["Script"] = new ScriptBlockDescriptor();

        AddFromExposedMethods(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Gets a <see cref="BlockDescriptor"/> by its unique <paramref name="id"/>
    /// and automatically casts it to the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected descriptor type.</typeparam>
    /// <param name="id">The descriptor id.</param>
    /// <returns>The typed descriptor.</returns>
    public T GetAs<T>(string id) where T : BlockDescriptor
    {
        if (!Descriptors.TryGetValue(id, out var descriptor))
        {
            throw new Exception($"No descriptor was found with the given id: {id}");
        }

        if (descriptor is T typedDescriptor)
        {
            return typedDescriptor;
        }

        throw new InvalidCastException($"Descriptor {id} cannot be cast to {typeof(T).Name}");
    }

    /// <summary>
    /// Attempts to resolve a block id to its canonical id.
    /// </summary>
    public bool TryResolveDescriptorId(string id, out string canonicalId)
    {
        if (Descriptors.ContainsKey(id))
        {
            canonicalId = id;
            return true;
        }

        return aliasMappings.TryGetValue(id, out canonicalId!);
    }

    /// <summary>
    /// Adds descriptors to the repository by finding exposed methods in the given
    /// <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    public void AddFromExposedMethods(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            var category = type.GetCustomAttribute<Attributes.BlockCategory>();
            if (category is null)
            {
                continue;
            }

            var typeNamespace = type.Namespace ?? throw new InvalidOperationException(
                $"Type {type.FullName} has no namespace");

            RegisterCategoryDefinition(type, typeNamespace, category);

            foreach (var method in type.GetMethods())
            {
                var attribute = method.GetCustomAttribute<Attributes.Block>();
                if (attribute is null)
                {
                    continue;
                }

                var blockId = attribute.id ?? method.Name;

                if (Descriptors.ContainsKey(blockId))
                {
                    throw new Exception($"Duplicate descriptor id: {blockId}");
                }

                var aliases = GetAliases(attribute, blockId);

                Descriptors[blockId] = new AutoBlockDescriptor
                {
                    Id = blockId,
                    MethodName = method.Name,
                    Async = method.CustomAttributes.Any(a => a.AttributeType == typeof(AsyncStateMachineAttribute))
                        || IsAsyncReturnType(method.ReturnType),
                    Name = attribute.name ?? GetReadableMethodName(method.Name),
                    Aliases = aliases,
                    Description = attribute.description ?? string.Empty,
                    ExtraInfo = attribute.extraInfo ?? string.Empty,
                    AssemblyFullName = assembly.FullName ?? assembly.GetName().Name ?? string.Empty,
                    Parameters = method.GetParameters()
                        .Where(p => p.ParameterType != typeof(BotData))
                        .Select(BuildBlockParameter)
                        .ToDictionary(p => p.Name, p => p),
                    ReturnType = ToVariableType(method.ReturnType),
                    Category = CreateBlockCategory(type, typeNamespace, category),
                    Images = method.GetCustomAttributes<Attributes.BlockImage>()
                        .ToDictionary(a => a.id, a => new BlockImageInfo
                        {
                            Name = a.id.ToReadableName(),
                            MaxWidth = a.maxWidth,
                            MaxHeight = a.maxHeight
                        })
                };

                foreach (var alias in aliases)
                {
                    RegisterAlias(alias, blockId);
                }
            }
        }

        AddBlockActions(assembly);
    }

    private void RegisterCategoryDefinition(
        Type type,
        string typeNamespace,
        Attributes.BlockCategory category)
        => categoryDefinitions[typeNamespace] = CreateBlockCategory(type, typeNamespace, category);

    private List<string> GetAliases(Attributes.Block attribute, string blockId)
    {
        var aliases = new List<string>();

        foreach (var alias in attribute.aliases)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new Exception($"Descriptor {blockId} has an empty alias");
            }

            if (alias == blockId || aliases.Contains(alias))
            {
                continue;
            }

            aliases.Add(alias);
        }

        return aliases;
    }

    private void RegisterAlias(string alias, string blockId)
    {
        if (Descriptors.ContainsKey(alias))
        {
            throw new Exception($"Alias {alias} conflicts with an existing descriptor id");
        }

        if (aliasMappings.TryGetValue(alias, out var existingBlockId))
        {
            throw new Exception($"Alias {alias} is already assigned to descriptor {existingBlockId}");
        }

        aliasMappings[alias] = blockId;
    }

    private void AddBlockActions(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods())
            {
                var attribute = method.GetCustomAttribute<Attributes.BlockAction>();
                if (attribute is null)
                {
                    continue;
                }

                if (!Descriptors.TryGetValue(attribute.parentBlockId, out var descriptor))
                {
                    throw new Exception($"Invalid descriptor id: {attribute.parentBlockId}");
                }

                descriptor.Actions.Add(new BlockActionInfo
                {
                    Name = attribute.name ?? GetReadableMethodName(method.Name),
                    Description = attribute.description ?? string.Empty,
                    Delegate = method.CreateDelegate<BlockActionDelegate>()
                });
            }
        }
    }

    private static BlockCategory CreateBlockCategory(
        Type type,
        string typeNamespace,
        Attributes.BlockCategory category)
    {
        var splitNamespace = typeNamespace.Split('.');

        return new BlockCategory
        {
            Name = category.name ?? splitNamespace[^1],
            Path = typeNamespace,
            Namespace = $"{typeNamespace}.{type.Name}",
            Description = category.description,
            ForegroundColor = category.foregroundColor,
            BackgroundColor = category.backgroundColor
        };
    }

    private static string GetReadableMethodName(string methodName)
        => methodName.EndsWith("Async", StringComparison.Ordinal)
            ? methodName[..^"Async".Length].ToReadableName()
            : methodName.ToReadableName();

    private static bool IsAsyncReturnType(Type returnType)
        => returnType == typeof(Task)
           || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
           || returnType == typeof(ValueTask)
           || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>));

    private static BlockParameter BuildBlockParameter(ParameterInfo info)
    {
        var parameter = ToBlockParameter(info);
        var variableParam = info.GetCustomAttribute<Attributes.Variable>();
        var interpParam = info.GetCustomAttribute<Attributes.Interpolated>();

        if (variableParam is not null)
        {
            parameter.InputMode = SettingInputMode.Variable;
            parameter.DefaultVariableName = variableParam.defaultVariableName ?? string.Empty;
        }
        else if (interpParam is not null)
        {
            parameter.InputMode = SettingInputMode.Interpolated;
        }

        return parameter;
    }

    /// <summary>
    /// Converts the return <paramref name="type"/> of a method to a <see cref="VariableType"/>.
    /// Returns null if the method returns void or Task.
    /// </summary>
    /// <param name="type">The CLR return type.</param>
    /// <returns>The corresponding variable type, or <see langword="null"/> when there is no return value.</returns>
    public static VariableType? ToVariableType(Type type)
    {
        if (_variableTypes.TryGetValue(type, out var value))
        {
            return value;
        }

        throw new InvalidCastException($"The type {type} could not be casted to VariableType");
    }

    /// <summary>
    /// Casts a C# variable with a given <paramref name="name"/>, <paramref name="type"/>
    /// and <paramref name="value"/> to a custom <see cref="Variable"/> object.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="type">The CLR type.</param>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The wrapped variable.</returns>
    public static Variable ToVariable(string name, Type type, dynamic value)
    {
        var t = ToVariableType(type);

        if (!t.HasValue)
        {
            throw new InvalidCastException($"Cannot cast type {type} to a variable");
        }

        Variable variable = t switch
        {
            VariableType.String => new StringVariable(value),
            VariableType.Bool => new BoolVariable(value),
            VariableType.ByteArray => new ByteArrayVariable(value),
            VariableType.DictionaryOfStrings => new DictionaryOfStringsVariable(value),
            VariableType.Float => new FloatVariable(value),
            VariableType.Int => new IntVariable(value),
            VariableType.ListOfStrings => new ListOfStringsVariable(value),
            _ => throw new NotImplementedException()
        };

        variable.Name = name;
        variable.Type = t.Value;
        return variable;
    }

    private static BlockParameter ToBlockParameter(ParameterInfo parameter)
    {
        var creators = new Dictionary<Type, Func<BlockParameter>>
        {
            {
                typeof(string), () => new StringParameter(parameter.Name!)
                {
                    DefaultValue = parameter.HasDefaultValue ? (string)parameter.DefaultValue! : string.Empty,
                    MultiLine = parameter.GetCustomAttribute<Attributes.MultiLine>() is not null
                }
            },
            {
                typeof(int), () => new IntParameter(parameter.Name!)
                {
                    DefaultValue = parameter.HasDefaultValue ? (int)parameter.DefaultValue! : 0
                }
            },
            {
                typeof(float), () => new FloatParameter(parameter.Name!)
                {
                    DefaultValue = parameter.HasDefaultValue ? (float)parameter.DefaultValue! : 0.0f
                }
            },
            {
                typeof(bool), () => new BoolParameter(parameter.Name!)
                {
                    DefaultValue = parameter.HasDefaultValue && (bool)parameter.DefaultValue!
                }
            },
            {
                typeof(List<string>), () => new ListOfStringsParameter(parameter.Name!)
            },
            {
                typeof(Dictionary<string, string>), () => new DictionaryOfStringsParameter(parameter.Name!)
            },
            {
                typeof(byte[]), () => new ByteArrayParameter(parameter.Name!)
            }
        };

        var blockParamAttribute = parameter.GetCustomAttribute<Attributes.BlockParam>();

        if (creators.TryGetValue(parameter.ParameterType, out var creator))
        {
            var blockParam = creator();

            if (blockParamAttribute is not null)
            {
                blockParam.AssignedName = blockParamAttribute.name;
                blockParam.Description = blockParamAttribute.description;
            }

            blockParam.Name = parameter.Name!;
            return blockParam;
        }

        if (parameter.ParameterType.IsEnum)
        {
            var blockParam = new EnumParameter(
                parameter.Name!,
                parameter.ParameterType,
                parameter.HasDefaultValue
                    ? parameter.DefaultValue!.ToString()!
                    : Enum.GetNames(parameter.ParameterType).First());

            if (blockParamAttribute is not null)
            {
                blockParam.AssignedName = blockParamAttribute.name;
                blockParam.Description = blockParamAttribute.description;
            }

            return blockParam;
        }

        throw new ArgumentException(
            $"Parameter {parameter.Name} has an invalid type ({parameter.ParameterType})");
    }

    /// <summary>
    /// Retrieves the category tree of all categories and block descriptors.
    /// </summary>
    /// <returns>The root node of the category tree.</returns>
    public CategoryTreeNode AsTree()
    {
        var root = new CategoryTreeNode
        {
            Name = "Root",
            Descriptors = [.. Descriptors.Values]
        };

        PushLeaves(root, 0);
        ResolveCategories(root, string.Empty);

        return root;
    }

    private void ResolveCategories(CategoryTreeNode node, string path)
    {
        foreach (var subCategory in node.SubCategories)
        {
            ResolveCategories(subCategory, JoinCategoryPath(path, subCategory.Name));
        }

        node.ResolvedCategory = ResolveCategory(node, path);
    }

    private BlockCategory ResolveCategory(CategoryTreeNode node, string path)
    {
        if (node.IsRoot)
        {
            return new BlockCategory
            {
                Name = node.Name,
                Description = "All block categories"
            };
        }

        if (categoryDefinitions.TryGetValue(path, out var category))
        {
            category.Name = node.Name;
            category.Path = path;
            return category;
        }

        if (node.Descriptors.Count > 0)
        {
            category = node.Descriptors.First().Category;
            category.Name = node.Name;
            category.Path = path;
            return category;
        }

        if (node.SubCategories.Count > 0)
        {
            category = node.SubCategories.First().Category;
            category.Name = node.Name;
            category.Path = path;
            category.Namespace = path;
            category.Description = $"Blocks in the {node.Name} category";
            return category;
        }

        return new BlockCategory
        {
            Name = node.Name,
            Path = path,
            Namespace = path,
            Description = $"Blocks in the {node.Name} category"
        };
    }

    private static string JoinCategoryPath(string parentPath, string categoryName)
        => string.IsNullOrEmpty(parentPath)
            ? categoryName
            : $"{parentPath}.{categoryName}";

    private void PushLeaves(CategoryTreeNode node, int level)
    {
        for (var i = 0; i < node.Descriptors.Count; i++)
        {
            var descriptor = node.Descriptors[i];
            var split = descriptor.Category.Path.Split('.');

            if (split.Length <= level)
            {
                continue;
            }

            var subCategoryName = split[level];
            var subCategoryNode = node.SubCategories.FirstOrDefault(s => s.Name == subCategoryName);

            if (subCategoryNode is null)
            {
                subCategoryNode = new CategoryTreeNode { Parent = node, Name = subCategoryName };
                node.SubCategories.Add(subCategoryNode);
            }

            subCategoryNode.Descriptors.Add(descriptor);
            node.Descriptors.RemoveAt(i);
            i--;
        }

        node.SubCategories = [.. node.SubCategories.OrderBy(s => s.Name)];
        node.Descriptors = [.. node.Descriptors.OrderBy(d => d.Name)];

        foreach (var subCategory in node.SubCategories)
        {
            PushLeaves(subCategory, level + 1);
        }
    }
}
