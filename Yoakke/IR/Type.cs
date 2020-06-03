using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    abstract class Type
    {
        public static readonly Type I32 = new IntType(32);
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
}
