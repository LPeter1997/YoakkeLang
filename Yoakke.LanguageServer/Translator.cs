using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Error;
using Yoakke.Syntax;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Yoakke.LanguageServer
{
    internal static class Translator
    {
        public static Diagnostic Translate(ICompileError error) => error switch
        {
            ArgCountMismatchError argcErr => Translate(argcErr),
            ExpectedTypeError expectedType => Translate(expectedType),
            InitializationError initError => Translate(initError),
            TypeMismatchError typeMismatchError => Translate(typeMismatchError),
            UndefinedSymbolError undefinedSymError => Translate(undefinedSymError),
            SyntaxError syntaxError => Translate(syntaxError.Error),
            _ => throw new NotImplementedException(),
        };

        public static Diagnostic Translate(ArgCountMismatchError argcErr)
        {
            var message = $"expected {argcErr.Expected} arguments but got {argcErr.Got} in procedure call";
            var related = new Container<DiagnosticRelatedInformation>();
            if (argcErr.Defined != null)
            {
                related = new Container<DiagnosticRelatedInformation>(new DiagnosticRelatedInformation
                {
                    Message = $"defined to have {argcErr.Expected} arguments here",
                    Location = TranslateLocation(argcErr.Defined.Span),
                });
            }
            return new Diagnostic
            {
                Message = message,
                Severity = DiagnosticSeverity.Error,
                Range = argcErr.Wrong == null ? null : Translate(argcErr.Wrong.Span),
                RelatedInformation = related,
            };
        }

        public static Diagnostic Translate(ExpectedTypeError expectedType)
        {
            var message = $"expected type {expectedType.Expected} but got {expectedType.Got}";
            if (expectedType.Context != null) message = $"{message} in {expectedType.Context}";
            Text.Span? span = expectedType.Place == null ? null : expectedType.Place.Span;
            var related = new Container<DiagnosticRelatedInformation>();
            if (expectedType.Note != null)
            {
                related = new Container<DiagnosticRelatedInformation>(new DiagnosticRelatedInformation
                {
                    Message = $"note: {expectedType.Note}",
                    Location = span == null ? null : TranslateLocation(span.Value),
                });
            }
            return new Diagnostic
            {
                Message = message,
                Severity = DiagnosticSeverity.Error,
                Range = span == null ? null : Translate(span.Value),
                RelatedInformation = related,
            };
        }

        public static Diagnostic Translate(TypeMismatchError typeMismatchError)
        {
            var message = $"expected type {typeMismatchError.Type1} but got {typeMismatchError.Type2}";
            if (typeMismatchError.Context != null) message = $"{message} in {typeMismatchError.Context}";
            var related = new Container<DiagnosticRelatedInformation>();
            if (typeMismatchError.Defined != null)
            {
                var relatedMessage = $"defined to be {typeMismatchError.Type1} here";
                if (typeMismatchError.ImplicitlyDefined) relatedMessage = $"(implicitly) {relatedMessage}";
                related = new Container<DiagnosticRelatedInformation>(new DiagnosticRelatedInformation 
                { 
                    Message = relatedMessage,
                    Location = TranslateLocation(typeMismatchError.Defined.Span),
                });
            }
            return new Diagnostic
            {
                Message = message,
                Severity = DiagnosticSeverity.Error,
                Range = typeMismatchError.Wrong == null ? null : Translate(typeMismatchError.Wrong.Span),
                RelatedInformation = related,
            };
        }

        public static Diagnostic Translate(UndefinedSymbolError undefinedSymError)
        {
            var message = $"unknown symbol {undefinedSymError.Name}";
            if (undefinedSymError.Context != null) message = $"{message} in {undefinedSymError.Context}";
            Text.Span? span = undefinedSymError.Reference == null ? null : undefinedSymError.Reference.Span;
            var related = new Container<DiagnosticRelatedInformation>();
            if (undefinedSymError.SimilarExistingNames.Any())
            {
                related = new Container<DiagnosticRelatedInformation>(new DiagnosticRelatedInformation
                {
                    Message = $"hint: did you mean {string.Join(" or ", undefinedSymError.SimilarExistingNames)}?",
                    Location = span == null ? null : TranslateLocation(span.Value),
                });
            }
            return new Diagnostic
            {
                Message = message,
                Severity = DiagnosticSeverity.Error,
                Range = span == null ? null : Translate(span.Value),
                RelatedInformation = related,
            };
        }

        public static Diagnostic Translate(InitializationError initError) => initError switch
        {
            DoubleInitializationError doubleInitError => Translate(doubleInitError),
            MissingInitializationError missingInitError => Translate(missingInitError),
            UnknownInitializedFieldError unknownInitError => Translate(unknownInitError),
            _ => throw new NotImplementedException(),
        };

        public static Diagnostic Translate(DoubleInitializationError doubleInitError)
        {
            var related = new Container<DiagnosticRelatedInformation>();
            if (doubleInitError.FirstInitialized != null)
            {
                related = new Container<DiagnosticRelatedInformation>(new DiagnosticRelatedInformation 
                { 
                    Message = "first initialized here",
                    Location = TranslateLocation(doubleInitError.FirstInitialized.Span),
                });
            }
            return new Diagnostic
            {
                Message = $"field {doubleInitError.FieldName} is already initialized",
                Severity = DiagnosticSeverity.Error,
                Range = doubleInitError.SecondInitialized == null ? null : Translate(doubleInitError.SecondInitialized.Span),
                RelatedInformation = related,
            };
        }

        public static Diagnostic Translate(MissingInitializationError missingInitError) => new Diagnostic
        {
            Message = $"missing field initializer for {string.Join(", ", missingInitError.MissingNames)}",
            Severity = DiagnosticSeverity.Error,
            Range = missingInitError.TypeInitializer == null ? null : Translate(missingInitError.TypeInitializer.Span),
        };

        public static Diagnostic Translate(UnknownInitializedFieldError unknownInitError)
        {
            Text.Span? span = unknownInitError.UnknownInitialized == null ? null : unknownInitError.UnknownInitialized.Span;
            var related = new Container<DiagnosticRelatedInformation>();
            if (unknownInitError.SimilarExistingNames.Any())
            {
                related = new Container<DiagnosticRelatedInformation>(new DiagnosticRelatedInformation
                {
                    Message = $"hint: did you mean {string.Join(" or ", unknownInitError.SimilarExistingNames)}?",
                    Location = span == null ? null : TranslateLocation(span.Value),
                });
            }
            return new Diagnostic
            {
                Message = $"no field {unknownInitError.UnknownFieldName} can be found in type {unknownInitError.Type}",
                Severity = DiagnosticSeverity.Error,
                Range = span == null ? null : Translate(span.Value),
                RelatedInformation = related,
            };
        }

        public static Diagnostic Translate(Syntax.Error.ISyntaxError syntaxError) => syntaxError switch
        {
            Syntax.Error.ExpectedTokenError expectedToken => Translate(expectedToken),
            Syntax.Error.UnexpectedTokenError unexpectedToken => Translate(unexpectedToken),
            Syntax.Error.UnterminatedTokenError unterminatedToken => Translate(unterminatedToken),
            _ => throw new NotImplementedException(),
        };

        public static Diagnostic Translate(Syntax.Error.ExpectedTokenError expectedToken)
        {
            var message = $"expected {string.Join(" or ", expectedToken.Expected.Select(tt => tt.ToText()))}";
            if (expectedToken.Context != null) message = $"{message} while parsing {expectedToken.Context}";
            var span = expectedToken.Expected.All(Syntax.Error.ExpectedTokenError.IsTerminatorToken) && expectedToken.Prev != null
                ? new Text.Span(expectedToken.Prev.Span.Source, expectedToken.Prev.Span.End, 1)
                : expectedToken.Got.Span;
            var related = new Container<DiagnosticRelatedInformation>();
            if (expectedToken.Starting != null)
            {
                related = new Container<DiagnosticRelatedInformation>(new DiagnosticRelatedInformation 
                { 
                    Location = TranslateLocation(expectedToken.Starting.Span),
                    Message = $"matching {expectedToken.Starting.Value} here",
                });
            }
            return new Diagnostic
            {
                Message = message,
                Severity = DiagnosticSeverity.Error,
                Range = Translate(span),
                RelatedInformation = related,
            };
        }

        public static Diagnostic Translate(Syntax.Error.UnexpectedTokenError unexpectedToken)
        {
            var message = $"unexpected token {unexpectedToken.Got.Value}";
            if (unexpectedToken.Context != null) message = $"{message} while parsing {unexpectedToken.Context}";
            return new Diagnostic
            {
                Message = message,
                Severity = DiagnosticSeverity.Error,
                Range = Translate(unexpectedToken.Got.Span),
            };
        }

        public static Diagnostic Translate(Syntax.Error.UnterminatedTokenError unterminatedToken)
        {
            var message = $"unterminated {unterminatedToken.Token.Type.ToText()}, expected {unterminatedToken.Close}";
            var span = new Text.Span(unterminatedToken.Token.Span.Source, unterminatedToken.Token.Span.End, 1);
            var related = new Container<DiagnosticRelatedInformation>(new DiagnosticRelatedInformation
            {
                Location = TranslateLocation(unterminatedToken.Token.Span),
                Message = $"starting here",
            });
            return new Diagnostic
            {
                Message = message,
                Severity = DiagnosticSeverity.Error,
                Range = Translate(span),
                RelatedInformation = related,
            };
        }

        public static Location TranslateLocation(Text.Span span) => new Location
        {
            Uri = DocumentUri.FromFileSystemPath(span.Source.Path),
            Range = Translate(span),
        };

        public static Range Translate(Text.Span span) => new Range(Translate(span.Start), Translate(span.End));

        public static Position Translate(Text.Position pos) => new Position(pos.Line, pos.Column);

        public static Text.Position Translate(Position pos) => new Text.Position(pos.Line, pos.Character);
    }
}
