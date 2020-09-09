using System;
using System.Numerics;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Values
{
    partial record Value
    {
        /// <summary>
        /// Integer value.
        /// </summary>
        public record Int : Value
        {
            /// <summary>
            /// The integer value.
            /// </summary>
            public readonly BigInteger Value;

            public override Type Type { get; }

            /// <summary>
            /// Initializes a new <see cref="Int"/>.
            /// </summary>
            /// <param name="type">The exact integer <see cref="Type"/>.</param>
            /// <param name="value">The integer value.</param>
            public Int(Type.Int type, BigInteger value)
            {
                if (value < type.MinValue || value > type.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(type), 
                        "The integer type is too small to store the given value!");
                }
                Type = type;
                Value = value;
            }

            public override string ToValueString() => $"{Value} as {Type}";
        }
    }
}
