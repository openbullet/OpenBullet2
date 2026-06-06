using Mapster;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Models.Pagination;
using System.Collections;
using System.Reflection;

namespace OpenBullet2.Web.Utils;

internal sealed class MapsterObjectMapper(TypeAdapterConfig config) : IObjectMapper
{
    private static readonly Type PagedListType = typeof(PagedList<>);
    private readonly TypeAdapterConfig _config = config;

    public TDestination Map<TDestination>(object source)
    {
        if (source is null)
        {
            return default!;
        }

        if (TryMapPagedList(source, typeof(TDestination), out var mapped))
        {
            return (TDestination)mapped!;
        }

        return source.Adapt<TDestination>(_config);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        if (source is null)
        {
            return destination;
        }

        source.Adapt(destination, _config);
        return destination;
    }

    public object? Map(object source, Type sourceType, Type destinationType)
    {
        if (source is null)
        {
            return null;
        }

        if (TryMapPagedList(source, destinationType, out var mapped))
        {
            return mapped;
        }

        return TypeAdapter.Adapt(source, sourceType, destinationType, _config);
    }

    private bool TryMapPagedList(object source, Type destinationType, out object? mapped)
    {
        mapped = null;

        var sourceType = source.GetType();

        if (!sourceType.IsGenericType || !destinationType.IsGenericType
            || sourceType.GetGenericTypeDefinition() != PagedListType
            || destinationType.GetGenericTypeDefinition() != PagedListType)
        {
            return false;
        }

        var sourceItemType = sourceType.GetGenericArguments()[0];
        var destinationItemType = destinationType.GetGenericArguments()[0];

        var sourceItems = (IEnumerable?)sourceType.GetProperty(nameof(PagedList<object>.Items))?.GetValue(source);

        if (sourceItems is null)
        {
            mapped = Activator.CreateInstance(destinationType);
            return true;
        }

        var destinationListType = typeof(List<>).MakeGenericType(destinationItemType);
        var destinationItems = (IList)Activator.CreateInstance(destinationListType)!;

        foreach (var item in sourceItems)
        {
            destinationItems.Add(item is null
                ? null!
                : Map(item, sourceItemType, destinationItemType)!);
        }

        var ctor = destinationType.GetConstructor(
            [typeof(IEnumerable<>).MakeGenericType(destinationItemType), typeof(int), typeof(int), typeof(int)]);

        if (ctor is null)
        {
            throw new InvalidOperationException(
                $"No compatible constructor found for {destinationType.FullName}");
        }

        mapped = ctor.Invoke(
        [
            destinationItems,
            sourceType.GetProperty(nameof(PagedList<object>.TotalCount))!.GetValue(source)!,
            sourceType.GetProperty(nameof(PagedList<object>.PageNumber))!.GetValue(source)!,
            sourceType.GetProperty(nameof(PagedList<object>.PageSize))!.GetValue(source)!
        ]);

        return true;
    }
}
