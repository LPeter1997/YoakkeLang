using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Reporting.Detail;
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
    /// An <see cref="IDiagnosticRenderer"/> that prints the <see cref="Diagnostic"/> as text.
    /// </summary>
    public class TextDiagnosticRenderer : IDiagnosticRenderer
    {
        private abstract class LinePrimitive { }
        private class SourceLine : LinePrimitive 
        {
            public SourceFile Source { get; set; }
            public int Line { get; set; }
        }
        private class DotLine : LinePrimitive { }
        private class AnnotationLine : LinePrimitive
        {
            public int AnnotatedLine { get; set; }
            public IEnumerable<SpannedDiagnosticInfo> Annotations { get; set; }
        }

        /// <summary>
        /// The <see cref="TextWriter"/> this renderer writes to.
        /// </summary>
        public TextWriter Writer { get; }
        /// <summary>
        /// How many lines to print before and after the relevant lines.
        /// </summary>
        public int SurroundingLines { get; set; } = 1;
        /// <summary>
        /// How big of a gap can we connect up between annotated lines.
        /// </summary>
        public int ConnectUpLines { get; set; } = 1;
        /// <summary>
        /// The tab size to use in spaces.
        /// </summary>
        public int TabSize { get; set; }

        private ColoredBuffer buffer;

        /// <summary>
        /// Initializes a new <see cref="TextDiagnosticRenderer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        public TextDiagnosticRenderer(TextWriter writer)
        {
            Writer = writer;
            buffer = new ColoredBuffer();
        }

        /// <summary>
        /// Initializes a new <see cref="TextDiagnosticRenderer"/> to write to stderr.
        /// </summary>
        public TextDiagnosticRenderer()
            : this(Console.Error)
        {
        }

        public void Render(Diagnostic diagnostic)
        {
            buffer.Clear();

            // First we write the head, something like
            // error[E123]: Type mismatch!
            RenderDiagnosticHead(diagnostic);

            // Now get every spanned information grouped by the file, ordered by their position
            var spannedInfo = diagnostic
                .Information
                .OfType<SpannedDiagnosticInfo>()
                .OrderBy(si => si.Span.Start)
                .GroupBy(si => si.Span.Source);

            // Print each group
            foreach (var group in spannedInfo) RenderSpannedGroup(group);

            // Finally we print any hints
            var hints = diagnostic.Information.OfType<HintDiagnosticInfo>();
            foreach (var hint in hints) RenderHint(hint);

            // Dump to output
            buffer.OutputTo(Writer);
        }

        private void RenderDiagnosticHead(Diagnostic diagnostic)
        {
            if (diagnostic.Severity == null && diagnostic.Message == null) return;

            if (diagnostic.Severity != null)
            {
                buffer.ForegroundColor = diagnostic.Severity.Color;
                buffer.Write(diagnostic.Severity.Description);
                if (diagnostic.Code != null) buffer.Write($"[{diagnostic.Code}]");
                buffer.ResetColor();
                if (diagnostic.Message != null) buffer.Write(": ");
            }
            if (diagnostic.Message != null) buffer.Write(diagnostic.Message);
            buffer.WriteLine();
        }

        // Spanned infos related to a single file
        private void RenderSpannedGroup(IEnumerable<SpannedDiagnosticInfo> infos)
        {
            var sourceFile = infos.First().Span.Source;
            Debug.Assert(sourceFile != null);

            // Generate all line primitives
            var linePrimitives = CollectLinesToRender(infos).ToList();
            // Find the largest line index printed
            var maxLineIndex = linePrimitives.OfType<SourceLine>().Select(l => l.Line).Max();
            // Create a padding to fit all line numbers from the largest of the group
            var lineNumberPadding = new string(' ', (maxLineIndex + 1).ToString().Length);

            // Print the ┌─ <file name>
            buffer.WriteLine($"{lineNumberPadding} ┌─ {sourceFile.Path}");
            // Pad lines
            buffer.WriteLine($"{lineNumberPadding} │");
            // Print all the line primitives
            foreach (var line in linePrimitives)
            {
                switch (line)
                {
                case SourceLine sourceLine:
                    buffer.WriteLine($"{(sourceLine.Line + 1).ToString().PadLeft(lineNumberPadding.Length)} │");
                    break;

                case AnnotationLine annotation:
                    buffer.WriteLine($"{lineNumberPadding} │");
                    break;

                case DotLine dotLine:
                    buffer.WriteLine($"{lineNumberPadding} │ ...");
                    break;
                }
            }
            // Pad lines
            buffer.WriteLine($"{lineNumberPadding} │");
        }

        // Collects all the line subgroups
        private IEnumerable<LinePrimitive> CollectLinesToRender(IEnumerable<SpannedDiagnosticInfo> infos)
        {
            // We need to group the spanned informations per line
            var groupedInfos = infos.GroupBy(si => si.Span.Start.Line);
            var sourceFile = infos.First().Span.Source;
            Debug.Assert(sourceFile != null);

            // Now we collect each line primitive
            int? lastLineIndex = null;
            foreach (var infoGroup in groupedInfos)
            {
                // First we determine the range we need to print for this info
                var currentLineIndex = infoGroup.Key;
                var minLineIndex = Math.Max(lastLineIndex ?? 0, currentLineIndex - SurroundingLines);
                var maxLineIndex = Math.Min(sourceFile.LineCount, currentLineIndex + SurroundingLines);
                // Determine if we need dotting or a line in between
                if (lastLineIndex != null)
                {
                    var difference = minLineIndex - lastLineIndex.Value;
                    if (difference <= ConnectUpLines)
                    {
                        // Difference is negligible, connect them up, no reason to dot it out
                        for (int i = 0; i < ConnectUpLines; ++i)
                        {
                            yield return new SourceLine { Source = sourceFile, Line = lastLineIndex.Value + i };
                        }
                    }
                    else
                    {
                        // Bigger difference, dot out
                        yield return new DotLine { };
                    }
                }
                lastLineIndex = maxLineIndex;
                // Now we need to print all the relevant lines
                for (int i = minLineIndex; i < maxLineIndex; ++i)
                {
                    yield return new SourceLine { Source = sourceFile, Line = i };
                    // If this was an annotated line, yield the annotation
                    if (i == infoGroup.Key) yield return new AnnotationLine { AnnotatedLine = i, Annotations = infoGroup };
                }
            }
        }

        private void RenderHint(HintDiagnosticInfo hint) => buffer.WriteLine($"hint: {hint.Message}");
    }
}
