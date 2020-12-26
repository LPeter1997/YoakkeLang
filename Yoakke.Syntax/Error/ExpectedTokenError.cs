using System.Collections.Generic;
using System.Linq;
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
        public string? Context { get; set; }
        /// <summary>
        /// If this is a token that comes in pair, then the starting <see cref="Token"/>, null otherwise.
        /// </summary>
        public Token? Starting { get; set; }

        public ExpectedTokenError(IEnumerable<TokenType> expected, Token? prev, Token got)
        {
            Expected = expected;
            Prev = prev;
            Got = got;
        }

        public Diagnostic GetDiagnostic()
        {
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Message = Context == null ? "Syntax error" : $"syntax error while parsing {Context}",
                Information =
                {
                    new PrimaryDiagnosticInfo
                    {
                        // For terminator tokens we show the end of the prev. token
                        Span = Expected.All(IsTerminatorToken) && Prev != null
                            ? new Span(Prev.Span.Source, Prev.Span.End, 1)
                            : Got.Span,
                        Message = $"expected {string.Join(" or ", Expected.Select(tt => tt.ToText()))} here",
                    },
                },
            };
            if (Starting != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = $"matching {Starting.Value} is here",
                    Span = Starting.Span,
                });
            }
            return diag;
        }

        private static bool IsTerminatorToken(TokenType tt) =>
               tt == TokenType.Semicolon 
            || tt == TokenType.CloseBrace 
            || tt == TokenType.CloseBracket 
            || tt == TokenType.CloseParen
            || tt == TokenType.End;
    }
}
