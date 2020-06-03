using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    abstract class Type
    {
        public static readonly Type Void = new VoidType();
        public static readonly Type I32 = new IntType(32);

        public static Type Ptr(Type elementType) =>
            new PtrType(elementType);
    }

    class VoidType : Type { }

    class IntType : Type
    {
        public readonly int Bits;

        public IntType(int bits)
        {
            Bits = bits;
        }
    }

    class PtrType : Type
    {
        public readonly Type ElementType;

        public PtrType(Type elementType)
        {
            ElementType = elementType;
        }
    }
}
