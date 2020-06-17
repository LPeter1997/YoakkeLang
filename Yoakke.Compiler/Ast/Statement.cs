﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Compiler.Ast
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
            /// True, if the <see cref="Expression"/> is followed by a semicolon.
            /// </summary>
            public bool HasSemicolon { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Expression_"/>.
            /// </summary>
            /// <param name="expression">The <see cref="Expression"/> to wrap up.</param>
            /// <param name="hasSemicolon">True, if the <see cref="Expression"/> is followed by a semicolon.</param>
            public Expression_(Expression expression, bool hasSemicolon)
            {
                Expression = expression;
                HasSemicolon = hasSemicolon;
            }
        }
    }
}
