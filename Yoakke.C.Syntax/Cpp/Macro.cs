using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.C.Syntax.Cpp
{
    /// <summary>
    /// The base class for every macro.
    /// </summary>
    public abstract class Macro
    {
        /// <summary>
        /// True, if '()' is needed for expansion.
        /// </summary>
        public abstract bool NeedsParens { get; }
        /// <summary>
        /// True, if this is a variadic-argument macro.
        /// </summary>
        public abstract bool IsVariadic { get; }
        /// <summary>
        /// The fixed, named list of parameters.
        /// </summary>
        public abstract IReadOnlyList<string> Parameters { get; }

        /// <summary>
        /// Expands this macro.
        /// </summary>
        /// <param name="callSiteIdent">The call-site identifier <see cref="Token"/> that referenced this macro.</param>
        /// <param name="args">The argument dictionary mapping argument names to <see cref="Token"/> sequences.</param>
        /// <returns>The expanded <see cref="IEnumerable{Token}"/>.</returns>
        public abstract IEnumerable<Token> Expand(Token callSiteIdent, IDictionary<string, IList<Token>> args);
    }

    /// <summary>
    /// A simple macro that requires no arguments and can be driven with a <see cref="Func{Token, IEnumerable{Token}}"/>.
    /// </summary>
    public class FuncLabelMacro : Macro
    {
        public override bool NeedsParens => false;
        public override bool IsVariadic => false;
        public override IReadOnlyList<string> Parameters { get; } = new string[] { };

        private readonly Func<Token, IEnumerable<Token>> expander;

        /// <summary>
        /// Initializes a new <see cref="FuncLabelMacro"/>.
        /// </summary>
        /// <param name="expander">The <see cref="Expander"/> function.</param>
        public FuncLabelMacro(Func<Token, IEnumerable<Token>> expander)
        {
            this.expander = expander;
        }

        public override IEnumerable<Token> Expand(Token callSiteIdent, IDictionary<string, IList<Token>> args)
        {
            Debug.Assert(args.Count == 0);
            return expander(callSiteIdent);
        }
    }

    /// <summary>
    /// A user-defined macro.
    /// </summary>
    public class UserMacro : Macro
    {
        public override bool NeedsParens { get; }
        public override bool IsVariadic { get; }
        public override IReadOnlyList<string> Parameters { get; }

        private IReadOnlyList<Token> substitution;

        public UserMacro(bool needsParens, bool isVariadic, IReadOnlyList<string> parameters, IReadOnlyList<Token> substitution)
        {
            NeedsParens = needsParens;
            IsVariadic = isVariadic;
            Parameters = parameters;
            this.substitution = substitution;
        }

        public override IEnumerable<Token> Expand(Token callSiteIdent, IDictionary<string, IList<Token>> args)
        {
            for (int offset = 0; offset < substitution.Count;)
            {
                var expansion = NextExpansion(args, ref offset);
                if (offset < substitution.Count && substitution[offset].Type == TokenType.HashHash)
                {
                    // It's a concatenation
                    ++offset;
                    if (offset >= substitution.Count)
                    {
                        // TODO
                        throw new NotImplementedException("## can't appear at either end of a macro expansion");
                    }
                    var otherExpansion = NextExpansion(args, ref offset);
                    foreach (var e in expansion.SkipLast(1)) yield return e;
                    var toCat1 = expansion.Last();
                    var toCat2 = otherExpansion.First();
                    yield return Concatenate(toCat1, toCat2);
                    foreach (var e in otherExpansion.Skip(1)) yield return e;
                }
                else
                {
                    // Just a single expansion
                    foreach (var e in expansion) yield return e;
                }
            }
        }

        // Expands argumentsm applies # automatically
        private IList<Token> NextExpansion(IDictionary<string, IList<Token>> args, ref int offset)
        {
            var t = substitution[offset];
            if (t.Type == TokenType.HashHash)
            {
                // TODO
                throw new NotImplementedException("## can't appear at either end of a macro expansion");
            }
            if (t.Type == TokenType.Hash)
            {
                // Stringification
                if (++offset >= substitution.Count)
                {
                    // TODO
                    throw new NotImplementedException("# needs an argument");
                }
                var expanded = ExpandToken(substitution[offset++], args, out var isExpansion);
                if (!isExpansion)
                {
                    // TODO
                    throw new NotImplementedException("# needs a real argument");
                }
                return new List<Token> { Stringify(expanded) };
            }
            else
            {
                // Just expand it
                return ExpandToken(substitution[offset++], args, out var _);
            }
        }

        private IList<Token> ExpandToken(Token toExpand, IDictionary<string, IList<Token>> args, out bool realExpansion)
        {
            if (args.TryGetValue(toExpand.Value, out var argValue))
            {
                // A named argument, substitute
                realExpansion = true;
                return argValue;
            }
            else
            {
                // Just a simple argument
                realExpansion = false;
                return new List<Token> { toExpand };
            }
        }

        private static Token Stringify(IList<Token> tokens)
        {
            Debug.Assert(tokens.Count > 0);

            var sb = new StringBuilder();
            for (int i = 0; i < tokens.Count; ++i)
            {
                if (i > 0 && tokens[i - 1].LogicalSpan.End != tokens[i].LogicalSpan.Start)
                {
                    // There is spacing in betweeh
                    sb.Append(' ');
                }
                sb.Append(tokens[i].Value);
            }
            // We need to escape the quotations
            sb.Replace("\"", "\\\"");
            // Surround with quotes
            sb.Insert(0, '"');
            sb.Append('"');
            var physicalSpan = new Span(tokens.First().PhysicalSpan, tokens.Last().PhysicalSpan);
            var logicalSpan = new Span(tokens.First().LogicalSpan, tokens.Last().LogicalSpan);
            return new Token(physicalSpan, logicalSpan, TokenType.StringLiteral, sb.ToString());
        }

        private static Token Concatenate(Token t1, Token t2)
        {
            var tokens = Lexer.Lex(t1.Value + t2.Value).ToList();
            if (tokens.Count != 2)
            {
                // TODO
                throw new NotImplementedException("Tokens don't make a single one pasted");
            }
            return tokens[0];
        }
    }
}
