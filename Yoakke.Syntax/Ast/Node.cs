using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Syntax.Ast
{
    /// <summary>
    /// The base class for every AST node.
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// The parse tree's node this one originates from.
        /// </summary>
        public readonly ParseTree.Node? ParseTreeNode;

        public Node(ParseTree.Node? parseTreeNode)
        {
            ParseTreeNode = parseTreeNode;
        }
    }
}
