using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Text;

namespace Yoakke.C.Syntax.Cpp
{
    /// <summary>
    /// Pre-processes a sequence of <see cref="Token"/>s, returning the pre-processed <see cref="Token"/>s.
    /// </summary>
    public class PreProcessor
    {
        // State that is shared in a single processing
        private class State
        {
            public IDictionary<string, Macro> Macros { get; set; } = new Dictionary<string, Macro>();
            public IList<string> IncludePaths { get; set; } = new List<string>();
            public ISet<string> PragmaOncedFiles { get; set; } = new HashSet<string>();
        }

        // Helper for control flow
        private struct ControlFlow
        {
            // Should we keep the current tokens
            public bool Keep { get; set; }
            // Is this depth-level satisfied already
            public bool Satisfied { get; set; }
        }

        private string root;
        private string fullPath;
        private State state;
        private PeekBuffer<Token> source = new PeekBuffer<Token>(Enumerable.Empty<Token>());
        private Stack<ControlFlow> controlStack = new Stack<ControlFlow>();

        private PreProcessor(string fileName, State state)
        {
            fullPath = Path.GetFullPath(fileName);
#pragma warning disable CS8601 // Possible null reference assignment.
            root = Path.GetDirectoryName(fullPath);
            Debug.Assert(root != null);
#pragma warning restore CS8601 // Possible null reference assignment.
            this.state = state;
            // We keep the top-level
            controlStack.Push(new ControlFlow { Keep = true, Satisfied = true });
        }

        public PreProcessor(string fileName)
            : this(fileName, new State())
        {
        }

        /// <summary>
        /// Adds an include path to search for include files.
        /// </summary>
        /// <param name="path">The path to search for includes.</param>
        public void AddIncludePath(string path) => state.IncludePaths.Add(path);

        /// <summary>
        /// Defines a <see cref="Macro"/>.
        /// </summary>
        /// <param name="name">The name to define.</param>
        /// <param name="macro">The <see cref="Macro"/> to assiciate the name with.</param>
        public void Define(string name, Macro macro) => state.Macros[name] = macro;

        /// <summary>
        /// Undefines a <see cref="Macro"/>.
        /// </summary>
        /// <param name="name">The name to undefine.</param>
        public void Undefine(string name) => state.Macros.Remove(name);

        /// <summary>
        /// True, if the given <see cref="Macro"/> is defined.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>True, if a <see cref="Macro"/> is defined under the given name.</returns>
        public bool IsDefined(string name) => state.Macros.ContainsKey(name);

        /// <summary>
        /// Pre-processes the whole input.
        /// </summary>
        /// <param name="tokens">The <see cref="IEnumerable{Token}"/>s to pre-process.</param>
        /// <returns>The <see cref="IEnumerable{Token}"/> of the pre-processed input.</returns>
        public IEnumerable<Token> Process(IEnumerable<Token> tokens)
        {
            source = new PeekBuffer<Token>(tokens);
            return All();
        }

        private IEnumerable<Token> All()
        {
            while (true)
            {
                // Terminate immediately on EOF
                if (Peek().Type == TokenType.End)
                {
                    yield return Consume();
                    break;
                }
                // Check control flow and any directive that's related to control-flow
                var control = controlStack.Peek();
                if (ParseDirective(out var directiveName, out var directiveArgs))
                {
                    if (!control.Keep)
                    {
                        // If we don't keep the code, we only care about control-flow directives
                        if (IsControlFlowDirective(directiveName))
                        {
                            HandleControlFlowDirective(directiveName, directiveArgs);
                        }
                    }
                    else
                    {
                        // We care about all directives
                        foreach (var t in HandleDirective(directiveName, directiveArgs)) yield return t;
                    }
                }
                else
                {
                    if (!control.Keep)
                    {
                        // We skip the current token and then try again
                        Consume();
                        continue;
                    }
                    // We keep everything here
                    ExpandUpcoming();
                    yield return Consume();
                }
            }
        }

        private void ExpandUpcoming()
        {
            if (ParseMacroCall(out var call)) ExpandMacro(call);
        }

        private IEnumerable<Token> HandleDirective(string name, IList<Token> arguments)
        {
            if (IsControlFlowDirective(name))
            {
                HandleControlFlowDirective(name, arguments);
                return Enumerable.Empty<Token>();
            }
            switch (name)
            {
            case "define": 
                HandleMacroDefinition(arguments);
                return Enumerable.Empty<Token>();

            case "undef":
                HandleUndef(arguments);
                return Enumerable.Empty<Token>();

            case "include":
                return HandleInclude(arguments);

            case "pragma":
                HandlePragma(arguments);
                return Enumerable.Empty<Token>();

            default: throw new NotImplementedException($"Unknown directive '{name}'!");
            }
        }

