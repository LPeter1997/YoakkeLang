using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Semantic;
using Yoakke.Syntax;

namespace Yoakke.Ast
{
    partial class Expression
    {
        /// <summary>
        /// An integer literal token.
        /// </summary>
        public class IntLit : Expression
        {
            /// <summary>
            /// The literal token itself.
            /// </summary>
            public Token Token { get; set; }

            /// <summary>
            /// Initializes a new <see cref="IntLit"/>.
            /// </summary>
            /// <param name="token">The literal token.</param>
            public IntLit(Token token)
            {
                Token = token;
            }
        }

        /// <summary>
        /// A string literal token.
        /// </summary>
        public class StrLit : Expression
        {
            /// <summary>
            /// The literal token itself.
            /// </summary>
            public Token Token { get; set; }

            /// <summary>
            /// Initializes a new <see cref="StrLit"/>.
            /// </summary>
            /// <param name="token">The literal token.</param>
            public StrLit(Token token)
            {
                Token = token;
            }
        }

        /// <summary>
        /// An identifier token.
        /// </summary>
        public class Ident : Expression
        {
            /// <summary>
            /// The identifier token.
            /// </summary>
            public Token Token { get; set; }

            /// <summary>
            /// The <see cref="Symbol"/> this name refers to.
            /// </summary>
            public Symbol? Symbol { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Ident"/>.
            /// </summary>
            /// <param name="token">The identifier token.</param>
            public Ident(Token token)
            {
                Token = token;
            }
        }

        /// <summary>
        /// An special identifier token that represents a compiler intrinsic.
        /// </summary>
        public class Intrinsic : Expression
        {
            /// <summary>
            /// The identifier token.
            /// </summary>
            public Token Token { get; set; }

            /// <summary>
            /// The <see cref="Symbol"/> this intrinsic identifier refers to.
            /// </summary>
            public Symbol.Intrinsic? Symbol { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Intrinsic"/>.
            /// </summary>
            /// <param name="token">The special identifier token.</param>
            public Intrinsic(Token token)
            {
                Token = token;
            }
        }

        /// <summary>
        /// A procedure type definition.
        /// Syntax:
        /// ```
        /// proc(ArgTypes...)
        /// ```
        /// or
        /// ```
        /// proc(ArgTypes...) -> RetType
        /// ```
        /// </summary>
        public class ProcType : Expression
        {
            /// <summary>
            /// The types of parameters this procedure type consists of.
            /// </summary>
            public List<Expression> ParameterTypes { get; set; }
            /// <summary>
            /// The optional return type of this procedure type. Can be null for "no return value" (unit type).
            /// </summary>
            public Expression? ReturnType { get; set; }

            /// <summary>
            /// Initializes a new <see cref="ProcType"/>.
            /// </summary>
            /// <param name="parameters">The parameter types.</param>
            /// <param name="returnType">The optional return type.</param>
            public ProcType(List<Expression> parameters, Expression? returnType)
            {
                ParameterTypes = parameters;
                ReturnType = returnType;
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
        public class Proc : Expression
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
                /// The <see cref="VariableSymbol"/> corresponding to this parameter.
                /// </summary>
                public Symbol.Variable? Symbol { get; set; }

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
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="parameters">The list of parameters the procedure takes.</param>
            /// <param name="returnType">The return type of the procedure.</param>
            /// <param name="body">The body of the procedure.</param>
            public Proc(List<Parameter> parameters, Expression? returnType, Expression body)
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
        public class Block : Expression
        {
            /// <summary>
            /// The list of <see cref="Statement"/>s this <see cref="Block"/> consists of.
            /// </summary>
            public List<Statement> Statements { get; set; }
            /// <summary>
            /// The value this <see cref="Block"/> evaluates to. Can be null, which means it 
            /// evaluates to nothing.
            /// </summary>
            public Expression? Value { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Block"/>.
            /// </summary>
            /// <param name="statements">The list of <see cref="Statement"/>s this block consists of.</param>
            /// <param name="value">The expression this block evaluates to.</param>
            public Block(List<Statement> statements, Expression? value)
            {
                Statements = statements;
                Value = value;
            }
        }

        /// <summary>
        /// An expression calling a procedure.
        /// Syntax:
        /// ```
        /// Expression(Arg1, Arg2, ...)
        /// ```
        /// </summary>
        public class Call : Expression
        {
            /// <summary>
            /// The procedure <see cref="Expression"/> that's being called.
            /// </summary>
            new public Expression Proc { get; set; }
            /// <summary>
            /// The arguments passed to the procedure.
            /// </summary>
            public List<Expression> Arguments { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Call"/>.
            /// </summary>
            /// <param name="proc">The procedure <see cref="Expression"/> that's being called.</param>
            /// <param name="arguments">The arguments passed to the procedure.</param>
            public Call(Expression proc, List<Expression> arguments)
            {
                Proc = proc;
                Arguments = arguments;
            }
        }
    }
}
