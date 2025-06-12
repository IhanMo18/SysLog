namespace SysLog.Service.Mappers;

public static class MapperTo
{
    public static TTarget Map<TSource, TTarget>(TSource source)
        where TTarget : new()
    {
        return (TTarget)Map(typeof(TSource), source, typeof(TTarget))!;
    }

    private static object? Map(Type sourceType, object? source, Type targetType)
    {
        if (source == null)
            return null;

        var target = Activator.CreateInstance(targetType)!;

        foreach (var prop in sourceType.GetProperties())
        {
            var targetProp = targetType.GetProperty(prop.Name);
            if (targetProp == null || !targetProp.CanWrite)
                continue;

            var value = prop.GetValue(source);

            if (value != null && !targetProp.PropertyType.IsAssignableFrom(prop.PropertyType))
            {
                value = Map(prop.PropertyType, value, targetProp.PropertyType);
            }

            targetProp.SetValue(target, value);
        }

        return target;
    }
}