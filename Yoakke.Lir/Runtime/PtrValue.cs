using System;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Runtime
{
    internal class PtrValue : Value
    {
        public Value? Value { get; set; }
        public int Offset { get; set; }

        private Type baseType;
        public override Type Type => GetTypeForPointer();

        public PtrValue(Type baseType)
        {
            this.baseType = baseType;
        }

        private Type GetTypeForPointer()
        {
            if (Offset == 0) return new Type.Ptr(baseType);
            // TODO
            throw new NotImplementedException();
        }

        public PtrValue OffsetBy(int amount)
        {
            var clone = (PtrValue)Clone();
            clone.Offset += amount;
            return clone;
        }

        // TODO
        public override string ToValueString() => "<some ptr>";
        public override bool Equals(Value? other) =>
               other is PtrValue p 
            && ((Value is null && p.Value is null) || (Value is not null && Value.Equals(p.Value)))
            && Offset == p.Offset;
        public override int GetHashCode() => HashCode.Combine(typeof(PtrValue), Value, Offset);
        public override Value Clone() => new PtrValue(baseType)
        {
            // NOTE: We DON'T clone this value, this allows us to act like a pointer!
            Value = Value,
            Offset = Offset,
        };
    }
}
