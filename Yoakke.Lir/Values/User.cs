using System;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Values
{
    partial class Value
    {
        /// <summary>
        /// A custom user type.
        /// </summary>
        public class User : Value
        {
            public override Type Type => Type.User_;

            /// <summary>
            /// The payload value.
            /// </summary>
            public ICloneable? Payload { get; set; }

            /// <summary>
            /// Initializes a new <see cref="User"/>.
            /// </summary>
            /// <param name="payload">The payload of the value.</param>
            public User(ICloneable? payload)
            {
                Payload = payload;
            }

            public override string ToValueString() => $"user<{Payload}>";
            public override bool Equals(Value? other) => 
                   other is User u 
                && (ReferenceEquals(Payload, u.Payload) || (Payload != null && Payload.Equals(u.Payload)));
            public override int GetHashCode() => HashCode.Combine(typeof(User), Payload);
            public override Value Clone() => new User((ICloneable?)Payload?.Clone());
        }
    }
}
