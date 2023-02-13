using AutoMapper;
using OpenBullet2.Web.Dtos;
using OpenBullet2.Web.Dtos.JobMonitor;
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

    static internal List<object> MapFrom<T>(
        IEnumerable<T> list, IRuntimeMapper mapper) where T : notnull
    {
        var mappedList = new List<object>();

        foreach (var item in list)
        {
            var type = item.GetType();
            var mappedType = PolyDtoCache.GetMapping(type);
            var mapped = (PolyDto)mapper.Map(item, type, mappedType);

            mapped.PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType(
                mapped.GetType()) ?? string.Empty;

            mappedList.Add(mapped);
        }

        return mappedList;
    }

    static internal List<TDest> MapBetween<TSource, TDest>(
        IEnumerable<JsonDocument> jsonDocuments,
        IRuntimeMapper mapper) where TSource : PolyDto
    {
        var mappedList = new List<TDest>();
        var list = ConvertPolyDtoList<TSource>(jsonDocuments);

        foreach (var item in list)
        {
            var type = item.GetType();
            var targetType = PolyDtoCache.GetMapping(type);
            var mapped = (TDest)mapper.Map(item, type, targetType);

            mappedList.Add(mapped);
        }

        return mappedList;
    }

    private static List<T> ConvertPolyDtoList<T>(
        IEnumerable<JsonDocument>? list) where T : PolyDto
    {
        if (list is null)
        {
            return new List<T>();
        }

        var subTypes = PolyDtoCache.GetSubTypes<T>();

        if (subTypes.Length == 0)
        {
            throw new Exception($"No subtypes found for type {typeof(T).FullName}");
        }

        var items = new List<T>();

        foreach (var jsonDocument in list)
        {
            var item = ConvertPolyDto<T>(jsonDocument);

            if (item is not null)
            {
                items.Add(item);
            }
        }

        return items;
    }

    private static T? ConvertPolyDto<T>(
        JsonDocument? jsonDocument) where T : PolyDto
    {
        if (jsonDocument is null)
        {
            return null;
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
