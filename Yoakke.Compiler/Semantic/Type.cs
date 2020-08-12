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
        public static readonly Type Unit = new Tuple(new List<Type>());
        public static readonly Type Type_ = new Primitive("type", IR.Type.Void_); // TODO
        public static readonly Type Str = new Primitive("str", IR.Type.Void_); // TODO
        public static readonly Type I32 = new Primitive("i32", IR.Type.I32);
        public static readonly Type Bool = new Primitive("bool", IR.Type.Bool);    
    }

    /// <summary>
    /// The base class for types in the compiler.
    /// </summary>
    public abstract partial class Type : Value, IEquatable<Type>
    {
        /// <summary>
        /// Returns the substitution for this <see cref="Type"/>. Could be itself, if there's no substitution.
        /// </summary>
        public virtual Type Substitution => this;

        /// <summary>
        /// True, if this <see cref="Type"/> is fully specified, meaning there are no type variables in it.
        /// </summary>
        public abstract bool IsFullySpecified { get; }

        public override bool Equals(Value? other) =>
            other is Type t && Substitution.Equals(t.Substitution);

        /// <summary>
        /// Checks, if this <see cref="Type"/> contains the given <see cref="Type"/>.
        /// This is useful for unification.
        /// </summary>
        /// <param name="type">The type to search for.</param>
        /// <returns>True, if the <see cref="Type"/> is contained.</returns>
        public abstract bool Contains(Type type);

        /// <summary>
        /// Unifies this <see cref="Type"/> with another one.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to unify this one with.</param>
        public abstract void UnifyWith(Type other);

        /// <summary>
        /// Deep-clones this <see cref="Type"/>.
        /// </summary>
        /// <returns>The deep-cloned <see cref="Type"/>.</returns>
        public abstract override Value Clone();

        /// <summary>
        /// Checks, if another <see cref="Type"/> equals with this one.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to compare.</param>
        /// <returns>True, if the two <see cref="Type"/>s are equal.</returns>
        public abstract bool Equals(Type? other);

        /// <summary>
        /// Calculates a hash-code for this <see cref="Type"/>.
        /// </summary>
        /// <returns>A calculated hash code.</returns>
        public abstract override int GetHashCode();

        /// <summary>
        /// Creates a <see cref="string"/> representation of this <see cref="Type"/>.
        /// </summary>
        /// <returns>The <see cref="string"/> representation of this <see cref="Type"/>.</returns>
        public abstract override string ToString();

        private static void UnifyLists((Type, IList<Type>) tup1, (Type, IList<Type>) tup2)
        {
            var (t1, list1) = tup1;
            var (t2, list2) = tup2;
            if (list1.Count != list2.Count) throw new TypeError(t1, t2);
            foreach (var (left, right) in list1.Zip(list2)) left.UnifyWith(right);
        }

        private static void UnifyDictionaries<TKey>(
            (Type, IDictionary<TKey, Type>) tup1, 
            (Type, IDictionary<TKey, Type>) tup2)
            where TKey: notnull
        {
            var (t1, dict1) = tup1;
            var (t2, dict2) = tup2;
            if (dict1.Count != dict2.Count) throw new TypeError(t1, t2);
            foreach (var kv in dict1)
            {
                if (!dict2.TryGetValue(kv.Key, out var value)) throw new TypeError(t1, t2);
                kv.Value.UnifyWith(value);
            }
        }
    }

    // Variants

    partial class Type
    {
        /// <summary>
        /// Represents a <see cref="Type"/> that's not inferred yet, and could be substituted for another.
        /// </summary>
        public class Variable : Type
        {
            private static int instanceCount = 0;

            private readonly int instanceId;
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

            public override Type Type => Type_;

            public override bool IsFullySpecified => 
                substitution == null ? false : Substitution.IsFullySpecified;

            private Variable(int id) { instanceId = id;  }

            public Variable() : this(instanceCount++) { }

            public override bool Contains(Type type)
            {
                type = type.Substitution;
                return substitution == null ? ReferenceEquals(this, type) : Substitution.Contains(type);
            }

            public override void UnifyWith(Type other)
            {
                other = other.Substitution;
                if (substitution != null)
                {
                    Substitution.UnifyWith(other);
                    return;
                }
                // Other type variable
                if (other is Variable var)
                {
                    if (ReferenceEquals(this, var)) return;
                }
                // Something else
                if (other.Contains(this)) throw new NotImplementedException("Type-recursion!");
                // Free to substitute
                substitution = other;
            }

            
            public override Value Clone()
            {
                // If there's a substitution, clone that
                if (substitution != null) return Substitution.Clone();
                // Otherwise, create a variable whose substitution is this one
                var result = new Variable(instanceId);
                result.substitution = this;
                return result;
            }

            public override bool Equals(Type? other) =>
                ReferenceEquals(Substitution, other?.Substitution);

            public override int GetHashCode() =>
                substitution == null ? this.HashCombinePoly(instanceId) : Substitution.GetHashCode();

            public override string ToString()
            {
                // We print it in a format of Xyz' using the same style Excel does
                var result = string.Empty;
                int counter = instanceId + 1;
                while (counter > 0)
                {
                    var remainder = (counter - 1) % 26;
                    result = (char)('a' + remainder) + result;
                    counter = (counter - remainder) / 26;
                }
                return char.ToUpper(result[0]) + result.Substring(1) + '\'';
            }
        }

        // TODO: This would require some proper subtyping. For now it's good enough for builtins.
        /// <summary>
        /// A simple way to do type-erasure, a <see cref="Type"/> that will unify with every other <see cref="Type"/>.
        /// </summary>
        public class Any : Type
        {
            public override Type Type => Type_;
            public override bool IsFullySpecified => throw new NotImplementedException();

            public override bool Contains(Type type) => Equals(type);

            public override void UnifyWith(Type other)
            {
                // NO-OP
            }

            public override Value Clone() => throw new NotImplementedException();
            public override bool Equals(Type? other) => ReferenceEquals(this, other?.Substitution);
            public override int GetHashCode() => throw new NotImplementedException();
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
            public override bool IsFullySpecified => true;

            /// <summary>
            /// Initializes a new <see cref="Primitive"/>.
            /// </summary>
            /// <param name="name">The name of this primitive.</param>
            public Primitive(string name, IR.Type irType)
            {
                Name = name;
                IrType = irType;
            }

            public override bool Contains(Type type) => Equals(type.Substitution);

            public override void UnifyWith(Type other)
            {
                other = other.Substitution;
                if (other is Variable v)
                {
                    v.UnifyWith(this);
                    return;
                }
                // TODO: We won't need this
                if (other is Any any)
                {
                    any.UnifyWith(this);
                    return;
                }
                if (!Equals(other)) throw new TypeError(this, other);
            }

            public override Value Clone() => new Primitive(Name, IrType);
            public override bool Equals(Type? other) => 
                other?.Substitution is Primitive o && Name == o.Name && IrType.EqualsNonNull(o.IrType);
            public override int GetHashCode() => this.HashCombinePoly(Name);
            public override string ToString() => Name;
        }

        /// <summary>
        /// A list of <see cref="Type"/>s, known as a tuple.
        /// </summary>
        new public class Tuple : Type
        {
            /// <summary>
            /// The <see cref="Type"/>s this <see cref="Tuple"/> type consists of.
            /// </summary>
            public readonly IList<Type> Types;

            public override Type Type => Type_;
            public override bool IsFullySpecified => Types.All(x => x.IsFullySpecified);

            /// <summary>
            /// Initializes a new <see cref="Tuple"/>.
            /// </summary>
            /// <param name="types">The <see cref="Type"/>s the tuple consists of.</param>
            public Tuple(IList<Type> types)
            {
                Types = types;
            }

            public override bool Contains(Type type) => Types.Any(x => x.Contains(type));

            public override void UnifyWith(Type other)
            {
                other = other.Substitution;
                if (other is Variable v)
                {
                    v.UnifyWith(this);
                    return;
                }
                // TODO: We won't need this
                if (other is Any any)
                {
                    any.UnifyWith(this);
                    return;
                }
                if (!(other is Tuple tup)) throw new TypeError(this, other);
                UnifyLists((this, Types), (tup, tup.Types));
            }

            public override Value Clone() =>
                new Tuple(Types.Select(x => (Type)x.Clone()).ToList());
            public override bool Equals(Type? other) =>
                   other?.Substitution is Tuple tup && Types.Count == tup.Types.Count 
                && Types.Zip(tup.Types).All(ts => ts.First.Equals(ts.Second));
            public override int GetHashCode() => this.HashCombinePoly(Types);
            public override string ToString() =>
                $"({string.Join(", ", Types.Select(x => x.Substitution.ToString()))})";
        }

        /// <summary>
        /// A procedure's <see cref="Type"/>.
        /// </summary>
        public class Proc : Type
        {
            /// <summary>
            /// The parameter <see cref="Type"/>s of this procedure <see cref="Type"/>.
            /// </summary>
            public readonly IList<Type> Parameters;
            /// <summary>
            /// The return <see cref="Type"/>.
            /// </summary>
            public readonly Type Return;

            public override Type Type => Type_;
            public override bool IsFullySpecified => 
                Parameters.All(x => x.IsFullySpecified) && Return.IsFullySpecified;

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

            public override bool Contains(Type type) => 
                Parameters.Any(x => x.Contains(type)) || Return.Contains(type);

            public override void UnifyWith(Type other)
            {
                other = other.Substitution;
                if (other is Variable v)
                {
                    v.UnifyWith(this);
                    return;
                }
                // TODO: We won't need this
                if (other is Any any)
                {
                    any.UnifyWith(this);
                    return;
                }
                if (!(other is Proc proc)) throw new TypeError(this, other);
                UnifyLists((this, Parameters), (proc, proc.Parameters));
                Return.UnifyWith(proc.Return);
            }

            public override Value Clone() =>
                new Proc(Parameters.Select(x => (Type)x.Clone()).ToList(), (Type)Return.Clone());
            public override bool Equals(Type? other) =>
                   other?.Substitution is Proc proc && Parameters.Count == proc.Parameters.Count
                && Parameters.Zip(proc.Parameters).All(ts => ts.First.Equals(ts.Second))
                && Return.Equals(proc.Return);
            public override int GetHashCode() =>
                this.HashCombinePoly(Parameters, Return);
            public override string ToString() =>
                $"proc({string.Join(", ", Parameters.Select(x => x.ToString()))}) -> {Return.Substitution}";
        }

        /// <summary>
        /// A user-defined structure with named fields.
        /// </summary>
        new public class Struct : Type
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

            public override Type Type => Type_;
            public override bool IsFullySpecified => Fields.Values.All(x => x.IsFullySpecified);

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

            public override bool Contains(Type type) =>
                Fields.Values.Any(x => x.Contains(type));

            public override void UnifyWith(Type other)
            {
                other = other.Substitution;
                if (other is Variable v)
                {
                    v.UnifyWith(this);
                    return;
                }
                // TODO: We won't need this
                if (other is Any any)
                {
                    any.UnifyWith(this);
                    return;
                }
                if (!(other.Substitution is Struct s)) throw new TypeError(this, other);
                if (Token != s.Token) throw new TypeError(this, s);
                UnifyDictionaries((this, Fields), (s, s.Fields));
            }

            public override Value Clone() =>
                new Struct(Token, Fields.ToDictionary(kv => kv.Key, kv => (Type)kv.Value.Clone()), Scope);
            public override bool Equals(Type? other) =>
                   other?.Substitution is Struct s
                && Token == s.Token
                && Fields.ValueEquals(s.Fields);
            public override int GetHashCode() =>
                this.HashCombinePoly(Token, Fields);
            public override string ToString() =>
                $"{Token.Value} {{ {string.Join("; ", Fields.Select(kv => $"{kv.Key}: {kv.Value}"))} }}";
        }
    }
}
