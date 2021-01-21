using System.Collections.Generic;

namespace Yoakke.Syntax.Ast
{
    /// <summary>
    /// Base class for expressions.
    /// </summary>
    public abstract partial class Expression : Node
    {
        protected Expression(ParseTree.Node? parseTreeNode)
            : base(parseTreeNode)
        {
        }
    }

    partial class Expression
    {
        /// <summary>
        /// The type of literals.
        /// </summary>
        public enum LitType
        {
            Integer,
            Bool,
            String,
        }

        /// <summary>
        /// Some literal token, like an integer, bool or string.
        /// </summary>
        public class Literal : Expression
        {
            /// <summary>
            /// The type of the literal.
            /// </summary>
            public readonly LitType Type;
            /// <summary>
            /// The literal value.
            /// </summary>
            public readonly string Value;

            public Literal(ParseTree.Node? parseTreeNode, LitType type, string value)
                : base(parseTreeNode)
            {
                Type = type;
                Value = value;
            }
        }

        /// <summary>
        /// An identifier.
        /// </summary>
        public class Identifier : Expression
        {
            /// <summary>
            /// The identifier name.
            /// </summary>
            public readonly string Name;

            public Identifier(ParseTree.Node? parseTreeNode, string name)
                : base(parseTreeNode)
            {
                Name = name;
            }
        }

        /// <summary>
        /// An array type.
        /// </summary>
        public class ArrayType : Expression
        {
            /// <summary>
            /// The length of the array.
            /// </summary>
            public readonly Expression Length;
            /// <summary>
            /// The element type of the array.
            /// </summary>
            public readonly Expression ElementType;

            public ArrayType(ParseTree.Node? parseTreeNode, Expression length, Expression elementType)
                : base(parseTreeNode)
            {
                Length = length;
                ElementType = elementType;
            }
        }

        /// <summary>
        /// A struct type expression.
        /// </summary>
        public class StructType : Expression
        {
            /// <summary>
            /// A single field inside a struct type.
            /// </summary>
            public class Field : Declaration
            {
                /// <summary>
                /// The name of the field.
                /// </summary>
                public readonly string Name;
                /// <summary>
                /// The type of the field.
                /// </summary>
                public readonly Expression Type;

                public Field(ParseTree.Node? parseTreeNode, string name, Expression type)
                    : base(parseTreeNode)
                {
                    Name = name;
                    Type = type;
                }
            }

            /// <summary>
            /// The 'struct' keyword.
            /// </summary>
            public readonly Token KwStruct;
            /// <summary>
            /// The <see cref="Field"/>s of the struct type.
            /// </summary>
            public readonly IReadOnlyList<Field> Fields;
            /// <summary>
            /// The list of <see cref="Declaration"/>s in the definition.
            /// </summary>
            public readonly IReadOnlyList<Statement> Declarations;

            public StructType(
                ParseTree.Node? parseTreeNode, 
                Token kwStruct, 
                IReadOnlyList<Field> fields,
                IReadOnlyList<Statement> declarations)
                : base(parseTreeNode)
            {
                KwStruct = kwStruct;
                Fields = fields;
                Declarations = declarations;
            }
        }

        /// <summary>
        /// A struct value initialization.
        /// </summary>
        public class StructValue : Expression
        {
            /// <summary>
            /// A single field initializer.
            /// </summary>
            public class Field : Statement
            {
                /// <summary>
                /// The name of the field.
                /// </summary>
                public readonly string Name;
                /// <summary>
                /// The initializer value.
                /// </summary>
                public readonly Expression Value;

                public Field(ParseTree.Node? parseTreeNode, string name, Expression value)
                    : base(parseTreeNode)
                {
                    Name = name;
                    Value = value;
                }
            }

            /// <summary>
            /// The struct type to create.
            /// </summary>
            new public readonly Expression StructType;
            /// <summary>
            /// The <see cref="Field"/>s initializers.
            /// </summary>
            public readonly IReadOnlyList<Field> Fields;

