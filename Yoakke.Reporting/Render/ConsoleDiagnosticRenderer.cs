using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Reporting.Info;
using Yoakke.Text;

namespace Yoakke.Reporting.Render
{
    // NOTE: Many improvements can be made, like discarding empty lines on ... edges or the edges of the whole
    // Example:
    //  2 | 
    //  3 | hello
    //  4 |
    //    | ...
    //  8 |
    //  9 | bye
    // 10 | 
    //
    // Could be shortened into
    //  3 | hello
    //    | ...
    //  9 | bye
    //
    // We could achieve this by splitting all the lines into continuous groups that are separated by '...' and then
    // trimming the edges.

    /// <summary>
    /// An <see cref="IDiagnosticRenderer"/> that prints the <see cref="Diagnostic"/> to console.
    /// </summary>
    public class ConsoleDiagnosticRenderer : IDiagnosticRenderer
    {
        private ConsoleColor decorationColor = ConsoleColor.Gray;
        private ConsoleColor textColor = ConsoleColor.White;
        private ConsoleColor highlightColor = ConsoleColor.Red;
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
                highlightColor = diagnostic.Severity?.Color ?? highlightColor;

                var source = spannedInfo.First().Span.Source;
                Debug.Assert(source != null);

                // We need the largest line number to pad the others
                var lineNumberPaddingLen = (spannedInfo.Select(si => si.Span.End.Line + linesAfter + 1).Max())
                    .ToString().Length;
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
            int? lastAnnotatedLineIndex = null;
            foreach (var info in infos)
            {
                // NOTE: We don't support multiple files yet in a single diagnostic
                // Maybe later we want to
                Debug.Assert(info.Span.Source == source);
                // NOTE: We don't support multiline spans either
                Debug.Assert(info.Span.Start.Line == info.Span.End.Line);
                // NOTE: We don't support multiple annotations in the same line either
                Debug.Assert(lastAnnotatedLineIndex != info.Span.Start.Line);

                var annotatedLineIndex = info.Span.Start.Line;
                lastAnnotatedLineIndex = annotatedLineIndex;

                // Calculate how many lines to go before and after
                int startIndex = Math.Max(lastLineIndex == null ? 0 : lastLineIndex.Value, annotatedLineIndex - linesBefore);
                int endIndex = Math.Min(annotatedLineIndex + linesAfter + 1, source.LineCount);

                if (lastLineIndex != null)
                {
                    var lineDifference = startIndex - lastLineIndex.Value;
                    if (lineDifference > 1)
                    {
                        // The difference between the last and the current line is greater than 1, put dots between
                        RenderLinePad(lineNumberPadding);
                        RenderDecoration("...");
                        Console.WriteLine();
                    }
                    else if (lineDifference == 1)
                    {
                        // The difference between the last and the current line is exactly 1, no sense to dot it out
                        RenderLineNumber(startIndex - 1, lineNumberPadding);
                        RenderSourceLine(source.Line(startIndex - 1).TrimEnd().ToString(), null);
                        Console.WriteLine();
                    }
                }
                
                lastLineIndex = endIndex;

                for (int lineIndex = startIndex; lineIndex != endIndex; ++lineIndex)
                {
                    RenderLineNumber(lineIndex, lineNumberPadding);
                    // TODO: if lineIndex == startIndex, annotate
                    // NOTE: All lines have to be printed per-character to have a uniform tab-size
                    var line = source.Line(lineIndex).TrimEnd().ToString();
                    if (lineIndex == annotatedLineIndex)
                    {
                        RenderSourceLine(line, info);
                        Console.WriteLine();

                        RenderLinePad(lineNumberPadding);
                        RenderSourceLineAnnotation(line, info);
                    }
                    else
                    {
                        RenderSourceLine(line, null);
                    }
                    Console.WriteLine();
                }
            }
        }

        private void RenderLinePad(string lineNumberPadding) =>
            RenderDecoration($"{lineNumberPadding} │ ");

        private void RenderLineNumber(int lineIndex, string lineNumberPadding) =>
            RenderDecoration($"{(lineIndex + 1).ToString().PadLeft(lineNumberPadding.Length)} │ ");

        private void RenderHint(HintDiagnosticInfo hint) => RenderText($"hint: {hint.Message}");

        private void RenderSourceLine(ReadOnlySpan<char> text, SpannedDiagnosticInfo? info)
        {
            Console.ForegroundColor = textColor;
            var lineCursor = new LineCursor();
            (int Start, int End)? highlightSpan = info is PrimaryDiagnosticInfo p ? (p.Span.Start.Column, p.Span.End.Column) : null;
            for (int i = 0; i < text.Length; ++i)
            {
                if (highlightSpan.HasValue)
                {
                    var span = highlightSpan.Value;
                    Console.ForegroundColor = (i >= span.Start && i < span.End) ? highlightColor : textColor;
                }
                char ch = text[i];
                if (lineCursor.Append(ch, out var advance))
                {
                    Console.Write(new string(' ', advance));
                }
                else
                {
                    Console.Write(ch);
                }
            }
            Console.ResetColor();
        }

        private void RenderSourceLineAnnotation(ReadOnlySpan<char> text, SpannedDiagnosticInfo info)
        {
            Console.ForegroundColor = decorationColor;
            var lineCursor = new LineCursor();
            var isPrimary = info is PrimaryDiagnosticInfo;
            (int Start, int End) span = (info.Span.Start.Column, info.Span.End.Column);
            for (int i = 0; i < span.End; ++i)
            {
                char ch = i >= text.Length ? ' ' : text[i];
                var filler = (i >= span.Start && i < span.End) ? (isPrimary ? '^' : '-') : ' ';
                if (lineCursor.Append(ch, out var advance))
                {
                    Console.Write(new string(filler, advance));
                }
                else
                {
                    Console.Write(filler);
                }
            }
            Console.ForegroundColor = isPrimary ? highlightColor : textColor;
            Console.Write($" {info.Message}");
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
