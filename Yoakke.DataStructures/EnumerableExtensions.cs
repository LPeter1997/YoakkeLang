using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// Extension algorithms for enumerables.
    /// </summary>
    public static class EnumerableExtensions
    {
        // TextReader //////////////////////////////////////////////////////////

        /// <summary>
        /// Adapts a <see cref="TextReader"/> to become an <see cref="IEnumerable{char}"/>.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to adapt.</param>
        /// <returns>The <see cref="IEnumerable{char}"/> that reads the <see cref="TextReader"/>
        /// until the end.</returns>
        public static IEnumerable<char> AsCharEnumerable(this TextReader reader)
        {
            while (true)
            {
                var code = reader.Read();
                if (code == -1) break;
                yield return (char)code;
            }
        }

        // Ordered merge ///////////////////////////////////////////////////////

        /// <summary>
        /// Merges two ordered <see cref="IEnumerable{T}"/>s and keeps the ordering.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="l1">The first ordered <see cref="IEnumerable{T}"/> to merge.</param>
        /// <param name="l2">The second ordered <see cref="IEnumerable{T}"/> to merge.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains elements from both sequences, 
        /// with kept ordering.</returns>
        public static IEnumerable<T> OrderedMerge<T>(this IEnumerable<T> l1, IEnumerable<T> l2)
            where T : IComparable<T> =>
            OrderedMerge(l1, l2, Comparer<T>.Default);

        /// <summary>
        /// Merges two ordered <see cref="IEnumerable{T}"/>s and keeps the ordering.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <typeparam name="TKey">The element type.</typeparam>
        /// <param name="l1">The first ordered <see cref="IEnumerable{TElement}"/> to merge.</param>
        /// <param name="l2">The second ordered <see cref="IEnumerable{TElement}"/> to merge.</param>
        /// <param name="keySelector">The comparison key selector function.</param>
        /// <returns>An <see cref="IEnumerable{TElement}"/> that contains elements from both sequences, 
        /// with kept ordering.</returns>
        public static IEnumerable<TElement> OrderedMerge<TElement, TKey>(
            this IEnumerable<TElement> l1, 
            IEnumerable<TElement> l2,
            Func<TElement, TKey> keySelector)
            where TKey : IComparable<TKey> =>
            OrderedMerge(l1, l2, keySelector, Comparer<TKey>.Default);

        /// <summary>
        /// Merges two ordered <see cref="IEnumerable{T}"/>s and keeps the ordering.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="l1">The first ordered <see cref="IEnumerable{T}"/> to merge.</param>
        /// <param name="l2">The second ordered <see cref="IEnumerable{T}"/> to merge.</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> to do the comparison with.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains elements from both sequences, 
        /// with kept ordering.</returns>
        public static IEnumerable<T> OrderedMerge<T>(this IEnumerable<T> l1, IEnumerable<T> l2, IComparer<T> comparer) =>
            OrderedMerge(l1, l2, x => x, comparer);

        /// <summary>
        /// Merges two ordered <see cref="IEnumerable{T}"/>s and keeps the ordering.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <typeparam name="TKey">The element type.</typeparam>
        /// <param name="l1">The first ordered <see cref="IEnumerable{TElement}"/> to merge.</param>
        /// <param name="l2">The second ordered <see cref="IEnumerable{TElement}"/> to merge.</param>
        /// <param name="keySelector">The comparison key selector function.</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> to do the comparison with.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains elements from both sequences, 
        /// with kept ordering.</returns>
        public static IEnumerable<TElement> OrderedMerge<TElement, TKey>(
            this IEnumerable<TElement> l1, 
            IEnumerable<TElement> l2, 
            Func<TElement, TKey> keySelector, 
            IComparer<TKey> comparer)
        {
            var e1 = l1.GetEnumerator();
            var e2 = l2.GetEnumerator();
            bool e1has = e1.MoveNext();
            bool e2has = e2.MoveNext();
            while (e1has && e2has)
            {
                if (comparer.Compare(keySelector(e1.Current), keySelector(e2.Current)) <= 0)
                {
                    yield return e1.Current;
                    e1has = e1.MoveNext();
                }
                else
                {
                    yield return e2.Current;
                    e2has = e2.MoveNext();
                }
            }
            while (e1has)
            {
                yield return e1.Current;
                e1has = e1.MoveNext();
            }
            while (e2has)
            {
                yield return e2.Current;
                e2has = e2.MoveNext();
            }
        }
    }
}
