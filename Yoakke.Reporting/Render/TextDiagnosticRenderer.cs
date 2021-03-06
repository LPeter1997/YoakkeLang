﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public int TabSize { get; set; } = 4;
        /// <summary>
        /// The <see cref="ISyntaxHighlighter"/> to use for the source code.
        /// </summary>
        public ISyntaxHighlighter SyntaxHighlighter { get; set; } = new NullSyntaxHighlighter();

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

            // Finally we print any footnotes
            var footnotes = diagnostic.Information.OfType<FootnoteDiagnosticInfo>();
            foreach (var footnote in footnotes) RenderFootnote(footnote);

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
            buffer.Write($"{lineNumberPadding} ┌─ {sourceFile.Path}");
            // If there is a primary info, write the line and column
            var primaryInfo = infos.FirstOrDefault(info => info.Severity != null);
            if (primaryInfo != null) buffer.Write($":{primaryInfo.Span.Start.Line + 1}:{primaryInfo.Span.Start.Column + 1}");
            buffer.WriteLine();
            // Pad lines
            buffer.WriteLine($"{lineNumberPadding} │");
            // Print all the line primitives
            foreach (var line in linePrimitives)
            {
                switch (line)
                {
                case SourceLine sourceLine:
                    buffer.Write($"{(sourceLine.Line + 1).ToString().PadLeft(lineNumberPadding.Length)} │ ");
                    RenderSourceLine(sourceLine);
                    buffer.WriteLine();
                    break;

                case AnnotationLine annotation:
                    RenderAnnotationLines(annotation, $"{lineNumberPadding} │ ");
                    break;

                case DotLine dotLine:
                    buffer.WriteLine($"{lineNumberPadding} │ ...");
                    break;
                }
            }
            // Pad lines
            buffer.WriteLine($"{lineNumberPadding} │");
        }

        private void RenderSourceLine(SourceLine sourceLine)
        {
            var xOffset = buffer.CursorX;
            // Print the source line with a default color
            buffer.ForegroundColor = ConsoleColor.White;
            var line = sourceLine.Source.Line(sourceLine.Line);
            var lineCur = new LineCursor { TabSize = TabSize };
            foreach (var ch in line)
            {
                if (ch == '\r' || ch == '\n') break;
                if (lineCur.Append(ch, out var advance)) buffer.CursorX += advance;
                else buffer.Write(ch);
            }
            // Do syntax highlighting, if needed
            var tokenInfo = SyntaxHighlighter.GetHighlightingForLine(sourceLine.Source, sourceLine.Line).ToList();
            if (tokenInfo.Count > 0)
            {
                // There are tokens to highlight
                var charIdx = 0;
                lineCur.Column = 0;
                var tokenInfoList = tokenInfo.OrderBy(ti => ti.StartIndex).ToList();
                foreach (var token in tokenInfoList)
                {
                    // Skip until the next token
                    for (; charIdx < token.StartIndex; ++charIdx)
                    {
                        lineCur.Append(line[charIdx], out var advance);
                    }
                    // Go through the token
                    var tokenStart = lineCur.Column;
                    for (; charIdx < token.StartIndex + token.Length; ++charIdx)
                    {
                        lineCur.Append(line[charIdx], out var advance);
                    }
                    var tokenEnd = lineCur.Column;
                    // Recolor the token
                    buffer.ForegroundColor = TokenKindToColor(token.Kind);
                    buffer.Recolor(xOffset + tokenStart, buffer.CursorY, tokenEnd - tokenStart, 1);
                }
            }
            buffer.ResetColor();
        }

        private void RenderAnnotationLines(AnnotationLine annotationLine, string prefix)
        {
            var sourceFile = annotationLine.Annotations.First().Span.Source;
            Debug.Assert(sourceFile != null);
            var line = sourceFile.Line(annotationLine.AnnotatedLine).TrimEnd();

            // Order annotations by starting position
            var annotationsOrdered = annotationLine.Annotations.OrderBy(si => si.Span.Start).ToList();
            // Now we draw the arrows to their correct places under the annotated line
            // Also collect physical column positions to extend the arrows
            var arrowHeadColumns = new List<(int Column, SpannedDiagnosticInfo Info)>();
            buffer.Write(prefix);
            var lineCur = new LineCursor { TabSize = TabSize };
            var charIdx = 0;
            foreach (var annot in annotationsOrdered)
            {
                // From the last character index until the start of this annotation we need to fill with spaces
                for (; charIdx < annot.Span.Start.Column; ++charIdx)
                {
                    if (charIdx < line.Length)
                    {
                        // Still in range of the line
                        lineCur.Append(line[charIdx], out var advance);
                        buffer.CursorX += advance;
                    }
                    else
                    {
                        // After the line
                        buffer.CursorX += 1;
                    }
                }
                // Now we are inside the span
                var arrowHead = annot.Severity != null ? '^' : '-';
                var startColumn = buffer.CursorX;
                arrowHeadColumns.Add((startColumn, annot));
                if (annot.Severity != null) buffer.ForegroundColor = annot.Severity.Color;
                for (; charIdx < annot.Span.End.Column; ++charIdx)
                {
                    if (charIdx < line.Length)
                    {
                        // Still in range of the line
                        lineCur.Append(line[charIdx], out var advance);
                        for (int i = 0; i < advance; ++i) buffer.Write(arrowHead);
                    }
                    else
                    {
                        // After the line
                        buffer.Write(arrowHead);
                    }
                }
                var endColumn = buffer.CursorX;
                if (annot.Severity != null)
                {
                    // Recolor the source line too
                    buffer.Recolor(startColumn, buffer.CursorY - 1, endColumn - startColumn, 1);
                    buffer.ResetColor();
                }
            }
            // Now we are done with arrows in the line, it's time to do the arrow bodies downwards
            // The first one will have N, the last 0 length bodies, decreasing by one
            // The last one just has the message inline
            {
                var lastAnnot = annotationsOrdered.Last();
                if (lastAnnot.Message != null) buffer.Write($" {lastAnnot.Message}");
                buffer.WriteLine();
            }
            // From now on all previous ones will be one longer than the ones later
            int arrowBaseLine = buffer.CursorY;
            int arrowBodyLength = 0;
            // We only consider annotations with messages
            foreach (var (col, annot) in arrowHeadColumns.SkipLast(1).Reverse().Where(a => a.Info.Message != null))
            {
                if (annot.Severity != null) buffer.ForegroundColor = annot.Severity.Color;
                // Draw the arrow
                buffer.Fill(col, arrowBaseLine, 1, arrowBodyLength, '│');
                buffer.Plot(col, arrowBaseLine + arrowBodyLength, '└');
                arrowBodyLength += 1;
                // Append the message
                buffer.Write($" {annot.Message}");
                if (annot.Severity != null) buffer.ResetColor();
            }
            // Fill the in between lines with the prefix
            for (int i = 0; i < arrowBodyLength; ++i)
            {
                buffer.WriteAt(0, arrowBaseLine + i, prefix);
            }
            // Reset cursor position
            buffer.CursorX = 0;
            buffer.CursorY = arrowBaseLine + arrowBodyLength;
        }

        // Collects all the line subgroups
        private IEnumerable<LinePrimitive> CollectLinesToRender(IEnumerable<SpannedDiagnosticInfo> infos)
        {
            // We need to group the spanned informations per line
            var groupedInfos = infos.GroupBy(si => si.Span.Start.Line).ToList();
            var sourceFile = infos.First().Span.Source;
            Debug.Assert(sourceFile != null);

            // Now we collect each line primitive
            int? lastLineIndex = null;
            for (int j = 0; j < groupedInfos.Count; ++j)
            {
                var infoGroup = groupedInfos[j];
                // First we determine the range we need to print for this info
                var currentLineIndex = infoGroup.Key;
                var minLineIndex = Math.Max(lastLineIndex ?? 0, currentLineIndex - SurroundingLines);
                var maxLineIndex = Math.Min(sourceFile.LineCount, currentLineIndex + SurroundingLines + 1);
                if (j < groupedInfos.Count - 1)
                {
                    // There's a chance we step over to the next annotation
                    var nextGroupLineIndex = groupedInfos[j + 1].Key;
                    maxLineIndex = Math.Min(maxLineIndex, nextGroupLineIndex);
                }
                // Determine if we need dotting or a line in between
                if (lastLineIndex != null)
                {
                    var difference = minLineIndex - lastLineIndex.Value;
                    if (difference <= ConnectUpLines)
                    {
                        // Difference is negligible, connect them up, no reason to dot it out
                        for (int i = 0; i < difference; ++i)
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

        private void RenderFootnote(FootnoteDiagnosticInfo hint) => buffer.WriteLine(hint.Message);

        private ConsoleColor TokenKindToColor(TokenKind tokenKind) => tokenKind switch
        {
            TokenKind.Comment => ConsoleColor.DarkGreen,
            TokenKind.Keyword => ConsoleColor.Magenta,
            TokenKind.Literal => ConsoleColor.Blue,
            TokenKind.Name => ConsoleColor.Cyan,
            TokenKind.Punctuation => ConsoleColor.White,
            TokenKind.Other => ConsoleColor.White,
            TokenKind.Operator => ConsoleColor.DarkCyan,
            _ => throw new NotImplementedException(),
        };
    }
}
