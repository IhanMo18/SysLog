using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SysLog.Service.Mappers;

public static class MapperTo
{
    public static TTarget Map<TSource, TTarget>(TSource source)
        where TTarget : new()
    {
        var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
        return (TTarget)Map(typeof(TSource), source!, typeof(TTarget), visited)!;
    }

    private static object? Map(Type sourceType, object? source, Type targetType, IDictionary<object, object> visited)
    {
        if (source == null)
            return null;

        if (visited.TryGetValue(source, out var existing))
            return existing;

        var target = Activator.CreateInstance(targetType)!;
        visited[source] = target;

        foreach (var prop in sourceType.GetProperties())
        {
            var targetProp = targetType.GetProperty(prop.Name);
            if (targetProp == null || !targetProp.CanWrite)
                continue;

            var value = prop.GetValue(source);

            if (value != null && !targetProp.PropertyType.IsAssignableFrom(prop.PropertyType))
            {
                value = Map(prop.PropertyType, value, targetProp.PropertyType, visited);
            }

            targetProp.SetValue(target, value);
        }

        return target;
    }
}

internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public static ReferenceEqualityComparer Instance { get; } = new();

    private ReferenceEqualityComparer() { }

    public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

    public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
}