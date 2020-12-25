using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Text
{
    /// <summary>
    /// Utility to keep track of visual column index by resolving tabs.
    /// </summary>
    public class LineCursor
    {
        /// <summary>
        /// The current visual column (0 based).
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// The tab size.
        /// </summary>
        public int TabSize { get; set; } = 4;

        /// <summary>
        /// Appends a character.
        /// </summary>
        /// <param name="ch">The character to append.</param>
        /// <param name="advance">The number of visual columns advanced.</param>
        /// <returns>True, if this character was a tab.</returns>
        public bool Append(char ch, out int advance)
        {
            bool isTab = false;
            advance = 0;
            if (ch == '\t')
            {
                advance = TabSize - Column % TabSize;
                isTab = true;
            }
            else if (!char.IsControl(ch))
            {
                advance = 1;
            }
            Column += advance;
            return isTab;
        }

        /// <summary>
        /// Same as <see cref="Append(char)"/>, without the out parameter.
        /// </summary>
        public bool Append(char ch) => Append(ch, out var _);
    }
}
