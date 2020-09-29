using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Syntax.Ast
{
    /// <summary>
    /// Base class for declaration <see cref="Statement"/>s.
    /// </summary>
    public abstract partial class Declaration : Statement
    {
        protected Declaration(ParseTree.Node? parseTreeNode)
            : base(parseTreeNode)
        {
        }
    }

    partial class Declaration
    {
        // TODO: Fill
    }
}
