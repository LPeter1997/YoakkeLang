using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Syntax
{
    /// <summary>
    /// Represents source code that can be accessed line-by-line.
    /// </summary>
    struct Source
    {
        /// <summary>
        /// The source text itself.
        /// </summary>
        public readonly string Text;
        /// <summary>
        /// The number of lines in the source.
        /// </summary>
        public int LineCount => lineStarts.Count;

        private readonly List<int> lineStarts;

        /// <summary>
        /// Initializes a new <see cref="Source"/> with the given source text.
        /// </summary>
        /// <param name="text">The source text to use.</param>
        public Source(string text)
        {
            Text = NormalizeNewline(text);
            lineStarts = LineStarts(Text);
        }

        /// <summary>
        /// Returns the part of the source code relevant to the line with the given index.
        /// </summary>
        /// <param name="index">The index of the line.</param>
        /// <returns>The <see cref="ReadOnlySpan{char}"/> of the relevant source of the line.</returns>
        public ReadOnlySpan<char> Line(int index)
        {
            if (index >= lineStarts.Count) return Text.AsSpan(Text.Length);
            if (index + 1 >= lineStarts.Count) return Text.AsSpan(lineStarts[index]);
            return Text.AsSpan(lineStarts[index], lineStarts[index + 1] - lineStarts[index]);
        }

        private static string NormalizeNewline(string source) =>
            source.Replace("\r\n", "\n").Replace("\r", "\n");

        private static List<int> LineStarts(string source)
        {
            var result = new List<int> { 0 };
            for (int i = 0; i < source.Length; ++i)
            {
                if (source[i] == '\n') result.Add(i + 1);
            }
            return result;
        }
    }
}
