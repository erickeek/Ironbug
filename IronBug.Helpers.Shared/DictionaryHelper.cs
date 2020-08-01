using System;
using System.Collections.Generic;

namespace IronBug.Helpers
{
    public static class DictionaryHelper
    {
        public static void AddRangeOverride<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> items)
        {
            items.ForEach(pair => source[pair.Key] = pair.Value);
        }

        public static void AddRangeNewOnly<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> items)
        {
            items.ForEach(pair =>
            {
                if (!source.ContainsKey(pair.Key))
                    source.Add(pair.Key, pair.Value);
            });
        }

        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> items)
        {
            items.ForEach(pair => source.Add(pair.Key, pair.Value));
        }

        private static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue defaultValue = default)
        {
            return source.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}