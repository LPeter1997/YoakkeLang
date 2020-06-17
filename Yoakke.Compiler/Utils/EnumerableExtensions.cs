using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Compiler.Utils
{
    static class EnumerableExtensions
    {
        /// <summary>
        /// Intertwines an <see cref="Action"/> between <see cref="Action{T}"/>s for an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the enumerable elements.</typeparam>
        /// <param name="enumerable">The enumerable to go through.</param>
        /// <param name="element">The <see cref="Action{T}"/> that's called for each element.</param>
        /// <param name="tween">The <see cref="Action"/> that's called between each element.</param>
        public static void Intertwine<T>(this IEnumerable<T> enumerable, Action<T> element, Action tween)
        {
            bool first = true;
            foreach (var e in enumerable)
            {
                if (!first) tween();
                first = false;

                element(e);
            }
        }

        /// <summary>
        /// Joins the given <see cref="IEnumerable{string}"/>.
        /// </summary>
        /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to join.</param>
        /// <param name="joiner">The string to join the elements with.</param>
        /// <returns>The joined string.</returns>
        public static string StringJoin(this IEnumerable<string?> enumerable, string joiner)
        {
            var builder = new StringBuilder();
            bool first = true;
            foreach (var e in enumerable)
            {
                if (!first) builder.Append(joiner);
                first = false;

                builder.Append(e);
            }
            return builder.ToString();
        }
    }
}
