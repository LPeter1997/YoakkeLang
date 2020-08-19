using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Runtime
{
    /// <summary>
    /// A virtual machine to execute IR code directly.
    /// </summary>
    public class VirtualMachine
    {
        /// <summary>
        /// The <see cref="Assembly"/> the VM executes.
        /// </summary>
        public readonly Assembly Assembly;

        private Stack<StackFrame> callStack = new Stack<StackFrame>();
        // TODO: Change this to void constant later
        private Value returnValue = Type.I32.NewValue(0);
        private IList<Instr> code;
        private IDictionary<object, int> addresses;
        private int instructionPointer;

        /// <summary>
        /// Initializes a new <see cref="VirtualMachine"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to load in for the VM.</param>
        public VirtualMachine(Assembly assembly)
        {
            Assembly = assembly;
            code = new List<Instr>();
            addresses = new Dictionary<object, int>();
            CompileAssembly();
        }

        // We simplify our assembly representation by removing labels and simply 
        // creating a list of instructions.
        // The only hardness will be in labels then, which can be transformed into
        // a Dictionary from label to address.
        private void CompileAssembly()
        {
            code.Clear();
            addresses.Clear();
            foreach (var proc in Assembly.Procedures)
            {
                addresses[proc] = code.Count;
                foreach (var bb in proc.BasicBlocks)
                {
                    addresses[bb] = code.Count;
                    foreach (var i in bb.Instructions) code.Add(i);
                }
            }
        }

        /// <summary>
        /// Executes the given procedure by name.
        /// </summary>
        /// <param name="proc">The procedure's name to execute.</param>
        /// <returns>The resulting <see cref="Value"/> of the call.</returns>
        public Value Execute(string name)
        {
            var proc = Assembly.Procedures.First(p => p.Name == name);
            Call(proc);
            while (callStack.Count > 0) ExecuteCycle();
            return returnValue;
        }

        private void ExecuteCycle()
        {
            var instr = code[instructionPointer];
            switch (instr)
            {
            case Instr.Ret ret:
                Return(Unwrap(ret.Value));
                break;

            default: throw new NotImplementedException();
            }
        }

        private void Call(Proc proc)
        {
            callStack.Push(new StackFrame(instructionPointer + 1));
            var address = addresses[proc];
            instructionPointer = address;
        }

        private void Return(Value value)
        {
            var top = callStack.Pop();
            // TODO: Only do this when the call stack got emptied
            // Or can we keep it as return value storage?
            returnValue = value;
            instructionPointer = top.ReturnAddress;
        }

        // TODO: Unwrap if register
        private Value Unwrap(Value value) => value;
    }
}
