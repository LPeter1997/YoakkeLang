using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    abstract class Instruction
    {
    }

    abstract class RegisterInstruction : Instruction
    {
        public abstract Type StoredType { get; }
        public int Register { get; set; }

        public RegisterInstruction(int register)
        {
            Register = register;
        }
    }

    class AllocInstruction : RegisterInstruction
    {
        private Type storedType;
        public Type ElementType { get; }
        public override Type StoredType => storedType;

        public AllocInstruction(int register, Type elementType) 
            : base(register)
        {
            storedType = Type.Ptr(elementType);
            ElementType = elementType;
        }
    }
}
