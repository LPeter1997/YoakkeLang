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
        /// <summary>
        /// Variable declaration and definition.
        /// </summary>
        public class Var : Statement
        {
            /// <summary>
            /// The variable name.
            /// </summary>
            public readonly string Name;
            /// <summary>
            /// The type of the variable.
            /// </summary>
            public readonly Expression? Type;
            /// <summary>
            /// The initial value.
            /// </summary>
            public readonly Expression? Value;

            public Var(ParseTree.Node? parseTreeNode, string name, Expression? type, Expression? value)
                : base(parseTreeNode)
            {
                Name = name;
                Type = type;
                Value = value;
            }
        }

        /// <summary>
        /// Return <see cref="Statement"/> with an optional value.
        /// </summary>
        public class Return : Statement
        {
            /// <summary>
            /// The returned value.
            /// </summary>
            public readonly Expression? Value;

            public Return(ParseTree.Node? parseTreeNode, Expression? value)
                : base(parseTreeNode)
            {
                Value = value;
            }
        }

        /// <summary>
        /// An <see cref="Expression"/> wrapped into a <see cref="Statement"/>.
        /// </summary>
        public class Expression_ : Statement
        {
            /// <summary>
            /// The wrapped expression.
            /// </summary>
            public readonly Expression Expression;
            /// <summary>
            /// True, if this <see cref="Statement"/> wrapped <see cref="Expression"/> is semicolon-terminated.
            /// </summary>
            public readonly bool HasSemicolon;

            public Expression_(ParseTree.Node? parseTreeNode, Expression expr, bool hasSemicolon)
                : base(parseTreeNode)
            {
                Expression = expr;
                HasSemicolon = hasSemicolon;
            }
        }
    }
}
