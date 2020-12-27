using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Reporting;
using Yoakke.Reporting.Info;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Error
{
    // TODO: Doc
    public class TypeMismatchError : ICompileError
    {
        /// <summary>
        /// The first <see cref="Type"/>.
        /// </summary>
        public readonly Type Type1;
        /// <summary>
        /// The second <see cref="Type"/>.
        /// </summary>
        public readonly Type Type2;
        /// <summary>
        /// The <see cref="Expression"/> the entity was defined to be of the given type.
        /// This corresponds to <see cref="Type1"/>.
        /// </summary>
        public Expression? Defined { get; set; }
        /// <summary>
        /// The <see cref="Expression"/> that tried to coerce into another, wrong type.
        /// This corresponds to <see cref="Type2"/>.
        /// </summary>
        public Expression? Wrong { get; set; }

        public TypeMismatchError(Type type1, Type type2)
        {
            Type1 = type1;
            Type2 = type2;
        }

        public Diagnostic GetDiagnostic()
        {
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Message = $"type mismatch between {Type1} and {Type2}",
            };
            var defined = Defined?.ParseTreeNode;
            var wrong = Wrong?.ParseTreeNode;
            if (defined != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = $"inferred to be {Type1} here",
                    Span = defined.Span,
                });
            }
            if (wrong != null)
            {
                diag.Information.Add(new PrimaryDiagnosticInfo
                {
                    Message = $"coerced to {Type2} here",
                    Span = wrong.Span,
                });
            }
            return diag;
        }
    }
}
