using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    // Constants, constructors

    partial class Type
    {
        public static readonly Type Any_ = new Any();
        new public static readonly Type Unit = new Tuple(new List<Type>());
        public static readonly Type Type_ = new Primitive("type");
        new public static readonly Type Str = new Primitive("str");
        public static readonly Type I32 = new Primitive("i32");
    }

    /// <summary>
    /// The base class for types in the compiler.
    /// </summary>
    abstract partial class Type : Value
    {
        /// <summary>
        /// Returns the substitution for this <see cref="Type"/>. Could be itself, if there's no substitution.
        /// </summary>
        protected virtual Type Substitution => this;

        public override bool EqualsNonNull(Value other) =>
            other is Type t && Substitution.EqualsNonNull(t.Substitution);

        /// <summary>
        /// Checks, if another <see cref="Type"/> equals with this one.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to compare.</param>
        /// <returns>True, if the two <see cref="Type"/>s are equal.</returns>
        public abstract bool EqualsNonNull(Type other);

        /// <summary>
        /// Checks, if this <see cref="Type"/> contains the given <see cref="Type"/>.
        /// This is useful for unification.
        /// </summary>
        /// <param name="type">The type to search for.</param>
        /// <returns>True, if the <see cref="Type"/> is contained.</returns>
        protected abstract bool Contains(Type type);

        /// <summary>
        /// Unifies this <see cref="Type"/> with another one.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to unify this one with.</param>
        public void Unify(Type other)
        {
            var s1 = Substitution;
            var s2 = other.Substitution;
            if (s2 is Variable v) v.UnifyInternal(s1);
            if (s2 is Any a) a.UnifyInternal(s1);
            else s1.UnifyInternal(s2);
        }

        /// <summary>
        /// Unifies this <see cref="Type"/> with another one.
        /// This is the internal version that always has substitutions passed in.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to unify this one with.</param>
        protected abstract void UnifyInternal(Type other);
    }

    // Variants

    partial class Type
    {
        /// <summary>
        /// Represents a <see cref="Type"/> that's not inferred yet, and could be substituted for another.
        /// </summary>
        public class Variable : Type
        {
            /// <summary>
            /// Utility to construct a list of <see cref="Variable"/>s.
            /// </summary>
            /// <param name="amount">The amount of <see cref="Variable"/>s to add to the list.</param>
            /// <returns>A list of <see cref="Variable"/>s.</returns>
            public static IList<Type> ListOf(int amount)
            {
                var result = new List<Type>(amount);
                for (int i = 0; i < amount; ++i) result.Add(new Variable());
                return result;
            }

            public override Type Type => Type_;

            private Type? substitution;
            protected override Type Substitution
            {
                get
                {
                    if (substitution == null) return this;
                    // Pruning
                    substitution = substitution.Substitution;
                    return substitution;
                }
            }

            public override bool EqualsNonNull(Type other) =>
                ReferenceEquals(Substitution, other.Substitution);

            protected override bool Contains(Type type) =>
                ReferenceEquals(Substitution, type);

            protected override void UnifyInternal(Type other)
            {
                Debug.Assert(substitution == null, "Can only substitute for a type variable once!");
                // Other type variable
                if (other is Variable var)
                {
                    if (ReferenceEquals(this, var)) return;
                    substitution = other;
                    return;
                }
                // Something else
                if (other.Contains(this)) throw new NotImplementedException("Type-recursion!");
                // Free to substitute
                substitution = other;
            }
        }

        // TODO: This would require some proper subtyping. For now it's good enough for builtins.
        /// <summary>
        /// A simple way to do type-erasure, a <see cref="Type"/> that will unify with every other <see cref="Type"/>.
        /// </summary>
        public class Any : Type
        {
            public override Type Type => Type.Type_;

            public override bool EqualsNonNull(Type other) =>
                ReferenceEquals(this, other);

            protected override bool Contains(Type type) =>
                EqualsNonNull(type);

            protected override void UnifyInternal(Type other)
            {
                // NO-OP
            }
        }

        /// <summary>
        /// A primitive <see cref="Type"/>.
        /// </summary>
        public class Primitive : Type
        {
            /// <summary>
            /// The name of this primitive <see cref="Type"/>.
            /// </summary>
            public readonly string Name;

            public override Type Type => Type_;

            /// <summary>
            /// Initializes a new <see cref="Primitive"/>.
            /// </summary>
            /// <param name="name">The name of this primitive.</param>
            public Primitive(string name)
            {
                Name = name;
            }

            public override bool EqualsNonNull(Type other) =>
                ReferenceEquals(this, other.Substitution);

            protected override bool Contains(Type type) =>
                EqualsNonNull(type);

            protected override void UnifyInternal(Type other)
            {
                if (!EqualsNonNull(other.Substitution)) throw new Exception($"Type mismatch {this} vs {other}");
            }

            public override string ToString() => Name;
        }

        /// <summary>
        /// Abstraction for product <see cref="Type"/>s.
        /// </summary>
        public abstract class Product : Type
        {
            public override Type Type => Type_;

            /// <summary>
            /// The components this <see cref="Product"/> type consists of.
            /// </summary>
            public abstract IEnumerable<Value> Components { get; }

            public override bool EqualsNonNull(Type other)
            {
                // Check if other is a product
                if (!(other.Substitution is Product p)) return false;
                // Check for same implementation types
                if (GetType() != other.GetType()) return false;
                // Check for sub-component count
                if (Components.Count() != p.Components.Count()) return false;
                // Check for sub-component equality
                return Components.Zip(p.Components).All(x => x.First.EqualsNonNull(x.Second));
            }

            protected override bool Contains(Type type)
            {
                if (ReferenceEquals(this, type.Substitution)) return true;
                foreach (var c in Components)
                {
                    if (c is Type t && t.Substitution.Contains(type)) return true;
                }
                return false;
            }

            protected override void UnifyInternal(Type other)
            {
                // Check if other is a product
                if (!(other.Substitution is Product p)) throw new Exception($"Type mismatch {this} vs {other.Substitution}");
                // Check for same implementation types
                if (GetType() != p.GetType()) throw new Exception($"Type mismatch {this} vs {p}");
                // Check for sub-component count
                if (Components.Count() != p.Components.Count()) throw new Exception($"Type mismatch {this} vs {p}");
                // Unify sub-components
                var i1 = Components.GetEnumerator();
                var i2 = p.Components.GetEnumerator();
                while (i1.MoveNext() && i2.MoveNext())
                {
                    var c1 = i1.Current;
                    var c2 = i2.Current;
                    if (c1 is Type t1 && c2 is Type t2)
                    {
                        // Type-type
                        t1.Unify(t2);
                    }
                    else if (c1 is Type || c2 is Type)
                    {
                        throw new Exception("Type-value mismatch!");
                    }
                    else if (!c1.Equals(c2))
                    {
                        // Value-value
                        throw new Exception("Value mismatch!");
                    }
                }
            }
        }

        /// <summary>
        /// A list of <see cref="Type"/>s, known as a tuple.
        /// </summary>
        new public class Tuple : Product
        {
            /// <summary>
            /// The <see cref="Type"/>s this <see cref="Tuple"/> type consists of.
            /// </summary>
            public readonly IList<Type> Types;

            public override IEnumerable<Value> Components => Types;

            /// <summary>
            /// Initializes a new <see cref="Tuple"/>.
            /// </summary>
            /// <param name="types">The <see cref="Type"/>s the tuple consists of.</param>
            public Tuple(IList<Type> types)
            {
                Types = types;
            }

            public override string ToString() =>
                $"({Types.Select(x => x.Substitution.ToString()).StringJoin(", ")})";
        }

        /// <summary>
        /// A procedure's <see cref="Type"/>.
        /// </summary>
        new public class Proc : Product
        {
            /// <summary>
            /// The parameter <see cref="Type"/>s of this procedure <see cref="Type"/>.
            /// </summary>
            public readonly IList<Type> Parameters;
            /// <summary>
            /// The return <see cref="Type"/>.
            /// </summary>
            public readonly Type Return;

            public override IEnumerable<Value> Components => Parameters.Append(Return);

            /// <summary>
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="parameters">The parameter <see cref="Type"/>s.</param>
            /// <param name="ret">The return <see cref="Type"/>.</param>
            public Proc(IList<Type> parameters, Type ret)
            {
                Parameters = parameters;
                Return = ret;
            }

            public override string ToString() =>
                $"proc({Parameters.Select(x => x.ToString()).StringJoin(", ")}) -> {Return.Substitution}";
        }
    }
}
