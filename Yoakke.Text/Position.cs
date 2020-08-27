using System;

namespace Yoakke.Text
{
    /// <summary>
    /// Represents 2D position inside some text.
    /// </summary>
#pragma warning disable CS0660, CS0661 // No reason to override Equals or GetHashCode
    public readonly struct Position
#pragma warning restore CS0660, CS0661
    {
        /// <summary>
        /// The 0-based line index.
        /// </summary>
        public readonly int Line;
        /// <summary>
        /// The 0-based column index.
        /// </summary>
        public readonly int Column;

        /// <summary>
        /// Initializes a new <see cref="Position"/>.
        /// </summary>
        /// <param name="line">The 0-based line index.</param>
        /// <param name="column">The 0-based column index.</param>
        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public static bool operator ==(Position p1, Position p2) => p1.Equals(p2);
        public static bool operator !=(Position p1, Position p2) => !(p1 == p2);
        public static bool operator <(Position p1, Position p2) => 
            p1.Line < p2.Line || (p1.Line == p2.Line && p1.Column < p2.Column);
        public static bool operator >(Position p1, Position p2) => p2 < p1;
        public static bool operator <=(Position p1, Position p2) => !(p1 > p2);
        public static bool operator >=(Position p1, Position p2) => !(p1 < p2);

        public override string ToString() => $"line {Line + 1}, column {Column + 1}";

        /// <summary>
        /// Creates a <see cref="Position"/> that's advanced in the current line by the given amount.
        /// </summary>
        /// <param name="amount">The amount to advance in the current line.</param>
        /// <returns>The <see cref="Position"/> in the same line, advanced by columns.</returns>
        public Position Advance(int amount = 1) => new Position(line: Line, column: Column + amount);

        /// <summary>
        /// Creates a <see cref="Position"/> that points to the first character of the next line.
        /// </summary>
        /// <returns>A <see cref="Position"/> in the next line's first character.</returns>
        public Position Newline() => new Position(line: Line + 1, column: 0);
    }
}
