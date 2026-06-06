using Mapster;
using OpenBullet2.Web.Dtos;
using OpenBullet2.Web.Exceptions;
using System.Text.Json;

namespace OpenBullet2.Web.Utils;

static internal class PolyMapper
{
    static internal object? MapFrom<T>(
        T item, TypeAdapterConfig config)
    {
        if (item is null)
        {
            return null;
        }

        var type = item.GetType();
        var mappedType = PolyDtoCache.GetMapping(type)
            ?? throw new MappingException($"No mapped type found for {type.FullName}");
        var adapted = TypeAdapter.Adapt(item, type, mappedType, config);

        if (adapted is not PolyDto mapped)
        {
            throw new MappingException($"Mapped type {mappedType.FullName} is not a PolyDto");
        }

        mapped.PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType(
            mapped.GetType()) ?? string.Empty;

        return mapped;
    }

    static internal List<object> MapAllFrom<T>(
        IEnumerable<T> list, TypeAdapterConfig config) where T : notnull
    {
        var mappedList = new List<object>();

        foreach (var item in list)
        {
            var mapped = MapFrom(item, config);

            if (mapped is not null)
            {
                mappedList.Add(mapped);
            }
        }

        return mappedList;
    }

    static internal TDest? MapBetween<TSource, TDest>(
        JsonElement jsonElement,
        TypeAdapterConfig config) where TSource : PolyDto
    {
        var item = ConvertPolyDto<TSource>(jsonElement);

        if (item is null)
        {
            return default;
        }

        var type = item.GetType();
        var targetType = PolyDtoCache.GetMapping(type)
            ?? throw new MappingException($"No target mapping found for {type.FullName}");
        var adapted = TypeAdapter.Adapt(item, type, targetType, config);
        return adapted is null ? default : (TDest)adapted;
    }

    static internal List<TDest> MapBetween<TSource, TDest>(
        IEnumerable<JsonElement> jsonElements,
        TypeAdapterConfig config) where TSource : PolyDto
    {
        var mappedList = new List<TDest>();

        foreach (var jsonElement in jsonElements)
        {
            TDest? mapped = MapBetween<TSource, TDest>(jsonElement, config);

            if (mapped is not null)
            {
                mappedList.Add(mapped);
            }
        }

        return mappedList;
    }

    static internal T? ConvertPolyDto<T>(
        JsonElement jsonElement) where T : PolyDto
    {
        if (jsonElement.ValueKind is JsonValueKind.Null)
        {
            return null;
        }

        var subTypes = PolyDtoCache.GetSubTypes<T>();

        if (subTypes.Length == 0)
        {
            throw new MappingException($"No subtypes found for type {typeof(T).FullName}");
        }

        var polyTypeName = jsonElement
            .GetProperty("_polyTypeName").GetString();

        if (polyTypeName is null)
        {
            throw new MappingException("The json document has no _polyTypeName field");
        }

        var subType = PolyDtoCache.GetPolyTypeFromName(polyTypeName);

        if (subType is null)
        {
            var validTypeNames = PolyDtoCache.GetValidPolyTypeNames<T>();
            throw new MappingException(
                $"Invalid _polyTypeName: {polyTypeName}. Valid values: {string.Join(", ", validTypeNames)}");
        }

        return (T?)jsonElement.Deserialize(subType, Globals.JsonOptions);
    }
}
