using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Represents a compile-time constant.
    /// </summary>
    abstract partial class Value
    {
        /// <summary>
        /// The <see cref="Type"/> of this constant value.
        /// </summary>
        public abstract Type Type { get; }
    }

    // Variants

    partial class Value
    {
        /// <summary>
        /// A compile-time <see cref="Value"/> that stores a <see cref="Type"/>.
        /// </summary>
        public class Type_ : Value
        {
            public override Type Type => Type.Type_;
            /// <summary>
            /// The <see cref="Type"/> this <see cref="Type_"/> stores.
            /// </summary>
            public Type Value { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Type_"/>.
            /// </summary>
            /// <param name="value">The <see cref="Type"/> this value stores.</param>
            public Type_(Type value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// A procedure as a compile-time <see cref="Value"/>.
        /// </summary>
        public class Proc : Value
        {
            private Type type;
            public override Type Type => type;

            /// <summary>
            /// The AST node of the procedure.
            /// </summary>
            public readonly Expression.Proc Node;

            /// <summary>
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="node">The AST node this procedure originates from.</param>
            /// <param name="type">The <see cref="Type"/> of this procedure.</param>
            public Proc(Expression.Proc node, Type type)
            {
                Node = node;
                this.type = type;
            }
        }

        /// <summary>
        /// A compiler intrinsic function <see cref="Value"/>.
        /// </summary>
        public class IntrinsicProc : Value
        {
            // TODO
            public override Type Type => throw new NotImplementedException();

            /// <summary>
            /// The intrinsic <see cref="Symbol"/>.
            /// </summary>
            public readonly Symbol.Intrinsic Symbol;

            /// <summary>
            /// Initializes a new <see cref="IntrinsicProc"/>.
            /// </summary>
            /// <param name="symbol">The intrinsic <see cref="Symbol"/>.</param>
            public IntrinsicProc(Symbol.Intrinsic symbol)
            {
                Symbol = symbol;
            }
        }

        /// <summary>
        /// A compile-time integral <see cref="Value"/>.
        /// </summary>
        public class Int : Value
        {
            private readonly Type type;
            public override Type Type => type;

            /// <summary>
            /// The integer value.
            /// </summary>
            public readonly BigInteger Value;

            /// <summary>
            /// Initializes a new <see cref="Int"/>.
            /// </summary>
            /// <param name="type">The type of the integer.</param>
            /// <param name="value">The value of the integer.</param>
            public Int(Type type, BigInteger value)
            {
                this.type = type;
                Value = value;
            }
        }

        // TODO: Later string value could be represented by a simple struct.
        // It doesn't have to be a compiler builtin type. (except for the literal)
        /// <summary>
        /// A compile-time string <see cref="Value"/>.
        /// </summary>
        public class Str : Value
        {
            public override Type Type => Type.Str;

            /// <summary>
            /// The string value.
            /// </summary>
            public readonly string Value;

            /// <summary>
            /// Initializes a new <see cref="Str"/>.
            /// </summary>
            /// <param name="value">The value of the string.</param>
            public Str(string value)
            {
                Value = value;
            }
        }
    }
}
