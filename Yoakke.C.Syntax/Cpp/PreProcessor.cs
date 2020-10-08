using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.C.Syntax.Cpp
{
    /// <summary>
    /// Pre-processes a sequence of <see cref="Token"/>s, returning the pre-processed <see cref="Token"/>s.
    /// </summary>
    public class PreProcessor
    {
        private SourceFile source;
        private IReadOnlyList<Token> tokens;
        private int tokenIndex = -1;
        private List<Token> peekBuffer = new List<Token>();

        /// <summary>
        /// Pre-processes the given <see cref="Token"/>s.
        /// </summary>
        /// <param name="tokens">The <see cref="IEnumerable{Token}"/> to pre-process.</param>
        /// <returns>The <see cref="IEnumerable{Token}"/> of the pre-processed input.</returns>
        public static IEnumerable<Token> Process(IEnumerable<Token> tokens)
        {
            var pp = new PreProcessor(tokens);

            while (true)
            {
                var result = pp.Next();
                bool hasEnd = false;
                foreach (var t in result)
                {
                    yield return t;
                    if (t.Type == TokenType.End) hasEnd = true;
                }
                if (hasEnd) break;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="PreProcessor"/>.
        /// </summary>
        /// <param name="tokens">The <see cref="IEnumerable{Token}"/>s to pre-process.</param>
        public PreProcessor(IEnumerable<Token> tokens)
        {
            this.tokens = tokens.ToArray();
            Debug.Assert(this.tokens.Count > 0);
            source = this.tokens.First().PhysicalSpan.Source;
        }

        /// <summary>
        /// Retrieves the next batch of <see cref="Token"/>s.
        /// Can return multiple <see cref="Token"/>s in case of macro expansions.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Token> Next()
        {
            if (ParseDirective(out var name, out var args))
            {
                Console.WriteLine($"Directive: {name}");
                foreach (var arg in args)
                {
                    Console.WriteLine($"    arg {arg.Value} - {arg.Type}");
                }
            }
            else
            {
                // TODO
                var t = Consume();
                Console.WriteLine($"Just a token: {t.Value} - {t.Type}");
                yield return t;
            }
        }

        // Parsers

        private bool ParseDirective([MaybeNullWhen(false)] out string name, [MaybeNullWhen(false)] out IList<Token> args)
        {
            var peek = Peek();
            var directiveLine = peek.LogicalSpan.End.Line;
            var peekPrev = PeekPrev();
            if (   peek.Type == TokenType.Hash 
                && (peekPrev == null || peekPrev.LogicalSpan.End.Line != directiveLine))
            {
                // It's a hash on a fresh line, could be a directive
                var ident = Peek(1);
                if (ident.Type == TokenType.Identifier)
                {
                    // Confirmed directive
                    name = ident.Value;
                    Consume(2);
                    args = new List<Token>();
                    for (; Peek().LogicalSpan.Start.Line == directiveLine; args.Add(Consume())) ;
                    return true;
                }
            }
            name = null;
            args = null;
            return false;
        }

        // Primitives for parsing //////////////////////////////////////////////

        private Token Consume()
        {
            var result = Peek();
            peekBuffer.RemoveAt(0);
            return result;
        }

        private void Consume(int amount) => peekBuffer.RemoveRange(0, amount);

        private Token Peek(int amount = 0)
        {
            while (peekBuffer.Count <= amount)
            {
                peekBuffer.Add(NextPrimitive());
            }
            return peekBuffer[amount];
        }

        private Token? PeekPrev()
        {
            if (tokenIndex <= 0) return null;
            return tokens[tokenIndex - 1];
        }

        private Token NextPrimitive()
        {
            if (tokenIndex + 1 < tokens.Count) ++tokenIndex;
            return tokens[tokenIndex];
        }
    }
}
