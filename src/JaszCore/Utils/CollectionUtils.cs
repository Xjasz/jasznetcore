using System;
using System.Collections.Generic;
using System.Linq;

namespace JaszCore.Utils
{
    public static class CollectionUtils
    {
        public static void TryAdd<T>(this ISet<T> set, T value)
        {
            if (!set.Contains(value))
            {
                set.Add(value);
            }
        }

        public static bool IsDefined<T>(this ICollection<T> items)
        {
            return items != null && items.Count > 0;
        }

        public static bool IsDefined<T>(this T[] items)
        {
            return items != null && items.Length > 0;
        }

        public static bool IsEmpty<T>(this ICollection<T> items)
        {
            return items == null || items.Count == 0;
        }


        public static bool IsEmpty<T>(this T[] items)
        {
            return !items.IsDefined();
        }


        public static bool AddAll<T, C>(this T items, T other) where T : ICollection<C>
        {
            foreach (C item in other)
            {
                items.Add(item);
            }
            return true;
        }

        public static bool RemoveAll<T, C>(this T items, T other) where T : ICollection<C>
        {
            foreach (C item in other)
            {
                items.Remove(item);
            }
            return true;
        }


        public static bool OptionalAdd<T, C>(this T collection, C item, Func<bool> predicate) where T : ICollection<C>
        {
            if (predicate())
            {
                collection.Add(item);
                return true;
            }
            return false;
        }

        public static bool ContainsAll<T>(this IEnumerable<T> items, IEnumerable<T> other)
        {
            ISet<T> source = new HashSet<T>(items);
            foreach (T item in other)
            {
                if (!source.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }


        public static bool Equals<K, V>(this IDictionary<K, V> dic1, IDictionary<K, V> dic2)
        {
            return dic1 != null && dic2 != null && dic1.Count == dic2.Count && !dic1.Except(dic2).Any();
        }

        private static T WhereX<T>(this IEnumerable<T> items, int sign, IComparer<T> comparerBy = null)
        {
            T resultItem = default;

            if (!items.Any())
            {
                return resultItem;
            }

            var comparer = comparerBy ?? Comparer<T>.Default;

            bool initialized = false;
            foreach (T item in items)
            {
                if (!initialized)
                {
                    initialized = true;
                    resultItem = item;
                    continue;
                }

                if (comparer.Compare(item, resultItem) * sign > 0)
                {
                    resultItem = item;
                }
            }
            return resultItem;
        }


        public static T WhereMax<T>(this IEnumerable<T> items, IComparer<T> comparerBy = null)
        {
            return items.WhereX(1, comparerBy);
        }

        public static T WhereMin<T>(this IEnumerable<T> items, IComparer<T> comparerBy = null)
        {
            return items.WhereX(-1, comparerBy);
        }

    }
}
