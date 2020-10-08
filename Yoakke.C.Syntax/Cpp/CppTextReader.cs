using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.C.Syntax.Cpp
{
    // TODO: The functionality here (buffering) is a duplicate of what the yoakke syntax lexer does
    // Maybe we should factor it out

    /// <summary>
    /// Basically does this:
    /// https://gcc.gnu.org/onlinedocs/gcc-10.2.0/cpp/Initial-processing.html#Initial-processing
    /// Up until line continuations.
    /// </summary>
    public class CppTextReader
    {
        private TextReader reader;
        private Cursor cursor = new Cursor();
        private StringBuilder peekBuffer = new StringBuilder();

        /// <summary>
        /// The current <see cref="Position"/> of this reader.
        /// </summary>
        public Position Position => cursor.Position;

        /// <summary>
        /// Initializes a new <see cref="CppTextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read characters from.</param>
        public CppTextReader(TextReader reader)
        {
            this.reader = reader;
        }

        /// <summary>
        /// Reads in the next character, escaping trigraphs and skipping line continuations.
        /// </summary>
        /// <returns></returns>
        public char? Next()
        {
            while (true)
            {
                char? toReturn = null;
                int toConsume = 0;
                // Check for trigraphs
                if (Matches("??"))
                {
                    // Possible trigraph
                    char? result = Peek(2) switch
                    {
                        '(' => '[',
                        ')' => ']',
                        '<' => '{',
                        '>' => '}',
                        '=' => '#',
                        '/' => '\\',
                        '\'' => '^',
                        '!' => '|',
                        '-' => '~',
                        _ => null,
                    };
                    if (result != null)
                    {
                        // It is a trigraph
                        toReturn = result.Value;
                        toConsume = 3;
                    }
                }
                if (toReturn == null)
                {
                    // Just a single character
                    toReturn = Peek(0);
                    toConsume = 1;
                }
                // Check for a line continuation
                if (toReturn == '\\' && IsEndOfLine(toConsume, out toConsume))
                {
                    // End of line, try a next character
                    Consume(toConsume);
                    continue;
                }
                // Check of EOF
                if (toReturn == '\0') return null;
                // Not EOF, eat it
                Consume(toConsume);
                return toReturn.Value;
            }
        }

        private bool IsEndOfLine(int offset, out int toConsume)
        {
            toConsume = offset;
            // Skip trailing spaces
            for (; Peek(offset) == ' '; ++offset) ;
            var peek1 = Peek(offset);
            if (peek1 == '\r' && Peek(offset + 1) == '\n')
            {
                // Continuation with Windows-style newline
                toConsume = offset + 2;
                return true;
            }
            if (peek1 == '\r' || peek1 == '\n')
            {
                // Continuation with Unix or OS-X 9-style newline
                toConsume = offset + 1;
                return true;
            }
            return false;
        }

        private void Consume(int len)
        {
            var peekContent = peekBuffer.ToString().Substring(0, len);
            peekBuffer.Remove(0, len);
            cursor.Append(peekContent);
        }

        private bool Matches(string str)
        {
            Peek(str.Length);
            for (int i = 0; i < str.Length; ++i)
            {
                if (peekBuffer[i] != str[i]) return false;
            }
            return true;
        }

        private char Peek(int amount, char eof = '\0')
        {
            while (peekBuffer.Length <= amount)
            {
                var code = reader.Read();
                var ch = code == -1 ? eof : (char)code;
                peekBuffer.Append(ch);
            }
            return peekBuffer[amount];
        }
    }
}
