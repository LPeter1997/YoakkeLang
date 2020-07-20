using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yoakke.Compiler.Utils
{
    /// <summary>
    /// Utilities and extensions for hashing.
    /// </summary>
    static class Hash
    {
        /// <summary>
        /// Calculates the hash value of the given values usin <see cref="AddAllDispatched(ref HashCode, object[])"/>.
        /// </summary>
        /// <param name="vs">The values to calculate the hash for.</param>
        /// <returns>The calculated hash value.</returns>
        public static int CombineDispatched(params object[] vs)
        {
            var hashCode = new HashCode();
            hashCode.AddAllDispatched(vs);
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Calculates the hash value of the given values usin <see cref="CombineDispatched"/>, including
        /// the type of the <see cref="object"/> it's called on.
        /// This makes it suitable for polymorphic values to be hashed.
        /// </summary>
        /// <param name="obj">The polymorphic <see cref="object"/> to include the type of in the hash.</param>
        /// <param name="vs">The values to include in the hash.</param>
        /// <returns>The calculated hash value.</returns>
        public static int HashCombinePoly(this object obj, params object[] vs) =>
            CombineDispatched(obj.GetType(), vs);

        /// <summary>
        /// Adds all of the values to the given <see cref="HashCode"/> using <see cref="AddDispatched(ref HashCode, object?)"/>.
        /// </summary>
        /// <param name="hashCode">The <see cref="HashCode"/> to add the values to.</param>
        /// <param name="vs">Te values to add.</param>
        public static void AddAllDispatched(ref this HashCode hashCode, params object[] vs)
        {
            foreach (var v in vs) hashCode.AddDispatched(v);
        }

        /// <summary>
        /// Adds a value to the given <see cref="HashCode"/>, either adding it using 
        /// <see cref="AddEnumerable(ref HashCode, IEnumerable)"/>,
        /// <see cref="AddDictionary{TKey, TValue}(ref HashCode, IDictionary{TKey, TValue})"/> or simply by 
        /// <see cref="HashCode.Add{T}(T)"/>, depending on the type.
        /// </summary>
        /// <param name="hashCode">The <see cref="HashCode"/> to add the value to.</param>
        /// <param name="obj">The value to add.</param>
        public static void AddDispatched(ref this HashCode hashCode, object? obj)
        {
            if (obj == null) return;
            var objType = obj.GetType();
            if (objType.IsGenericType && typeof(IDictionary).IsAssignableFrom(objType))
            {
                // IDictionary<K, V>
                var method = typeof(Hash).GetMethod(nameof(AddDictionary));
                var genericMethod = method?.MakeGenericMethod(objType.GenericTypeArguments);
                var args = new object[] { hashCode, obj };
                genericMethod?.Invoke(null, args);
                hashCode = (HashCode)args[0];
            }
            else if (obj is IEnumerable enumerable)
            {
                hashCode.AddEnumerable(enumerable);
            }
            else
            {
                hashCode.Add(obj);
            }
        }

        /// <summary>
        /// Adds an <see cref="IEnumerable"/> to the <see cref="HashCode"/> by adding each element.
        /// </summary>
        /// <param name="hashCode">The <see cref="HashCode"/> to add the values to.</param>
        /// <param name="enumerable">The <see cref="IEnumerable"/> of elements to add.</param>
        public static void AddEnumerable(ref this HashCode hashCode, IEnumerable enumerable)
        {
            // NOTE: We changed to AddDispatched here
            foreach (var element in enumerable) hashCode.AddDispatched(element);
        }

        /// <summary>
        /// Adds an <see cref="IDictionary{TKey, TValue}"/> to the <see cref="HashCode"/>, while providing a stable hash.
        /// It adds each key and value, sorted by the key.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="hashCode">The <see cref="HashCode"/> to add the values to.</param>
        /// <param name="dict">The <see cref="IDictionary{TKey, TValue}"/> of values to add.</param>
        public static void AddDictionary<TKey, TValue>(ref this HashCode hashCode, IDictionary<TKey, TValue> dict)
            where TKey: notnull
        {
            foreach (var kv in dict.OrderBy(kv => kv.Key))
            {
                hashCode.Add(kv.Key);
                hashCode.Add(kv.Value);
            }
        }
    }
}
