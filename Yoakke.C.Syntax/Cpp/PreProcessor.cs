using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
        // Helper for control flow
        private struct ControlFlow
        {
            // Should we keep the current tokens
            public bool Keep { get; set; }
            // Is this depth-level satisfied already
            public bool Satisfied { get; set; }
        }

        private PeekBuffer<Token> source = new PeekBuffer<Token>(Enumerable.Empty<Token>());
        private Dictionary<string, Macro> macros = new Dictionary<string, Macro>();
        private Stack<ControlFlow> controlStack = new Stack<ControlFlow>();
        private List<string> includePaths = new List<string>();

        private PreProcessor(string rootPath)
        {
            // We keep the top-level
            controlStack.Push(new ControlFlow { Keep = true, Satisfied = true });
            // Always search the current path
            AddIncludePath(".");
        }

        public PreProcessor()
            : this(".")
        {
        }

        /// <summary>
        /// Adds an include path to search for include files.
        /// </summary>
        /// <param name="path">The path to search for includes.</param>
        public void AddIncludePath(string path) => includePaths.Add(path);

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
            source = new PeekBuffer<Token>(tokens);
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
                // Terminate immediately on EOF
                if (Peek().Type == TokenType.End) return Consume();
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
                        HandleDirective(directiveName, directiveArgs);
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
                    // TODO: Parse macro call
                    return Consume();
                }
            }
        }

        private void HandleDirective(string name, IList<Token> arguments)
        {
            if (IsControlFlowDirective(name))
            {
                HandleControlFlowDirective(name, arguments);
                return;
            }
            switch (name)
            {
            case "define": 
                HandleMacroDefinition(arguments);
                break;

            case "include":
                HandleInclude(arguments);
                break;

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
            var macroName = Expect(TokenType.Identifier);
            bool needsParens = false;
            bool isVariadic = false;
            var parameters = new List<string>();
            if (Match(TokenType.OpenParen))
            {
                // A macro with arguments
                needsParens = true;
                // TODO
                throw new NotImplementedException();
            }
            // The remaining things are expansion
            var expansion = source.Buffer.ToList();
            var userMacro = new UserMacro(needsParens, isVariadic, parameters.ToArray(), expansion.ToArray());
            Define(macroName.Value, userMacro);
        });

        private void HandleInclude(IList<Token> arguments)
        {
            if (arguments.Count == 0 || arguments[0].Type != TokenType.StringLiteral)
            {
                // TODO
                throw new NotImplementedException("Expected a string literal!");
            }
            var includeFileName = arguments[0].Value;
            includeFileName = includeFileName.Substring(1, includeFileName.Length - 2);
            // Search include paths
            foreach (var includePath in includePaths)
            {
                var filePath = Path.Combine(includePath, includeFileName);
                if (!File.Exists(filePath)) continue;
                // We found the file to include
                // We just insert all of the tokens into the peek buffer

                // TODO: We need to modify the include paths!
                // While we are processing the new tokens, '.' is relative to _that_ file!
                var sourceText = new StreamReader(filePath).AsCharEnumerable();
                var tokensToInsert = Lexer.Lex(CppTextReader.Process(sourceText));
                source.PushFront(tokensToInsert);

                return;
            }

            // TODO
            throw new NotImplementedException($"Can't find include '{includeFileName}'!");
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

        public bool ParseMacroCall([MaybeNullWhen(false)] out MacroCall call)
        {
            var peek = Peek();
            if (IsIdent(peek) && macros.TryGetValue(peek.Value, out var macro))
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
                    // TODO
                    throw new NotImplementedException("TODO");
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

        private int ParseExpression() => ParseBinaryExpression();

        private int ParseBinaryExpression(int precedence = 0)
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

        private int ParsePrefixExpression()
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

        private int ParseAtomicExpression()
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
                return int.Parse(intLit.Value);
            }
            // TODO
            throw new NotImplementedException();
        }

        private static int PerformBinaryOperation(TokenType op, int left, int right) => op switch
        {
            TokenType.Or => (left != 0 || right != 0) ? 1 : 0,
            TokenType.And => (left == 0 || right == 0) ? 0 : 1,
            TokenType.GreaterEqual => (left >= right) ? 1 : 0,
            _ => throw new NotImplementedException(),
        };

        private static int PerformPrefixOperation(TokenType op, int n) => op switch
        {
            TokenType.Not => (n == 0) ? 1 : 0,
            _ => throw new NotImplementedException(),
        };

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
