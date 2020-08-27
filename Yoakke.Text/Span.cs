using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Text
{
    /// <summary>
    /// An 2D interval of text positions.
    /// </summary>
#pragma warning disable CS0660, CS0661 // No reason to override Equals or GetHashCode
    public readonly struct Span
#pragma warning restore CS0660, CS0661
    {
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
        /// <param name="start">The first <see cref="Position"/> that's inside this span.</param>
        /// <param name="end">The first <see cref="Position"/> after this span.</param>
        public Span(Position start, Position end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Initializes a new <see cref="Span"/>.
        /// </summary>
        /// <param name="start">The first <see cref="Position"/> that's inside this span.</param>
        /// <param name="length">The length of this span.</param>
        public Span(Position start, int length)
            : this(start, start.Advance(length))
        {
        }

        public static bool operator ==(Span s1, Span s2) => s1.Equals(s2);
        public static bool operator !=(Span s1, Span s2) => !(s1 == s2);

        /// <summary>
        /// Checks, if a given <see cref="Position"/> is within the bounds of this <see cref="Span"/>.
        /// </summary>
        /// <param name="position">The <see cref="Position"/> to check.</param>
        /// <returns>True, if the <see cref="Position"/> is contained in this <see cref="Span"/>.</returns>
        public bool Contains(Position position) => Start <= position && position < End;
    }
}
