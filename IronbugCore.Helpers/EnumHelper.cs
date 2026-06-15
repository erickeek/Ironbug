using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace IronbugCore.Helpers;

public static class EnumHelper
{
    private static readonly ConcurrentDictionary<(Type EnumType, string Name, Type AttributeType), object?> AttributeCache = new();

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
            return CollectionExtensions.GetValueOrDefault(Map, value);
        }

        private static Dictionary<TEnum, TAttribute?> Build()
        {
            var enumType = typeof(TEnum);
            var map = new Dictionary<TEnum, TAttribute?>();

            foreach (var value in (TEnum[])Enum.GetValues(enumType))
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

    /// <summary>
    /// Objeto serializável de um enum no formato legado: cada nome→valor na raiz, mais
    /// "enums" (array de { Value, Name, Display }) e "names" (valor→display).
    /// O trabalho de reflexão é cacheado por <typeparamref name="TEnum"/>.
    /// </summary>
    public static object ToViewModel<TEnum>() where TEnum : struct, Enum
    {
        return Build(EnumEntryCache<TEnum>.Entries);
    }

    /// <summary>
    /// Como <see cref="ToViewModel{TEnum}()"/>, omitindo os valores informados em <paramref name="exclude"/>.
    /// </summary>
    public static object ToViewModel<TEnum>(params TEnum[] exclude) where TEnum : struct, Enum
    {
        if (exclude.Length == 0)
            return Build(EnumEntryCache<TEnum>.Entries);

        var excluded = new HashSet<TEnum>(exclude);
        return BuildFiltered<TEnum>(value => !excluded.Contains(value));
    }

    /// <summary>
    /// Como <see cref="ToViewModel{TEnum}()"/>, mantendo apenas os valores em que <paramref name="predicate"/> retorna true.
    /// </summary>
    public static object ToViewModel<TEnum>(Func<TEnum, bool> predicate) where TEnum : struct, Enum
    {
        return BuildFiltered(predicate);
    }

    private static object BuildFiltered<TEnum>(Func<TEnum, bool> predicate) where TEnum : struct, Enum
    {
        var values = EnumEntryCache<TEnum>.Values;
        var entries = EnumEntryCache<TEnum>.Entries;

        var filtered = new List<EnumEntry>(entries.Count);
        for (var i = 0; i < values.Length; i++)
        {
            if (predicate(values[i]))
                filtered.Add(entries[i]);
        }

        return Build(filtered);
    }

    private static Dictionary<string, object> Build(IReadOnlyList<EnumEntry> entries)
    {
        var json = new Dictionary<string, object>(entries.Count + 2);
        var enums = new object[entries.Count];
        var names = new Dictionary<int, string>(entries.Count);

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            enums[i] = new { entry.Value, entry.Name, entry.Display };
            names[entry.Value] = entry.Display;
            json[entry.Name] = entry.Value;
        }

        json["enums"] = enums;
        json["names"] = names;
        return json;
    }

    private readonly record struct EnumEntry(int Value, string Name, string Display);

    private static class EnumEntryCache<TEnum> where TEnum : struct, Enum
    {
        public static readonly TEnum[] Values = Enum.GetValues<TEnum>();
        public static readonly IReadOnlyList<EnumEntry> Entries = Build();

        private static EnumEntry[] Build()
        {
            var entries = new EnumEntry[Values.Length];

            for (var i = 0; i < Values.Length; i++)
            {
                var value = Values[i];
                var name = value.ToString();
                var display = value.Attribute<TEnum, DisplayAttribute>()?.Name ?? name;
                entries[i] = new EnumEntry(Convert.ToInt32(value), name, display);
            }

            return entries;
        }
    }
}