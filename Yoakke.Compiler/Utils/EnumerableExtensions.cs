﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Compiler.Utils
{
    /// <summary>
    /// <see cref="IEnumerable{T}"/> utilities.
    /// </summary>
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
    }
}
