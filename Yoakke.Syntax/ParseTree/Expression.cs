using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax.ParseTree
{
    /// <summary>
    /// Base for every expression <see cref="Node"/>.
    /// </summary>
    public abstract partial class Expression : Node
    {
    }

    partial class Expression
    {
        /// <summary>
        /// A literal <see cref="Token"/>: identifier, string, int, bool, etc.
        /// </summary>
        public class Literal : Expression
        {
            public override Span Span => Token.Span;

            /// <summary>
            /// The <see cref="Token"/>.
            /// </summary>
            public readonly Token Token;

            public Literal(Token token)
            {
                Token = token;
            }
        }

        /// <summary>
        /// A struct type definition.
        /// </summary>
        public class StructType : Expression
        {
            /// <summary>
            /// A single field inside a <see cref="StructType"/>.
            /// </summary>
            public class Field : Declaration
            {
                public override Span Span => new Span(Name.Span, Type.Span);

                /// <summary>
                /// Attached documentation.
                /// </summary>
                public readonly CommentGroup? Doc;
                /// <summary>
                /// The name of the field.
                /// </summary>
                public readonly Token Name;
                /// <summary>
                /// The ':'.
                /// </summary>
                public readonly Token Colon;
                /// <summary>
                /// The type of the field.
                /// </summary>
                public readonly Expression Type;
                /// <summary>
                /// The ';'.
                /// </summary>
                public readonly Token Semicolon;
                /// <summary>
                /// Inline comment.
                /// </summary>
                public readonly CommentGroup? LineComment;

                public Field(
                    CommentGroup? doc, 
                    Token name, 
                    Token colon, 
                    Expression type,
                    Token semicolon,
                    CommentGroup? lineComment)
                {
                    Doc = doc;
                    Name = name;
                    Colon = colon;
                    Type = type;
                    Semicolon = semicolon;
                    LineComment = lineComment;
                }
            }

            public override Span Span => new Span(KwStruct.Span, CloseBrace.Span);

            /// <summary>
            /// The 'struct' keyword.
            /// </summary>
            public readonly Token KwStruct;
            /// <summary>
            /// The '{'.
            /// </summary>
            public readonly Token OpenBrace;
            /// <summary>
            /// The <see cref="Field"/>s.
            /// </summary>
            public readonly IReadOnlyList<Field> Fields;
            /// <summary>
            /// The '}'.
            /// </summary>
            public readonly Token CloseBrace;

            public StructType(Token kwStruct, Token openBrace, IReadOnlyList<Field> fields, Token closeBrace)
            {
                KwStruct = kwStruct;
                OpenBrace = openBrace;
                Fields = fields;
                CloseBrace = closeBrace;
            }
        }

        /// <summary>
        /// A struct value initialization.
        /// </summary>
        public class StructValue : Expression
        {
            /// <summary>
            /// A single field initialization inside a <see cref="StructValue"/>.
            /// </summary>
            public class Field : Statement
            {
                public override Span Span => new Span(Name.Span, Semicolon.Span);

                /// <summary>
                /// The name of the field.
                /// </summary>
                public readonly Token Name;
                /// <summary>
                /// The '='.
                /// </summary>
                public readonly Token Assign;
                /// <summary>
                /// The value of the field.
                /// </summary>
                public readonly Expression Value;
                /// <summary>
                /// The ';'.
                /// </summary>
                public readonly Token Semicolon;

                public Field(Token name, Token assign, Expression value, Token semicolon)
                {
                    Name = name;
                    Assign = assign;
                    Value = value;
                    Semicolon = semicolon;
                }
            }

            public override Span Span => new Span(Type.Span, CloseBrace.Span);

            /// <summary>
            /// The struct type.
            /// </summary>
            public readonly Expression Type;
            /// <summary>
            /// The '{'.
            /// </summary>
            public readonly Token OpenBrace;
            /// <summary>
            /// The <see cref="Field"/>s.
            /// </summary>
            public readonly IReadOnlyList<Field> Fields;
            /// <summary>
            /// The '}'.
            /// </summary>
            public readonly Token CloseBrace;

            public StructValue(Expression type, Token openBrace, IReadOnlyList<Field> fields, Token closeBrace)
            {
                Type = type;
                OpenBrace = openBrace;
                Fields = fields;
                CloseBrace = closeBrace;
            }
        }

        /// <summary>
        /// A procedure signature type.
        /// </summary>
        public class ProcSignature : Expression
        {
            /// <summary>
            /// A single parameter inside a signature.
            /// </summary>
            public class Parameter : Declaration
            {
                public override Span Span => new Span(Name?.Span ?? Type.Span, Type.Span);

                /// <summary>
                /// The name of the parameter, if given.
                /// </summary>
                public readonly Token? Name;
                /// <summary>
                /// The ':'.
                /// </summary>
                public readonly Token? Colon;
                /// <summary>
                /// The type of the parameter.
                /// </summary>
                public readonly Expression Type;

                public Parameter(Token? name, Token? colon, Expression type)
                {
                    Name = name;
                    Colon = colon;
                    Type = type;
                }
            }

            public override Span Span => new Span(KwProc.Span, Return?.Span ?? CloseParen.Span);

            /// <summary>
            /// The 'proc' keyword.
            /// </summary>
            public readonly Token KwProc;
            /// <summary>
            /// The '('.
            /// </summary>
            public readonly Token OpenParen;
            /// <summary>
            /// The list of parameters.
            /// </summary>
            public readonly IReadOnlyList<WithComma<Parameter>> Parameters;
            /// <summary>
            /// The ')'.
            /// </summary>
            public readonly Token CloseParen;
            /// <summary>
            /// The '->', if there's a return type.
            /// </summary>
            public readonly Token? Arrow;
            /// <summary>
            /// The return type.
            /// </summary>
            public readonly Expression? Return;

            public ProcSignature(
                Token kwProc, 
                Token openParen, 
                IReadOnlyList<WithComma<Parameter>> parameters, 
                Token closeParen, 
                Token? arrow, 
                Expression? @return)
            {
                KwProc = kwProc;
                OpenParen = openParen;
                Parameters = parameters;
                CloseParen = closeParen;
                Arrow = arrow;
                Return = @return;
            }
        }

        /// <summary>
        /// A procedure.
        /// </summary>
        public class Proc : Expression
        {
            public override Span Span => new Span(Signature.Span, Body.Span);

            /// <summary>
            /// The signature.
            /// </summary>
            public readonly ProcSignature Signature;
            /// <summary>
            /// The procedure body.
            /// </summary>
            public readonly Block Body;

            public Proc(ProcSignature signature, Block body)
            {
                Signature = signature;
                Body = body;
            }
        }

        /// <summary>
        /// A code block.
        /// </summary>
        public class Block : Expression
        {
            public override Span Span => new Span(OpenBrace.Span, CloseBrace.Span);

            /// <summary>
            /// The '{'.
            /// </summary>
            public readonly Token OpenBrace;
            /// <summary>
            /// The list of statements.
            /// </summary>
            public readonly IReadOnlyList<Statement> Statements;
            /// <summary>
            /// The value the block evaluates to.
            /// </summary>
            public readonly Expression? Value;
            /// <summary>
            /// The '}'.
            /// </summary>
            public readonly Token CloseBrace;

            public Block(Token openBrace, IReadOnlyList<Statement> statements, Expression? value, Token closeBrace)
            {
                OpenBrace = openBrace;
                Statements = statements;
                Value = value;
                CloseBrace = closeBrace;
            }
        }

        /// <summary>
        /// A procedure call.
        /// </summary>
        public class Call : Expression
        {
            public override Span Span => new Span(Procedure.Span, CloseParen.Span);

            /// <summary>
            /// The called procedure.
            /// </summary>
            public readonly Expression Procedure;
            /// <summary>
            /// The '('.
            /// </summary>
            public readonly Token OpenParen;
            /// <summary>
            /// The list of arguments.
            /// </summary>
            public readonly IReadOnlyList<WithComma<Expression>> Arguments;
            /// <summary>
            /// The ')'.
            /// </summary>
            public readonly Token CloseParen;

            public Call(
                Expression procedure, 
                Token openParen, 
                IReadOnlyList<WithComma<Expression>> arguments, 
                Token closeParen)
            {
                Procedure = procedure;
                OpenParen = openParen;
                Arguments = arguments;
                CloseParen = closeParen;
            }
        }

        /// <summary>
        /// An if-else conditional.
        /// </summary>
        public class If : Expression
        {
            /// <summary>
            /// An else-if part of an if-expression.
            /// </summary>
            public class ElseIf : Expression
            {
                public override Span Span => new Span(KwElse.Span, Then.Span);

                /// <summary>
                /// The 'else' keyword.
                /// </summary>
                public readonly Token KwElse;
                /// <summary>
                /// The 'if' keyword.
                /// </summary>
                public readonly Token KwIf;
                /// <summary>
                /// The condition.
                /// </summary>
                public readonly Expression Condition;
                /// <summary>
                /// The then block.
                /// </summary>
                public readonly Block Then;

                public ElseIf(Token kwElse, Token kwIf, Expression condition, Block then)
                {
                    KwElse = kwElse;
                    KwIf = kwIf;
                    Condition = condition;
                    Then = then;
                }
            }

            public override Span Span => GetSpan();

            /// <summary>
            /// The 'if' keyword.
            /// </summary>
            public readonly Token KwIf;
            /// <summary>
            /// The condition.
            /// </summary>
            public readonly Expression Condition;
            /// <summary>
            /// The then block.
            /// </summary>
            public readonly Block Then;
            /// <summary>
            /// The else-if blocks.
            /// </summary>
            public readonly IReadOnlyList<ElseIf> ElseIfs;
            /// <summary>
            /// The 'else' keyword, if there's an else block.
            /// </summary>
            public readonly Token? KwElse;
            /// <summary>
            /// The else block.
            /// </summary>
            public readonly Block? Else;

            public If(
                Token kwIf, 
                Expression condition, 
                Block then, 
                IReadOnlyList<ElseIf> elseIfs, 
                Token? ksElse, 
                Block? @else)
            {
                KwIf = kwIf;
                Condition = condition;
                Then = then;
                ElseIfs = elseIfs;
                KwElse = ksElse;
                Else = @else;
            }

            private Span GetSpan()
            {
                var start = KwIf.Span.Start;
                var end = Then.Span.End;
                if (Else != null)
                {
                    end = Else.Span.End;
                }
                else if (ElseIfs.Count > 0)
                {
                    end = ElseIfs.Last().Span.End;
                }
                return new Span(KwIf.Span.Source, start, end);
            }
        }

        /// <summary>
        /// A while loop.
        /// </summary>
        public class While : Expression
        {
            public override Span Span => new Span(KwWhile.Span, Body.Span);

            /// <summary>
            /// The 'while' keyword.
            /// </summary>
            public readonly Token KwWhile;
            /// <summary>
            /// The condition.
            /// </summary>
            public readonly Expression Condition;
            /// <summary>
            /// The loop body.
            /// </summary>
            public readonly Block Body;

            public While(Token kwWhile, Expression condition, Block body)
            {
                KwWhile = kwWhile;
                Condition = condition;
                Body = body;
            }
        }

        /// <summary>
        /// An <see cref="Expression"/> with two operands.
        /// </summary>
        public class Binary : Expression
        {
            public override Span Span => new Span(Left.Span, Right.Span);

            /// <summary>
            /// The left-hand side operand.
            /// </summary>
            public readonly Expression Left;
            /// <summary>
            /// The operator.
            /// </summary>
            public readonly Token Operator;
            /// <summary>
            /// The right-hand side operand.
            /// </summary>
            public readonly Expression Right;

            public Binary(Expression left, Token @operator, Expression right)
            {
                Left = left;
                Operator = @operator;
                Right = right;
            }
        }

        /// <summary>
        /// A dot-path access.
        /// </summary>
        public class DotPath : Expression
        {
            public override Span Span => new Span(Left.Span, Right.Span);

            /// <summary>
            /// The left-hand side operand.
            /// </summary>
            public readonly Expression Left;
            /// <summary>
            /// The dot operator.
            /// </summary>
            public readonly Token Dot;
            /// <summary>
            /// The right-hand side identifier.
            /// </summary>
            public readonly Token Right;

            public DotPath(Expression left, Token dot, Token right)
            {
                Left = left;
                Dot = dot;
                Right = right;
            }
        }

        /// <summary>
        /// An <see cref="Expression"/> inside parenthesis.
        /// </summary>
        public class Parenthesized : Expression
        {
            public override Span Span => new Span(OpenParen.Span, CloseParen.Span);

            /// <summary>
            /// The '('.
            /// </summary>
            public readonly Token OpenParen;
            /// <summary>
            /// The contained expression.
            /// </summary>
            public readonly Expression Inside;
            /// <summary>
            /// The ')'.
            /// </summary>
            public readonly Token CloseParen;

            public Parenthesized(Token openParen, Expression inside, Token closeParen)
            {
                OpenParen = openParen;
                Inside = inside;
                CloseParen = closeParen;
            }
        }
    }
}
