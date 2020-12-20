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
        private int linesBefore = 1;
        private int linesAfter = 1;
        private int tabSize = 4;

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

                RenderAnnotatedLines(spannedInfo, lineNumberPadding);

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
            Console.WriteLine();
        }

        private void RenderAnnotatedLines(IEnumerable<SpannedDiagnosticInfo> infos, string lineNumberPadding)
        {
            var source = infos.First().Span.Source;
            Debug.Assert(source != null);

            int? lastLineIndex = null;
            foreach (var info in infos)
            {
                // NOTE: We don't support multiple files yet in a single diagnostic
                // Maybe later we want to
                Debug.Assert(info.Span.Source == source);
                // NOTE: We don't support multiline spans either
                Debug.Assert(info.Span.Start.Line == info.Span.End.Line);
                // NOTE: We don't support multiple annotations in the same line either
                Debug.Assert(lastLineIndex != info.Span.Start.Line);

                var annotatedLineIndex = info.Span.Start.Line;

                // Calculate how many lines to go before and after
                int startIndex = Math.Max(lastLineIndex == null ? 0 : lastLineIndex.Value, annotatedLineIndex - linesBefore);
                int endIndex = Math.Min(annotatedLineIndex + linesAfter + 1, source.LineCount);
                lastLineIndex = endIndex;

                for (int lineIndex = startIndex; lineIndex != endIndex; ++lineIndex)
                {
                    RenderLineNumber(lineIndex, lineNumberPadding);
                    // TODO: if lineIndex == startIndex, annotate
                    // NOTE: All lines have to be printed per-character to have a uniform tab-size
                    RenderSourceLine(source.Line(lineIndex).ToString());
                    Console.WriteLine();
                }
            }
        }

        private void RenderLinePad(string lineNumberPadding) =>
            RenderDecoration($"{lineNumberPadding} │ ");

        private void RenderLineNumber(int lineIndex, string lineNumberPadding) =>
            RenderDecoration($"{(lineIndex + 1).ToString().PadLeft(lineNumberPadding.Length)} │ ");

        private void RenderHint(HintDiagnosticInfo hint) => RenderText($"hint: {hint.Message}");

        private void RenderSourceLine(ReadOnlySpan<char> span)
        {
            Console.ForegroundColor = textColor;
            int column = 0;
            foreach (var ch in span)
            {
                if (ch == '\t')
                {
                    var advance = tabSize - column % tabSize;
                    column += advance;
                    Console.Write(new string(' ', advance));
                }
                else if (!char.IsControl(ch))
                {
                    Console.Write(ch);
                    column += 1;
                }
            }
            Console.ResetColor();
        }

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
