using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Reporting;
using Yoakke.Reporting.Info;
using Yoakke.Syntax.Ast;
using Yoakke.Syntax.ParseTree;

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
        /// The <see cref="IParseTreeElement"/> where the entity was defined to be of the given type.
        /// This corresponds to <see cref="Type1"/>.
        /// </summary>
        public IParseTreeElement? Defined { get; set; }
        /// <summary>
        /// The <see cref="IParseTreeElement"/> that tried to coerce into another, wrong type.
        /// This corresponds to <see cref="Type2"/>.
        /// </summary>
        public IParseTreeElement? Wrong { get; set; }
        /// <summary>
        /// The context where the mismatch happened.
        /// </summary>
        public string? Context { get; set; }
        /// <summary>
        /// True, if the type definition was implicit.
        /// </summary>
        public bool ImplicitlyDefined { get; set; } = false;

        public TypeMismatchError(Type type1, Type type2)
        {
            Type1 = type1;
            Type2 = type2;
        }

        public Diagnostic GetDiagnostic()
        {
            var msgSuffix = Context == null ? string.Empty : $" in {Context}";
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Message = $"type mismatch between {Type1} and {Type2}{msgSuffix}",
            };
            if (Defined != null)
            {
                var defPrefix = ImplicitlyDefined ? "(implicitly) " : string.Empty;
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = $"{defPrefix}defined to be {Type1} here",
                    Span = Defined.Span,
                });
            }
            if (Wrong != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = $"coerced to {Type2} here",
                    Span = Wrong.Span,
                    Severity = Severity.Error,
                });
            }
            return diag;
        }
    }
}
