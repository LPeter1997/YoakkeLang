using System;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Runtime
{
    internal enum PtrPlacement
    {
        StackFrame,
        Globals,
        Constants,
    }

    internal class PtrValue : Value
    {
        public readonly PtrPlacement Placement;
        public readonly int Segment;
        public int Offset { get; set; }

        public readonly Type BaseType;
        public override Type Type => new Type.Ptr(BaseType);

        public PtrValue(PtrPlacement placement, int segment, Type baseType)
        {
            Placement = placement;
            Segment = segment;
            BaseType = baseType;
        }

        public PtrValue OffsetBy(int amount) => OffsetBy(amount, BaseType);
        public PtrValue OffsetBy(int amount, Type newType) => new PtrValue(Placement, Segment, newType)
        {
            Offset = Offset + amount,
        };

        public override string ToValueString() => $"{Offset} as {Type}";
        public override bool Equals(Value? other) =>
            other is PtrValue p && Segment == p.Segment && Offset == p.Offset;
        public override int GetHashCode() => HashCode.Combine(typeof(PtrValue), Segment, Offset);
        public override Value Clone() => OffsetBy(0);
    }
}
