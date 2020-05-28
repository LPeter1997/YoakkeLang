using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Ast
{
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
