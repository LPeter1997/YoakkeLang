using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    abstract class Instruction
    {
    }

    abstract class ValueInstruction : Instruction
    {
        public RegisterValue Value { get; set; }

        public ValueInstruction(RegisterValue value)
        {
            Value = value;
        }
    }

    class AllocInstruction : ValueInstruction
    {
        public Type ElementType { get; }

        public AllocInstruction(RegisterValue value) 
            : base(value)
        {
            if (value.Type is PtrType ptr)
            {
                ElementType = ptr.ElementType;
            }
            else throw new ArgumentException("Allocation requires a pointer register type!", nameof(value));
        }
    }

    class RetInstruction : Instruction
    {
        public Value? Value { get; set; }

        public RetInstruction(Value? value = null)
        {
            Value = value;
        }
    }
}