        private void HandleControlFlowDirective(string name, IList<Token> arguments)
        {
            var control = controlStack.Peek();
            switch (name)
            {
            case "if":
            case "ifdef":
            case "ifndef":
                if (!control.Keep)
                {
                    // We didn't keep the surrounding scope, we don't keep this either
                    // We say that this is satisfied because there's no way anything else can satisfy this depth
                    controlStack.Push(new ControlFlow { Keep = false, Satisfied = true });
                }
                else
                {
                    // We keep this scope, we need a condition
                    bool condition;
                    if (name == "if")
                    {
                        condition = EvaluateCondition(arguments);
                    }
                    else
                    {
                        var ident = arguments[0];
                        condition = IsDefined(ident.Value);
                        if (name == "ifndef") condition = !condition;
                    }
                    controlStack.Push(new ControlFlow { Keep = condition, Satisfied = condition });
                }
                break;

            case "elif":
                controlStack.Pop();
                if (control.Satisfied)
                {
                    // We were satisfied before, don't keep
                    controlStack.Push(new ControlFlow { Keep = false, Satisfied = true });
                }
                else
                {
                    // We weren't satisfied, evaluate condition
                    var condition = EvaluateCondition(arguments);
                    controlStack.Push(new ControlFlow { Keep = condition, Satisfied = condition });
                }
                break;

            case "else":
                controlStack.Pop();
                controlStack.Push(new ControlFlow { Keep = !control.Satisfied, Satisfied = true });
                break;

            case "endif":
                controlStack.Pop();
                break;

            default: throw new InvalidOperationException();
            }
        }

        private void HandleMacroDefinition(IList<Token> arguments) => SwitchSource(arguments, () =>
        {
            string ParseMacroParam(out bool isVariadic)
            {
                isVariadic = false;
                if (Match(TokenType.Identifier, out var ident)) return ident.Value;
                if (Match(TokenType.Ellipsis))
                {
                    isVariadic = true;
                    return "__VA_ARGS__";
                }
                // TODO
                throw new NotImplementedException();
            }

            var macroName = Expect(TokenType.Identifier);
            bool needsParens = false;
            bool isVariadic = false;
            var parameters = new List<string>();
            // NOTE: The macro name needs to touch the open paren!
            if (Match(TokenType.OpenParen, out var openParen) 
            && macroName.LogicalSpan.End == openParen.LogicalSpan.Start)
            {
                // A macro with arguments
                needsParens = true;
                if (!Match(TokenType.CloseParen))
                {
                    // We have parameters, parse the first one
                    parameters.Add(ParseMacroParam(out isVariadic));
                    while (!isVariadic && Match(TokenType.Comma)) parameters.Add(ParseMacroParam(out isVariadic));
                    Expect(TokenType.CloseParen);
                }
            }
            // The remaining things are expansion
            var expansion = new List<Token>();
            while (!source.IsEnd) expansion.Add(Consume());
            var userMacro = new UserMacro(needsParens, isVariadic, parameters.ToArray(), expansion.ToArray());
            Define(macroName.Value, userMacro);
        });

        private void HandleUndef(IList<Token> arguments)
        {
            if (arguments.Count == 0 || !IsIdent(arguments[0]))
            {
                // TODO
                throw new NotImplementedException("Expected an identifier!");
            }
            var macroName = arguments[0].Value;
            Undefine(macroName);
        }

        private IEnumerable<Token> HandleInclude(IList<Token> arguments)
        {
            if (arguments.Count == 0 || arguments[0].Type != TokenType.StringLiteral)
            {
                // TODO
                throw new NotImplementedException("Expected a string literal!");
            }
            var includeFileName = arguments[0].Value;
            includeFileName = includeFileName.Substring(1, includeFileName.Length - 2);
            // Search include paths
            foreach (var includePath in state.IncludePaths.Prepend(root))
            {
                var filePath = Path.GetFullPath(Path.Combine(includePath, includeFileName));
                if (!File.Exists(filePath)) continue;
                // We found the file to include
                // Check if it's pragma onced
                if (state.PragmaOncedFiles.Contains(filePath))
                {
                    // Yes, let's skip it
                    return Enumerable.Empty<Token>();
                }
                // Create a sub-processor at the new root
                var sourceText = new StreamReader(filePath).AsCharEnumerable();
                var tokensToPreProcess = Lexer.Lex(CppTextReader.Process(sourceText));
                var pp = new PreProcessor(filePath, state);
                return pp.Process(tokensToPreProcess).SkipLast(1);
            }

            // TODO
            throw new NotImplementedException($"Can't find include '{includeFileName}'!");
        }

