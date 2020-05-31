using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Syntax
{
    class UnclosedCommentError : CompileError
    {
        public Position StartPosition { get; set; }
        public Position EndPosition { get; set; }

        public UnclosedCommentError(Position startPosition, Position endPosition)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
        }

        public override void Show()
        {
            Console.WriteLine($"Syntax error {EndPosition}!");
            Console.WriteLine(Annotation.Annotate(StartPosition, new AnnotationSettings { ArrowText = "starting here..." }));
            Console.WriteLine("...");
            Console.WriteLine(Annotation.Annotate(EndPosition, new AnnotationSettings { ArrowText = "missing here" }));
            Console.WriteLine("Unclosed nested comment!");
        }
    }

    class ExpectedError : CompileError
    {
        public string Expected { get; set; }
        public Token Got { get; set; }
        public string? Context { get; set; }

        public ExpectedError(string expected, Token got, string? context = null)
        {
            Expected = expected;
            Got = got;
            Context = context;
        }

        public override void Show()
        {
            Console.WriteLine($"Syntax error {Got.Position}!");
            Console.WriteLine(Annotation.Annotate(Got.Position));
            var got = Got.Type == TokenType.End ? "end of file" : Got.Value;
            var context = Context == null ? string.Empty : $" while parsing {Context}";
            Console.WriteLine($"Expected {Expected}, but got '{got}'{context}!");
        }
    }
}
