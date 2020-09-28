using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax.ParseTree
{
    /// <summary>
    /// The base class for every parse tree node.
    /// </summary>
    public abstract class Node : IParseTreeElement
    {
        /// <summary>
        /// The source <see cref="Span"/> of this <see cref="Node"/>.
        /// </summary>
        public abstract Span Span { get; }

        /// <summary>
        /// All of the children of this <see cref="Node"/>.
        /// </summary>
        public abstract IEnumerable<IParseTreeElement> Children { get; }

        public virtual IEnumerable<Token> Tokens
        {
            get
            {
                foreach (var child in Children)
                {
                    if (child is Token t) yield return t;
                    else if (child is Node n)
                    {
                        foreach (var token in child.Tokens) yield return token;
                    }
                    else throw new InvalidOperationException();
                }
            }
        }
    }
}