        private void HandlePragma(IList<Token> arguments)
        {
            if (arguments.Count == 0)
            {
                // TODO
                throw new NotImplementedException("Pragma without args!");
            }
            var pragmaName = arguments[0].Value;
            if (pragmaName == "once")
            {
                state.PragmaOncedFiles.Add(fullPath);
            }
            else
            {
                // TODO: Unknown pragma
                Console.WriteLine($"Unknown pragma: {pragmaName}");
            }
        }

        private bool EvaluateCondition(IList<Token> tokens) => SwitchSource(tokens, () =>
        {
            // TODO: Expand the source
            var result = ParseExpression() != 0;
            if (Peek().Type != TokenType.End)
            {
                // TODO
                throw new NotImplementedException();
            }
            return result;
        });

        private void ExpandMacro(MacroCall call)
        {
            switch (call.Macro)
            {
            case UserMacro um:
                ExpandUserMacro(um, call);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void ExpandUserMacro(UserMacro macro, MacroCall call)
        {
            // Any argument expansion
            IList<Token> GetExpansion(ref int i)
            {
                var left = GetPrefixExpansion(ref i);
                if (macro.Substitution.Count > i && macro.Substitution[i].Type == TokenType.HashHash)
                {
                    ++i;
                    var right = GetPrefixExpansion(ref i);
                    // TODO: Concat last from left and first from right
                    // TODO
                    throw new NotImplementedException();
                }
                else
                {
                    return left;
                }
            }

            // An element with a possible '#' before
            IList<Token> GetPrefixExpansion(ref int i)
            {
                var peek = macro.Substitution[i];
                if (peek.Type == TokenType.Hash)
                {
                    ++i;
                    var tokens = GetAtomicExpansion(ref i, out bool expanded);
                    if (!expanded)
                    {
                        // TODO
                        throw new NotImplementedException("'#' requires a macro arg!");
                    }
                    return new List<Token> { Stringify(tokens) };
                }
                else
                {
                    return GetAtomicExpansion(ref i, out bool _);
                }
            }

            // A single element that's either an argument or a literal token to paste
            IList<Token> GetAtomicExpansion(ref int i, out bool expanded)
            {
                var element = macro.Substitution[i++];
                if (call.Arguments.TryGetValue(element.Value, out var sub))
                {
                    expanded = true;
                    return sub;
                }
                else
                {
                    expanded = false;
                    return new List<Token> { element };
                }
            }

            if (macro.NeedsParens)
            {
                for (int i = 0; i < macro.Substitution.Count;)
                {
                    var expanded = GetExpansion(ref i);
                    source.PushFront(expanded);
                }
            }
            else
            {
                // Just simply dump it
                source.PushFront(macro.Substitution);
            }
        }

        private static Token Stringify(IList<Token> tokens)
        {
            // TODO
            throw new NotImplementedException();
        }

        private static Token Concat(Token left, Token right)
        {
            // TODO
            throw new NotImplementedException();
        }

        // Parsers /////////////////////////////////////////////////////////////

        private bool ParseDirective(
            [MaybeNullWhen(false)] out string name,
            [MaybeNullWhen(false)] out IList<Token> arguments)
        {
            var peek = Peek();
            var directiveLine = peek.LogicalSpan.End.Line;
            if (peek.Type == TokenType.Hash
                // First token or first in line
                && (!source.TryPrev(out var prev) || prev.LogicalSpan.End.Line != directiveLine))
            {
                // Hash on a fresh line, if an identifier comes up, it's definitely a directive
                var ident = Peek(1);
                if (IsIdent(ident))
                {
                    // It is a directive
                    Consume(2);
                    name = ident.Value;
                    arguments = new List<Token>();
                    for (;
                        // While it's not EOF and we are in the same line
                           Peek().Type != TokenType.End 
                        && Peek().LogicalSpan.Start.Line == directiveLine;
                        arguments.Add(Consume())) ;
                    return true;
                }
            }
            name = null;
            arguments = null;
            return false;
        }

        private bool ParseMacroCall([MaybeNullWhen(false)] out MacroCall call)
        {
            IList<Token> ParseMacroArg()
            {
                var result = new List<Token>();
                var depth = 0;
                while (true)
                {
                    var peek = Peek();
                    // Open parens increase depth
                    if (peek.Type == TokenType.OpenParen) ++depth;
                    else if (peek.Type == TokenType.CloseParen)
                    {
                        // Close parens on depth 0 means end of call
                        if (depth == 0) break;
                        // Otherwise we are nested, elevate the level
                        --depth;
                    }
                    // A comma on depth 0 means next argument in call
                    else if (peek.Type == TokenType.Comma && depth == 0) break;
                    // If we are here, we need the next token
                    // NOTE: Do we want to keep expanding after each token?
                    // Or do we want to just do that after each expansion
                    // Expand for safety
                    ExpandUpcoming();
                    // Consume the upcoming token
                    result.Add(Consume());
                }
                return result;
            }

            var peek = Peek();
            if (IsIdent(peek) && state.Macros.TryGetValue(peek.Value, out var macro))
            {
                // We need to fill these out
                var callSiteIdent = peek;
                var args = new Dictionary<string, IList<Token>>();

                if (!macro.NeedsParens)
                {
                    // We are done, this is a macro without arguments
                    Consume();
                    call = new MacroCall(macro, callSiteIdent, args);
                    return true;
                }
                // The macro requires parenthesis
                var openParen = Peek(1);
                if (openParen.Type == TokenType.OpenParen)
                {
                    // We are committed to calling the macro
                    Consume(2);
                    // From now on call errors are real hard errors
                    // Parse arguments
                    var flatArgumentList = new List<IList<Token>>();
                    if (!Match(TokenType.CloseParen))
                    {
                        flatArgumentList.Add(ParseMacroArg());
                        while (Match(TokenType.Comma)) flatArgumentList.Add(ParseMacroArg());
                        Expect(TokenType.CloseParen);
                    }
                    // Now we have the flat argument list, we need to make it into a dictionary
                    if (macro.Parameters.Count == 1 && flatArgumentList.Count == 0)
                    {
                        // Edge-case where we called MACRO() and the macro needs a single parameter
                        // In this case we interpret it as a single, empty argument
                        flatArgumentList.Add(new List<Token>());
                    }
                    // Check if we have enough arguments
                    if (flatArgumentList.Count < macro.Parameters.Count)
                    {
                        // TODO
                        throw new NotImplementedException("Not enough macro arguments for call!");
                    }
                    // Assign the fix ones to names
                    int argIndex = 0;
                    for (; argIndex < macro.Parameters.Count; ++argIndex)
                    {
                        args[macro.Parameters[argIndex]] = flatArgumentList[argIndex];
                    }
                    // Check if we need variadic args
                    if (!macro.IsVariadic && flatArgumentList.Count != macro.Parameters.Count)
                    {
                        // TODO
                        throw new NotImplementedException("Too many args for non-variadic macro call!");
                    }
                    if (macro.IsVariadic)
                    {
                        // Assign all the remaining macros into __VA_ARGS__
                        var vaArgs = flatArgumentList.Skip(argIndex).SelectMany(l => l).ToList();
                        args["__VA_ARGS__"] = vaArgs;
                    }
                    // Construct the call
                    call = new MacroCall(macro, callSiteIdent, args);
                    return true;
                }
            }
            call = null;
            return false;
        }

        // Expression parsing //////////////////////////////////////////////////

        private static readonly TokenType[][] precedenceTable = new TokenType[][]
        {
            new TokenType[]{ TokenType.Or },
            new TokenType[]{ TokenType.And },
            new TokenType[]{ TokenType.Bitor },
            new TokenType[]{ TokenType.Bitxor },
            new TokenType[]{ TokenType.Bitand },
            new TokenType[]{ TokenType.Equal, TokenType.NotEqual },
            new TokenType[]{ TokenType.Greater, TokenType.Less, TokenType.GreaterEqual, TokenType.LessEqual },
            new TokenType[]{ TokenType.LeftShift, TokenType.RightShift },
            new TokenType[]{ TokenType.Add, TokenType.Subtract },
            new TokenType[]{ TokenType.Multiply, TokenType.Divide, TokenType.Modulo },
        };

        private static readonly TokenType[] prefixOps = new TokenType[]
        {
            TokenType.Add, TokenType.Subtract, TokenType.Not, TokenType.Bitnot,
        };

        private long ParseExpression() => ParseBinaryExpression();

        private long ParseBinaryExpression(int precedence = 0)
        {
            if (precedence >= precedenceTable.Length) return ParsePrefixExpression();

            var ops = precedenceTable[precedence];
            var left = ParseBinaryExpression(precedence + 1);
            while (ops.Contains(Peek().Type))
            {
                var op = Consume();
                var right = ParseBinaryExpression(precedence + 1);
                left = PerformBinaryOperation(op.Type, left, right);
            }
            return left;
        }

        private long ParsePrefixExpression()
        {
            if (prefixOps.Contains(Peek().Type))
            {
                var op = Consume();
                var n = ParsePrefixExpression();
                return PerformPrefixOperation(op.Type, n);
            }
            else
            {
                return ParseAtomicExpression();
            }
        }

        private long ParseAtomicExpression()
        {
            if (Match("defined"))
            {
                // Defined expression
                bool needsParens = Match(TokenType.OpenParen);
                var ident = Expect(TokenType.Identifier);
                if (needsParens) Expect(TokenType.CloseParen);
                return IsDefined(ident.Value) ? 1 : 0;
            }
            if (Match(TokenType.OpenParen))
            {
                // Grouping
                var result = ParseExpression();
                Expect(TokenType.CloseParen);
                return result;
            }
            if (Match(TokenType.Identifier))
            {
                // TODO: Should we expand here if macro?
                // Unexpanded macro
                return 0;
            }
            if (Match(TokenType.IntLiteral, out var intLit))
            {
                return ParseIntLiteral(intLit.Value);
            }
            // TODO
            throw new NotImplementedException();
        }

        private static long PerformBinaryOperation(TokenType op, long left, long right) => op switch
        {
            TokenType.Or => (left != 0 || right != 0) ? 1 : 0,
            TokenType.And => (left == 0 || right == 0) ? 0 : 1,
            TokenType.Equal => (left == right) ? 1 : 0,
            TokenType.NotEqual => (left != right) ? 1 : 0,
            TokenType.Greater => (left > right) ? 1 : 0,
            TokenType.GreaterEqual => (left >= right) ? 1 : 0,
            TokenType.Less => (left < right) ? 1 : 0,
            TokenType.LessEqual => (left <= right) ? 1 : 0,
            _ => throw new NotImplementedException(),
        };

        private static long PerformPrefixOperation(TokenType op, long n) => op switch
        {
            TokenType.Not => (n == 0) ? 1 : 0,
            _ => throw new NotImplementedException(),
        };

        private static long ParseIntLiteral(string intLit)
        {
            if (intLit.EndsWith('L')) intLit = intLit.Substring(0, intLit.Length - 1);
            if (intLit.StartsWith("0x")) return Convert.ToInt32(intLit, 16);
            return int.Parse(intLit);
        }

        // Primitives //////////////////////////////////////////////////////////

        private Token Expect(TokenType tt)
        {
            if (!Match(tt, out var result))
            {
                // TODO
                throw new NotImplementedException();
            }
            return result;
        }

        private bool Match(TokenType tt) => Match(tt, out var _);
        private bool Match(string value) => Match(value, out var _);

        private bool Match(string value, [MaybeNullWhen(false)] out Token token)
        {
            var peek = Peek();
            if (peek.Value == value)
            {
                token = Consume();
                return true;
            }
            token = null;
            return false;
        }

        private bool Match(TokenType tt, [MaybeNullWhen(false)] out Token token)
        {
            var peek = Peek();
            if ((tt == TokenType.Identifier && IsIdent(peek)) || peek.Type == tt)
            {
                token = Consume();
                return true;
            }
            token = null;
            return false;
        }

        private static readonly Token DefaultToken = new Token(new Span(), new Span(), TokenType.End, string.Empty);
        private Token Peek(int amount = 0) => source.PeekOrDefault(amount, DefaultToken);
        private Token Consume() => source.Consume();
        private void Consume(int amount) => source.Consume(amount);

        private void SwitchSource(IList<Token> newTokens, Action action) =>
            SwitchSource(newTokens, () => { action(); return 0; });

        private TResult SwitchSource<TResult>(IList<Token> newTokens, Func<TResult> func)
        {
            var oldSource = source;
            source = new PeekBuffer<Token>(newTokens);
            var result = func();
            source = oldSource;
            return result;
        }

        // Helpers /////////////////////////////////////////////////////////////

        private static bool IsControlFlowDirective(string name) => name switch
        {
            "if" => true,
            "ifdef" => true,
            "ifndef" => true,
            "elif" => true,
            "else" => true,
            "endif" => true,
            _ => false,
        };

        private static bool IsIdent(Token token) =>
               token.Type == TokenType.Identifier
            || (token.Value.Length > 0 && !char.IsDigit(token.Value.First()) && token.Value.All(ch => IsIdent(ch)));

        private static bool IsIdent(char ch) => char.IsLetterOrDigit(ch) || ch == '_';
    }
}
