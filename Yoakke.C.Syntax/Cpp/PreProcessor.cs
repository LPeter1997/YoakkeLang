using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.C.Syntax.Cpp
{
    /// <summary>
    /// Pre-processes a sequence of <see cref="Token"/>s, returning the pre-processed <see cref="Token"/>s.
    /// </summary>
    public class PreProcessor
    {
        private SourceFile source;
        private IReadOnlyList<Token> tokens;
        private int tokenIndex = -1;

        /// <summary>
        /// Initializes a new <see cref="PreProcessor"/>.
        /// </summary>
        /// <param name="tokens">The <see cref="IEnumerable{Token}"/>s to pre-process.</param>
        public PreProcessor(IEnumerable<Token> tokens)
        {
            this.tokens = tokens.ToArray();
            Debug.Assert(this.tokens.Count > 0);
            source = this.tokens.First().PhysicalSpan.Source;
        }
    }
}
