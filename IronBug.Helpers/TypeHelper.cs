using System;
using System.Linq;
using System.Reflection;

namespace IronBug.Helpers
{
    public static class TypeHelper
    {
        public static PropertyInfo[] GetClassProperties(this Type type)
        {
            return type.GetRuntimeProperties().Where(p => p.PropertyType.IsClassValueType()).ToArray();
        }

        public static bool IsClassValueType(this Type type)
        {
            var info = type;
            return type != typeof(string) && (info.IsClass || info.IsAbstract || info.IsInterface);
        }

        public static bool IsAssignableTo<T>(this Type type)
        {
            var toInfo = typeof(T);
            var fromInfo = type;
            return toInfo.IsAssignableFrom(fromInfo);
        }
    }
}