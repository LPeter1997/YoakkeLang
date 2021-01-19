using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Reporting;
using Yoakke.Reporting.Info;

namespace Yoakke.Syntax.Error
{
    /// <summary>
    /// An error when the parser encounters a token that was unexpected.
    /// </summary>
    public class UnexpectedTokenError : ISyntaxError
    {
        /// <summary>
        /// The got token.
        /// </summary>
        public readonly Token Got;
        /// <summary>
        /// The name of the parsed construct for hinting.
        /// </summary>
        public string? Context { get; set; }

        public UnexpectedTokenError(Token got)
        {
            Got = got;
        }

        public Diagnostic GetDiagnostic()
        {
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Message = Context == null ? "syntax error" : $"syntax error while parsing {Context}",
                Information =
                {
                    new SpannedDiagnosticInfo
                    {
                        Span = Got.Span,
                        Message = $"unexpected token here",
                        Severity = Severity.Error,
                    },
                },
            };
            return diag;
        }
    }
}
