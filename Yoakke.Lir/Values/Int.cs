using System;
using Yoakke.DataStructures;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Values
{
    partial class Value
    {
        /// <summary>
        /// Integer value.
        /// </summary>
        public class Int : Value
        {
            /// <summary>
            /// The integer value.
            /// </summary>
            public readonly BigInt Value;

            public override Type Type { get; }

            /// <summary>
            /// Initializes a new <see cref="Int"/>.
            /// </summary>
            /// <param name="type">The exact integer <see cref="Type"/>.</param>
            /// <param name="value">The integer value.</param>
            public Int(Type.Int type, BigInt value)
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
            public override bool Equals(Value? other) => other is Int i && Value == i.Value;
            public override int GetHashCode() => HashCode.Combine(typeof(Int), Value);
            public override Value Clone() => new Int((Type.Int)Type, Value);
        }
    }
}
