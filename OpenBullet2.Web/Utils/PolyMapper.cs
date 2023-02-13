using AutoMapper;
using OpenBullet2.Web.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenBullet2.Web.Utils;

static internal class PolyMapper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

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
        JsonDocument jsonDocument,
        IRuntimeMapper mapper) where TSource : PolyDto
    {
        var item = ConvertPolyDto<TSource>(jsonDocument);

        if (item is null)
        {
            return default;
        }

        var type = item.GetType();
        var targetType = PolyDtoCache.GetMapping(type);
        return (TDest)mapper.Map(item, type, targetType);
    }

    static internal List<TDest> MapBetween<TSource, TDest>(
        IEnumerable<JsonDocument> jsonDocuments,
        IRuntimeMapper mapper) where TSource : PolyDto
    {
        var mappedList = new List<TDest>();

        foreach (var jsonDocument in jsonDocuments)
        {
            var mapped = MapBetween<TSource, TDest>(jsonDocument, mapper);

            if (mapped is not null)
            {
                mappedList.Add(mapped);
            }
        }

        return mappedList;
    }

    private static T? ConvertPolyDto<T>(
        JsonDocument? jsonDocument) where T : PolyDto
    {
        if (jsonDocument is null)
        {
            return null;
        }

        var subTypes = PolyDtoCache.GetSubTypes<T>();

        if (subTypes.Length == 0)
        {
            throw new Exception($"No subtypes found for type {typeof(T).FullName}");
        }

        var polyTypeName = jsonDocument.RootElement
            .GetProperty("_polyTypeName").GetString();

        if (polyTypeName is null)
        {
            throw new Exception($"The json document has no _polyTypeName field");
        }

        var subType = PolyDtoCache.GetPolyTypeFromName(polyTypeName);

        if (subType is null)
        {
            var validTypeNames = PolyDtoCache.GetValidPolyTypeNames<T>();
            throw new Exception($"Invalid _polyTypeName: {polyTypeName}. Valid values: {string.Join(", ", validTypeNames)}");
        }

        return (T?)jsonDocument.Deserialize(subType, _jsonOptions);
    }
}
