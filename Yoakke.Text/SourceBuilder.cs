using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Append(object? value)
        {
            builder.Append(value);
            UpdateCursor();
        }

        public void AppendLine(string value = "")
        {
            builder.AppendLine(value);
            UpdateCursor();
        }

        public void AppendJoin<T>(string separator, IEnumerable<T> enumerable)
        {
            builder.AppendJoin(separator, enumerable);
            UpdateCursor();
        }

        public void AppendJoin<T>(char separator, IEnumerable<T> enumerable)
        {
            builder.AppendJoin(separator, enumerable);
            UpdateCursor();
        }

        public void Clear()
        {
            builder.Clear();
            cursor.Reset();
        }

        public override string ToString() => builder.ToString();

        private void UpdateCursor()
        {
            for (; cursorIndex < builder.Length; ++cursorIndex)
            {
                cursor.Append(builder[cursorIndex]);
            }
        }
    }
}
