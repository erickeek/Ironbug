using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IronBug.Helpers
{
    public static class AttributeHelper
    {
        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, out T attribute, bool inherit = true) where T : Attribute
        {
            attribute = provider.Attribute<T>(inherit);
            return attribute != null;
        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
        {
            return provider.Attributes<T>(inherit).Any();
        }

        public static IEnumerable<T> Attributes<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        public static T Attribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
        {
            return provider.Attributes<T>(inherit).SingleOrDefault();
        }
    }
}