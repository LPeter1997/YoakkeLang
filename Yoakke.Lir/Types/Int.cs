using System;
using Yoakke.DataStructures;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Types
{
    partial class Type
    {
        /// <summary>
        /// Integer type.
        /// </summary>
        public class Int : Type
        {
            public readonly bool Signed;
            public readonly int Bits;

            public Int(bool signed, int bits)
            {
                Signed = signed;
                Bits = bits;
            }

            /// <summary>
            /// Returns the maximum integer value that this <see cref="Type"/> can store.
            /// </summary>
            public BigInt MaxValue => BigInt.MaxValue(Signed, Bits);

            /// <summary>
            /// Returns the minimum integer value that this <see cref="Type"/> can store.
            /// </summary>
            public BigInt MinValue => BigInt.MinValue(Signed, Bits);

            /// <summary>
            /// Creates a new <see cref="Value.Int"/> from an integer value with this <see cref="Type"/>.
            /// </summary>
            /// <param name="value">The integer value.</param>
            /// <returns>The created <see cref="Value.Int"/>.</returns>
            public Value.Int NewValue(BigInt value) => new Value.Int(this, value);

            /// <summary>
            /// <see cref="NewValue(BigInt)"/>.
            /// </summary>
            public Value.Int NewValue(long value) => NewValue(new BigInt(Signed, Bits, value));

            /// <summary>
            /// <see cref="NewValue(BigInt)"/>.
            /// </summary>
            public Value.Int NewValue(ulong value) => NewValue(new BigInt(Signed, Bits, value));

            public override string ToTypeString() => $"{(Signed ? 'i' : 'u')}{Bits}";
            public override bool Equals(Type? other) =>
                other is Int i && Signed == i.Signed && Bits == i.Bits;
            public override int GetHashCode() => HashCode.Combine(typeof(Int), Signed, Bits);
        }
    }
}
