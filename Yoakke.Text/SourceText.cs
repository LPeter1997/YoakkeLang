using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Yoakke.Text
{
    /// <summary>
    /// Represents a source text that originates from some path.
    /// </summary>
    public class SourceText : IEquatable<SourceText>
    {
        /// <summary>
        /// The path this file originates from.
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// The actual source text.
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// The number of lines in this source.
        /// </summary>
        public int LineCount => lineStarts.Count;

        private List<int> lineStarts = new List<int>();

        /// <summary>
        /// Initializes a new <see cref="SourceText"/>.
        /// </summary>
        /// <param name="path">The path the source originates from.</param>
        /// <param name="text">The source text.</param>
        public SourceText(string path, string text)
        {
            Path = path;
            Text = text;
            CalculateLineStarts();
        }

        /// <summary>
        /// Retrieves a span for the given line.
        /// </summary>
        /// <param name="index">The index of the line.</param>
        /// <returns>The span containing the given line's text.</returns>
        public ReadOnlySpan<char> GetLine(int index)
        {
            var startIndex = LineIndexToTextIndex(index);
            var endIndex = LineIndexToTextIndex(index + 1);
            return Text.AsSpan().Slice(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Does an edit on the contents of this source file.
        /// </summary>
        /// <param name="range">The range to replace.</param>
        /// <param name="newContent">The new contents of the range.</param>
        public void Edit(Range range, string newContent)
        {
            var from = IndexOf(range.Start);
            var to = IndexOf(range.End);
            Text = $"{Text.Substring(0, from)}{newContent}{Text.Substring(to)}";
            // NOTE: This is quite inefficient but will work for now
            // We could redo the lines either from this point
            // Or we could make it all lazy as required
            lineStarts.Clear();
            CalculateLineStarts();
        }

        /// <summary>
        /// Calculates the 1D index in the source text based on a position.
        /// </summary>
        /// <param name="position">The position to get the index for.</param>
        /// <returns>The corresponding index in the source <see cref="Text"/>.</returns>
        public int IndexOf(Position position)
        {
            var lineIndex = LineIndexToTextIndex(position.Line);
            var nextLineIndex = LineIndexToTextIndex(position.Line + 1);
            var lineLength = nextLineIndex - lineIndex;
            var columnOffset = Math.Min(position.Column, lineLength);
            return lineIndex + columnOffset;
        }

        public override bool Equals(object? obj) => Equals(obj as SourceText);
        public bool Equals(SourceText? other) => 
            other is not null && other.Path == Path && other.Text == Text;
        public override int GetHashCode() => HashCode.Combine(Path, Text);

        private int LineIndexToTextIndex(int index) => index >= lineStarts.Count 
            ? Text.Length
            : lineStarts[index];
        
        private void CalculateLineStarts()
        {
            var cursor = new Cursor();
            int lastLine = 0;
            lineStarts.Add(0);
            for (int i = 0; i < Text.Length; ++i)
            {
                cursor.Append(Text[i]);
                // If our column is at the start, there's a possibility for a newline
                if (cursor.Position.Column == 0)
                {
                    if (cursor.Position.Line != lastLine)
                    {
                        // Yes, this is a different line
                        lineStarts.Add(i + 1);
                        lastLine = cursor.Position.Line;
                    }
                    else
                    {
                        // No, the line is the same
                        // This could be a windows newline or some control character
                        // Update it
                        lineStarts[lineStarts.Count - 1] = i + 1;
                    }
                }
            }
        }
    }
}
