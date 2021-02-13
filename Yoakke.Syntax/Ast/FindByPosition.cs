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
        public static Node? Find(Node root, Position position) => new FindByPosition(position).Visit(root);

        private Position position;

        private FindByPosition(Position position)
        {
            this.position = position;
        }

        protected override Node? Visit(Declaration declaration) => CheckAndVisit(declaration);
        protected override Node? Visit(Statement statement) => CheckAndVisit(statement);
        protected override Node? Visit(Expression expression) => CheckAndVisit(expression);

        // Override leaf nodes
        protected override Node? Visit(Expression.Identifier ident) => ident;
        protected override Node? Visit(Expression.Literal lit) => lit;

        private Node? CheckAndVisit(Node node)
        {
            var parseTreeNode = node.ParseTreeNode;
            // Certainly know it's not here
            if (parseTreeNode != null && !parseTreeNode.Span.Contains(position)) return null;
            // Could be in the children
            return base.Visit(node);
        }
    }
}
