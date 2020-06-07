using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Syntax
{
    /// <summary>
    /// A syntax error for unclosed delimeters.
    /// </summary>
    class UnclosedDelimeterError : CompileError
    {
        /// <summary>
        /// The opening position of the unclosed delimeter.
        /// </summary>
        public Position StartPosition { get; set; }
        /// <summary>
        /// The end of file, where the unclosed delimeter causes the error.
        /// </summary>
        public Position EndPosition { get; set; }
        /// <summary>
        /// The name of the delimeter that was forgotten.
        /// </summary>
        public string Delimeter { get; set; }

        /// <summary>
        /// Initializes a new <see cref="UnclosedDelimeterError"/>.
        /// </summary>
        /// <param name="startPosition">The opening position of the unclosed comment.</param>
        /// <param name="endPosition">The end of file, where the unclosed comment causes the error.</param>
        /// <param name="delimeter">The name of the delimeter that was forgotten.</param>
        public UnclosedDelimeterError(Position startPosition, Position endPosition, string delimeter)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Delimeter = delimeter;
        }

        public override void Show()
        {
            Console.WriteLine($"Syntax error {EndPosition}!");
            Console.WriteLine(Annotation.Annotate(StartPosition, new AnnotationSettings { ArrowText = "starting here..." }));
            Console.WriteLine("...");
            Console.WriteLine(Annotation.Annotate(EndPosition, new AnnotationSettings { ArrowText = "missing here" }));
            Console.WriteLine($"Unclosed {Delimeter}!");
        }
    }

    /// <summary>
    /// A syntax error, where a given construct was expected, but something else was found instead.
    /// </summary>
    class ExpectedError : CompileError
    {
        /// <summary>
        /// The name of the expected construct.
        /// </summary>
        public string Expected { get; set; }
        /// <summary>
        /// The token we got instead.
        /// </summary>
        public Token Got { get; set; }
        /// <summary>
        /// An optional context. This is so we can append something like: "while parsing <context>".
        /// </summary>
        public string? Context { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ExpectedError"/>.
        /// </summary>
        /// <param name="expected">The name of the expected construct.</param>
        /// <param name="got">The token we got instead.</param>
        /// <param name="context">An optional context. Null by default.</param>
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
