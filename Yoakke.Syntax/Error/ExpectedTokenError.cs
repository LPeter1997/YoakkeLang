using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Reporting;
using Yoakke.Reporting.Info;
using Yoakke.Text;

namespace Yoakke.Syntax.Error
{
    /// <summary>
    /// An error when the parser expects a certain token but it didn't appear in the stream.
    /// </summary>
    public class ExpectedTokenError : ISyntaxError
    {
        /// <summary>
        /// The expected token types.
        /// </summary>
        public readonly IEnumerable<TokenType> Expected;
        /// <summary>
        /// The previously matched token.
        /// </summary>
        public readonly Token? Prev;
        /// <summary>
        /// The got token.
        /// </summary>
        public readonly Token Got;
        /// <summary>
        /// The name of the parsed construct for hinting.
        /// </summary>
        public readonly string? Context;

        public ExpectedTokenError(IEnumerable<TokenType> expected, Token? prev, Token got, string? context)
        {
            Expected = expected;
            Prev = prev;
            Got = got;
            Context = context;
        }

        public Diagnostic GetDiagnostic() => new Diagnostic
        {
            Severity = Severity.Error,
            Message = Context == null ? "Syntax error" : $"Syntax error while parsing {Context}",
            Information =
            {
                new PrimaryDiagnosticInfo
                {
                    // For terminator tokens we show the end of the prev. token
                    Span = Expected.All(IsTerminatorToken) && Prev != null 
                        ? new Span(Prev.Span.Source, Prev.Span.End, 1)
                        : Got.Span,
                    Message = $"expected {string.Join("or", Expected.Select(tt => tt.ToText()))}",
                },
            },
        };

        private static bool IsTerminatorToken(TokenType tt) =>
               tt == TokenType.Semicolon 
            || tt == TokenType.CloseBrace 
            || tt == TokenType.CloseBracket 
            || tt == TokenType.CloseParen
            || tt == TokenType.End;
    }
}
