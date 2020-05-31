using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Syntax
{
    /// <summary>
    /// Settings for annotating the source code for warnings and errors.
    /// </summary>
    class AnnotationSettings
    {
        /// <summary>
        /// Returns the default settings.
        /// </summary>
        public static AnnotationSettings Default => new AnnotationSettings();

        /// <summary>
        /// The size of the tab character. Four by default.
        /// </summary>
        public int TabSize { get; set; } = 4;
        /// <summary>
        /// The lines to print before the annotated line.
        /// </summary>
        public int LinesBefore { get; set; } = 1;
        /// <summary>
        /// The lines to print after the annotated line.
        /// </summary>
        public int LinesAfter { get; set; } = 1;
        /// <summary>
        /// The padding character to use to pad line numbers.
        /// </summary>
        public char LineNumberPadding { get; set; } = ' ';
        /// <summary>
        /// The separator string that separates line numbers from the line text.
        /// </summary>
        public string LineNumberSeparator { get; set; } = " | ";
        /// <summary>
        /// The arrow body character.
        /// </summary>
        public char ArrowBody { get; set; } = '_';
        /// <summary>
        /// The arrow head character.
        /// </summary>
        public char ArrowHead { get; set; } = '^';
        /// <summary>
        /// The separator string that separates the <see cref="ArrowText"/> from the arrow.
        /// </summary>
        public string ArrowTextSeparator { get; set; } = " ";
        /// <summary>
        /// The text appended after the arrow.
        /// </summary>
        public string ArrowText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Utility for annotating source code.
    /// </summary>
    static class Annotation
    {
        /// <summary>
        /// Annotates the source code using the default <see cref="AnnotationSettings"/>.
        /// </summary>
        /// <param name="position">The position to annotate.</param>
        /// <returns>The annotated source code.</returns>
        public static string Annotate(Position position) =>
            Annotate(position, AnnotationSettings.Default);

        /// <summary>
        /// Annotates the source code using the given <see cref="AnnotationSettings"/>.
        /// </summary>
        /// <param name="position">The position to annotate.</param>
        /// <param name="settings">The settings to use for the annotation.</param>
        /// <returns>The annotated source code.</returns>
        public static string Annotate(Position position, AnnotationSettings settings)
        {
            var result = new StringBuilder();
            int startIndex = Math.Max(position.Line - settings.LinesBefore, 0);
            int endIndex = Math.Min(position.Line + settings.LinesAfter + 1, position.Source.LineCount);
            int paddingLength = (endIndex + 1).ToString().Length;

            for (int lineIndex = startIndex; lineIndex < endIndex; ++lineIndex)
            {
                var line = position.Source.Line(lineIndex);
                WriteLineNumber(result, settings, lineIndex + 1, paddingLength);
                if (lineIndex == position.Line)
                {
                    int column = 0;
                    IterateString(line, settings, (ch, idx, pos) =>
                    {
                        result.Append(ch);
                        if (idx == position.Column) column = pos;
                    });
                    WriteLineNumber(result, settings, null, paddingLength);
                    result
                        .Append(settings.ArrowBody, column)
                        .Append(settings.ArrowHead)
                        .Append(settings.ArrowTextSeparator)
                        .Append(settings.ArrowText)
                        .Append('\n');
                }
                else
                {
                    IterateString(line, settings, (ch, _idx, _pos) => result.Append(ch));
                }
            }

            return result.ToString().Trim();
        }

        private static void IterateString(ReadOnlySpan<char> text, AnnotationSettings settings, Action<char, int, int> action)
        {
            int pos = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];
                if (ch == '\t')
                {
                    var tabSize = settings.TabSize - pos % settings.TabSize;
                    for (int j = 0; j < tabSize; ++j) action(' ', i, pos + j);
                    pos += tabSize;
                }
                else
                {
                    action(ch, i, pos);
                    ++pos;
                }
            }
        }

        private static void WriteLineNumber(StringBuilder builder, AnnotationSettings settings, int? line, int padding)
        {
            if (line == null)
            {
                builder.Append(' ', padding);
            }
            else
            {
                builder.Append(line.Value.ToString().PadLeft(padding, settings.LineNumberPadding));
            }

            builder.Append(settings.LineNumberSeparator);
        }
    }
}
