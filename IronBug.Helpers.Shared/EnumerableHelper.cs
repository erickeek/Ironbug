using IronBug.Helpers.EnumerableComponents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronBug.Helpers
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> Except<T, TComparer>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TComparer> comparer)
        {
            return first.Except(second, new FuncEqualityComparer<T, TComparer>(comparer));
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }
        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Remove(item);
        }

        public static void Merge<T>(this ICollection<T> collection, IEnumerable<T> updated)
        {
            updated = updated.ToArray();

            var added = updated.Except(collection).ToArray();
            var removed = collection.Except(updated).ToArray();

            collection.RemoveRange(removed);
            collection.AddRange(added);
        }
    }
}