using System;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Runtime
{
    internal class PtrValue : Value
    {
        public int Segment;
        public int Offset { get; set; }
        public readonly Type BaseType;
        public override Type Type => new Type.Ptr(BaseType);

        public PtrValue(int segment, Type baseType)
        {
            Segment = segment;
            BaseType = baseType;
        }

        public PtrValue OffsetBy(int amount, Type newType) => new PtrValue(Segment, newType)
        {
            Offset = Offset + amount,
        };

        public override string ToValueString() => $"{Offset} as {Type}";
        public override bool Equals(Value? other) =>
            other is PtrValue p && Segment == p.Segment && Offset == p.Offset;
        public override int GetHashCode() => HashCode.Combine(typeof(PtrValue), Segment, Offset);
        public override Value Clone() => new PtrValue(Segment, BaseType)
        {
            Offset = Offset,
        };
    }
}