            public StructValue(ParseTree.Node? parseTreeNode, Expression structType, IReadOnlyList<Field> fields)
                : base(parseTreeNode)
            {
                StructType = structType;
                Fields = fields;
            }
        }

        /// <summary>
        /// A procedure signature or procedure type.
        /// </summary>
        public class ProcSignature : Expression
        {
            /// <summary>
            /// A single procedure parameter.
            /// </summary>
            public class Parameter : Declaration
            {
                /// <summary>
                /// The name of the parameter.
                /// </summary>
                public readonly string? Name;
                /// <summary>
                /// The type of the parameter.
                /// </summary>
                public readonly Expression Type;

                public Parameter(ParseTree.Node? parseTreeNode, string? name, Expression type)
                : base(parseTreeNode)
                {
                    Name = name;
                    Type = type;
                }
            }

            /// <summary>
            /// The list pf <see cref="Parameter"/>s.
            /// </summary>
            public readonly IReadOnlyList<Parameter> Parameters;
            /// <summary>
            /// The return type.
            /// </summary>
            public readonly Expression? Return;

            public ProcSignature(ParseTree.Node? parseTreeNode, IReadOnlyList<Parameter> parameters, Expression? ret)
                : base(parseTreeNode)
            {
                Parameters = parameters;
                Return = ret;
            }
        }

        /// <summary>
        /// A procedure value.
        /// </summary>
        public class Proc : Expression
        {
            /// <summary>
            /// The signature of the procedure.
            /// </summary>
            public readonly ProcSignature Signature;
            /// <summary>
            /// The body of the procedure.
            /// </summary>
            public readonly Expression Body;

            public Proc(ParseTree.Node? parseTreeNode, ProcSignature signature, Expression body)
                : base(parseTreeNode)
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
            /// <summary>
            /// The <see cref="Statement"/>s in the code block.
            /// </summary>
            public readonly IReadOnlyList<Statement> Statements;
            /// <summary>
            /// The value the code block evaluates to.
            /// </summary>
            public readonly Expression? Value;

            public Block(ParseTree.Node? parseTreeNode, IReadOnlyList<Statement> statements, Expression? value)
                : base(parseTreeNode)
            {
                Statements = statements;
                Value = value;
            }
        }

        /// <summary>
        /// A procedure call.
        /// </summary>
        public class Call : Expression
        {
            /// <summary>
            /// The called procedure.
            /// </summary>
            public readonly Expression Procedure;
            /// <summary>
            /// The arguments to call the procedure with.
            /// </summary>
            public readonly IReadOnlyList<Expression> Arguments;

            public Call(ParseTree.Node? parseTreeNode, Expression procedure, IReadOnlyList<Expression> arguments)
                : base(parseTreeNode)
            {
                Procedure = procedure;
                Arguments = arguments;
            }
        }

        /// <summary>
        /// An array-subscript.
        /// </summary>
        public class Subscript : Expression
        {
            /// <summary>
            /// The accessed array.
            /// </summary>
            public readonly Expression Array;
            /// <summary>
            /// The array index.
            /// </summary>
            public readonly Expression Index;

            public Subscript(ParseTree.Node? parseTreeNode, Expression array, Expression index)
                : base(parseTreeNode)
            {
                Array = array;
                Index = index;
            }
        }

        /// <summary>
        /// An if-else expression.
        /// </summary>
        public class If : Expression
        {
            /// <summary>
            /// The condition.
            /// </summary>
            public readonly Expression Condition;
            /// <summary>
            /// The truthy expression.
            /// </summary>
            public readonly Expression Then;
            /// <summary>
            /// The falsy expression.
            /// </summary>
            public readonly Expression? Else;

            public If(ParseTree.Node? parseTreeNode, Expression condition, Expression then, Expression? els)
                : base(parseTreeNode)
            {
                Condition = condition;
                Then = then;
                Else = els;
            }
        }

