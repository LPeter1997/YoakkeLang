using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    // Constants

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    partial class Value
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public static readonly Value Unit = new Tuple(new List<Value>());
    }

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
        /// A special <see cref="Value"/> to denote that it's under evaluation.
        /// Used to avoid infinite recursion.
        /// </summary>
        public class UnderEvaluation : Value
        {
            private Type type = new Type.Variable();
            public override Type Type => type;

            public override bool EqualsNonNull(Value other)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// A procedure as a compile-time <see cref="Value"/>.
        /// </summary>
        public class Proc : Value
        {
            /// <summary>
            /// The AST node of the procedure.
            /// </summary>
            public readonly Expression.Proc Node;

            private Type type;
            public override Type Type => Assert.NonNullValue(type);

            /// <summary>
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="node">The AST node this procedure originates from.</param>
            /// <param name="type">The <see cref="Type"/> of the procedure.</param>
            public Proc(Expression.Proc node, Type type)
            {
                Node = node;
                this.type = type;
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
                other is IntrinsicProc i && ReferenceEquals(Symbol, i.Symbol);

            public override string ToString() =>
                Symbol.Name;
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

            public override string ToString() =>
                $"external({Name})";
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

            public override string ToString() =>
                Value.ToString();
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

            public override string ToString() =>
                $"\"{Value}\"";
        }

        /// <summary>
        /// A tuple of <see cref="Value"/>s.
        /// </summary>
        public class Tuple : Value
        {
            /// <summary>
            /// The list of <see cref="Value"/>s this <see cref="Tuple"/> consists of.
            /// </summary>
            public readonly IList<Value> Values;

            private Type type;
            public override Type Type => type;

            /// <summary>
            /// Initializes a new <see cref="Tuple"/>.
            /// </summary>
            /// <param name="values">The list of <see cref="Value"/>s this tuple consists of.</param>
            public Tuple(IList<Value> values)
            {
                Values = values;
                type = new Type.Tuple(values.Select(x => x.Type).ToList());
            }

            public override bool EqualsNonNull(Value other) =>
                   other is Tuple t
                && Values.Count == t.Values.Count
                && Values.Zip(t.Values).All(vs => vs.First.EqualsNonNull(vs.Second));

            public override string ToString() =>
                $"({Values.Select(x => x.ToString()).StringJoin(", ")})";
        }
    }
}
