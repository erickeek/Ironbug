using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IronBug.Helpers
{
    public static class EnumHelper
    {
        public static Enum GetValue(this int value, Type enumType)
        {
            return (Enum)Enum.ToObject(enumType, value);
        }

        public static TAttribute Attribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            return type.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }

        public static string DisplayName(this Enum value)
        {
            var attribute = value.Attribute<DisplayAttribute>();
            return attribute != null ? attribute.Name : value.ToString();
        }
    }
}