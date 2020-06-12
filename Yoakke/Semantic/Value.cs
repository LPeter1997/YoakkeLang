using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Represents a compile-time constant.
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    abstract partial class Value : IEquatable<Value>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// The <see cref="Type"/> of this constant value.
        /// </summary>
        public abstract Type Type { get; }

        public override bool Equals(object? obj) =>
            obj is Value v && Equals(v);

        public bool Equals(Value? other) =>
            other != null && EqualsNonNull(other);

        /// <summary>
        /// Checks, if another <see cref="Value"/> equals with this one.
        /// </summary>
        /// <param name="other">The other <see cref="Value"/> to compare.</param>
        /// <returns>True, if the two <see cref="Value"/>s are equal.</returns>
        public abstract bool EqualsNonNull(Value other);
    }

    // Variants

    partial class Value
    {
        /// <summary>
        /// A procedure as a compile-time <see cref="Value"/>.
        /// </summary>
        public class Proc : Value
        {
            /// <summary>
            /// The AST node of the procedure.
            /// </summary>
            public readonly Expression.Proc Node;

            public override Type Type => Assert.NonNullValue(Node.EvaluationType);

            /// <summary>
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="node">The AST node this procedure originates from.</param>
            public Proc(Expression.Proc node)
            {
                Node = node;
            }

            public override bool EqualsNonNull(Value other) =>
                other is Proc o && ReferenceEquals(Node, o.Node);
        }

        /// <summary>
        /// A compiler intrinsic function <see cref="Value"/>.
        /// </summary>
        public class IntrinsicProc : Value
        {
            /// <summary>
            /// The intrinsic <see cref="Symbol"/>.
            /// </summary>
            public readonly Symbol.Intrinsic Symbol;

            public override Type Type => Symbol.Type;

            /// <summary>
            /// Initializes a new <see cref="IntrinsicProc"/>.
            /// </summary>
            /// <param name="symbol">The intrinsic <see cref="Symbol"/>.</param>
            public IntrinsicProc(Symbol.Intrinsic symbol)
            {
                Symbol = symbol;
            }

            public override bool EqualsNonNull(Value other) =>
                throw new NotImplementedException();
        }

        /// <summary>
        /// An external symbol as a <see cref="Value"/>.
        /// </summary>
        public class Extern : Value
        {
            /// <summary>
            /// The name of this external symbol.
            /// </summary>
            public readonly string Name;

            private Type type;
            public override Type Type => type;

            /// <summary>
            /// Initializes a new <see cref="Extern"/>.
            /// </summary>
            /// <param name="name">The name of the external symbol.</param>
            /// <param name="type">The <see cref="Type"/> of the external symbol.</param>
            public Extern(string name, Type type)
            {
                this.type = type;
                Name = name;
            }

            public override bool EqualsNonNull(Value other) =>
                   other is Extern e
                && Name == e.Name
                && Type.EqualsNonNull(e.Type)
                ;
        }

        /// <summary>
        /// A compile-time integral <see cref="Value"/>.
        /// </summary>
        public class Int : Value
        {
            /// <summary>
            /// The integer value.
            /// </summary>
            public readonly BigInteger Value;

            private readonly Type type;
            public override Type Type => type;

            // TODO: Make sure the passed in type is int?
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

            // TODO: Do we count in the type?
            public override bool EqualsNonNull(Value other) =>
                   other is Int i
                && type.EqualsNonNull(i.Type)
                && Value == i.Value
                ;
        }

        // TODO: Later string value could be represented by a simple struct.
        // It doesn't have to be a compiler builtin type. (except for the literal)
        /// <summary>
        /// A compile-time string <see cref="Value"/>.
        /// </summary>
        public class Str : Value
        {
            /// <summary>
            /// The string value.
            /// </summary>
            public readonly string Value;

            public override Type Type => Type.Str;

            /// <summary>
            /// Initializes a new <see cref="Str"/>.
            /// </summary>
            /// <param name="value">The value of the string.</param>
            public Str(string value)
            {
                Value = value;
            }

            public override bool EqualsNonNull(Value other) =>
                other is Str s && Value == s.Value;
        }
    }
}
