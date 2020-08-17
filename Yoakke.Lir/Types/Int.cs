using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Types
{
    partial record Type
    {
        /// <summary>
        /// Integer type.
        /// </summary>
        public record Int(bool Signed, int Bits) : Type
        {
            public override string ToString() => $"{(Signed ? 'i' : 'u')}{Bits}";

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
        }
    }
}
