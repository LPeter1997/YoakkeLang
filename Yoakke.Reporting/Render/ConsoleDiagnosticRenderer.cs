using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Reporting.Info;

namespace Yoakke.Reporting.Render
{
    /// <summary>
    /// An <see cref="IDiagnosticRenderer"/> that prints the <see cref="Diagnostic"/> to console.
    /// </summary>
    public class ConsoleDiagnosticRenderer : IDiagnosticRenderer
    {
        private ConsoleColor decorationColor = ConsoleColor.Gray;
        private ConsoleColor textColor = ConsoleColor.White;

        public void Render(Diagnostic diagnostic)
        {
            // First we write the head, something like
            // error[E123]: Type mismatch!
            RenderDiagnosticHead(diagnostic);

            var spannedInfo = diagnostic.Information.OfType<SpannedDiagnosticInfo>().OrderBy(si => si.Span.Start);
            var primaryInfo = spannedInfo.OfType<PrimaryDiagnosticInfo>().FirstOrDefault();

            if (spannedInfo.Any())
            {
                var source = spannedInfo.First().Span.Source;
                Debug.Assert(source != null);

                // We need the largest line number to pad the others
                var lineNumberPaddingLen = (spannedInfo.Select(si => si.Span.End.Line + 1).Max()).ToString().Length;
                var lineNumberPadding = new string(' ', lineNumberPaddingLen);

                // If there is a primary information source, write the head for it
                if (primaryInfo != null) RenderPrimaryInfoHead(primaryInfo, lineNumberPadding);

                RenderLinePad(lineNumberPadding);
                Console.WriteLine();

                // TODO: Annotated lines
                RenderLineNumber(5, lineNumberPadding);
                Console.WriteLine();

                RenderLinePad(lineNumberPadding);
                Console.WriteLine();
            }


            // Finally we print any hints
            var hints = diagnostic.Information.OfType<HintDiagnosticInfo>();
            foreach (var hint in hints) RenderHint(hint);
        }

        private void RenderDiagnosticHead(Diagnostic diagnostic)
        {
            if (diagnostic.Severity == null && diagnostic.Message == null) return;

            if (diagnostic.Severity != null)
            {
                Console.ForegroundColor = diagnostic.Severity.Color;
                Console.Write(diagnostic.Severity.Description);
                if (diagnostic.Code != null)
                {
                    Console.Write('[');
                    Console.Write(diagnostic.Code);
                    Console.Write(']');
                }
                Console.ResetColor();
                if (diagnostic.Message != null) RenderText(": ");
            }

            if (diagnostic.Message != null) RenderText(diagnostic.Message);

            Console.WriteLine();
        }

        private void RenderPrimaryInfoHead(PrimaryDiagnosticInfo primary, string lineNumberPadding)
        {
            var primarySpan = primary.Span;
            RenderDecoration($"{lineNumberPadding} ┌─ ");
            Debug.Assert(primarySpan.Source != null);
            RenderText($"{primarySpan.Source.Path}:{primarySpan.Start.Line + 1}:{primarySpan.Start.Column + 1}");
        }

        private void RenderLinePad(string lineNumberPadding) =>
            RenderDecoration($"{lineNumberPadding} │");

        private void RenderLineNumber(int lineNumber, string lineNumberPadding) =>
            RenderDecoration($"{(lineNumber + 1).ToString().PadLeft(lineNumberPadding.Length)} │");

        private void RenderHint(HintDiagnosticInfo hint) => RenderText($"hint: {hint.Message}");

        private void RenderText(string msg)
        {
            Console.ForegroundColor = textColor;
            Console.Write(msg);
            Console.ResetColor();
        }

        private void RenderDecoration(string msg)
        {
            Console.ForegroundColor = decorationColor;
            Console.Write(msg);
            Console.ResetColor();
        }
    }
}
