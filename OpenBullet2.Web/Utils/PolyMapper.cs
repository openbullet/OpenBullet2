using AutoMapper;
using OpenBullet2.Web.Dtos;
using OpenBullet2.Web.Exceptions;
using System.Text.Json;

namespace OpenBullet2.Web.Utils;

static internal class PolyMapper
{
    static internal object? MapFrom<T>(
        T item, IRuntimeMapper mapper)
    {
        if (item is null)
        {
            return null;
        }

        var type = item.GetType();
        var mappedType = PolyDtoCache.GetMapping(type);
        var mapped = (PolyDto)mapper.Map(item, type, mappedType);

        mapped.PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType(
            mapped.GetType()) ?? string.Empty;

        return mapped;
    }

    static internal List<object> MapAllFrom<T>(
        IEnumerable<T> list, IRuntimeMapper mapper) where T : notnull
    {
        var mappedList = new List<object>();

        foreach (var item in list)
        {
            var mapped = MapFrom(item, mapper);

            if (mapped is not null)
            {
                mappedList.Add(mapped);
            }
        }

        return mappedList;
    }

    static internal TDest? MapBetween<TSource, TDest>(
        JsonElement jsonElement,
        IRuntimeMapper mapper) where TSource : PolyDto
    {
        var item = ConvertPolyDto<TSource>(jsonElement);

        if (item is null)
        {
            return default;
        }

        var type = item.GetType();
        var targetType = PolyDtoCache.GetMapping(type);
        return (TDest)mapper.Map(item, type, targetType);
    }

    static internal List<TDest> MapBetween<TSource, TDest>(
        IEnumerable<JsonElement> jsonElements,
        IRuntimeMapper mapper) where TSource : PolyDto
    {
        var mappedList = new List<TDest>();

        foreach (var jsonElement in jsonElements)
        {
            var mapped = MapBetween<TSource, TDest>(jsonElement, mapper);

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
