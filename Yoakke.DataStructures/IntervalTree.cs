using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    // TODO: This is a simple, but linear implementation!
    // We could use the idea RangeTree does, but adapt it to half-open intervals.

    /// <summary>
    /// A simple <see cref="IIntervalTree{TKey, TValue}"/> implementation.
    /// </summary>
    /// <typeparam name="TKey">The interval endpoint type.</typeparam>
    /// <typeparam name="TValue">The associated value type.</typeparam>
    public class IntervalTree<TKey, TValue> : IIntervalTree<TKey, TValue>
    {
        public int Count => elements.Count;
        public IComparer<TKey> Comparer { get; }
        public IEnumerable<TValue> Values => elements.Select(e => e.Value);

        private List<IntervalValuePair<TKey, TValue>> elements = new List<IntervalValuePair<TKey, TValue>>();

        /// <summary>
        /// Initializes a new <see cref="IntervalTree{TKey, TValue}"/> with the given comparer.
        /// </summary>
        /// <param name="comparer">The comparer to use.</param>
        public IntervalTree(IComparer<TKey> comparer)
        {
            Comparer = comparer;
        }

        /// <summary>
        /// Initializes a new <see cref="IntervalTree{TKey, TValue}"/> with the default comparator.
        /// </summary>
        public IntervalTree()
            : this(Comparer<TKey>.Default)
        {
        }

        public IEnumerator<IntervalValuePair<TKey, TValue>> GetEnumerator() => elements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<IntervalValuePair<TKey, TValue>> Query(TKey point) =>
            elements.Where(e => Contains(e.Start, e.End, point));

        public IEnumerable<IntervalValuePair<TKey, TValue>> Query(TKey start, TKey end) =>
            elements.Where(e => Intersects(e.Start, e.End, start, end));

        public void Add(TKey from, TKey to, TValue value)
        {
            if (Comparer.Compare(from, to) > 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(from)} can't be larger than {nameof(to)}!");
            }
            elements.Add(new IntervalValuePair<TKey, TValue>(from, to, value));
        }

        public void Remove(TValue value)
        {
#pragma warning disable CS8602
            var idx = elements.FindIndex(e => value.Equals(e.Value));
#pragma warning restore CS8602
            if (idx >= 0) elements.RemoveAt(idx);
        }

        public void Clear() => elements.Clear();

        private bool Contains(TKey start, TKey end, TKey point) =>
               Comparer.Compare(start, point) <= 0
            && Comparer.Compare(point, end) < 0;

        private bool Intersects(TKey start1, TKey end1, TKey start2, TKey end2) =>
               !(Comparer.Compare(start1, end2) >= 0
              || Comparer.Compare(start2, end1) >= 0);
    }
}
