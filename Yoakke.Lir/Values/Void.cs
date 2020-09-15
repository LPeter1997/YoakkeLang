using System;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Values
{
    partial class Value
    {
        /// <summary>
        /// Void value.
        /// </summary>
        public class Void : Value
        {
            public override Type Type => Type.Void_;

            /// <summary>
            /// Initializes a new <see cref="Void"/>.
            /// </summary>
            public Void()
            {
            }

            public override string ToValueString() => "void";
            public override bool Equals(Value? other) => other is Void;
            public override int GetHashCode() => HashCode.Combine(typeof(Void));
            // NOTE: Singleton
            public override Value Clone() => this;
        }
    }
}
