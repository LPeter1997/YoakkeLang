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
        private Stack<ControlInfo> controlFlowStack = new Stack<ControlInfo>();

        /// <summary>
        /// Initializes a new <see cref="PreProcessor"/>.
        /// </summary>
        /// <param name="tokens">The <see cref="IEnumerable{Token}"/>s to pre-process.</param>
        public PreProcessor(IEnumerable<Token> tokens)
        {
            controlFlowStack.Push(new ControlInfo { Keep = true, Satisfied = true });
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
                var control = controlFlowStack.Peek();
                
                if (ParseDirective(out var directiveName, out var directiveArgs))
                {
                    switch (directiveName)
                    {
                    // Control flow

                    case "if":
                    case "ifdef":
                    case "ifndef":
                    {
                        if (!control.Keep)
                        {
                            controlFlowStack.Push(new ControlInfo { Keep = false, Satisfied = true });
                            break;
                        }
                        var parser = new SubParser(directiveArgs);
                        bool condition;
                        if (directiveName == "if")
                        {
                            condition = EvaluateCondition(parser);
                        }
                        else
                        {
                            var ident = parser.Expect(TokenType.Identifier);
                            condition = IsDefined(ident.Value);
                            if (directiveName == "ifndef") condition = !condition;
                        }
                        controlFlowStack.Push(new ControlInfo { Keep = condition, Satisfied = condition });
                    }
                    break;

                    case "elif":
                    {
                        controlFlowStack.Pop();
                        if (control.Satisfied)
                        {
                            // The last branch was already satisfied, we don't want to keep from now on
                            controlFlowStack.Push(new ControlInfo { Keep = false, Satisfied = true });
                            break;
                        }
                        // The last branch was not satisfied
                        var parser = new SubParser(directiveArgs);
                        bool condition = EvaluateCondition(parser);
                        controlFlowStack.Push(new ControlInfo { Keep = condition, Satisfied = condition });
                    }
                    break;

                    case "else":
                        controlFlowStack.Pop();
                        if (control.Satisfied)
                        {
                            // We were satisfied the last time
                            controlFlowStack.Push(new ControlInfo { Keep = false, Satisfied = true });
                            break;
                        }
                        // We were not satisfied, the else needs to be included
                        controlFlowStack.Push(new ControlInfo { Keep = true, Satisfied = true });
                        break;

                    case "endif":
                        controlFlowStack.Pop();
                        break;

                    default:
                        if (!control.Keep) break;
                        // Other directives
                        switch (directiveName)
                        {
                        case "define": 
                            ParseUserMacro(directiveArgs);
                            break;

                        case "undef":
                            var parser = new SubParser(directiveArgs);
                            var name = parser.Expect(TokenType.Identifier);
                            Undefine(name.Value);
                            break;

                        default: 
                            throw new NotImplementedException($"Unknown directive '{directiveName}'!");
                        }
                        break;
                    }
                }
                else if (control.Keep)
                {
                    if (ParseMacroCall(out var macro, out var callSiteIdent, out var macroArgs))
                    {
                        var args = ParseMacroArgs(macro, macroArgs);
                        var result = macro.Expand(callSiteIdent, args);
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
                else
                {
                    Consume();
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
                // We need the extra crud here because of keywords...
                if (SubParser.IsIdent(ident))
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
            if (SubParser.IsIdent(peek) && macros.TryGetValue(peek.Value, out macro))
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

        private void ParseUserMacro(IList<Token> tokens)
        {
            var parser = new SubParser(tokens);
            var name = parser.Expect(TokenType.Identifier);

            bool needsParens = false;
            bool isVariadic = false;
            var parametersSet = new HashSet<string>();
            var parameters = new List<string>();
            var expansion = new List<Token>();

            // Argument list
            if (parser.Matches(TokenType.OpenParen))
            {
                needsParens = true;
                if (!parser.Matches(TokenType.CloseParen))
                {
                    while (true)
                    {
                        if (parser.Matches(TokenType.Identifier, out var param))
                        {
                            if (!parametersSet.Add(param.Value))
                            {
                                // TODO
                                throw new NotImplementedException("Duplicate parameter name");
                            }
                            parameters.Add(param.Value);
                            if (!parser.Matches(TokenType.Comma)) break;
                        }
                        else if (parser.Matches(TokenType.Ellipsis))
                        {
                            // Must be the last one
                            isVariadic = true;
                            break;
                        }
                        else
                        {
                            // TODO
                            throw new NotImplementedException("Unexpected argument");
                        }
                    }
                    parser.Expect(TokenType.CloseParen);
                }
            }

            // Expansion
            expansion.AddRange(parser.Remaining());

            var macro = new UserMacro(needsParens, isVariadic, parameters, expansion);
            Define(name.Value, macro);
        }

        // Pair of named and variadic args
        private static IDictionary<string, IList<Token>> ParseMacroArgs(Macro macro, IList<Token> args)
        {
            // We don't need to care about caller parenthesis here
            var namedArgs = new Dictionary<string, IList<Token>>();
            var variadicArgs = new List<Token>();
            if (macro.Parameters.Count == 1 && args.Count == 0)
            {
                // A special case, we passed no args but the macro expects a single parameter
                // We pass in an empty argument
                namedArgs[macro.Parameters.First()] = new List<Token>();
            }
            else
            {
                var parser = new SubParser(args);
                if (!parser.IsEnd)
                {
                    bool lastComma = false;
                    bool first = true;
                    // First we need to fill in the named argument list
                    foreach (var argName in macro.Parameters)
                    {
                        first = false;
                        // A macro argument is simply everything until a balanced ',' or ')'
                        var argValue = ParseMacroArg(parser);
                        namedArgs.Add(argName, argValue);
                        lastComma = parser.Matches(TokenType.Comma);
                        if (!lastComma) break;
                    }
                    // Continue parsing, if there was a comma, these go to the variadic args
                    while (first || lastComma)
                    {
                        first = false;
                        var argValue = ParseMacroArg(parser);
                        variadicArgs.AddRange(argValue);
                        lastComma = parser.Matches(TokenType.Comma);
                    }
                }
                // Check if we collected enough arguments
                if (namedArgs.Count < macro.Parameters.Count)
                {
                    // TODO
                    throw new NotImplementedException("Not enough macro arguments!");
                }
                // Check if we even required variadic args
                if (!macro.IsVariadic && variadicArgs.Count > 0)
                {
                    // TODO
                    throw new NotImplementedException("Too many macro arguments!");
                }
            }
            if (macro.IsVariadic)
            {
                namedArgs["__VA_ARGS__"] = variadicArgs;
            }
            return namedArgs;
        }

        private static IList<Token> ParseMacroArg(SubParser parser) =>
            parser.ParseBalancedUntil(TokenType.Comma, TokenType.CloseParen).ToList();

        private static bool EvaluateCondition(SubParser parser)
        {
            // TODO
            throw new NotImplementedException();
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

        // Helper for control flow

        private class ControlInfo
        {
            public bool Keep { get; set; }
            public bool Satisfied { get; set; }
        }

        // A helper parser structure ///////////////////////////////////////////

        private class SubParser
        {
            public bool IsEnd => offset >= tokens.Count;

            private IList<Token> tokens;
            private int offset;

            public SubParser(IList<Token> tokens)
            {
                this.tokens = tokens;
            }

            public IEnumerable<Token> Remaining() => tokens.Skip(offset);

            public IEnumerable<Token> ParseBalancedUntil(params TokenType[] tts)
            {
                while (true)
                {
                    if (IsEnd || tts.Any(tt => Peek(tt))) break;
                    if (Peek(TokenType.OpenParen))
                    {
                        foreach (var t in ParseBalancedGroup()) yield return t;
                    }
                    else
                    {
                        yield return Consume();
                    }
                }
            }

            public IEnumerable<Token> ParseBalancedGroup()
            {
                yield return Expect(TokenType.OpenParen);
                int depth = 1;
                while (depth > 0)
                {
                    if (IsEnd)
                    {
                        throw new NotImplementedException("Unbalanced parenthesis!");
                    }
                    var token = Consume();
                    yield return token;
                    if (token.Type == TokenType.OpenParen) ++depth;
                    else if (token.Type == TokenType.CloseParen) --depth;
                }
            }

            public Token Expect(TokenType tt)
            {
                if (!Matches(tt, out var token))
                {
                    throw new NotImplementedException($"Expected {tt}");
                }
                return token;
            }

            public bool Matches(TokenType tt) => Matches(tt, out var _);

            public bool Matches(TokenType tt, [MaybeNullWhen(false)] out Token token)
            {
                if (Peek(tt))
                {
                    token = Consume();
                    return true;
                }
                token = null;
                return false;
            }

            public bool Peek(TokenType tt)
            {
                if (tokens.Count <= offset) return false;
                var token = tokens[offset];
                if (tt == TokenType.Identifier) return IsIdent(token);
                return token.Type == tt;
            }

            public Token Consume() => tokens[offset++];

            public static bool IsIdent(Token token) =>
                   token.Type == TokenType.Identifier
                || (token.Value.Length > 0 && !char.IsDigit(token.Value.First()) && token.Value.All(ch => IsIdent(ch)));

            private static bool IsIdent(char ch) => char.IsLetterOrDigit(ch) || ch == '_';
        }
    }
}
