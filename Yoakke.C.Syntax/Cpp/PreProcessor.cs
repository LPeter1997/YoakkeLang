using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        private class ControlFlow
        {
            // Should we keep the current tokens
            public bool Keep { get; set; }
            // Is this depth-level satisfied already
            public bool Satisfied { get; set; }
        }

        private PeekBuffer<Token> source = new PeekBuffer<Token>(Enumerable.Empty<Token>());
        private Dictionary<string, Macro> macros = new Dictionary<string, Macro>();
        private Stack<ControlFlow> controlStack = new Stack<ControlFlow>();

        public PreProcessor()
        {
            // We keep the top-level
            controlStack.Push(new ControlFlow { Keep = true, Satisfied = true });
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
            switch (name)
            {
            default: throw new NotImplementedException($"Unknown directive '{name}'!");
            }
        }

        private void HandleControlFlowDirective(string name, IList<Token> arguments)
        {
            switch (name)
            {
            default: throw new InvalidOperationException();
            }
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
                if (ident.Type == TokenType.Identifier)
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

        private Token Peek(int amount = 0) => source.Peek(amount);
        private Token Consume() => source.Consume();
        private void Consume(int amount) => source.Consume(amount);

        // Helpers /////////////////////////////////////////////////////////////

        private static bool IsControlFlowDirective(string name) => name switch
        {
            "if" => true,
            "elif" => true,
            "else" => true,
            "ifdef" => true,
            "ifndef" => true,
            _ => false,
        };

        private static bool IsIdent(Token token) =>
               token.Type == TokenType.Identifier
            || (token.Value.Length > 0 && !char.IsDigit(token.Value.First()) && token.Value.All(ch => IsIdent(ch)));

        private static bool IsIdent(char ch) => char.IsLetterOrDigit(ch) || ch == '_';
    }
}
