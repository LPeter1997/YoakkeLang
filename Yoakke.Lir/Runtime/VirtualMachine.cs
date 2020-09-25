using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Yoakke.DataStructures;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Types;
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
        private IDictionary<Global, PtrValue> globalsToPointers;

        // Runtime
        private Stack<StackFrame> callStack = new Stack<StackFrame>();
        private List<byte[]> memorySegments = new List<byte[]>();
        private List<byte[]> globalMemorySegments = new List<byte[]>();
        private Value returnValue = Value.Void_;
        private int instructionPointer;
        private SizeContext sizeContext = new SizeContext
        {
            PointerSize = 8,
        };

        private StackFrame StackFrame => callStack.Peek();

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
            globalsToPointers = new Dictionary<Global, PtrValue>();
            CompileAssembly();
        }

        // We simplify our assembly representation by removing labels and simply 
        // creating a list of instructions.
        // The only hardness will be in labels then, which can be transformed into
        // a Dictionary from label to address.
        private void CompileAssembly()
        {
            // TODO: Finish procedure and call it
            //LoadExternals();
            AllocateGlobals();
            FlattenCode();
        }

        private void LoadExternals()
        {
            // TODO: Implement this
            if (Assembly.Externals.Count == 0) return;
#if false
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
#endif
        }

        private void AllocateGlobals()
        {
            // NOTE: We add a 0th index global that'll never be referred to.
            // This is so we can use negatives to annotate globals
            globalMemorySegments.Add(new byte[0]);
            foreach (var global in Assembly.Globals)
            {
                var buffer = new byte[SizeOf(global.UnderlyingType)];
                var segmentIndex = globalMemorySegments.Count;
                var ptr = new PtrValue(-segmentIndex, global.UnderlyingType);
                globalsToPointers.Add(global, ptr);
                globalMemorySegments.Add(buffer);
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
                var called = Unwrap(call.Procedure);
                if (called is Proc proc)
                {
                    var arguments = call.Arguments.Select(Unwrap);
                    Call(proc, arguments);
                }
                else if (called is Extern external)
                {
                    // TODO: Check type, call
                    throw new NotImplementedException();
                }
                else
                {
                    // TODO: Check if function type, is function pointer...
                    throw new NotImplementedException();
                }
            }
            break;

            case Instr.Jmp jmp:
                instructionPointer = addresses[jmp.Target];
                break;

            case Instr.JmpIf jmpIf:
            {
                var condValue = (Value.Int)Unwrap(jmpIf.Condition);
                bool condition = !condValue.Value.IsZero;
                instructionPointer = addresses[condition ? jmpIf.Then : jmpIf.Else];
            }
            break;

            case Instr.Store store:
            {
                var address = Unwrap(store.Target);
                var value = Unwrap(store.Value);

                if (address is Global globalVal)
                {
                    // We just make it a PtrVal!
                    address = globalsToPointers[globalVal];
                }

                if (address is PtrValue ptrVal)
                {
                    WriteManagedPtr(ptrVal, value.Clone());
                }
                else
                {
                    // TODO: Native pointers?
                    throw new NotImplementedException();
                }
                ++instructionPointer;
            }
            break;

            case ValueInstr vinstr:
            {
                StackFrame[vinstr.Result] = Evaluate(vinstr);
                ++instructionPointer;
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        private Value Evaluate(ValueInstr instr)
        {
            switch (instr)
            {
            case Instr.Alloc alloc:
            {
                return Allocate(alloc.Allocated);
            }

            case Instr.Load load:
            {
                var address = Unwrap(load.Source);
                if (address is Global globalVal)
                {
                    // We just make it a pointer!
                    address = globalsToPointers[globalVal];
                }
                if (address is PtrValue ptrVal)
                {
                    return ReadManagedPtr(ptrVal);
                }
                // TODO: Native pointers?
                throw new NotImplementedException();
            }

            case Instr.Cmp cmp:
            {
                var left = Unwrap(cmp.Left);
                var right = Unwrap(cmp.Right);
                bool boolResult;
                if (cmp.Comparison == Comparison.eq)
                {
                    boolResult = left.Equals(right);
                }
                else if (cmp.Comparison == Comparison.ne)
                {
                    boolResult = !left.Equals(right);
                }
                else if (left is Value.Int leftInt && right is Value.Int rightInt)
                {
                    boolResult = cmp.Comparison switch
                    {
                        Comparison.Gr => leftInt.Value > rightInt.Value,
                        Comparison.Le => leftInt.Value < rightInt.Value,
                        Comparison.GrEq => leftInt.Value >= rightInt.Value,
                        Comparison.LeEq => leftInt.Value <= rightInt.Value,
                        _ => throw new InvalidOperationException(),
                    };
                }
                else
                {
                    throw new InvalidOperationException();
                }
                return Type.I32.NewValue(new BigInt(true, 32, boolResult ? 1 : 0));
            }

            case ArithInstr arith:
            {
                var left = Unwrap(arith.Left);
                var right = Unwrap(arith.Right);
                if (left is Value.Int leftInt && right is Value.Int rightInt)
                {
                    var intResult = arith switch
                    {
                        Instr.Add => leftInt.Value + rightInt.Value,
                        Instr.Sub => leftInt.Value - rightInt.Value,
                        Instr.Mul => leftInt.Value * rightInt.Value,
                        Instr.Div => leftInt.Value / rightInt.Value,
                        Instr.Mod => leftInt.Value % rightInt.Value,
                        _ => throw new InvalidOperationException(),
                    };
                    var resultType = (Type.Int)arith.Result.Type;
                    return new Value.Int(resultType, intResult);
                }
                if (left is PtrValue leftPtr && right is Value.Int rightInt2)
                {
                    var typeSize = SizeOf(((Type.Ptr)leftPtr.Type).Subtype);
                    var offset = arith switch
                    {
                        Instr.Add => typeSize * (int)rightInt2.Value,
                        Instr.Sub => -typeSize * (int)rightInt2.Value,
                        _ => throw new InvalidOperationException(),
                    };
                    return leftPtr.OffsetBy(offset, leftPtr.BaseType);
                }
                throw new InvalidOperationException();
            }

            case BitwiseInstr bitwise:
            {
                var left = Unwrap(bitwise.Left);
                var right = Unwrap(bitwise.Right);
                if (left is Value.Int leftInt && right is Value.Int rightInt)
                {
                    var intResult = bitwise switch
                    {
                        Instr.BitAnd => leftInt.Value & rightInt.Value,
                        Instr.BitOr => leftInt.Value | rightInt.Value,
                        Instr.BitXor => leftInt.Value ^ rightInt.Value,
                        _ => throw new InvalidOperationException(),
                    };
                    var resultType = (Type.Int)bitwise.Result.Type;
                    return new Value.Int(resultType, intResult);
                }
                throw new InvalidOperationException();
            }

            case BitShiftInstr bitshift:
            {
                var left = Unwrap(bitshift.Shifted);
                var right = Unwrap(bitshift.Amount);
                if (left is Value.Int leftInt && right is Value.Int rightInt)
                {
                    var intResult = bitshift switch
                    {
                        Instr.Shl => leftInt.Value << (int)rightInt.Value,
                        Instr.Shr => leftInt.Value >> (int)rightInt.Value,
                        _ => throw new InvalidOperationException(),
                    };
                    var resultType = (Type.Int)bitshift.Result.Type;
                    return new Value.Int(resultType, intResult);
                }
                throw new InvalidOperationException();
            }

            case Instr.ElementPtr elementPtr:
            {
                var value = Unwrap(elementPtr.Value);
                var index = elementPtr.Index.Value;
                var structTy = (Type.Struct)((Type.Ptr)value.Type).Subtype;
                var offset = sizeContext.OffsetOf(structTy.Definition, index);
                if (value is PtrValue managedPtr)
                {
                    var resultType = structTy.Definition.Fields[index];
                    return managedPtr.OffsetBy(offset, resultType);
                }
                // TODO: Native ptr
                throw new NotImplementedException();
            }

            case Instr.Cast cast:
            {
                var value = Unwrap(cast.Value);
                if (cast.Target is Type.Ptr toType && value.Type is Type.Ptr)
                {
                    if (value is PtrValue managedPtr)
                    {
                        return managedPtr.OffsetBy(0, toType.Subtype);
                    }
                    // TODO: Native ptr
                    throw new NotImplementedException();
                }
                throw new InvalidOperationException();
            }

            default: throw new NotImplementedException();
            }
        }

        private void Call(Proc proc, IEnumerable<Value> arguments)
        {
            if (proc.Parameters.Count != arguments.Count())
            {
                throw new ArgumentOutOfRangeException($"Parameter count mismatch when calling procedure '{proc.Name}'!");
            }
            // Create the stack frame
            var newFrame = new StackFrame(
                returnAddress: instructionPointer + 1,
                registerCount: proc.GetRegisterCount(), 
                allocationIndex: memorySegments.Count);
            // NOTE: We evaluate arguments here because we might still need the caller frame's register values!
            // Arguments
            foreach (var (reg, value) in proc.Parameters.Zip(arguments))
            {
                newFrame[reg] = value;
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
            // Free memory
            memorySegments.RemoveRange(top.AllocationIndex, memorySegments.Count - top.AllocationIndex);
            instructionPointer = top.ReturnAddress;
            // Return value
            if (callStack.Count > 0)
            {
                // Assign return value, if the call stack still contains elements
                var callIns = (Instr.Call)code[instructionPointer - 1];
                StackFrame[callIns.Result] = value;
            }
            else
            {
                // Store it for the outside
                returnValue = value;
            }
        }

        private Value Unwrap(Value value) => value switch
        {
            ISymbol sym => sym switch
            {
                Extern ext => ReadNativePtr(ext.Type, externals[ext]),
                _ => value,
            },
            Register reg => StackFrame[reg],
            _ => value,
        };

        private PtrValue Allocate(Type type)
        {
            var buffer = new byte[SizeOf(type)];
            var segmentIndex = memorySegments.Count;
            memorySegments.Add(buffer);
            return new PtrValue(segmentIndex, type);
        }

        private int SizeOf(Value value) => SizeOf(value.Type);
        private int SizeOf(Type type) => sizeContext.SizeOf(type);

        private Value ReadManagedPtr(PtrValue value)
        {
            var typeToRead = value.BaseType;
            byte[] memory;
            if (value.Segment >= 0) memory = memorySegments[value.Segment];
            else memory = globalMemorySegments[-value.Segment];
            var segment = memory.AsSpan().Slice(value.Offset);
            return ReadFromManagedMemory(ref segment, typeToRead);
        }

        private Value ReadFromManagedMemory(ref Span<byte> bytes, Type typeToRead)
        {
            switch (typeToRead)
            {
            case Type.Int i:
            {
                var size = SizeOf(i);
                var value = new BigInt(i.Signed, i.Bits, bytes.Slice(0, size));
                bytes = bytes.Slice(size);
                return new Value.Int(i, value);
            }

            case Type.Ptr p:
            {
                // TODO: We can't differentiate between native and managed pointers!
                // This could very well be some native pointer
                var segment = BitConverter.ToInt32(bytes.Slice(0, 4));
                var offset = BitConverter.ToInt32(bytes.Slice(4, 4));
                bytes = bytes.Slice(8);
                return new PtrValue(segment, p.Subtype) { Offset = offset };
            }

            default: throw new NotImplementedException();
            }
        }

        private void WriteManagedPtr(PtrValue ptr, Value value)
        {
            byte[] memory;
            if (ptr.Segment >= 0) memory = memorySegments[ptr.Segment];
            else memory = globalMemorySegments[-ptr.Segment];
            var segment = memory.AsSpan().Slice(ptr.Offset);
            WriteToManagedMemory(ref segment, value);
        }

        private void WriteToManagedMemory(ref Span<byte> bytes, Value value)
        {
            switch (value)
            {
            case Value.Int i:
            {
                i.Value.TryWriteBytes(bytes, out var written);
                bytes = bytes.Slice(written);
            }
            break;

            case PtrValue p:
            {
                var segment = BitConverter.GetBytes(p.Segment);
                var offset = BitConverter.GetBytes(p.Offset);
                segment.CopyTo(bytes);
                offset.CopyTo(bytes.Slice(4));
                bytes = bytes.Slice(8);
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        private static Value ReadNativePtr(Type type, IntPtr intPtr)
        {
            // TODO: Proper implementation
            unsafe
            {
                switch (type)
                {
                case Type.Int i:
                    if (i.Signed && i.Bits == 32)
                    {
                        Int32 val = *(Int32*)intPtr.ToPointer();
                        return Type.I32.NewValue(new BigInt(true, 32, val));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                default: throw new NotImplementedException();
                }
            }
        }

        private static void WriteNativePtr(IntPtr intPtr, Value value)
        {
            // TODO: Proper implementation
            throw new NotImplementedException();
        }
    }
}
