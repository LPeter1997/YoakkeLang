using System;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Runtime
{
    internal class PtrValue : Value
    {
        public readonly byte[] Segment;
        public int Offset { get; set; }
        public readonly Type BaseType;
        public override Type Type { get; }

        public PtrValue(byte[] segment, Type baseType)
        {
            Segment = segment;
            BaseType = baseType;
            Type = new Type.Ptr(baseType);
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
