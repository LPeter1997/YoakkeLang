using System.Collections.Generic;
using Yoakke.Text;

namespace Yoakke.Reporting.Render
{
    /// <summary>
    /// An interface to allow custom syntax highlighting in the printed source code.
    /// </summary>
    public interface ISyntaxHighlighter
    {
        /// <summary>
        /// Asks for syntax highlighting for a single source line.
        /// </summary>
        /// <param name="sourceFile">The <see cref="SourceText"/> that contains the line.</param>
        /// <param name="lineIndex">The index of the line in the <see cref="SourceText"/>.</param>
        /// <returns>A list of <see cref="ColoredToken"/>s in any order. Does not have to assign a token
        /// to every single character.</returns>
        public IEnumerable<ColoredToken> GetHighlightingForLine(SourceText sourceFile, int lineIndex);
    }
}
