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
    public abstract class InitializationError : ICompileError
    {
        /// <summary>
        /// The struct type expression before the initializer braces.
        /// </summary>
        public readonly IParseTreeElement? StructType;

        public InitializationError(IParseTreeElement? structType)
        {
            StructType = structType;
        }

        public abstract Diagnostic GetDiagnostic();
    }

    // TODO: Doc
    public class DoubleInitializationError : InitializationError
    {
        /// <summary>
        /// The name of the field that was already initialized.
        /// </summary>
        public readonly string FieldName;
        /// <summary>
        /// The place where the field was initialized first.
        /// </summary>
        public IParseTreeElement? FirstInitialized { get; set; }
        /// <summary>
        /// The place where the field was initialized again.
        /// </summary>
        public IParseTreeElement? SecondInitialized { get; set; }

        public DoubleInitializationError(IParseTreeElement? structType, string fieldName) 
            : base(structType)
        {
            FieldName = fieldName;
        }

        public override Diagnostic GetDiagnostic()
        {
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Message = $"field {FieldName} is already initialized",
            };
            if (FirstInitialized != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = "first initialized here",
                    Span = FirstInitialized.Span,
                });
            }
            if (SecondInitialized != null)
            {
                diag.Information.Add(new PrimaryDiagnosticInfo
                {
                    Message = "tried to reinitialize here",
                    Span = SecondInitialized.Span,
                });
            }
            return diag;
        }
    }
}
