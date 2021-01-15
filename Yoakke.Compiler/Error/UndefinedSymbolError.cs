using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Reporting;
using Yoakke.Reporting.Info;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Error
{
    // TODO: Doc
    public class UndefinedSymbolError : ICompileError
    {
        /// <summary>
        /// The referred name.
        /// </summary>
        public readonly string? Name;
        /// <summary>
        /// The <see cref="Token"/> that referred to the undefined symbol.
        /// </summary>
        public readonly Token? Reference;
        /// <summary>
        /// A list of similar symbol names that do exist.
        /// </summary>
        public IEnumerable<string> SimilarExistingNames { get; set; } = Enumerable.Empty<string>();

        public UndefinedSymbolError(string name)
        {
            Name = name;
        }

        public UndefinedSymbolError(Token reference)
        {
            Name = reference.Value;
            Reference = reference;
        }

        public Diagnostic GetDiagnostic()
        {
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Message = $"unknown symbol {Name}",
            };
            if (Reference != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = "referred here",
                    Span = Reference.Span,
                    Severity = Severity.Error,
                });
            }
            if (SimilarExistingNames.Any())
            {
                diag.Information.Add(new FootnoteDiagnosticInfo
                {
                    Message = $"hint: did you mean {string.Join(" or ", SimilarExistingNames)}?",
                });
            }
            return diag;
        }
    }
}
