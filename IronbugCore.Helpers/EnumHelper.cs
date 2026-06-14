using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace IronbugCore.Helpers;

public static class EnumHelper
{
    private static readonly ConcurrentDictionary<(Type EnumType, string Name, Type AttributeType), object?> AttributeCache =
        new ConcurrentDictionary<(Type, string, Type), object?>();

    public static Enum GetValue(this int value, Type enumType)
    {
        return (Enum)Enum.ToObject(enumType, value);
    }

    /// <summary>
    /// Fast path: the attribute table for each (TEnum, TAttribute) pair is built once and
    /// served from a lock-free typed cache, with no boxing or per-call string allocation.
    /// </summary>
    public static TAttribute? Attribute<TEnum, TAttribute>(this TEnum value)
        where TEnum : struct, Enum
        where TAttribute : Attribute
    {
        return EnumAttributeCache<TEnum, TAttribute>.Get(value);
    }

    public static TAttribute? Attribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        if (name == null)
            return null;

        var attribute = AttributeCache.GetOrAdd((type, name, typeof(TAttribute)),
            key => key.EnumType.GetField(key.Name)
                ?.GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault());

        return (TAttribute?)attribute;
    }

    private static class EnumAttributeCache<TEnum, TAttribute>
        where TEnum : struct, Enum
        where TAttribute : Attribute
    {
        private static readonly Dictionary<TEnum, TAttribute?> Map = Build();

        public static TAttribute? Get(TEnum value)
        {
            return Map.TryGetValue(value, out var attribute) ? attribute : null;
        }

        private static Dictionary<TEnum, TAttribute?> Build()
        {
            var enumType = typeof(TEnum);
            var map = new Dictionary<TEnum, TAttribute?>();

            foreach (TEnum value in (TEnum[])Enum.GetValues(enumType))
            {
                map[value] = enumType.GetField(value.ToString())
                    ?.GetCustomAttributes(typeof(TAttribute), false)
                    .OfType<TAttribute>()
                    .SingleOrDefault();
            }

            return map;
        }
    }

    public static string DisplayName(this Enum value, string defaultValue = "Não definido")
    {
        var type = value.GetType();

        if (!Enum.IsDefined(type, value))
            return defaultValue;

        var attribute = value.Attribute<DisplayAttribute>();
        return attribute?.Name ?? value.ToString();
    }

    public static object ToViewModel<TEnum>() where TEnum : struct, Enum
    {
        var enumType = typeof(TEnum);
        var json = new Dictionary<string, object>();
        var enums = new List<object>();
        var names = new Dictionary<int, string>();

        foreach (var e in Enum.GetValues(enumType))
        {
            var typedEnum = (TEnum)e;
            var value = Convert.ToInt32(e);
            var name = e.ToString()!;
            // Valores vêm de GetValues (sempre definidos), então usamos o fast path
            // direto — equivalente a DisplayName() sem o Enum.IsDefined/boxing.
            var displayName = typedEnum.Attribute<TEnum, DisplayAttribute>()?.Name ?? name;

            enums.Add(new
            {
                Value = value,
                Name = name,
                Display = displayName
            });

            names.Add(value, displayName);
            json.Add(name, value);
        }

        json.Add("enums", enums.ToArray());
        json.Add("names", names);

        return json;
    }
}