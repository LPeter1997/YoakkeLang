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
        /// <summary>
        /// A full file's AST.
        /// </summary>
        public class File : Declaration
        {
            /// <summary>
            /// The list of <see cref="Statement"/>s.
            /// </summary>
            public readonly IReadOnlyList<Statement> Statements;

            public File(ParseTree.Node node, IReadOnlyList<Statement> statements)
                : base(node)
            {
                Statements = statements;
            }
        }

        /// <summary>
        /// Constant definition.
        /// </summary>
        public class Const : Declaration
        {
            /// <summary>
            /// The constant name.
            /// </summary>
            public readonly string Name;
            /// <summary>
            /// The type of the constant.
            /// </summary>
            public readonly Expression? Type;
            /// <summary>
            /// The assigned value.
            /// </summary>
            public readonly Expression Value;

            public Const(ParseTree.Node parseTreeNode, string name, Expression? type, Expression value)
                : base(parseTreeNode)
            {
                Name = name;
                Type = type;
                Value = value;
            }
        }
    }
}
