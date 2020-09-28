using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax.ParseTree
{
    /// <summary>
    /// A tag interface for everything that the parse-tree can contain.
    /// </summary>
    public interface IParseTreeElement
    {
        /// <summary>
        /// The <see cref="Span"/> of this element.
        /// </summary>
        public Span Span { get; }
        /// <summary>
        /// The <see cref="Token"/>s this element consists of.
        /// </summary>
        public IEnumerable<Token> Tokens { get; }
    }
}
