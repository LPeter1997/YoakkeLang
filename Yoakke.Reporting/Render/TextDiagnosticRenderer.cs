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
        /// <summary>
        /// The <see cref="TextWriter"/> this renderer writes to.
        /// </summary>
        public TextWriter Writer { get; }
        /// <summary>
        /// How many lines to print before and after the relevant lines.
        /// </summary>
        public int SurroundingLines { get; set; } = 1;
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
            foreach (var group in spannedInfo) RenderSpannedGroup(group.Key, group);

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

        private void RenderSpannedGroup(SourceFile? sourceFile, IEnumerable<SpannedDiagnosticInfo> infos)
        {
            Debug.Assert(sourceFile != null);

            // Create a padding to fit all line numbers from the largest of the group
            var lineNumberPadding = new string(' ', (infos.Last().Span.End.Line + 1).ToString().Length);

            // Print the ┌─ <file name>
            buffer.WriteLine($"{lineNumberPadding} ┌─ {sourceFile.Path}");
            // Pad lines
            buffer.WriteLine($"{lineNumberPadding} │");
            // TODO: Print annotated lines
            // Pad lines
            buffer.WriteLine($"{lineNumberPadding} │");
        }

        private void RenderHint(HintDiagnosticInfo hint) => buffer.WriteLine($"hint: {hint.Message}");
    }
}
