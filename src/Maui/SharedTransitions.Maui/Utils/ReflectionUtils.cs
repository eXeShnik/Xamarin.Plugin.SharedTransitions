using System.Reflection;

namespace Plugin.SharedTransitions.Shared.Utils;

public static class ReflectionUtils
{
    public static T GetPropertyValue<T>(this PropertyInfo propertyInfo, object instance)
    {
        return propertyInfo.GetValue(instance) is T value ? value : default;
    }

    public static T GetFieldValue<T>(this FieldInfo fieldInfo, object instance)
    {
        return fieldInfo.GetValue(instance) is T value ? value : default;
    }
}