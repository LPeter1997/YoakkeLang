using System;
using Yoakke.Lir.Instructions;

namespace Yoakke.Lir.Types
{
    /// <summary>
    /// Base for every type.
    /// </summary>
    public abstract partial class Type : IInstrArg, IEquatable<Type>
    {
        public static readonly Void Void_ = new Void();
        public static readonly User User_ = new User();
        public static readonly Int U32 = new Int(false, 32);
        public static readonly Int U64 = new Int(false, 64);
        public static readonly Int I32 = new Int(true, 32);
        public static readonly Int I64 = new Int(true, 64);

        public override bool Equals(object? obj) => obj is Type t && Equals(t);
        public abstract bool Equals(Type? other);
        public abstract override int GetHashCode();
        public abstract string ToTypeString();
    }
}
