using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yoakke.Compiler.Syntax;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Semantic
{
    // Constants, constructors

    partial class Type
    {
        public static readonly Type Any_ = new Any();
        new public static readonly Type Unit = new Tuple(new List<Type>());
        public static readonly Type Type_ = new Primitive("type", IR.Type.Void_); // TODO
        public static readonly Type Str = new Primitive("str", IR.Type.Void_); // TODO
        public static readonly Type I32 = new Primitive("i32", IR.Type.I32);
        public static readonly Type Bool = new Primitive("bool", IR.Type.Bool);
    }

    /// <summary>
    /// The base class for types in the compiler.
    /// </summary>
    public abstract partial class Type : Value
    {
        /// <summary>
        /// Returns the substitution for this <see cref="Type"/>. Could be itself, if there's no substitution.
        /// </summary>
        public virtual Type Substitution => this;

        public override bool Equals(Value? other) =>
            other is Type t && Substitution.Equals(t.Substitution);

        /// <summary>
        /// Unifies this <see cref="Type"/> with another one.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to unify this one with.</param>
        public void Unify(Type other)
        {
            var s1 = Substitution;
            var s2 = other.Substitution;
            if (s2 is Variable v) v.UnifyInternal(s1);
            else if (s2 is Any a) a.UnifyInternal(s1);
            else s1.UnifyInternal(s2);
        }

        /// <summary>
        /// Unifies this <see cref="Type"/> with another one.
        /// This is the internal version that always has substitutions passed in.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to unify this one with.</param>
        protected abstract void UnifyInternal(Type other);

        /// <summary>
        /// Checks, if this <see cref="Type"/> contains the given <see cref="Type"/>.
        /// This is useful for unification.
        /// </summary>
        /// <param name="type">The type to search for.</param>
        /// <returns>True, if the <see cref="Type"/> is contained.</returns>
        public abstract bool Contains(Type type);

        /// <summary>
        /// Checks, if another <see cref="Type"/> equals with this one.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to compare.</param>
        /// <returns>True, if the two <see cref="Type"/>s are equal.</returns>
        public abstract bool Equals(Type other);
    }

    // Variants

    partial class Type
    {
        /// <summary>
        /// Represents a <see cref="Type"/> that's not inferred yet, and could be substituted for another.
        /// </summary>
        public class Variable : Type
        {
            public override Type Type => Type_;

            private Type? substitution;
            public override Type Substitution
            {
                get
                {
                    if (substitution == null) return this;
                    // Pruning
                    substitution = substitution.Substitution;
                    return substitution;
                }
            }

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

            public override bool Contains(Type type) =>
                ReferenceEquals(Substitution, type);
            public override bool Equals(Type other) =>
                ReferenceEquals(Substitution, other.Substitution);
            public override int GetHashCode() => throw new NotImplementedException();
            public override Value Clone() => throw new NotImplementedException();
            public override string ToString() => "<variable>";
        }

        // TODO: This would require some proper subtyping. For now it's good enough for builtins.
        /// <summary>
        /// A simple way to do type-erasure, a <see cref="Type"/> that will unify with every other <see cref="Type"/>.
        /// </summary>
        public class Any : Type
        {
            public override Type Type => Type.Type_;

            protected override void UnifyInternal(Type other)
            {
                // NO-OP
            }

            public override bool Contains(Type type) => Equals(type);
            public override bool Equals(Type other) => ReferenceEquals(this, other.Substitution);
            public override int GetHashCode() => throw new NotImplementedException();
            public override Value Clone() => throw new NotImplementedException();
            public override string ToString() => "<any>";
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
            /// <summary>
            /// The IR translation of this <see cref="Type"/>.
            /// </summary>
            public readonly IR.Type IrType;

            public override Type Type => Type_;

            /// <summary>
            /// Initializes a new <see cref="Primitive"/>.
            /// </summary>
            /// <param name="name">The name of this primitive.</param>
            public Primitive(string name, IR.Type irType)
            {
                Name = name;
                IrType = irType;
            }

            protected override void UnifyInternal(Type other)
            {
                if (!Equals(other.Substitution)) throw new TypeError(this, other);
            }

            public override bool Contains(Type type) => Equals(type);
            public override bool Equals(Type other) => 
                other.Substitution is Primitive o && Name == o.Name && IrType.EqualsNonNull(o.IrType);
            public override int GetHashCode() =>
                //this.HashCombinePoly(Name);
                throw new NotImplementedException();
            public override Value Clone() => new Primitive(Name, IrType);
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

            protected override void UnifyInternal(Type other)
            {
                // Check if other is a product
                if (!(other.Substitution is Product p)) throw new TypeError(this, other.Substitution);
                // Check for same implementation types
                if (GetType() != p.GetType()) throw new TypeError(this, p);
                // Check for sub-component count
                if (Components.Count() != p.Components.Count()) throw new TypeError(this, p);
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

            public override bool Contains(Type type)
            {
                if (ReferenceEquals(this, type.Substitution)) return true;
                foreach (var c in Components)
                {
                    if (c is Type t && t.Substitution.Contains(type)) return true;
                }
                return false;
            }

            public override bool Equals(Type other)
            {
                // Check if other is a product
                if (!(other.Substitution is Product p)) return false;
                // Check for same implementation types
                if (GetType() != other.GetType()) return false;
                // Check for sub-component count
                if (Components.Count() != p.Components.Count()) return false;
                // Check for sub-component equality
                return Components.Zip(p.Components).All(x => x.First.Equals(x.Second));
            }

            public override int GetHashCode() =>
                //this.HashCombinePoly(Components);
                throw new NotImplementedException();
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

            public override Value Clone() =>
                new Tuple(Types.Select(x => (Type)x.Clone()).ToList());
            public override string ToString() =>
                $"({Types.Select(x => x.Substitution.ToString()).StringJoin(", ")})";
        }

        /// <summary>
        /// A procedure's <see cref="Type"/>.
        /// </summary>
        public class Proc : Product
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

            public override Value Clone() =>
                new Proc(Parameters.Select(x => (Type)x.Clone()).ToList(), (Type)Return.Clone());
            public override string ToString() =>
                $"proc({Parameters.Select(x => x.ToString()).StringJoin(", ")}) -> {Return.Substitution}";
        }

        /// <summary>
        /// A user-defined structure with named fields.
        /// </summary>
        new public class Struct : Product
        {
            /// <summary>
            /// The 'struct' <see cref="Token"/> that defined this <see cref="Struct"/>.
            /// </summary>
            public readonly Token Token;
            /// <summary>
            /// The data fields, each with a name and <see cref="Type"/>.
            /// </summary>
            public readonly IDictionary<string, Type> Fields;
            /// <summary>
            /// The <see cref="Scope"/> this <see cref="Struct"/> declares it's associated constants in.
            /// </summary>
            public readonly Scope Scope;

            public override IEnumerable<Value> Components => Fields.Values;

            /// <summary>
            /// Initializes a new <see cref="Struct"/>.
            /// </summary>
            /// <param name="token">The 'struct' <see cref="Token"/> that defined this struct.</param>
            /// <param name="fields">The data fields, each with a name and <see cref="Type"/>.</param>
            /// <param name="scope">The <see cref="Scope"/> this struct declares it's associated constants in.</param>
            public Struct(Token token, IDictionary<string, Type> fields, Scope scope)
            {
                Token = token;
                Fields = fields;
                Scope = scope;
            }

            protected override void UnifyInternal(Type other)
            {
                // Check if other is a struct
                // NOTE: This is repeated in Product.UnifyInternal, but we need to check field names here
                // so we need to cast other to a struct
                if (!(other.Substitution is Struct s)) throw new TypeError(this, other.Substitution);
                if (Fields.Count != s.Fields.Count) throw new TypeError(this, s);
                foreach (var f in Fields.Keys)
                {
                    if (!s.Fields.ContainsKey(f)) throw new TypeError(this, s);
                }
                base.UnifyInternal(s);
            }

            public override bool Equals(Type other) =>
                   other.Substitution is Struct s 
                && Token == s.Token 
                && Fields.Keys.Count == s.Fields.Keys.Count
                && Fields.Keys.All(k => s.Fields.ContainsKey(k))
                && base.Equals(other);

            public override Value Clone() =>
                new Struct(Token, Fields.ToDictionary(kv => kv.Key, kv => (Type)kv.Value.Clone()), Scope);
            public override string ToString() =>
                $"{Token.Value} {{ {Fields.Select(kv => $"{kv.Key}: {kv.Value}").StringJoin("; ")} }}";
        }
    }
}
