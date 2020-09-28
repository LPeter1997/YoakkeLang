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
        public abstract Span Span { get; }
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

        /// <summary>
        /// All of the children of this <see cref="Node"/>.
        /// </summary>
        public abstract IEnumerable<IParseTreeElement> Children { get; }
    }

    /// <summary>
    /// A <see cref="Node"/> that has an optional trailing comma.
    /// </summary>
    public class WithComma<T> : Node where T : Node
    {
        public override Span Span => new Span(Element.Span, Comma?.Span ?? Element.Span);
        public override IEnumerable<IParseTreeElement> Children
        {
            get
            {
                yield return Element;
                if (Comma != null) yield return Comma;
            }
        }

        /// <summary>
        /// The element.
        /// </summary>
        public readonly T Element;
        /// <summary>
        /// The optional comma.
        /// </summary>
        public readonly Token? Comma;

        public WithComma(T element, Token? comma)
        {
            Element = element;
            Comma = comma;
        }
    }
}
