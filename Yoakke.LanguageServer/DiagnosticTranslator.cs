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
    internal static class DiagnosticTranslator
    {
        public static Diagnostic Translate(ICompileError error) => error switch
        {
            SyntaxError syntaxError => Translate(syntaxError.Error),
            _ => throw new NotImplementedException(),
        };

        public static Diagnostic Translate(Syntax.Error.ISyntaxError syntaxError) => syntaxError switch
        {
            Syntax.Error.ExpectedTokenError expectedToken => Translate(expectedToken),
            Syntax.Error.UnexpectedTokenError unexpectedToken => new Diagnostic { }, // TODO
            Syntax.Error.UnterminatedTokenError unterminatedToken => new Diagnostic { }, // TODO
            _ => throw new NotImplementedException(),
        };

        public static Diagnostic Translate(Syntax.Error.ExpectedTokenError expectedToken)
        {
            var message = $"syntax error: expected {string.Join(" or ", expectedToken.Expected.Select(tt => tt.ToText()))}";
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
                    Message = $"matching {expectedToken.Starting.Value} here"
                });
            }
            return new Diagnostic
            {
                Message = message,
                Severity = DiagnosticSeverity.Error,
                Range = Translate(span),
            };
        }

        public static Location TranslateLocation(Text.Span span) => new Location
        {
            Uri = OmniSharp.Extensions.LanguageServer.Protocol.DocumentUri.Parse(span.Source.Path),
            Range = Translate(span),
        };

        public static Range Translate(Text.Span span) => new Range(Translate(span.Start), Translate(span.End));

        public static Position Translate(Text.Position pos) => new Position(pos.Line, pos.Column);
    }
}
