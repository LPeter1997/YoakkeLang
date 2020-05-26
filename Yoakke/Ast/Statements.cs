using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Ast
{
    /// <summary>
    /// A list of statements.
    /// </summary>
    class ProgramDeclaration : Declaration
    {
        /// <summary>
        /// The list of <see cref="Declaration"/>s the <see cref="ProgramDeclaration"/> consists of.
        /// </summary>
        public List<Declaration> Declarations { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ProgramDeclaration"/>.
        /// </summary>
        /// <param name="declarations">The list of <see cref="Declaration"/>s the program consists of.</param>
        public ProgramDeclaration(List<Declaration> declarations)
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
        /// <param name="expression">The <see cref="Expression"/> to wrap up.</param>
        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
        }
    }
}
