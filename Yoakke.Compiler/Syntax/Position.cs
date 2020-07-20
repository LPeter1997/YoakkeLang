using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Yoakke.Compiler.Syntax
{
    /// <summary>
    /// Position in the souce text.
    /// </summary>
    public readonly struct Position : IEquatable<Position>
    {
        /// <summary>
        /// The <see cref="Source"/> this <see cref="Position"/> belongs to.
        /// </summary>
        public readonly Source Source;
        /// <summary>
        /// Zero-based line index.
        /// </summary>
        public readonly int Line;
        /// <summary>
        /// Zero-based column index.
        /// </summary>
        public readonly int Column;

        /// <summary>
        /// Initializes a new <see cref="Position"/>.
        /// </summary>
        /// <param name="source">The <see cref="Source"/> this position belongs to.</param>
        /// <param name="line">The zero-based line index.</param>
        /// <param name="column">The zero-based column index.</param>
        public Position(Source source, int line, int column)
        {
            Source = source;
            Line = line;
            Column = column;
        }

        public override bool Equals(object? obj) =>
            obj != null && obj is Position p && Equals(p);

        public bool Equals(Position o) =>
                   Line == o.Line 
                && Column == o.Column 
                && Source.Path == o.Source.Path;

        public override int GetHashCode() =>
            HashCode.Combine(Source.Path, Line, Column);

        public static bool operator ==(Position p1, Position p2) => p1.Equals(p2);
        public static bool operator !=(Position p1, Position p2) => !(p1 == p2);

        /// <summary>
        /// Creates a <see cref="Position"/> that's advanced in the current line by the given amount.
        /// </summary>
        /// <param name="amount">The amount to advance in the current line.</param>
        /// <returns>The position in the same line, advanced by columns.</returns>
        public Position Advance(int amount = 1) =>
            new Position(source: Source, line: Line, column: Column + amount);

        /// <summary>
        /// Creates a <see cref="Position"/> that points to the first character of the next line.
        /// </summary>
        /// <returns>A position in the next line's first character.</returns>
        public Position Newline() =>
            new Position(source: Source, line: Line + 1, column: 0);

        public override string ToString() =>
            $"in file '{Source.Path}': line {Line}, column {Column}";
    }
}
