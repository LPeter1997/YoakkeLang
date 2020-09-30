using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Yoakke.Text
{
    /// <summary>
    /// Represents a source text that originates from some path.
    /// </summary>
    public class SourceFile : IEquatable<SourceFile>
    {
        /// <summary>
        /// The path this file originates from.
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// The full path of the file.
        /// </summary>
        public readonly string FullPath;
        /// <summary>
        /// True, if this is a real source on the hard-disk.
        /// </summary>
        public bool IsReal => File.Exists(FullPath);
        /// <summary>
        /// The actual source text.
        /// </summary>
        public readonly string Text;
        /// <summary>
        /// The number of lines in this source.
        /// </summary>
        public int LineCount => lineStarts.Count;

        private IList<int> lineStarts = new List<int>();

        /// <summary>
        /// Initializes a new <see cref="SourceFile"/>.
        /// </summary>
        /// <param name="path">The path the source originates from.</param>
        /// <param name="text">The source text.</param>
        public SourceFile(string path, string text)
        {
            Path = path;
            FullPath = System.IO.Path.GetFullPath(path);
            Text = text;
            CalculateLineStarts();
        }

        /// <summary>
        /// Retrieves a span for the given line.
        /// </summary>
        /// <param name="index">The index of the line.</param>
        /// <returns>The span containing the given line's text.</returns>
        public ReadOnlySpan<char> Line(int index)
        {
            var startIndex = LineIndexToTextIndex(index);
            var endIndex = LineIndexToTextIndex(index + 1);
            return Text.AsSpan().Slice(startIndex, endIndex - startIndex);
        }

        public override bool Equals(object? obj) => obj is SourceFile f && Equals(f);
        public bool Equals(SourceFile? other) =>
            other != null && other.FullPath == FullPath;
        public override int GetHashCode() => FullPath.GetHashCode();

        private int LineIndexToTextIndex(int index) => index >= lineStarts.Count 
            ? Text.Length
            : lineStarts[index];
        
        private void CalculateLineStarts()
        {
            var cursor = new Cursor();
            lineStarts.Add(0);
            for (int i = 0; i < Text.Length; ++i)
            {
                cursor.Append(Text[i]);
                // If our column is at the start, there's a possibility for a newline
                if (cursor.Position.Column == 0)
                {
                    if (cursor.Position.Line != lineStarts.Last())
                    {
                        // Yes, this is a different line
                        lineStarts.Add(i);
                    }
                    else
                    {
                        // No, the line is the same
                        // This could be a windows newline or some control character
                        // Update it
                        lineStarts[lineStarts.Count - 1] = i;
                    }
                }
            }
        }
    }
}
