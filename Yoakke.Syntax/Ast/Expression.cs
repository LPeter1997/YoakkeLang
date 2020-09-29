using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Syntax.Ast
{
    /// <summary>
    /// Base class for expressions.
    /// </summary>
    public abstract partial class Expression : Node
    {
        protected Expression(ParseTree.Node? parseTreeNode)
            : base(parseTreeNode)
        {
        }
    }

    partial class Expression
    {
        // TODO: Fill
    }
}
