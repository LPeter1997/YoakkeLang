using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Syntax;

namespace Yoakke.Ast
{
    /// <summary>
    /// Anything in the form of
    /// ```
    /// const Name = Value;
    /// ```
    /// or
    /// ```
    /// const Name: Type = Value;
    /// ```
    /// </summary>
    class ConstDefinition : Declaration
    {
        /// <summary>
        /// The name of the constant defined.
        /// </summary>
        public Token Name { get; set; }
        /// <summary>
        /// The type of the constant defined. Can be null, which means that it will be inferred.
        /// </summary>
        public Expression? Type { get; set; }
        /// <summary>
        /// The value the constant gets defined with.
        /// </summary>
        public Expression Value { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ConstDefinition"/>.
        /// </summary>
        /// <param name="name">The name of the constant defined.</param>
        /// <param name="type">The type of the constant defined.</param>
        /// <param name="value">The value the constant gets defined with.</param>
        public ConstDefinition(Token name, Expression? type, Expression value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }
}
