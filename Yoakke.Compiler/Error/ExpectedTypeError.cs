using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Reporting;
using Yoakke.Reporting.Info;
using Yoakke.Syntax.ParseTree;

namespace Yoakke.Compiler.Error
{
    // TODO: Doc
    public class ExpectedTypeError : ICompileError
    {
        /// <summary>
        /// The <see cref="Type"/> that was expected.
        /// </summary>
        public readonly Type Expected;
        /// <summary>
        /// The <see cref="Type"/> that appeared instead.
        /// </summary>
        public readonly Type Got;
        /// <summary>
        /// The <see cref="IParseTreeElement"/> where the wrong <see cref="Type"/> appeared.
        /// </summary>
        public IParseTreeElement? Place { get; set; }
        /// <summary>
        /// The context where the mismatch happened.
        /// </summary>
        public string? Context { get; set; }
        /// <summary>
        /// Note, if any.
        /// </summary>
        public string? Note { get; set; }

        public ExpectedTypeError(Type expected, Type got)
        {
            Expected = expected;
            Got = got;
        }

        public Diagnostic GetDiagnostic()
        {
            var msgSuffix = Context == null ? string.Empty : $" in {Context}";
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Message = $"expected type {Expected} but got {Got}{msgSuffix}",
            };
            if (Place != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = $"expected to be type {Expected} here, but got {Got}",
                    Span = Place.Span,
                    Severity = Severity.Error,
                });
            }
            if (Note != null)
            {
                diag.Information.Add(new FootnoteDiagnosticInfo
                {
                    Message = $"note: {Note}",
                });
            }
            return diag;
        }
    }
}
