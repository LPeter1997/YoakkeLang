using System.Collections.Generic;
using System.Text;

namespace Yoakke.Text
{
    /// <summary>
    /// A <see cref="StringBuilder"/>-like utility, that keeps track of the <see cref="Position"/>
    /// while building the string.
    /// </summary>
    public class SourceBuilder
    {
        public int Capacity
        {
            get => builder.Capacity;
            set => builder.Capacity = value;
        }
        public char this[int index]
        {
            get => builder[index];
            set => builder[index] = value;
        }
        public int Length
        {
            get => builder.Length;
            set => builder.Length = value;
        }
        public int MaxCapacity => builder.MaxCapacity;

        /// <summary>
        /// The current <see cref="Position"/> of the writing cursor.
        /// </summary>
        public Position Position => cursor.Position;

        private Cursor cursor = new Cursor();
        private StringBuilder builder = new StringBuilder();
        private int cursorIndex;

        public Span Append(object? value)
        {
            builder.Append(value);
            return UpdateCursor();
        }

        public Span AppendLine(string value = "")
        {
            builder.AppendLine(value);
            return UpdateCursor();
        }

        public Span AppendJoin<T>(string separator, IEnumerable<T> enumerable)
        {
            builder.AppendJoin(separator, enumerable);
            return UpdateCursor();
        }

        public Span AppendJoin<T>(char separator, IEnumerable<T> enumerable)
        {
            builder.AppendJoin(separator, enumerable);
            return UpdateCursor();
        }

        public void Clear()
        {
            builder.Clear();
            cursor.Reset();
        }

        public override string ToString() => builder.ToString();

        private Span UpdateCursor()
        {
            var start = cursor.Position;
            for (; cursorIndex < builder.Length; ++cursorIndex)
            {
                cursor.Append(builder[cursorIndex]);
            }
            var end = cursor.Position;
            return new Span(start, end);
        }
    }
}
