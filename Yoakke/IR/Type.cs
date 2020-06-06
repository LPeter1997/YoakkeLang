using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    /// <summary>
    /// Base class for every IR type.
    /// </summary>
    abstract partial class Type
    {
        /// <summary>
        /// The <see cref="Type"/> constant for void (no return value).
        /// </summary>
        public static readonly Type Void_ = new Void();
        /// <summary>
        /// The <see cref="Type"/> constant for i32 type.
        /// </summary>
        public static readonly Type I32 = new Int(true, 32);
    }

    // Operations

    partial class Type
    {
        /// <summary>
        /// Checks wether two <see cref="Type"/>s are the same.
        /// </summary>
        /// <param name="t1">The first <see cref="Type"/> to compare.</param>
        /// <param name="t2">The second <see cref="Type"/> to compare.</param>
        /// <returns>True, if the two <see cref="Type"/>s are equal.</returns>
        public static bool Same(Type t1, Type t2) =>
            t1 switch
            {
                Void _ => t2 is Void,
                Int i1 => t2 is Int i2 && i1.Signed == i2.Signed && i1.Bits == i2.Bits,
                Ptr p1 => t2 is Ptr p2 && Same(p1.ElementType, p2.ElementType),
                _ => throw new NotImplementedException(),
            };
    }

    // Variants

    partial class Type
    {
        /// <summary>
        /// Void <see cref="Type"/>.
        /// </summary>
        public class Void : Type { }

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
        }
    }
}
