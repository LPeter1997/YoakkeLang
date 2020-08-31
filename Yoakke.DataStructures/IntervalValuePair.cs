using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// A single half-open interval with some associated value.
    /// </summary>
    /// <typeparam name="TKey">The interval bounds type.</typeparam>
    /// <typeparam name="TValue">The associated value type.</typeparam>
    public readonly struct IntervalValuePair<TKey, TValue>
    {
        /// <summary>
        /// The start of the interval (included).
        /// </summary>
        public readonly TKey Start;
        /// <summary>
        /// The end of the interval (excluded).
        /// </summary>
        public readonly TKey End;
        /// <summary>
        /// The associated value.
        /// </summary>
        public readonly TValue Value;

        /// <summary>
        /// Initializes a new <see cref="IntervalValuePair{TKey, TValue}"/>.
        /// </summary>
        /// <param name="start">The start of the interval (included).</param>
        /// <param name="end">The end of the interval (excluded).</param>
        /// <param name="value">The associated value.</param>
        public IntervalValuePair(TKey start, TKey end, TValue value)
        {
            Start = start;
            End = end;
            Value = value;
        }
    }
}
