namespace Yoakke.Text
{
    /// <summary>
    /// Utility to keep track of the <see cref="Position"/> by feeding in characters from a source.
    /// </summary>
    public class Cursor
    {
        /// <summary>
        /// The current <see cref="Position"/>.
        /// </summary>
        public Position Position { get; private set; }

        private bool lastReturn;

        /// <summary>
        /// Appends the given character, moving the <see cref="Position"/> of this <see cref="Cursor"/>.
        /// </summary>
        /// <param name="c">The character to append.</param>
        public void Append(char c)
        {
            if (c == '\r')
            {
                // Can be an OS-X 9 line-break
                Position = Position.Newline();
                lastReturn = true;
            }
            else if (c == '\n')
            {
                // if lastReturn, then it was a Windows \r\n, we already advanced a line for that
                // Unix-style
                if (!lastReturn) Position = Position.Newline();
                lastReturn = false;
            }
            else
            {
                lastReturn = false;
                // Only advance, if this is a visual character
                if (!char.IsControl(c) || c == '\t') Position = Position.Advance();
            }
        }

        /// <summary>
        /// Appends the given string, moving the <see cref="Position"/> of this <see cref="Cursor"/>.
        /// Basically appends each character of the string.
        /// </summary>
        /// <param name="str">The string to append.</param>
        public void Append(string str)
        {
            foreach (var c in str) Append(c);
        }

        /// <summary>
        /// Resets the <see cref="Position"/> to 0, 0.
        /// </summary>
        public void Reset() => Position = new Position();
    }
}
