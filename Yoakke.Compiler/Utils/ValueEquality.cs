using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Yoakke.Compiler.Utils
{
    /// <summary>
    /// Utilities for value equality between .NET types.
    /// </summary>
    static class ValueEquality
    {
        /// <summary>
        /// Checks value equality between two <see cref="IList{T}"/>s.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="list1">The first <see cref="IList{T}"/> to compare.</param>
        /// <param name="list2">The second <see cref="IList{T}"/> to compare.</param>
        /// <returns>True, if the two lists are equal.</returns>
        public static bool ValueEquals<T>(this IList<T> list1, IList<T> list2) where T: IEquatable<T>
        {
            if (list1.Count != list2.Count) return false;
            for (int i = 0; i < list1.Count; ++i)
            {
                var v1 = list1[i];
                var v2 = list2[i];
                if (v1 == null)
                {
                    if (v2 != null) return false;
                }
                else if (!v1.Equals(v2)) return false;
            }
            return true;
        }

        /// <summary>
        /// Checks value equality between two <see cref="IDictionary{TKey, TValue}"/>s.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="dict1">The first <see cref="IDictionary{TKey, TValue}"/> to compare.</param>
        /// <param name="dict2">The second <see cref="IDictionary{TKey, TValue}"/> to compare.</param>
        /// <returns>True, if the two dictionaries are equal.</returns>
        public static bool ValueEquals<TKey, TValue>(this IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2) 
            where TKey: notnull
            where TValue: IEquatable<TValue>
        {
            if (dict1.Count != dict2.Count) return false;
            foreach (var kv in dict1)
            {
                if (!dict2.TryGetValue(kv.Key, out var value)) return false;
                if (kv.Value == null)
                {
                    if (value != null) return false;
                }
                else if (!kv.Value.Equals(value)) return false;
            }
            return true;
        }
    }
}
