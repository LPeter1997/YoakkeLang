using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Yoakke.IR
{
    abstract class Value
    {
        public abstract Type Type { get; }
    }

    class RegisterValue : Value
    {
        private Type type;
        public override Type Type => type;
        public int Index { get; set; }

        public RegisterValue(Type type, int index)
        {
            this.type = type;
            Index = index;
        }
    }

    class IntValue : Value
    {
        private IntType type;
        public override Type Type => type;
        public BigInteger Value { get; set; }

        public IntValue(IntType type, BigInteger value)
        {
            this.type = type;
            Value = value;
        }
    }
}
