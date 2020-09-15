using System;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Runtime
{
    internal class PtrValue : Value
    {
        public Value? Value { get; set; }
        public int Offset { get; set; }

        public override Type Type { get; }

        public PtrValue(Type type)
        {
            Type = type;
        }

        // TODO
        public override string ToValueString() => "<some ptr>";
        public override bool Equals(Value? other) =>
               other is PtrValue p 
            && ((Value is null && p.Value is null) || (Value is not null && Value.Equals(p.Value)))
            && Offset == p.Offset;
        public override int GetHashCode() => HashCode.Combine(typeof(PtrValue), Value, Offset);
        public override Value Clone() => new PtrValue(Type)
        {
            // NOTE: We DON'T clone this value, this allows us to act like a pointer!
            Value = Value,
            Offset = Offset,
        };
    }
}
