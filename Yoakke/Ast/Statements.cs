using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Ast
{
    /// <summary>
    /// A list of statements.
    /// </summary>
    class ProgramStatement : Statement
    {
        /// <summary>
        /// The list of <see cref="Declaration"/>s the <see cref="ProgramStatement"/> consists of.
        /// </summary>
        public List<Declaration> Declarations { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ProgramStatement"/>.
        /// </summary>
        /// <param name="statements">The list of other <see cref="Statement"/>s this one consists of.</param>
        public ProgramStatement(List<Declaration> declarations)
        {
            Declarations = declarations;
        }
    }

    /// <summary>
    /// An <see cref="Exception"/> that has been wrapped up in a <see cref="Statement"/>, so it can
    /// appear in statement position.
    /// </summary>
    class ExpressionStatement : Statement
    {
        /// <summary>
        /// The wrapped up <see cref="Expression"/>.
        /// </summary>
        public Expression Expression { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ExpressionStatement"/>.
        /// </summary>
        /// <param name="expression">The expression to wrap up.</param>
        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
        }
    }
}
