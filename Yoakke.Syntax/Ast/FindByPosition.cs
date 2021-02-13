using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax.Ast
{
    /// <summary>
    /// Utility to find a <see cref="Node"/> by position.
    /// </summary>
    public class FindByPosition : Visitor<Node>
    {
        // TODO: Doc
        public static Node? Find(Node root, Position position)
        {
            var finder = new FindByPosition(position);
            finder.Visit(root);
            return finder.found;
        }

        private Position position;
        private Node? found;

        private FindByPosition(Position position)
        {
            this.position = position;
        }

        protected override Node? Visit(Declaration declaration) =>
            CanBeInHere(declaration) ? base.Visit(declaration) : null;
        protected override Node? Visit(Statement statement) =>
            CanBeInHere(statement) ? base.Visit(statement) : null;
        protected override Node? Visit(Expression expression) =>
            CanBeInHere(expression) ? base.Visit(expression) : null;

        // Override leaf nodes
        protected override Node? Visit(Expression.Identifier ident) => found = ident;
        protected override Node? Visit(Expression.Literal lit) => found = lit;

        private bool CanBeInHere(Node node)
        {
            var parseTreeNode = node.ParseTreeNode;
            // Certainly know it's not here
            if (parseTreeNode != null && !parseTreeNode.Span.Contains(position)) return false;
            // Could be in the children
            return true;
        }
    }
}
