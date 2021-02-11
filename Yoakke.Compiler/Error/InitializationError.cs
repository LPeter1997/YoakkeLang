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
    // TODO: We could merge all initialization errors into one!
    // This would help missing initializer errors that were caused by misspelling
    // A probably misspelled initializer should not be reported separately, but be mentioned as a hint instead for the 
    // missing one

    // TODO: Doc
    public abstract class InitializationError : ICompileError
    {
        /// <summary>
        /// The <see cref="Type"/> of the initialized thing.
        /// </summary>
        public readonly Type Type;
        /// <summary>
        /// The type expression before the initializer braces.
        /// </summary>
        public IParseTreeElement? TypeInitializer { get; set; }

        public InitializationError(Type type)
        {
            Type = type;
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

        public DoubleInitializationError(Type type, string fieldName) 
            : base(type)
        {
            FieldName = fieldName;
        }

        public override Diagnostic GetDiagnostic()
        {
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                //Message = $"field {FieldName} is already initialized in type {Type}",
                Message = $"field {FieldName} is already initialized",
            };
            if (TypeInitializer != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = "double initializers in this",
                    Span = TypeInitializer.Span,
                });
            }
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
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = "tried to reinitialize here",
                    Span = SecondInitialized.Span,
                    Severity = Severity.Error,
                });
            }
            return diag;
        }
    }

    // TODO: Doc
    public class MissingInitializationError : InitializationError
    {
        /// <summary>
        /// The missing field names.
        /// </summary>
        public readonly IEnumerable<string> MissingNames;

        public MissingInitializationError(Type type, IEnumerable<string> names) 
            : base(type)
        {
            MissingNames = names;
        }

        public override Diagnostic GetDiagnostic()
        {
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                //Message = $"missing field initializer for {string.Join(", ", MissingNames)} in type {Type}",
                Message = $"missing field initializer for {string.Join(", ", MissingNames)}",
            };
            if (TypeInitializer != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = "missing initializers from this",
                    Span = TypeInitializer.Span,
                });
            }
            return diag;
        }
    }

    // TODO: Doc
    public class UnknownInitializedFieldError : InitializationError
    {
        /// <summary>
        /// The name of the unknown field.
        /// </summary>
        public readonly string UnknownFieldName;
        /// <summary>
        /// The unknown initializer.
        /// </summary>
        public IParseTreeElement? UnknownInitialized { get; set; }
        /// <summary>
        /// A list of similar field names that do exist.
        /// </summary>
        public IEnumerable<string> SimilarExistingNames { get; set; } = Enumerable.Empty<string>();

        public UnknownInitializedFieldError(Type type, string name)
            : base(type)
        {
            UnknownFieldName = name;
        }

        public override Diagnostic GetDiagnostic()
        {
            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Message = $"no field {UnknownFieldName} can be found in type {Type}",
            };
            if (TypeInitializer != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = "wrong initializers in this",
                    Span = TypeInitializer.Span,
                });
            }
            if (UnknownInitialized != null)
            {
                diag.Information.Add(new SpannedDiagnosticInfo
                {
                    Message = "unknown field initialized here",
                    Span = UnknownInitialized.Span,
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
