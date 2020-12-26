using Yoakke.Reporting;
using Yoakke.Reporting.Info;
using Yoakke.Text;

namespace Yoakke.Syntax.Error
{
    /// <summary>
    /// An error for a token that was missing it's closing delimiter, like an ending quote for
    /// strings.
    /// </summary>
    public class UnterminatedTokenError : ISyntaxError
    {
        /// <summary>
        /// The lexed token.
        /// </summary>
        public readonly Token Token;
        /// <summary>
        /// The expected closing delimiter.
        /// </summary>
        public readonly string Close;

        public UnterminatedTokenError(Token token, string close)
        {
            Token = token;
            Close = close;
        }

        public Diagnostic GetDiagnostic() => new Diagnostic
        {
            Severity = Severity.Error,
            Message = $"Unterminated {Token.Type.ToText()}!",
            Information =
            {
                new SpannedDiagnosticInfo
                { 
                    Span = new Span(Token.Span.Source, Token.Span.Start, 1),
                    Message = "starting here",
                },
                new PrimaryDiagnosticInfo
                {
                    Span = new Span(Token.Span.Source, Token.Span.End, 1),
                    Message = $"missing {Close} here",
                },
            },
        };
    }
}