        /// <summary>
        /// A while loop.
        /// </summary>
        public class While : Expression
        {
            /// <summary>
            /// The condition.
            /// </summary>
            public readonly Expression Condition;
            /// <summary>
            /// The loop body.
            /// </summary>
            public readonly Expression Body;

            public While(ParseTree.Node? parseTreeNode, Expression condition, Expression body)
                : base(parseTreeNode)
            {
                Condition = condition;
                Body = body;
            }
        }

        /// <summary>
        /// The different binary operator kinds.
        /// </summary>
        public enum BinOp
        {
            // Basic arithmetic
            Add, Subtract, Multiply, Divide, Modulo,
            // Relations
            Equals, NotEquals, Greater, Less, GreaterEqual, LessEqual,
            // Bitwise
            BitAnd, BitOr, BitXor,
            // Shift
            LeftShift, RightShift,
            // Logical
            And, Or,
            // Assignment
            Assign,
            AddAssign, SubtractAssign, MultiplyAssign, DivideAssign, ModuloAssign,
        }

        /// <summary>
        /// The compound binary operators and their corredponding primitive operators.
        /// </summary>
        public static IDictionary<BinOp, BinOp> CompoundBinaryOperators = new Dictionary<BinOp, BinOp>
        {
            { BinOp.AddAssign     , BinOp.Add      },
            { BinOp.SubtractAssign, BinOp.Subtract },
            { BinOp.MultiplyAssign, BinOp.Multiply },
            { BinOp.DivideAssign  , BinOp.Divide   },
            { BinOp.ModuloAssign  , BinOp.Modulo   },
        };

        /// <summary>
        /// A binary operation.
        /// </summary>
        public class Binary : Expression
        {
            /// <summary>
            /// The left-hand side of the operation.
            /// </summary>
            public readonly Expression Left;
            /// <summary>
            /// The operator kind.
            /// </summary>
            public readonly BinOp Operator;
            /// <summary>
            /// The right-hand side of the operation.
            /// </summary>
            public readonly Expression Right;

            public Binary(ParseTree.Node? parseTreeNode, Expression left, BinOp op, Expression right)
                : base(parseTreeNode)
            {
                Left = left;
                Operator = op;
                Right = right;
            }
        }

        /// <summary>
        /// The different unary operator kinds.
        /// </summary>
        public enum UnaryOp
        {
            // +, -, !
            Ponote, Negate, Not,
            // *, &, ~
            PointerType, AddressOf, Dereference,
        }

        /// <summary>
        /// A unary operation.
        /// </summary>
        public class Unary : Expression
        {
            /// <summary>
            /// The operator kind.
            /// </summary>
            public readonly UnaryOp Operator;
            /// <summary>
            /// The operand.
            /// </summary>
            public readonly Expression Operand;

            public Unary(ParseTree.Node? parseTreeNode, UnaryOp op, Expression operand)
                : base(parseTreeNode)
            {
                Operator = op;
                Operand = operand;
            }
        }

        /// <summary>
        /// A subpath accessed with a dot.
        /// </summary>
        public class DotPath : Expression
        {
            /// <summary>
            /// The left-hand side of the dot path.
            /// </summary>
            public readonly Expression Left;
            /// <summary>
            /// The accessed identifier.
            /// </summary>
            public readonly string Right;

            public DotPath(ParseTree.Node? parseTreeNode, Expression left, string right)
                : base(parseTreeNode)
            {
                Left = left;
                Right = right;
            }
        }

        /// <summary>
        /// A const-evaluated expression.
        /// </summary>
        public class Const : Expression
        {
            /// <summary>
            /// The expression inside to evaluate compile-time.
            /// </summary>
            public readonly Expression Subexpression;

            public Const(ParseTree.Node? parseTreeNode, Expression subexpression)
                : base(parseTreeNode)
            {
                Subexpression = subexpression;
            }
        }
    }
}
