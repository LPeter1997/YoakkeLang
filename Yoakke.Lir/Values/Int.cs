using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;

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
                    throw new System.ArgumentOutOfRangeException(nameof(type), 
                        "The integer type is too small to store the given value!");
                }
                Type = type;
                Value = value;
            }

            public override string ToString() => Value.ToString();
        }
    }
}
