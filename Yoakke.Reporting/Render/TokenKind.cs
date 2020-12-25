using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Reporting.Render
{
    /// <summary>
    /// The kinds a token can be in the source code.
    /// </summary>
    public enum TokenKind
    {
        Comment,
        Keyword,
        Literal,
        Name,
        Punctuation,
    }
}
