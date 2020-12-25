using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Reporting.Render
{
    /// <summary>
    /// A default <see cref="ISyntaxHighlighter"/> that does not do any highlighting.
    /// </summary>
    public class NullSyntaxHighlighter : ISyntaxHighlighter
    {
        public IEnumerable<ColoredToken> GetHighlightingForLine(SourceFile sourceFile, int lineIndex) =>
            Enumerable.Empty<ColoredToken>();
    }
}
