using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Capriccioso
{
    /// <summary>
    /// Extension methods for collections (List, Array, IEnumerable).
    /// </summary>
    /// <example>
    /// <code>
    /// // Check if null or empty
    /// List&lt;int&gt; numbers = null;
    /// if (numbers.IsNullOrEmpty())
    /// {
    ///     Debug.Log("No numbers!");
    /// }
    /// 
    /// // Get random element
    /// List&lt;string&gt; names = new List&lt;string&gt; { "Alice", "Bob", "Charlie" };
    /// string randomName = names.GetRandom();
    /// 
    /// // Shuffle in place
    /// names.Shuffle();
    /// 
    /// // Get shuffled copy
    /// List&lt;string&gt; shuffled = names.Shuffled();
    /// 
    /// // Safe access with default
    /// string item = names.GetOrDefault(10, "Unknown"); // Returns "Unknown" if out of range
    /// 
    /// // First and Last with default
    /// string first = names.FirstOrDefault();
    /// string last = names.LastOrDefault();
    /// 
    /// // Remove random element
    /// string removed = names.RemoveRandom();
    /// 
    /// // Array shuffle
    /// int[] array = { 1, 2, 3, 4, 5 };
    /// array.Shuffle();
    /// </code>
    /// </example>
    public static class CollectionExtensions
    {
        #region Null/Empty Checks

        /// <summary>
        /// Returns true if the list is null or has no elements.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Returns true if the array is null or has no elements.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// Returns true if the collection is not null and has elements.
        /// </summary>
        public static bool HasElements<T>(this IList<T> list)
        {
            return list != null && list.Count > 0;
        }

        #endregion

        #region Random Access

        /// <summary>
        /// Returns a random element from the list.
        /// </summary>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Cannot get random element from empty list.");
            }
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Returns a random element from the array.
        /// </summary>
        public static T GetRandom<T>(this T[] array)
        {
            if (array.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Cannot get random element from empty array.");
            }
            return array[Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Tries to get a random element. Returns false if empty.
        /// </summary>
        public static bool TryGetRandom<T>(this IList<T> list, out T result)
        {
            if (list.IsNullOrEmpty())
            {
                result = default;
                return false;
            }
            result = list[Random.Range(0, list.Count)];
            return true;
        }

        /// <summary>
        /// Removes and returns a random element from the list.
        /// </summary>
        public static T RemoveRandom<T>(this IList<T> list)
        {
            if (list.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Cannot remove random element from empty list.");
            }
            int index = Random.Range(0, list.Count);
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        #endregion

        #region Safe Access

        /// <summary>
        /// Returns the element at index, or default value if out of range.
        /// </summary>
        public static T GetOrDefault<T>(this IList<T> list, int index, T defaultValue = default)
        {
            if (list == null || index < 0 || index >= list.Count)
            {
                return defaultValue;
            }
            return list[index];
        }

        /// <summary>
        /// Returns the element at index, or default value if out of range.
        /// </summary>
        public static T GetOrDefault<T>(this T[] array, int index, T defaultValue = default)
        {
            if (array == null || index < 0 || index >= array.Length)
            {
                return defaultValue;
            }
            return array[index];
        }

        /// <summary>
        /// Returns the first element or default if empty.
        /// </summary>
        public static T FirstOrDefault<T>(this IList<T> list, T defaultValue = default)
        {
            return list.IsNullOrEmpty() ? defaultValue : list[0];
        }

        /// <summary>
        /// Returns the last element or default if empty.
        /// </summary>
        public static T LastOrDefault<T>(this IList<T> list, T defaultValue = default)
        {
            return list.IsNullOrEmpty() ? defaultValue : list[list.Count - 1];
        }

        #endregion

        #region Shuffle

        /// <summary>
        /// Shuffles the list in place using Fisher-Yates algorithm.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Shuffles the array in place using Fisher-Yates algorithm.
        /// </summary>
        public static void Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        /// <summary>
        /// Returns a new shuffled copy of the list.
        /// </summary>
        public static List<T> Shuffled<T>(this IList<T> list)
        {
            List<T> copy = new List<T>(list);
            copy.Shuffle();
            return copy;
        }

        #endregion

        #region ForEach

        /// <summary>
        /// Executes an action for each element with its index.
        /// </summary>
        public static void ForEach<T>(this IList<T> list, Action<T, int> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }

        #endregion

        #region Dictionary Extensions

        /// <summary>
        /// Gets a value or adds it if not present.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
        {
            if (!dict.TryGetValue(key, out TValue value))
            {
                value = valueFactory();
                dict[key] = value;
            }
            return value;
        }

        /// <summary>
        /// Gets a value or returns default if not present.
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            return dict.TryGetValue(key, out TValue value) ? value : defaultValue;
        }

        #endregion
    }
}
