using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Compiler.Utils
{
    /// <summary>
    /// Utilities for hashing.
    /// </summary>
    static class Hash
    {
        /// <summary>
        /// Combines hash values.
        /// For <see cref="IEnumerable"/>s, each element is hashed.
        /// </summary>
        /// <param name="values">The values to combine.</param>
        /// <returns>The combined hash value.</returns>
        public static int Combine(params object[] values)
        {
            var result = new HashCode();
            CombineInternal(ref result, values);
            return result.ToHashCode();
        }

        /// <summary>
        /// Combines hash values of a polymorphic object. The hash will include the type tag of the polymorphic type.
        /// </summary>
        /// <param name="obj">The polymorphic <see cref="object"/>.</param>
        /// <param name="values">The values to combine alongside the <see cref="object"/>.</param>
        /// <returns>The combined hash value.</returns>
        public static int HashCombinePoly(this object obj, params object[] values) =>
            Combine(obj.GetType(), values);

        private static void CombineInternal(ref HashCode result, params object?[] values)
        {
            foreach (var value in values)
            {
                if (value is IEnumerable enumerable)
                {
                    foreach (var element in enumerable) CombineInternal(ref result, element);
                }
                else
                {
                    result.Add(value);
                }
            }
        }
    }
}
