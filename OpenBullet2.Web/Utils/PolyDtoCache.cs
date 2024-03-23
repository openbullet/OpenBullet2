using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos;
using System.Reflection;

namespace OpenBullet2.Web.Utils;

/// <summary>
/// Caches mappings that involve polymorphic types using reflection.
/// </summary>
static internal class PolyDtoCache
{
    private static readonly Dictionary<Type, Type[]> _subTypes = new();
    private static readonly Dictionary<Type, string> _polyTypeNames = new();
    private static readonly Dictionary<string, Type> _polyTypes = new();
    private static readonly Dictionary<Type, Type> _mappings = new();

    static internal void Scan()
    {
        var assembly = Assembly.GetAssembly(typeof(PolyDtoCache));

        if (assembly is null)
        {
            return;
        }

        var polyDtoTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(PolyDto)));

        // For each poly type, get the subtypes and map their discrims
        foreach (var polyDtoType in polyDtoTypes)
        {
            var subTypes = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(polyDtoType))
                .ToArray();

            if (subTypes.Any())
            {
                _subTypes[polyDtoType] = subTypes;

                foreach (var subType in subTypes)
                {
                    var polyType = PolyTypeAttribute.FromType(subType)?.PolyType;

                    if (polyType is null)
                    {
                        continue;
                    }

                    _polyTypeNames[subType] = polyType;
                    _polyTypes[polyType] = subType;
                }
            }
        }

        // Scan for type mappings
        // Note that the relationships should be bijective (1:1)
        foreach (var type in assembly.GetTypes())
        {
            var mapsFromAttributes = type.GetCustomAttributes<MapsFromAttribute>();

            foreach (var mapsFrom in mapsFromAttributes)
            {
                _mappings[mapsFrom.SourceType] = type;
            }

            var mapsToAttributes = type.GetCustomAttributes<MapsToAttribute>();

            foreach (var mapsTo in mapsToAttributes)
            {
                _mappings[type] = mapsTo.DestinationType;
            }
        }
    }

    static internal Type[] GetSubTypes<T>() where T : PolyDto
    {
        var polyDtoType = typeof(T);
        return _subTypes.TryGetValue(polyDtoType, out var subType)
            ? subType
            : Array.Empty<Type>();
    }

    static internal string?[] GetValidPolyTypeNames<T>() where T : PolyDto
        => GetSubTypes<T>().Select(GetPolyTypeNameFromType).ToArray();

    static internal string? GetPolyTypeNameFromType(Type subType)
        => _polyTypeNames.TryGetValue(subType, out var polyType)
            ? polyType
            : null;

    static internal string? GetPolyTypeNameFromType<T>()
        => GetPolyTypeNameFromType(typeof(T));

    static internal Type? GetPolyTypeFromName(string polyTypeName)
        => _polyTypes.TryGetValue(polyTypeName, out var type)
            ? type
            : null;

    static internal Type? GetMapping(Type type)
        => _mappings.TryGetValue(type, out var mappedType)
            ? mappedType
            : null;
}
