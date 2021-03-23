using System;

namespace Yoakke.Text
{
    // TODO: Since we have made Range, we could use it here and rename span to something like SourceSpan
    // And have SourceFile be necessary here

    /// <summary>
    /// An 2D interval of text positions.
    /// </summary>
#pragma warning disable CS0660, CS0661 // No reason to override Equals or GetHashCode
    public struct Span
#pragma warning restore CS0660, CS0661
    {
        /// <summary>
        /// The <see cref="SourceFile"/> this <see cref="Span"/> originates from.
        /// </summary>
        public SourceFile? Source { get; set; }
        /// <summary>
        /// The first <see cref="Position"/> that's inside this <see cref="Span"/>.
        /// </summary>
        public readonly Position Start;
        /// <summary>
        /// The first <see cref="Position"/> after this <see cref="Span"/>.
        /// </summary>
        public readonly Position End;

        /// <summary>
        /// Initializes a new <see cref="Span"/>.
        /// </summary>
        /// <param name="source">The <see cref="SourceFile"/> this span originates from.</param>
        /// <param name="start">The first <see cref="Position"/> that's inside this span.</param>
        /// <param name="end">The first <see cref="Position"/> after this span.</param>
        public Span(SourceFile? source, Position start, Position end)
        {
            if (end < start) throw new ArgumentException("The end can't be smaller than the start!");

            Source = source;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Initializes a new <see cref="Span"/>.
        /// </summary>
        /// <param name="start">The first <see cref="Position"/> that's inside this span.</param>
        /// <param name="end">The first <see cref="Position"/> after this span.</param>
        public Span(Position start, Position end)
            : this(null, start, end)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Span"/>.
        /// </summary>
        /// <param name="source">The <see cref="SourceFile"/> this span originates from.</param>
        /// <param name="start">The first <see cref="Position"/> that's inside this span.</param>
        /// <param name="length">The length of this span.</param>
        public Span(SourceFile? source, Position start, int length)
            : this(source, start, start.Advance(length))
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Span"/>.
        /// </summary>
        /// <param name="start">The first <see cref="Position"/> that's inside this span.</param>
        /// <param name="length">The length of this span.</param>
        public Span(Position start, int length)
            : this(null, start, start.Advance(length))
        {
        }

        /// <summary>
        /// Initializes a new, empty <see cref="Span"/>.
        /// </summary>
        /// <param name="source">The <see cref="SourceFile"/> this span originates from.</param>
        public Span(SourceFile? source)
            : this(source, new Position(), 0)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Span"/> by arching over two others.
        /// </summary>
        /// <param name="from">The starting span.</param>
        /// <param name="to">The ending span.</param>
        public Span(Span from, Span to)
            : this(from.Source, from.Start, to.End)
        {
            if (!Equals(from.Source, to.Source))
            {
                throw new ArgumentException("The two spans originate from different sources!");
            }
        }

        /// <summary>
        /// Checks if a given <see cref="Position"/> is within the bounds of this <see cref="Span"/>.
        /// </summary>
        /// <param name="position">The <see cref="Position"/> to check.</param>
        /// <returns>True, if the <see cref="Position"/> is contained in this <see cref="Span"/>.</returns>
        public bool Contains(Position position) => Start <= position && position < End;

        /// <summary>
        /// Checks if this <see cref="Span"/> intersects with another one.
        /// </summary>
        /// <param name="other">The other <see cref="Span"/> to check intersection with.</param>
        /// <returns>True, if the two <see cref="Span"/>s intersect.</returns>
        public bool Intersects(Span other) => !(Start >= other.End || other.Start >= End);
    }
}
