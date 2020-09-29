using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Syntax.Ast
{
    /// <summary>
    /// Base class for statements.
    /// </summary>
    public abstract partial class Statement : Node
    {
        protected Statement(ParseTree.Node? parseTreeNode) 
            : base(parseTreeNode)
        {
        }
    }

    partial class Statement
    {
        // TODO: Fill
    }
}
