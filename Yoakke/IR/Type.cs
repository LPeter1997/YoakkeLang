using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    /// <summary>
    /// Base class for every IR type.
    /// </summary>
    abstract class Type
    {
        /// <summary>
        /// The <see cref="Type"/> constant for void (no return value).
        /// </summary>
        public static readonly Type Void = new VoidType();
        /// <summary>
        /// The <see cref="Type"/> constant for i32 type.
        /// </summary>
        public static readonly Type I32 = new IntType(true, 32);

        /// <summary>
        /// Creates a pointer <see cref="Type"/> for the given element <see cref="Type"/>.
        /// </summary>
        /// <param name="elementType">The element <see cref="Type"/> to create a pointer <see cref="Type"/> of.</param>
        /// <returns>The pointer <see cref="Type"/>.</returns>
        public static Type Ptr(Type elementType) =>
            new PtrType(elementType);

        /// <summary>
        /// Checks wether two <see cref="Type"/>s are the same.
        /// </summary>
        /// <param name="t1">The first <see cref="Type"/> to compare.</param>
        /// <param name="t2">The second <see cref="Type"/> to compare.</param>
        /// <returns>True, if the two <see cref="Type"/>s are equal.</returns>
        public static bool Same(Type t1, Type t2) =>
            t1 switch
            {
                VoidType _ => t2 is VoidType,
                IntType i1 => t2 is IntType i2 && i1.Signed == i2.Signed && i1.Bits == i2.Bits,
                PtrType p1 => t2 is PtrType p2 && Same(p1.ElementType, p2.ElementType),
                _ => throw new NotImplementedException(),
            };
    }

    /// <summary>
    /// Void <see cref="Type"/>.
    /// </summary>
    class VoidType : Type { }

    /// <summary>
    /// Integral <see cref="Type"/>.
    /// </summary>
    class IntType : Type
    {
        public readonly bool Signed;
        /// <summary>
        /// The number of bits this <see cref="IntType"/> consists of.
        /// </summary>
        public readonly int Bits;

        /// <summary>
        /// Initializes a new <see cref="IntType"/>.
        /// </summary>
        /// <param name="signed">True, if this integer should be signed.</param>
        /// <param name="bits">The number of bits this integer type should have.</param>
        public IntType(bool signed, int bits)
        {
            Signed = signed;
            Bits = bits;
        }
    }

    /// <summary>
    /// A pointer to another <see cref="Type"/>.
    /// </summary>
    class PtrType : Type
    {
        /// <summary>
        /// The <see cref="Type"/> this <see cref="PtrType"/> points to.
        /// </summary>
        public readonly Type ElementType;

        /// <summary>
        /// Initializes a new <see cref="PtrType"/>.
        /// </summary>
        /// <param name="elementType">The <see cref="Type"/> this <see cref="PtrType"/> should point to.</param>
        public PtrType(Type elementType)
        {
            ElementType = elementType;
        }
    }
}
