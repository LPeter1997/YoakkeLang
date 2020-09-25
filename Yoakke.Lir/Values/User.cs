using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
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
            public object? Payload { get; set; }

            /// <summary>
            /// Initializes a new <see cref="User"/>.
            /// </summary>
            /// <param name="payload">The payload of the value.</param>
            public User(object? payload)
            {
            }

            public override string ToValueString() => "void";
            public override bool Equals(Value? other) => 
                   other is User u 
                && (ReferenceEquals(Payload, u.Payload) || (Payload != null && Payload.Equals(u.Payload)));
            public override int GetHashCode() => HashCode.Combine(typeof(User), Payload);
            public override Value Clone()
            {
                if (Payload == null) return new User(null);
                if (Payload is ICloneable cloneable) return new User(cloneable.Clone());
                throw new InvalidOperationException("Can't clone a non-cloneable user payload!");
            }
        }
    }
}
