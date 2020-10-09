using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Text;

namespace Yoakke.C.Syntax.Cpp
{
    /// <summary>
    /// Basically does this:
    /// https://gcc.gnu.org/onlinedocs/gcc-10.2.0/cpp/Initial-processing.html#Initial-processing
    /// Up until line-continuations.
    /// </summary>
    public class CppTextReader : IEnumerator<PositionedChar>
    {
        /// <summary>
        /// Processes the given source.
        /// </summary>
        /// <param name="source">The source characters to process.</param>
        /// <returns>The processed characters that have no trigraphs or line-continuations.</returns>
        public static IEnumerable<PositionedChar> Process(IEnumerable<char> source)
        {
            var reader = new CppTextReader(source);
            while (reader.MoveNext()) yield return reader.Current;
        }

        /// <summary>
        /// Same as <see cref="Process(IEnumerable{char})"/>.
        /// </summary>
        public static IEnumerable<PositionedChar> Process(TextReader reader) => Process(reader.AsCharEnumerable());

        private PeekBuffer<char> source;
        private Cursor cursor = new Cursor();
        private PositionedChar? current;

        public PositionedChar Current
        {
            get
            {
                if (current == null) throw new InvalidOperationException();
                return current.Value;
            }
        }
        object IEnumerator.Current => Current;

        /// <summary>
        /// Initializes a new <see cref="CppTextReader"/>.
        /// </summary>
        /// <param name="source">The source to read characters from.</param>
        public CppTextReader(IEnumerable<char> source)
        {
            this.source = new PeekBuffer<char>(source);
        }

        /// <summary>
        /// Initializes a new <see cref="CppTextReader"/>.
        /// </summary>
        /// <param name="reader">The reader to read characters from.</param>
        public CppTextReader(TextReader reader)
            : this(reader.AsCharEnumerable())
        {
        }

        public bool MoveNext()
        {
            SkipBlanks();
            current = null;
            int toConsume = 0;
            // Check for trigraph
            if (Matches("??"))
            {
                // Possible trigraph
                char? result = TranslateTrigraph(Peek(2));
                if (result != null)
                {
                    // It is a trigraph
                    current = new PositionedChar(cursor.Position, result.Value);
                    toConsume = 3;
                }
            }
            if (current == null)
            {
                // Just a single character
                current = new PositionedChar(cursor.Position, Peek(0));
                toConsume = 1;
            }
            if (current.Value.Char == '\0') current = null;
            Consume(toConsume);
            return current != null;
        }

        public void Reset() => throw new NotSupportedException();

        public void Dispose() { }

        private void SkipBlanks()
        {
            while (true)
            {
                char? peek = null;
                int toConsume = 0;
                // Check for trigraphs
                if (Matches("??"))
                {
                    // Possible trigraph
                    char? result = TranslateTrigraph(Peek(2));
                    if (result != null)
                    {
                        // It is a trigraph
                        peek = result.Value;
                        toConsume = 3;
                    }
                }
                if (peek == null)
                {
                    // Just a single character
                    peek = Peek(0);
                    toConsume = 1;
                }
                // Check for a line continuation
                if (peek == '\\' && IsEndOfLine(toConsume, out toConsume))
                {
                    // End of line, try a next character
                    Consume(toConsume);
                    continue;
                }
                // Non-blank
                return;
            }
        }

        private static char? TranslateTrigraph(char ch) => ch switch
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
            var peekContent = string.Concat(source.Buffer.Take(len));
            source.Consume(len);
            cursor.Append(peekContent);
        }

        private bool Matches(string str)
        {
            Peek(str.Length);
            return source.Buffer
                .Take(str.Length)
                .SequenceEqual(str);
        }

        private char Peek(int amount, char eof = '\0') => source.PeekOrDefault(amount, eof);
    }
}
