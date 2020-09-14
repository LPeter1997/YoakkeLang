using System.Collections.Generic;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// Utilities for constructing <see cref="IValueList{T}"/>s.
    /// </summary>
    public static class ValueListExtensions
    {
        /// <summary>
        /// Wraps the given <see cref="IList{T}"/> as an <see cref="IValueList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to wrap.</param>
        /// <returns>The wrapped list.</returns>
        public static IValueList<T> AsValueList<T>(this IList<T> list) =>
            list as IValueList<T> ?? new ValueList<T>(list);
    }
}
