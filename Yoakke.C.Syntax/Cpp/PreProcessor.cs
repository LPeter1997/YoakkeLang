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
        private SourceFile source = new SourceFile("unknown", string.Empty);
        private IReadOnlyList<Token> tokens = new Token[] { };
        private int tokenIndex = -1;
        private List<Token> peekBuffer = new List<Token>();
        private IDictionary<string, Macro> macros = new Dictionary<string, Macro>();

        /// <summary>
        /// Initializes a new <see cref="PreProcessor"/>.
        /// </summary>
        /// <param name="tokens">The <see cref="IEnumerable{Token}"/>s to pre-process.</param>
        public PreProcessor(IEnumerable<Token> tokens)
        {
        }

        /// <summary>
        /// Defines a <see cref="Macro"/>.
        /// </summary>
        /// <param name="name">The name to define.</param>
        /// <param name="macro">The <see cref="Macro"/> to assiciate the name with.</param>
        public void Define(string name, Macro macro) => macros[name] = macro;

        /// <summary>
        /// Undefines a <see cref="Macro"/>.
        /// </summary>
        /// <param name="name">The name to undefine.</param>
        public void Undefine(string name) => macros.Remove(name);

        /// <summary>
        /// True, if the given <see cref="Macro"/> is defined.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>True, if a <see cref="Macro"/> is defined under the given name.</returns>
        public bool IsDefined(string name) => macros.ContainsKey(name);

        /// <summary>
        /// Pre-processes the whole input.
        /// </summary>
        /// <param name="tokens">The <see cref="IEnumerable{Token}"/>s to pre-process.</param>
        /// <returns>The <see cref="IEnumerable{Token}"/> of the pre-processed input.</returns>
        public IEnumerable<Token> Process(IEnumerable<Token> tokens)
        {
            this.tokens = tokens.ToArray();
            Debug.Assert(this.tokens.Count > 0);
            source = this.tokens.First().PhysicalSpan.Source;

            while (true)
            {
                var t = Next();
                yield return t;
                if (t.Type == TokenType.End) break;
            }
        }

        private Token Next()
        {
            while (true)
            {
                if (ParseDirective(out var directiveName, out var directiveArgs))
                {
                    // TODO
                    Console.WriteLine($"Directive: {directiveName}");
                    foreach (var arg in directiveArgs)
                    {
                        Console.WriteLine($"    arg {arg.Value} - {arg.Type}");
                    }
                    throw new NotImplementedException();
                }
                else if (ParseMacroCall(out var macro, out var callSiteIdent, out var macroArgs))
                {
                    var (namedArgs, variadicArgs) = ParseMacroArgs(macro, macroArgs);
                    var result = macro.Expand(callSiteIdent, namedArgs, variadicArgs);
                    // Insert it to the beginning of the peek buffer
                    peekBuffer.InsertRange(0, result);
                }
                else
                {
                    // TODO
                    var t = Consume();
                    return t;
                }
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

        private bool ParseMacroCall(
            [MaybeNullWhen(false)] out Macro macro,
            [MaybeNullWhen(false)] out Token callSiteIdent,
            [MaybeNullWhen(false)] out IList<Token> args)
        {
            var peek = Peek();
            if (peek.Type == TokenType.Identifier && macros.TryGetValue(peek.Value, out macro))
            {
                // There's a macro with the given name
                callSiteIdent = peek;
                // If it requires no parenthesis, we are done
                if (!macro.NeedsParens)
                {
                    args = new List<Token>();
                    Consume();
                    return true;
                }
                else if (Peek(1).Type == TokenType.OpenParen)
                {
                    // Requires parenthesis and we are invoking it
                    // From now on everything else is an error
                    Consume(2);
                    args = new List<Token>();
                    int depth = 1;
                    while (true)
                    {
                        var next = Consume();
                        if (next.Type == TokenType.End)
                        {
                            // TODO
                            throw new NotImplementedException("Unclosed macro call");
                        }
                        if (next.Type == TokenType.CloseParen)
                        {
                            --depth;
                            if (depth == 0) break;
                        }
                        if (next.Type == TokenType.OpenParen) ++depth;
                        args.Add(next);
                    }
                    return true;
                }
            }
            macro = null;
            args = null;
            callSiteIdent = null;
            return false;
        }

        // Pair of named and variadic args
        private static (IDictionary<string, IList<Token>>, IList<IList<Token>>) ParseMacroArgs(Macro macro, IList<Token> args)
        {
            // We don't need to care about caller parenthesis here
            var namedArgs = new Dictionary<string, IList<Token>>();
            var variadicArgs = new List<IList<Token>>();
            if (macro.Parameters.Count == 1 && args.Count == 0)
            {
                // A special case, we passed no args but the macro expects a single parameter
                // We pass in an empty argument
                namedArgs[macro.Parameters.First()] = new List<Token>();
            }
            else
            {
                int offset = 0;
                bool lastComma = false;
                bool first = true;
                // First we need to fill in the named argument list
                foreach (var argName in macro.Parameters)
                {
                    first = false;
                    var argValue = ParseMacroArg(args, ref offset);
                    namedArgs.Add(argName, argValue);
                    lastComma = ParseComma(args, ref offset);
                    if (!lastComma) break;
                }
                // Check if we collected enough arguments
                if (namedArgs.Count < macro.Parameters.Count)
                {
                    // TODO
                    throw new NotImplementedException("Not enough macro arguments!");
                }
                // Continue parsing, if there was a comma, these go to the variadic args
                while (first || lastComma)
                {
                    first = false;
                    var argValue = ParseMacroArg(args, ref offset);
                    variadicArgs.Add(argValue);
                    lastComma = ParseComma(args, ref offset);
                }
                // Check if we even required variadic args
                if (!macro.IsVariadic && variadicArgs.Count > 0)
                {
                    // TODO
                    throw new NotImplementedException("Too many macro arguments!");
                }
            }
            return (namedArgs, variadicArgs);
        }

        private static IList<Token> ParseMacroArg(IList<Token> source, ref int offset)
        {
            var result = new List<Token>();
            while (offset < source.Count)
            {
                var t = source[offset];
                if (t.Type == TokenType.Comma) break;
                ++offset;
                result.Add(t);
                if (t.Type == TokenType.OpenParen)
                {
                    int depth = 1;
                    while (depth > 0)
                    {
                        if (offset >= source.Count)
                        {
                            // TODO
                            throw new NotImplementedException("Unclosed macro argument!");
                        }
                        t = source[offset++];
                        result.Add(t);
                        if (t.Type == TokenType.OpenParen) ++depth;
                        else if (t.Type == TokenType.CloseParen) --depth;
                    }
                }
            }
            return result;
        }

        private static bool ParseComma(IList<Token> source, ref int offset)
        {
            if (source.Count > offset && source[offset].Type == TokenType.Comma)
            {
                ++offset;
                return true;
            }
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
