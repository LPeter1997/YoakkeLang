using System;
using System.Diagnostics;
using System.Text;

namespace Yoakke.Compiler.TypeSystem
{
    /// <summary>
    /// The base for every type.
    /// </summary>
    public abstract partial class Type : IEquatable<Type>
    {
        /// <summary>
        /// The substitution for this <see cref="Type"/>.
        /// </summary>
        internal virtual Type Substitution => this;

        public override bool Equals(object obj) => obj is Type t && Equals(t);

        /// <summary>
        /// Checks, if this <see cref="Type"/> contains another one.
        /// </summary>
        /// <param name="other">The <see cref="Type"/> to search for in this one.</param>
        /// <returns>True, if this <see cref="Type"/> contains the <paramref name="other"/> one.</returns>
        internal abstract bool Contains(Type other);
        /// <summary>
        /// Unifies this <see cref="Type"/> with another one, meaning that they are checked for compatibility and
        /// constraints are enforced between them.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to unify with this one.</param>
        public abstract void UnifyWith(Type other);
        /// <summary>
        /// Deep-copies this <see cref="Type"/>. The clone will have the same constraints as this one and type-variables
        /// will cross-reference.
        /// </summary>
        /// <returns>A deep-copy of this <see cref="Type"/>.</returns>
        public abstract Type Clone();
        /// <summary>
        /// Checks, if this <see cref="Type"/> equals (means: equivalent) with another one.
        /// </summary>
        /// <param name="other">The <see cref="Type"/> to check equality with.</param>
        /// <returns>True, if this <see cref="Type"/> is equivalent to the <paramref name="other"/> one.</returns>
        public abstract bool Equals(Type other);
        /// <summary>
        /// Calculates a hash-code for this <see cref="Type"/>.
        /// </summary>
        /// <returns>A calculated hash-code.</returns>
        public abstract override int GetHashCode();
        /// <summary>
        /// Creates a <see cref="string"/> representation of this <see cref="Type"/>.
        /// </summary>
        /// <returns>The <see cref="string"/> representation of this <see cref="Type"/>.</returns>
        public abstract override string ToString();
    }

    partial class Type
    {
        /// <summary>
        /// A placeholder <see cref="Type"/> that can be substituted for another.
        /// </summary>
        public class Var : Type
        {
            private static int instanceCounter = 0;

            private readonly int instanceId = instanceCounter++;
            private Type? substitution;
            /// <summary>
            /// Returns the substitution of this <see cref="Var"/>. Returns itself, if there's no substitution.
            /// </summary>
            internal override Type Substitution
            {
                get
                {
                    if (substitution == null) return this;
                    // Pruning
                    if (substitution is Var var) substitution = var.Substitution;
                    return substitution;
                }
            }

            internal override bool Contains(Type other) =>
                substitution == null ? Equals(other) : Substitution.Contains(other);

            public override void UnifyWith(Type other)
            {
                // When substitution is not null, unify the substitution
                if (substitution != null)
                {
                    Substitution.UnifyWith(other);
                    return;
                }
                // substitution is null
                other = other.Substitution;
                // Unifying with self is a no-op
                if (this == other) return;
                // Check if this is a type recursion
                if (other.Contains(this)) throw new TypeRecursionError(other, this);
                // Do the substitution
                substitution = other;
            }

            public override Type Clone()
            {
                // If there's a substitution, clone that
                if (substitution != null) return Substitution.Clone();
                // No substitution, we have to return a type-variable whose substitution is this one
                var clone = new Var();
                clone.substitution = this;
                return clone;
            }

            public override bool Equals(Type other) => Substitution == other.Substitution;

            public override int GetHashCode() => HashCode.Combine(GetType(), Substitution);

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

        /// <summary>
        /// A primitive <see cref="Type"/>.
        /// </summary>
        public class Prim : Type
        {
            /// <summary>
            /// The name of the primitive.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// Initializes a new <see cref="Prim"/>.
            /// </summary>
            /// <param name="name">The name of the primitive.</param>
            public Prim(string name)
            {
                Name = name;
            }

            internal override bool Contains(Type other) => this == other.Substitution;

            public override void UnifyWith(Type other)
            {
                other = other.Substitution;
                if (!(other is Prim prim) || Name != prim.Name) throw new TypeMismatchError(this, other);
            }

            public override Type Clone() => new Prim(Name);

            public override bool Equals(Type other) =>
                other.Substitution is Prim prim && Name == prim.Name;

            public override int GetHashCode() => HashCode.Combine(GetType(), Name);

            public override string ToString() => Name;
        }
    }
}
