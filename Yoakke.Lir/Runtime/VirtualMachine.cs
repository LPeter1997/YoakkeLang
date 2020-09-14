using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        /// <param name="arguments">The list of argument <see cref="Value"/>s to call the procedure with.</param>
        /// <returns>The resulting <see cref="Value"/> of the call.</returns>
        public Value Execute(string name, IEnumerable<Value> arguments)
        {
            var proc = Assembly.Procedures.First(p => p.Name == name);
            return Execute(proc, arguments);
        }

        /// <summary>
        /// Executes the given procedure.
        /// </summary>
        /// <param name="proc">The procedure to call.</param>
        /// <param name="arguments">The list of argument <see cref="Value"/>s to call the procedure with.</param>
        /// <returns>The resulting <see cref="Value"/> of the call.</returns>
        public Value Execute(Proc proc, IEnumerable<Value> arguments)
        {
            Call(proc, arguments);
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
                if (proc is ISymbol sym)
                {
                    if (sym is Proc irProc)
                    {
                        var arguments = call.Arguments.Select(Unwrap);
                        Call(irProc, arguments);
                    }
                    else
                    {
                        Debug.Assert(sym is Extern);
                        var external = (Extern)sym;
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

            case Instr.Jmp jmp:
                instructionPointer = addresses[jmp.Target];
                break;

            case Instr.JmpIf jmpIf:
            {
                var cond = Unwrap(jmpIf.Condition);
                if (!(cond is Value.Int icond))
                {
                    // TODO
                    throw new NotImplementedException();
                }
                instructionPointer = addresses[icond.Value != 0 ? jmpIf.Then : jmpIf.Else];
            }
            break;

            case Instr.Alloc alloc:
            {
                var ptr = new PtrValue(alloc.Allocated);
                callStack.Peek().Registers[alloc.Result.Index] = ptr;
                ++instructionPointer;
            }
            break;

            case Instr.Store store:
            {
                // TODO: This is not very sophisticated, what about native pointers?
                var address = Unwrap(store.Target);
                var value = Unwrap(store.Value);
                if (!(address is PtrValue ptrVal))
                {
                    // TODO
                    throw new NotImplementedException();
                }
                if (ptrVal.Offset != 0)
                {
                    // TODO
                    throw new NotImplementedException();
                }
                ptrVal.Value = value;
                ++instructionPointer;
            }
            break;

            case Instr.Load load:
            {
                // TODO: This is not very sophisticated, what about native pointers?
                var address = Unwrap(load.Address);
                if (!(address is PtrValue ptrVal))
                {
                    // TODO
                    throw new NotImplementedException();
                }
                if (ptrVal.Offset != 0)
                {
                    // TODO
                    throw new NotImplementedException();
                }
                var loadedValue = ptrVal.Value;
                if (loadedValue == null)
                {
                    // TODO: Read from uninitialized memory
                    throw new InvalidOperationException();
                }
                callStack.Peek().Registers[load.Result.Index] = loadedValue;
                ++instructionPointer;
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        private void Call(Proc proc, IEnumerable<Value> arguments)
        {
            // TODO: Proper error?
            Debug.Assert(proc.Parameters.Count == arguments.Count());
            // Create the stack frame
            var newFrame = new StackFrame(instructionPointer + 1, proc.GetRegisterCount());
            // NOTE: We evaluate arguments here because we might still need the caller frame's register values!
            // Arguments
            foreach (var (reg, value) in proc.Parameters.Zip(arguments))
            {
                newFrame.Registers[reg.Index] = value;
            }
            // Push frame to call stack
            callStack.Push(newFrame);
            // Instruction pointer
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
            ISymbol sym => sym switch
            {
                Extern ext => ReadValueFromPtr(ext.Type, externals[ext]),
                _ => value,
            },
            Register reg => callStack.Peek().Registers[reg.Index],
            _ => value,
        };

        private static Value ReadValueFromPtr(Type type, IntPtr intPtr)
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
