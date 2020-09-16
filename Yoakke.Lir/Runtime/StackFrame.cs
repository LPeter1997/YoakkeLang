using Yoakke.Lir.Values;

namespace Yoakke.Lir.Runtime
{
    internal class StackFrame
    {
        public readonly int ReturnAddress;
        public readonly Value[] Registers;

        public StackFrame(int returnAddress, int registerCount)
        {
            ReturnAddress = returnAddress;
            Registers = new Value[registerCount];
        }

        public Value this[Register register]
        {
            get => Registers[register.Index];
            set => Registers[register.Index] = value;
        }
    }
}
