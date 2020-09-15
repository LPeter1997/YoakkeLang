using System;
using System.Numerics;
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
            public BigInteger MaxValue => (new BigInteger(1) << Bits) / (Signed ? 2 : 1) - 1;

            /// <summary>
            /// Returns the minimum integer value that this <see cref="Type"/> can store.
            /// </summary>
            public BigInteger MinValue => -(new BigInteger(1) << Bits) / (Signed ? 2 : 1);

            /// <summary>
            /// Creates a new <see cref="Value.Int"/> from an integer value with this <see cref="Type"/>.
            /// </summary>
            /// <param name="value">The integer value.</param>
            /// <returns>The created <see cref="Value.Int"/>.</returns>
            public Value.Int NewValue(BigInteger value) => new Value.Int(this, value);

            public override string ToString() => $"{(Signed ? 'i' : 'u')}{Bits}";
            public override bool Equals(Type? other) =>
                other is Int i && Signed == i.Signed && Bits == i.Bits;
            public override int GetHashCode() => HashCode.Combine(typeof(Int), Signed, Bits);
        }
    }
}
