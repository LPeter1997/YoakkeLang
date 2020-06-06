using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Ast
{
    partial class Statement
    {
        /// <summary>
        /// An <see cref="Exception"/> that has been wrapped up in a <see cref="Statement"/>, so it can
        /// appear in statement position.
        /// </summary>
        public class Expression_ : Statement
        {
            /// <summary>
            /// The wrapped up <see cref="Expression"/>.
            /// </summary>
            public Expression Expression { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Expression_"/>.
            /// </summary>
            /// <param name="expression">The <see cref="Expression"/> to wrap up.</param>
            public Expression_(Expression expression)
            {
                Expression = expression;
            }
        }
    }
}
