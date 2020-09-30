using System.Collections.Generic;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// Utilities for constructing value-semantics collections.
    /// </summary>
    public static class ValueCollectionExtensions
    {
        /// <summary>
        /// Wraps the given <see cref="IList{T}"/> as an <see cref="IValueList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to wrap.</param>
        /// <returns>The wrapped list.</returns>
        public static IValueList<T> AsValueList<T>(this IList<T> list) =>
            list as IValueList<T> ?? new ValueList<T>(list);

        /// <summary>
        /// Wraps the given <see cref="IDictionary{TKey, TValue}"/> as an <see cref="IValueDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="dict">The dictionary to wrap.</param>
        /// <returns>The wrapped dictionary.</returns>
        public static IValueDictionary<TKey, TValue> AsValueDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dict)
            where TKey : notnull =>
            dict as IValueDictionary<TKey, TValue> ?? new ValueDictionary<TKey, TValue>(dict);
    }
}
