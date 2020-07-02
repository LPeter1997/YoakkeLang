using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using Yoakke.Compiler.Ast;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Semantic
{
    // Constants

    partial class Value
    {
        public static readonly Value Unit = new Tuple(new List<Value>());
    }

    /// <summary>
    /// Represents a compile-time constant.
    /// </summary>
    public abstract partial class Value
    {
        /// <summary>
        /// The <see cref="Type"/> of this constant value.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Checks, if another <see cref="Value"/> equals with this one.
        /// </summary>
        /// <param name="other">The other <see cref="Value"/> to compare.</param>
        /// <returns>True, if the two <see cref="Value"/>s are equal.</returns>
        public abstract bool Equals(Value other);

        /// <summary>
        /// Calculates the hash code of this <see cref="Value"/>.
        /// </summary>
        /// <returns>The hash code of this <see cref="Value"/>.</returns>
        public abstract override int GetHashCode();

        /// <summary>
        /// Deep-clones this <see cref="Value"/>.
        /// </summary>
        /// <returns>The deep-clone of this <see cref="Value"/>.</returns>
        public abstract Value Clone();

        /// <summary>
        /// Creates a <see cref="string"/> representation of this <see cref="Value"/>.
        /// </summary>
        /// <returns>The <see cref="string"/> representation of this <see cref="Value"/>.</returns>
        public abstract override string ToString();
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
            public override Type Type { get; } = new Type.Variable();

            public override bool Equals(Value other) => throw new NotImplementedException();
            public override int GetHashCode() => throw new NotImplementedException();
            public override Value Clone() => throw new NotImplementedException();
            public override string ToString() => throw new NotImplementedException();
        }

        /// <summary>
        /// A pseudo-<see cref="Value"/> for compile-time only to emulate lvalues.
        /// NOTE: This is only needed for the tree-based evaluator, if we decide to design an IR,
        /// this can be dropped.
        /// </summary>
        public class Lvalue : Value
        {
            /// <summary>
            /// The getter function to read out the underlying <see cref="Value"/>.
            /// </summary>
            public readonly Func<Value> Getter;
            /// <summary>
            /// The setter function to write the underlying <see cref="Value"/>.
            /// </summary>
            public readonly Action<Value> Setter;

            public override Type Type => Getter().Type;

            /// <summary>
            /// Initializes a new <see cref="Lvalue"/>.
            /// </summary>
            /// <param name="getter">The getter function to read out the underlying value.</param>
            /// <param name="setter">The setter function to write the underlying value.</param>
            public Lvalue(Func<Value> getter, Action<Value> setter)
            {
                Getter = getter;
                Setter = setter;
            }

            // TODO
            public override bool Equals(Value other) => throw new NotImplementedException();
            // TODO
            public override int GetHashCode() => throw new NotImplementedException();
            // NOTE: Does it make sense to clone this?
            public override Value Clone() => this;
            // TODO
            public override string ToString() => throw new NotImplementedException();
        }

        /// <summary>
        /// A compiler intrinsic procedure <see cref="Value"/>.
        /// </summary>
        public class IntrinsicProc : Value
        {
            /// <summary>
            /// The <see cref="Func{T, TResult}"/> that gets evaluated compile-time.
            /// </summary>
            public readonly Func<List<Value>, Value> Function;

            public override Type Type { get; }

            /// <summary>
            /// Initializes a new <see cref="IntrinsicProc"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> of the intrinsic procedure.</param>
            /// <param name="func">The intrinsic C# function.</param>
            public IntrinsicProc(Type type, Func<List<Value>, Value> func)
            {
                Type = type;
                Function = func;
            }

            public override bool Equals(Value other) =>
                other is IntrinsicProc i && Type.Equals(i.Type) && ReferenceEquals(Function, i.Function);
            public override int GetHashCode() =>
                //this.HashCombinePoly(Type, Function);
                throw new NotImplementedException();
            // NOTE: Does it make sense to clone this?
            public override Value Clone() => this;
            public override string ToString() => "<intrinsic>";
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

            public override Type Type { get; }

            /// <summary>
            /// Initializes a new <see cref="Extern"/>.
            /// </summary>
            /// <param name="name">The name of the external symbol.</param>
            /// <param name="type">The <see cref="Type"/> of the external symbol.</param>
            public Extern(string name, Type type)
            {
                Type = type;
                Name = name;
            }

            public override bool Equals(Value other) =>
                other is Extern e && Name == e.Name && Type.Equals(e.Type);
            public override int GetHashCode() =>
                //this.HashCombinePoly(Type, Name);
                throw new NotImplementedException();
            // NOTE: Does it make sense to clone this?
            public override Value Clone() => this;
            public override string ToString() => $"external({Name})";
        }

        /// <summary>
        /// A compile-time primitive <see cref="Value"/>.
        /// </summary>
        public class Primitive<T> : Value
        {
            /// <summary>
            /// The primitive value.
            /// </summary>
            public readonly T Value;

            public override Type Type { get; }

            /// <summary>
            /// Initializes a new <see cref="Primitive{T}"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> of the primitive.</param>
            /// <param name="value">The value of the primitive.</param>
            public Primitive(Type type, T value)
            {
                Type = type;
                Value = value;
            }

            // TODO: Do we count in the type?
            public override bool Equals(Value other) =>
                other is Primitive<T> p && Type.Equals(p.Type) && 
                ((Value == null && p.Value == null) || (Value != null && Value.Equals(p.Value)));
            public override int GetHashCode() =>
                //this.HashCombinePoly(Type, Value);
                throw new NotImplementedException();
            public override Value Clone() => new Primitive<T>(Type, Value);
            public override string ToString() => Value?.ToString() ?? throw new NotImplementedException();
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

            public override Type Type { get; }

            /// <summary>
            /// Initializes a new <see cref="Tuple"/>.
            /// </summary>
            /// <param name="values">The list of <see cref="Value"/>s this tuple consists of.</param>
            public Tuple(IList<Value> values)
            {
                Values = values;
                Type = new Type.Tuple(values.Select(x => x.Type).ToList());
            }

            public override bool Equals(Value other) =>
                   other is Tuple t
                && Type.Equals(t.Type)
                && Values.Zip(t.Values).All(vs => vs.First.Equals(vs.Second));
            public override int GetHashCode() =>
                //this.HashCombinePoly(Type, Values);
                throw new NotImplementedException();
            public override Value Clone() => 
                new Tuple(Values.Select(x => x.Clone()).ToList());
            public override string ToString() => 
                $"({Values.Select(x => x.ToString()).StringJoin(", ")})";

        }

        /// <summary>
        /// A struct <see cref="Value"/>.
        /// </summary>
        public class Struct : Value
        {
            /// <summary>
            /// The field <see cref="Value"/>s of this <see cref="Struct"/>.
            /// </summary>
            public readonly IDictionary<string, Value> Fields;

            public override Type Type { get; }

            /// <summary>
            /// Initializes a new <see cref="Struct"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> of the created struct.</param>
            /// <param name="fields">The field <see cref="Value"/>s of the created struct.</param>
            public Struct(Type type, IDictionary<string, Value> fields)
            {
                Type = type;
                Fields = fields;
            }

            public override bool Equals(Value other) =>
                   other is Struct s
                && Type.Equals(s.Type)
                && Fields.All(f => f.Value.Equals(s.Fields[f.Key]));
            // TODO: Maybe order matters here?
            public override int GetHashCode() =>
                //this.HashCombinePoly(Type, Fields);
                throw new NotImplementedException();
            public override Value Clone() => 
                new Struct(Type, Fields.ToDictionary(kv => kv.Key, kv => kv.Value.Clone()));
            public override string ToString() => 
                $"{Type} {{ {Fields.Select(kv => $"{kv.Key} = {kv.Value}").StringJoin("; ")} }}";
        }
    }
}
