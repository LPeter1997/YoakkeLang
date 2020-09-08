using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
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

        // Code
        private IList<Instr> code;
        private IDictionary<object, int> addresses;
        private IDictionary<Extern, IntPtr> externals;

        // Runtime
        private Stack<StackFrame> callStack = new Stack<StackFrame>();
        // TODO: Change this to void constant later
        private Value returnValue = Type.I32.NewValue(0);
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
            externals = new Dictionary<Extern, IntPtr>();
            CompileAssembly();
        }

        // We simplify our assembly representation by removing labels and simply 
        // creating a list of instructions.
        // The only hardness will be in labels then, which can be transformed into
        // a Dictionary from label to address.
        private void CompileAssembly()
        {
            LoadExternals();
            FlattenCode();
        }

        private void LoadExternals()
        {
            if (Assembly.Externals.Count == 0) return;
            // We need to compile every external binary to a DLL
            // TODO: We'd need to target what this application _is_
            // If this application is x86, we need x86, ...
            var linker = Toolchains.All().First().Linker;
            var externalBinaries = Assembly.BinaryReferences.ToList();
            foreach (var ext in externalBinaries) linker.SourceFiles.Add(ext);
            linker.OutputKind = OutputKind.DynamicLibrary;
            // Export external symbols
            foreach (var sym in Assembly.Externals) linker.Exports.Add(sym);
            // NOTE: Don't we need to delete this when the VM dies?
            var linkedBinariesPath = Path.GetTempFileName();
            if (linker.Link(linkedBinariesPath) != 0)
            {
                // TODO
                throw new NotImplementedException();
            }
            // Collect externals
            // NOTE: Don't we need to free this when the VM dies?
            var linkedBinaries = NativeLibrary.Load(linkedBinariesPath);
            externals.Clear();
            foreach (var ext in Assembly.Externals)
            {
                externals[ext] = NativeLibrary.GetExport(linkedBinaries, $"{ext.Name}");
            }
        }

        private void FlattenCode()
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

            case Instr.Call call:
            {
                var proc = Unwrap(call.Procedure);
                if (proc is Value.Symbol sym)
                {
                    if (sym.Value is Proc irProc)
                    {
                        // TODO: Assign arguments?
                        Call(irProc);
                    }
                    else
                    {
                        Debug.Assert(sym.Value is Extern);
                        var external = (Extern)sym.Value;
                        // TODO
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    // TODO: Like.. function pointers and stuff
                    throw new NotImplementedException();
                }
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        // TODO: Arguments?
        private void Call(Proc proc)
        {
            callStack.Push(new StackFrame(instructionPointer + 1, proc.GetRegisterCount()));
            var address = addresses[proc];
            instructionPointer = address;
        }

        private void Return(Value value)
        {
            var top = callStack.Pop();
            instructionPointer = top.ReturnAddress;
            // Return value
            if (callStack.Count > 0)
            {
                // Assign return value, if the call stack still contains elements
                var callIns = (Instr.Call)code[instructionPointer - 1];
                callStack.Peek().Registers[callIns.Result.Index] = value;
            }
            else
            {
                // Store it for the outside
                returnValue = value;
            }
        }

        // TODO: Differentiate lvalues and rvalues?
        private Value Unwrap(Value value) => value switch
        {
            Value.Symbol sym => sym.Value switch
            {
                Extern ext => ReadValueFromPtr(ext.Type, externals[ext]),
                _ => value,
            },
            Value.Register reg => callStack.Peek().Registers[reg.Value.Index],
            _ => value,
        };

        private Value ReadValueFromPtr(Type type, IntPtr intPtr)
        {
            unsafe
            {
                switch (type)
                {
                case Type.Int i:
                    if (i.Signed && i.Bits == 32)
                    {
                        Int32 val = *(Int32*)intPtr.ToPointer();
                        return Type.I32.NewValue(val);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                default: throw new NotImplementedException();
                }
            }
        }
    }
}
