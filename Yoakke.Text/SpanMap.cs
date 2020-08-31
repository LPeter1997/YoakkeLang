using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;

namespace Yoakke.Text
{
    /// <summary>
    /// An associative container from <see cref="Span"/>s to values.
    /// </summary>
    /// <typeparam name="TValue">The associated value type.</typeparam>
    public class SpanMap<TValue> : IEnumerable<SpanValuePair<TValue>>
    {
        /// <summary>
        /// The number of entries.
        /// </summary>
        public int Count => intervals.Count;
        /// <summary>
        /// The <see cref="Span"/>s in this container.
        /// </summary>
        public IEnumerable<Span> Spans => this.Select(x => x.Span);
        /// <summary>
        /// The associated <see cref="TValue"/>s in this container.
        /// </summary>
        public IEnumerable<TValue> Values => this.Select(x => x.Value);

        private IntervalTree<Position, TValue> intervals = new IntervalTree<Position, TValue>();

        public IEnumerator<SpanValuePair<TValue>> GetEnumerator() =>
            intervals.Select(Convert).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Queries all <see cref="Span"/>s that contains the given <see cref="Position"/>.
        /// </summary>
        /// <param name="position">The <see cref="Position"/> to find.</param>
        /// <returns>An <see cref="IEnumerable{SpanValuePair{TValue}}"/> of all <see cref="Span"/>s
        /// containing the given <see cref="Position"/>.</returns>
        public IEnumerable<SpanValuePair<TValue>> Query(Position position) =>
            intervals.Query(position).Select(Convert);

        /// <summary>
        /// Queries all <see cref="Span"/>s that intersects with the given <see cref="Span"/>.
        /// </summary>
        /// <param name="span">The <see cref="Span"/> to check intersection with.</param>
        /// <returns>An <see cref="IEnumerable{SpanValuePair{TValue}}"/> of all <see cref="Span"/>s
        /// intersecting with the given <see cref="Span"/>.</returns>
        public IEnumerable<SpanValuePair<TValue>> Query(Span span) =>
            intervals.Query(span.Start, span.End).Select(Convert);

        /// <summary>
        /// Adds a new <see cref="Span"/> and an associated value to this container.
        /// </summary>
        /// <param name="span">The <see cref="Span"/> to add.</param>
        /// <param name="value">The associated value.</param>
        public void Add(Span span, TValue value) => intervals.Add(span.Start, span.End, value);

        /// <summary>
        /// Removes the given value from this container.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        public void Remove(TValue value) => intervals.Remove(value);

        /// <summary>
        /// Removes all elements from this container.
        /// </summary>
        public void Clear() => intervals.Clear();

        private static SpanValuePair<TValue> Convert(IntervalValuePair<Position, TValue> pair) =>
            new SpanValuePair<TValue>(new Span(pair.Start, pair.End), pair.Value);
    }
}
