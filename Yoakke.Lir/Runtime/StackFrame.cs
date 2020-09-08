using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
