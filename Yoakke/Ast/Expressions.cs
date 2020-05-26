using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Syntax;

namespace Yoakke.Ast
{
    /// <summary>
    /// An integer literal token.
    /// </summary>
    class IntLiteralExpression : Expression
    {
        /// <summary>
        /// The literal token itself.
        /// </summary>
        public Token Token { get; set; }

        /// <summary>
        /// Initializes a new <see cref="IntLiteralExpression"/>.
        /// </summary>
        /// <param name="token">The literal token.</param>
        public IntLiteralExpression(Token token)
        {
            Token = token;
        }
    }

    /// <summary>
    /// An identifier token.
    /// </summary>
    class IdentifierExpression : Expression
    {
        /// <summary>
        /// The identifier token.
        /// </summary>
        public Token Token { get; set; }

        /// <summary>
        /// Initializes a new <see cref="IdentifierExpression"/>.
        /// </summary>
        /// <param name="token">The identifier token.</param>
        public IdentifierExpression(Token token)
        {
            Token = token;
        }
    }

    /// <summary>
    /// A procedure definition.
    /// They are in the form of:
    /// ```
    /// proc(Args...) { Body }
    /// ```
    /// or
    /// ```
    /// proc(Args...) -> RetType { Body }
    /// ```
    /// </summary>
    class ProcExpression : Expression
    {
        /// <summary>
        /// A single parameter inside a procedure's parameter list.
        /// They are in the form of:
        /// ```
        /// Name: Type
        /// ```
        /// </summary>
        public class Parameter
        {
            /// <summary>
            /// The name <see cref="Token"/> of the <see cref="Parameter"/>.
            /// </summary>
            public Token Name { get; set; }
            /// <summary>
            /// The type <see cref="Expression"/> of the <see cref="Parameter"/>.
            /// </summary>
            public Expression Type { get; set; }

            /// <summary>
            /// Initializes the <see cref="Parameter"/> with the given name and type.
            /// </summary>
            /// <param name="name">The name <see cref="Token"/> of the parameter.</param>
            /// <param name="type">The type <see cref="Expression"/> of the parameter.</param>
            public Parameter(Token name, Expression type)
            {
                Name = name;
                Type = type;
            }
        }

        /// <summary>
        /// The list of <see cref="Parameters"/> this procedure takes.
        /// </summary>
        public List<Parameter> Parameters { get; set; }
        /// <summary>
        /// The return type of this procedure. Can be null for no return value.
        /// </summary>
        public Expression? ReturnType { get; set; }
        /// <summary>
        /// The body of this procedure.
        /// </summary>
        public Expression Body { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ProcExpression"/>.
        /// </summary>
        /// <param name="parameters">The list of parameters the procedure takes.</param>
        /// <param name="returnType">The return type of the procedure.</param>
        /// <param name="body">The body of the procedure.</param>
        public ProcExpression(List<Parameter> parameters, Expression? returnType, Expression body)
        {
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
        }
    }

    /// <summary>
    /// An expression consisting of a list of statements and an optional value.
    /// Looks like:
    /// ```
    /// {
    ///     statements...
    ///     optional Expression
    /// }
    /// ```
    /// </summary>
    class BlockExpression : Expression
    {
        /// <summary>
        /// The list of <see cref="Statement"/>s this <see cref="BlockExpression"/> consists of.
        /// </summary>
        public List<Statement> Statements { get; set; }
        /// <summary>
        /// The value this <see cref="BlockExpression"/> evaluates to. Can be null, which means it 
        /// evaluates to nothing.
        /// </summary>
        public Expression? Value { get; set; }

        /// <summary>
        /// Initializes a new <see cref="BlockExpression"/>.
        /// </summary>
        /// <param name="statements">The list of <see cref="Statement"/>s this block consists of.</param>
        /// <param name="value">The expression this block evaluates to.</param>
        public BlockExpression(List<Statement> statements, Expression? value)
        {
            Statements = statements;
            Value = value;
        }
    }
}
