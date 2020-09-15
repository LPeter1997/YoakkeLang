using System;
using Yoakke.DataStructures;
using Yoakke.Lir.Instructions;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Values
{
    /// <summary>
    /// Base for every value.
    /// </summary>
    public abstract partial class Value : IInstrArg, IEquatable<Value>, ICloneable<Value>
    {
        public static readonly Void Void_ = new Void();

        /// <summary>
        /// The type of this <see cref="Value"/>.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Converts this <see cref="Value"/> to it's string representation.
        /// </summary>
        /// <returns>The string representation of this <see cref="Value"/>.</returns>
        public abstract string ToValueString();

        public override string ToString() => ToValueString();

        // IEquatable
        public abstract bool Equals(Value? other);
        public abstract override int GetHashCode();
        // ICloneable
        public abstract Value Clone();

        public override bool Equals(object? obj) => obj is Value v && Equals(v);
        public static bool operator ==(Value v1, Value v2) => v1.Equals(v2);
        public static bool operator !=(Value v1, Value v2) => !(v1 == v2);
    }
}
