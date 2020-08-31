using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// Interface for interval trees.
    /// </summary>
    /// <typeparam name="TKey">The interval bounds type.</typeparam>
    /// <typeparam name="TValue">The associated value type for each interval.</typeparam>
    public interface IIntervalTree<TKey, TValue> : IEnumerable<IntervalValuePair<TKey, TValue>>
    {
        /// <summary>
        /// The number of intervals contained in the tree.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The comparer for this tree's interval bounds.
        /// </summary>
        public IComparer<TKey> Comparer { get; }

        /// <summary>
        /// The values contained inside the tree.
        /// </summary>
        public IEnumerable<TValue> Values { get; }

        /// <summary>
        /// Retrieves all intervals that contain the given point.
        /// </summary>
        /// <param name="point">The point that needs to be contained.</param>
        /// <returns>All <see cref="IntervalValuePair{TKey, TValue}"/>s that contain the given point.</returns>
        public IEnumerable<IntervalValuePair<TKey, TValue>> Query(TKey point);

        /// <summary>
        /// Retrieves all intervals that intersect with the given interval.
        /// </summary>
        /// <param name="start">The start of the interval (included).</param>
        /// <param name="end">The end of the interval (excluded).</param>
        /// <returns>All <see cref="IntervalValuePair{TKey, TValue}"/>s that intersect with the given interval.</returns>
        public IEnumerable<IntervalValuePair<TKey, TValue>> Query(TKey start, TKey end);

        /// <summary>
        /// Adds an interval to the tree.
        /// </summary>
        /// <param name="start">The start of the interval (included).</param>
        /// <param name="end">The end of the interval (excluded).</param>
        /// <param name="value">The associated value.</param>
        public void Add(TKey start, TKey end, TValue value);

        /// <summary>
        /// Removes the given value.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        public void Remove(TValue value);

        /// <summary>
        /// Removes all elements from the tree-
        /// </summary>
        public void Clear();
    }
}
