using System;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Values
{
    partial class Value
    {
        /// <summary>
        /// A custom user value. Immutable to be safe.
        /// </summary>
        public class User : Value
        {
            public override Type Type => Type.User_;

            /// <summary>
            /// The payload value.
            /// </summary>
            public readonly object Payload;

            /// <summary>
            /// Initializes a new <see cref="User"/>.
            /// </summary>
            /// <param name="payload">The payload of the value.</param>
            public User(object payload)
            {
                Payload = payload;
            }

            public override string ToValueString() => $"user<{Payload}>";
            public override bool Equals(Value? other) =>
                other is User u && Payload.Equals(u.Payload);
            public override int GetHashCode() => HashCode.Combine(typeof(User), Payload);
            // NOTE: Makes no sense to clone this, as it's immutable
            public override Value Clone() => this;
        }
    }
}
