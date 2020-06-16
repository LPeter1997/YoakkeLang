using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Compiler.Utils
{
    /// <summary>
    /// Utility for hashing lists.
    /// </summary>
    static class HashList
    {
        /// <summary>
        /// Hashes an <see cref="IEnumerable{T}"/>, combining each element's hash code.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerable.</typeparam>
        /// <param name="vs">The list of values to combine.</param>
        /// <returns>The combined hash of the list of values.</returns>
        public static int Combine<T>(IEnumerable<T> vs)
        {
            var h = new HashCode();
            foreach (var v in vs) h.Add(v);
            return h.ToHashCode();
        }
    }
}
