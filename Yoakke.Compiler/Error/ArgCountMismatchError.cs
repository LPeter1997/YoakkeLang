using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Reporting;
using Yoakke.Reporting.Info;
using Yoakke.Syntax.ParseTree;

namespace Yoakke.Compiler.Error
{
    // TODO: Doc
    public class ArgCountMismatchError : ICompileError
    {
        /// <summary>
        /// The expected number of arguments.
        /// </summary>
        public readonly int Expected;
        /// <summary>
        /// The number of arguments got.
        /// </summary>
        public readonly int Got;
        /// <summary>
        /// The <see cref="IParseTreeElement"/> where the procedure was defined.
        /// </summary>
        public IParseTreeElement? Defined { get; set; }
        /// <summary>
        /// The <see cref="IParseTreeElement"/> that tried to call the procedure with the wrong number of arguments.
        /// This corresponds to <see cref="Type2"/>.
        /// </summary>
        public IParseTreeElement? Wrong { get; set; }

        public ArgCountMismatchError(int expected, int got)
        {
            Expected = expected;
            Got = got;
        }

        public Diagnostic GetDiagnostic()
        {
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Message = $"expected {Expected} arguments but got {Got} in procedure call",
            };
            if (Defined != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = $"defined to have {Expected} parameters here",
                    Span = Defined.Span,
                });
            }
            if (Wrong != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = $"tried to call with {Got} arguments here",
                    Span = Wrong.Span,
                    Severity = Severity.Error,
                });
            }
            return diag;
        }
    }
}
