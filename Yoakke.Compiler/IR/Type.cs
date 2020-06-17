using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.IR
{
    /// <summary>
    /// Base class for every IR type.
    /// </summary>
    abstract partial class Type : IEquatable<Type>
    {
        /// <summary>
        /// The <see cref="Type"/> constant for void (no return value).
        /// </summary>
        public static readonly Type Void_ = new Void();
        /// <summary>
        /// The <see cref="Type"/> constant for i32 type.
        /// </summary>
        public static readonly Type I32 = new Int(true, 32);
        /// <summary>
        /// The <see cref="Type"/> constant for a boolean type.
        /// </summary>
        public static readonly Type Bool = new Int(false, 1);

        public override bool Equals(object? obj) =>
            obj is Type t && Equals(t);

        public bool Equals(Type? other) =>
            other != null && EqualsNonNull(other);

        /// <summary>
        /// Checks, if another <see cref="Type"/> equals with this one.
        /// </summary>
        /// <param name="other">The other <see cref="Type"/> to compare.</param>
        /// <returns>True, if the two <see cref="Type"/>s are equal.</returns>
        public abstract bool EqualsNonNull(Type other);

        public override abstract int GetHashCode();
    }

    // Variants

    partial class Type
    {
        /// <summary>
        /// Void <see cref="Type"/>.
        /// </summary>
        public class Void : Type
        {
            public override bool EqualsNonNull(Type other) =>
                ReferenceEquals(this, other);

            public override int GetHashCode() =>
                HashCode.Combine(GetType());
        }

        /// <summary>
        /// Integral <see cref="Type"/>.
        /// </summary>
        public class Int : Type
        {
            public readonly bool Signed;
            /// <summary>
            /// The number of bits this <see cref="Int"/> consists of.
            /// </summary>
            public readonly int Bits;

            /// <summary>
            /// Initializes a new <see cref="Int"/>.
            /// </summary>
            /// <param name="signed">True, if this integer should be signed.</param>
            /// <param name="bits">The number of bits this integer type should have.</param>
            public Int(bool signed, int bits)
            {
                Signed = signed;
                Bits = bits;
            }

            public override bool EqualsNonNull(Type other) =>
                other is Int i && Bits == i.Bits && Signed == i.Signed;

            public override int GetHashCode() =>
                HashCode.Combine(GetType(), Signed, Bits);
        }

        /// <summary>
        /// A pointer to another <see cref="Type"/>.
        /// </summary>
        public class Ptr : Type
        {
            /// <summary>
            /// The <see cref="Type"/> this <see cref="Ptr"/> points to.
            /// </summary>
            public readonly Type ElementType;

            /// <summary>
            /// Initializes a new <see cref="Ptr"/>.
            /// </summary>
            /// <param name="elementType">The <see cref="Type"/> this <see cref="Ptr"/> should point to.</param>
            public Ptr(Type elementType)
            {
                ElementType = elementType;
            }

            public override bool EqualsNonNull(Type other) =>
                other is Ptr p && ElementType.EqualsNonNull(p.ElementType);

            public override int GetHashCode() =>
                HashCode.Combine(GetType(), ElementType);
        }

        /// <summary>
        /// A callable procedure <see cref="Type"/>.
        /// </summary>
        public class Proc : Type
        {
            /// <summary>
            /// The list of parameter <see cref="Type"/>s.
            /// </summary>
            public readonly List<Type> Parameters;
            /// <summary>
            /// The return <see cref="Type"/>.
            /// </summary>
            public readonly Type ReturnType;

            /// <summary>
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="parameters">The list of parameter <see cref="Type"/>s.</param>
            /// <param name="returnType">The return <see cref="Type"/>.</param>
            public Proc(List<Type> parameters, Type returnType)
            {
                Parameters = parameters;
                ReturnType = returnType;
            }

            public override bool EqualsNonNull(Type other) =>
                   other is Proc p
                && ReturnType.EqualsNonNull(p.ReturnType)
                && Parameters.Count == p.Parameters.Count
                && Parameters.Zip(p.Parameters).All(ts => ts.First.EqualsNonNull(ts.Second));

            public override int GetHashCode() =>
                HashCode.Combine(GetType(), HashList.Combine(Parameters), ReturnType);
        }

        /// <summary>
        /// A structure <see cref="Type"/>. Note that in this IR names are dropped, the structure fields are referred by
        /// index.
        /// </summary>
        public class Struct : Type
        {
            /// <summary>
            /// The list of field <see cref="Type"/>s of this <see cref="Struct"/>.
            /// </summary>
            public readonly List<Type> Fields;

            /// <summary>
            /// Initializes a new <see cref="Struct"/>.
            /// </summary>
            /// <param name="fields">The field <see cref="Type"/>s of this struct.</param>
            public Struct(List<Type> fields)
            {
                Fields = fields;
            }

            public override bool EqualsNonNull(Type other) =>
                   other is Struct s
                && Fields.Count == s.Fields.Count
                && Fields.Zip(s.Fields).All(fs => fs.First.EqualsNonNull(fs.Second));

            public override int GetHashCode() =>
                HashCode.Combine(GetType(), HashList.Combine(Fields));
        }
    }
}
