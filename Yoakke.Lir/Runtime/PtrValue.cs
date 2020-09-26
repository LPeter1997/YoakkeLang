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
        NonManaged,
    }

    internal abstract class PtrValue : Value
    {
        public abstract PtrPlacement Placement { get; }

        public readonly Type BaseType;
        public override Type Type => new Type.Ptr(BaseType);

        public PtrValue(Type baseType)
        {
            BaseType = baseType;
        }

        public PtrValue OffsetBy(int amount) => OffsetBy(amount, BaseType);
        public abstract PtrValue OffsetBy(int amount, Type newBaseType);
        public override Value Clone() => OffsetBy(0);
    }

    internal class NativePtrValue : PtrValue
    {
        public override PtrPlacement Placement => PtrPlacement.NonManaged;
        public IntPtr Pointer { get; set; }

        public NativePtrValue(IntPtr pointer, Type baseType)
            : base(baseType)
        {
            Pointer = pointer;
        }

        public override PtrValue OffsetBy(int amount, Type newType) => 
            new NativePtrValue(new IntPtr(Pointer.ToInt64() + amount), newType);

        public override string ToValueString() => $"{Pointer} as {Type.ToTypeString()}";
        public override bool Equals(Value? other) =>
            other is NativePtrValue p && Pointer == p.Pointer;
        public override int GetHashCode() => HashCode.Combine(typeof(NativePtrValue), Pointer);
    }

    internal class ManagedPtrValue : PtrValue
    {
        public override PtrPlacement Placement { get; }
        public readonly int Segment;
        public readonly int Offset;
        
        public ManagedPtrValue(PtrPlacement placement, int segment, int offset, Type baseType)
            : base(baseType)
        {
            Placement = placement;
            Segment = segment;
            Offset = offset;
        }

        public override PtrValue OffsetBy(int amount, Type newType) =>
            new ManagedPtrValue(Placement, Segment, Offset + amount, newType);

        public override string ToValueString() => $"{Offset} as {Type.ToTypeString()}";
        public override bool Equals(Value? other) =>
               other is ManagedPtrValue p
            && Placement == p.Placement
            && Segment == p.Segment 
            && Offset == p.Offset;
        public override int GetHashCode() => HashCode.Combine(typeof(ManagedPtrValue), Placement, Segment, Offset);
    }
}
