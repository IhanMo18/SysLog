namespace SysLog.Service.Mappers;

public static class MapperTo
{
public static TTarget Map<TSource, TTarget>(TSource source)
    where TTarget : new()
    {
        var target = new TTarget();

        if (source is null)
        {
            return target;
        }

        foreach (var prop in typeof(TSource).GetProperties())
        {
            var targetProp = typeof(TTarget).GetProperty(prop.Name);
            if (targetProp != null && targetProp.CanWrite)
            {
                targetProp.SetValue(target, prop.GetValue(source));
            }
        }
        return target;
    }
}