using Yoakke.Lir.Values;

namespace Yoakke.Lir.Runtime
{
    internal class StackFrame
    {
        public readonly int ReturnAddress;
        public readonly Value[] Registers;
        public readonly int AllocationIndex;

        public StackFrame(int returnAddress, int registerCount, int allocationIndex)
        {
            ReturnAddress = returnAddress;
            Registers = new Value[registerCount];
            AllocationIndex = allocationIndex;
        }

        public Value this[Register register]
        {
            get => Registers[register.Index];
            set => Registers[register.Index] = value;
        }
    }
}
