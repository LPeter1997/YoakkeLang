﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Syntax;

namespace Yoakke.Compiler.Ast
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

            public override Expression CloneExpression() =>
                new IntLit(Token);
        }

        /// <summary>
        /// A bool literal token.
        /// </summary>
        public class BoolLit : Expression
        {
            /// <summary>
            /// The literal token itself.
            /// </summary>
            public Token Token { get; set; }

            /// <summary>
            /// Initializes a new <see cref="IntLit"/>.
            /// </summary>
            /// <param name="token">The literal token.</param>
            public BoolLit(Token token)
            {
                Token = token;
            }

            public override Expression CloneExpression() =>
                new BoolLit(Token);
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

            /// <summary>
            /// Escapes this string value.
            /// </summary>
            /// <returns>The escaped string.</returns>
            public string Escape()
            {
                var result = new StringBuilder(Token.Value.Length);
                // Ignore first and last characters, they are the quotes
                for (int i = 1; i < Token.Value.Length - 1; ++i)
                {
                    var ch = Token.Value[i];
                    if (ch == '\\')
                    {
                        throw new NotImplementedException("Escapes are not implemented yet!");
                    }
                    else
                    {
                        result.Append(ch);
                    }
                }
                return result.ToString();
            }

            public override Expression CloneExpression() =>
                new StrLit(Token);
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

            public override Expression CloneExpression() =>
                new Ident(Token);
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

            public override Expression CloneExpression() =>
                new Intrinsic(Token);
        }

        /// <summary>
        /// A path separated by dot.
        /// Syntax:
        /// ```
        /// Expression . Identifier
        /// ```
        /// </summary>
        public class DotPath : Expression
        {
            /// <summary>
            /// The left-hand-side of the '.'.
            /// </summary>
            public Expression Left { get; set; }
            /// <summary>
            /// The right-hand-side of the '.', which is a single <see cref="Token"/>.
            /// </summary>
            public Token Right { get; set; }

            /// <summary>
            /// Initializes a new <see cref="DotPath"/>.
            /// </summary>
            /// <param name="left">The left-hand-side of the '.'.</param>
            /// <param name="right">The right-hand-side of the '.'.</param>
            public DotPath(Expression left, Token right)
            {
                Left = left;
                Right = right;
            }

            public override Expression CloneExpression() =>
                new DotPath(Left.CloneExpression(), Right);
        }

        /// <summary>
        /// A structure type definition.
        /// Syntax:
        /// ```
        /// struct {
        ///     name1: Type1;
        ///     ...
        /// }
        /// ```
        /// </summary>
        public class StructType : Expression
        {
            /// <summary>
            /// A field declaration inside the struct definition.
            /// Syntax:
            /// ```
            /// name: Type;
            /// ```
            /// </summary>
            public class Field : ICloneable
            {
                /// <summary>
                /// The name of the <see cref="Field"/>.
                /// </summary>
                public Token Name { get; set; }
                /// <summary>
                /// The type of the <see cref="Field"/>.
                /// </summary>
                public Expression Type { get; set; }

                /// <summary>
                /// Initializes a new <see cref="Field"/>.
                /// </summary>
                /// <param name="name">The name of the field.</param>
                /// <param name="type">The type of the field.</param>
                public Field(Token name, Expression type)
                {
                    Name = name;
                    Type = type;
                }

                public object Clone() =>
                    CloneField();

                public Field CloneField() =>
                    new Field(Name, Type.CloneExpression());
            }

            /// <summary>
            /// The 'struct' <see cref="Token"/> that started this <see cref="StructType"/>.
            /// </summary>
            public Token Token { get; set; }

            /// <summary>
            /// The list of <see cref="Field"/> declarations.
            /// </summary>
            public List<Field> Fields { get; set; }
            /// <summary>
            /// The <see cref="Declaration"/>s inside the struct definition.
            /// </summary>
            public List<Declaration> Declarations { get; set; }

            /// <summary>
            /// Initializes a new <see cref="StructType"/>.
            /// </summary>
            /// <param name="token">The 'struct' <see cref="Token"/> that started this type.</param>
            /// <param name="fields">The list of <see cref="Field"/> declarations.</param>
            /// <param name="declarations">The <see cref="Declaration"/>s inside the struct definition.</param>
            public StructType(Token token, List<Field> fields, List<Declaration> declarations)
            {
                Token = token;
                Fields = fields;
                Declarations = declarations;
            }

            public override Expression CloneExpression() =>
                new StructType(
                    Token, 
                    Fields.Select(x => x.CloneField()).ToList(), 
                    Declarations.Select(x => x.CloneDeclaration()).ToList());
        }

        /// <summary>
        /// A structure type instantiation.
        /// Syntax:
        /// ```
        /// Struct Type {
        ///     Field1 = Value 1;
        ///     ...
        /// }
        /// ```
        /// </summary>
        public class StructValue : Expression
        {
            /// <summary>
            /// A field initialization inside a struct initialization.
            /// Syntax:
            /// ```
            /// name = Value;
            /// ```
            /// </summary>
            public class Field : ICloneable
            {
                /// <summary>
                /// The name of the <see cref="Field"/> being initialized.
                /// </summary>
                public Token Name { get; set; }
                /// <summary>
                /// The value of the <see cref="Field"/> being initialized.
                /// </summary>
                public Expression Value { get; set; }

                /// <summary>
                /// Initializes a new <see cref="Field"/>.
                /// </summary>
                /// <param name="name">The name of the field being initialized.</param>
                /// <param name="value">The value of the field being initialized.</param>
                public Field(Token name, Expression value)
                {
                    Name = name;
                    Value = value;
                }

                public object Clone() =>
                    CloneField();

                public Field CloneField() =>
                    new Field(Name, Value.CloneExpression());
            }

            /// <summary>
            /// The structure type to instantiate,
            /// </summary>
            new public Expression StructType { get; set; }
            /// <summary>
            /// The initializer <see cref="Field"/>s.
            /// </summary>
            public List<Field> Fields { get; set; }

            /// <summary>
            /// Initializes a new <see cref="StructValue"/>.
            /// </summary>
            /// <param name="structType">The type of the struct to instantiate.</param>
            /// <param name="fields">The initializer <see cref="Field"/>s.</param>
            public StructValue(Expression structType, List<Field> fields)
            {
                StructType = structType;
                Fields = fields;
            }

            public override Expression CloneExpression() =>
                new StructValue(
                    StructType.CloneExpression(), 
                    Fields.Select(x => x.CloneField()).ToList());
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

            public override Expression CloneExpression() =>
                new ProcType(
                    ParameterTypes.Select(x => x.CloneExpression()).ToList(),
                    ReturnType?.CloneExpression());
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
        public class ProcValue : Expression
        {
            /// <summary>
            /// A single parameter inside a procedure's parameter list.
            /// They are in the form of:
            /// ```
            /// Name: Type
            /// ```
            /// </summary>
            public class Parameter : ICloneable
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

                public object Clone() =>
                    CloneParameter();

                public Parameter CloneParameter() =>
                    new Parameter(Name, Type.CloneExpression());
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
            /// Initializes a new <see cref="ProcValue"/>.
            /// </summary>
            /// <param name="parameters">The list of parameters the procedure takes.</param>
            /// <param name="returnType">The return type of the procedure.</param>
            /// <param name="body">The body of the procedure.</param>
            public ProcValue(List<Parameter> parameters, Expression? returnType, Expression body)
            {
                Parameters = parameters;
                ReturnType = returnType;
                Body = body;
            }

            public override Expression CloneExpression() =>
                new ProcValue(
                    Parameters.Select(x => x.CloneParameter()).ToList(),
                    ReturnType?.CloneExpression(),
                    Body.CloneExpression());
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
            /// The <see cref="Scope"/> the value is returned to.
            /// </summary>
            public Scope? ReturnTarget { get; set; }

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

            public override Expression CloneExpression() =>
                new Block(Statements.Select(x => x.CloneStatement()).ToList(), Value?.CloneExpression());
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
            public Expression Proc { get; set; }
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

            public override Expression CloneExpression() =>
                new Call(Proc.CloneExpression(), Arguments.Select(x => x.CloneExpression()).ToList());
        }

        /// <summary>
        /// Represents a conditional <see cref="Expression"/>.
        /// </summary>
        public class If : Expression
        {
            /// <summary>
            /// The condition that decides which arm will be evaluated.
            /// </summary>
            public Expression Condition { get; set; }
            /// <summary>
            /// The <see cref="Expression"/> that gets evaluated when the <see cref="Condition"/> is true.
            /// </summary>
            public Expression Then { get; set; }
            /// <summary>
            /// The <see cref="Expression"/> that gets evaluated when the <see cref="Condition"/> is false.
            /// </summary>
            public Expression? Else { get; set; }

            /// <summary>
            /// Initializes a new <see cref="If"/>.
            /// </summary>
            /// <param name="condition">The condition that decides which arm will be evaluated.</param>
            /// <param name="then">The <see cref="Expression"/> that gets evaluated when the condition is true.</param>
            /// <param name="els">The <see cref="Expression"/> that gets evaluated when the condition is false.</param>
            public If(Expression condition, Expression then, Expression? els)
            {
                Condition = condition;
                Then = then;
                Else = els;
            }

            public override Expression CloneExpression() =>
                new If(Condition.CloneExpression(), Then.CloneExpression(), Else?.CloneExpression());
        }

        /// <summary>
        /// A binary operation between two <see cref="Expression"/>s.
        /// </summary>
        public class BinOp : Expression
        {
            /// <summary>
            /// The left-hand-side operand.
            /// </summary>
            public Expression Left { get; set; }
            /// <summary>
            /// The operator <see cref="Token"/>.
            /// </summary>
            public Token Operator { get; set; }
            /// <summary>
            /// The right-hand-side operand.
            /// </summary>
            public Expression Right { get; set; }

            /// <summary>
            /// Initializes a new <see cref="BinOp"/>.
            /// </summary>
            /// <param name="left">The left-hand-side operand.</param>
            /// <param name="op">The operator <see cref="Token"/>.</param>
            /// <param name="right">The right-hand-side operand.</param>
            public BinOp(Expression left, Token op, Expression right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }

            public override Expression CloneExpression() =>
                new BinOp(Left.CloneExpression(), Operator, Right.CloneExpression());
        }
    }
}
