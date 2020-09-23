using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    internal enum ReturnMethod
    {
        None,
        Eax,
        EaxEdx,
        Address,
    }

    /// <summary>
    /// For assembling for X86.
    /// </summary>
    public class X86Assembler
    {
        private X86Assembly result = new X86Assembly();
        private X86Proc? currentProcedure;
        private X86BasicBlock? currentBasicBlock;
        private SizeContext sizeContext = new SizeContext { PointerSize = 4, };

        private string? nextComment = null;
        private IDictionary<BasicBlock, X86BasicBlock> basicBlocks = new Dictionary<BasicBlock, X86BasicBlock>();
        private IDictionary<Proc, X86Proc> procs = new Dictionary<Proc, X86Proc>();
        private IDictionary<Lir.Register, int> registerOffsets = new Dictionary<Lir.Register, int>();
        private RegisterPool registerPool = new RegisterPool();

        /// <summary>
        /// Compiles an <see cref="X86Assembly"/> from a <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to compile.</param>
        /// <returns>The compiled <see cref="X86Assembly"/>.</returns>
        public static X86Assembly Assemble(Assembly assembly)
        {
            return new X86Assembler().Compile(assembly);
        }

        private X86Assembly Compile(Assembly assembly)
        {
            // First we forward declare procedures and basic blocks
            ForwardDeclare(assembly);
            // TODO: Externals? Globals?
            // Compile procedures
            foreach (var proc in assembly.Procedures) CompileProc(proc);
            return result;
        }

        private void ForwardDeclare(Assembly assembly)
        {
            // Forward declare procedures
            foreach (var proc in assembly.Procedures)
            {
                var x86proc = new X86Proc(GetSymbolName(proc));
                procs[proc] = x86proc;
                // Forward declare each basic block inside
                foreach (var bb in proc.BasicBlocks)
                {
                    basicBlocks[bb] = new X86BasicBlock(x86proc, bb.Name);
                }
            }
        }

        private void CompileProc(Proc proc)
        {
            currentProcedure = procs[proc];
            currentProcedure.Visibility = proc.Visibility;
            result.Procedures.Add(currentProcedure);

            // We need to allocate enough stack space for the locals
            registerOffsets.Clear();
            var registers = proc.BasicBlocks
                .SelectMany(bb => bb.Instructions)
                .Where(ins => ins is ValueInstr)
                // NOTE: Cast returned nullable for some reason
                .Select(ins => (ValueInstr)ins)
                .Select(ins => ins.Result);
            // TODO: This is probably only true for Cdecl?
            // Start from 4 because of the top one being the return address
            int offset = sizeContext.PointerSize;
            // Get how we return from here
            var returnMethod = GetReturnMethod(proc.Return);
            if (returnMethod == ReturnMethod.Address)
            {
                // We return by copying to a return address
                // The return address is on the top of the stack
                offset += sizeContext.PointerSize;
            }
            // Collect parameter offsets, they are the other way in reverse order
            foreach (var r in proc.Parameters)
            {
                offset += SizeOf(r);
                registerOffsets[r] = offset;
            }
            // Let's collect each local offset relative to EBP
            offset = 0;
            foreach (var r in registers)
            {
                offset -= SizeOf(r);
                registerOffsets[r] = offset;
            }

            // We need to write the prologue in an initial basic block
            currentBasicBlock = new X86BasicBlock(currentProcedure);
            currentProcedure.BasicBlocks.Add(currentBasicBlock);
            WriteProcPrologue(proc);
            // Calculate space for locals
            var allocSize = registers.Sum(SizeOf);
            // Allocate space for the locals
            WriteInstr(X86Op.Sub, Register.esp, new Operand.Literal(DataWidth.dword, allocSize));

            // Now just compile all basic blocks
            foreach (var bb in proc.BasicBlocks) CompileBasicBlock(proc, bb);
        }

        private void CompileBasicBlock(Proc proc, BasicBlock basicBlock)
        {
            Debug.Assert(currentProcedure != null);
            currentBasicBlock = basicBlocks[basicBlock];
            currentProcedure.BasicBlocks.Add(currentBasicBlock);

            // Just compile each instruction
            foreach (var ins in basicBlock.Instructions) CompileInstr(proc, ins);
        }

        private void CompileInstr(Proc proc, Instr instr)
        {
            registerPool.FreeAll();
            CommentInstr(instr);
            switch (instr)
            {
            case Instr.Call call:
            {
                if (!(call.Procedure.Type is Type.Proc procTy))
                {
                    throw new InvalidOperationException();
                }
                if (procTy.CallConv == CallConv.Cdecl)
                {
                    // Push arguments backwards, track the offset of esp
                    var espOffset = 0;
                    int i = 0;
                    foreach (var arg in call.Arguments.Reverse())
                    {
                        var argValue = CompileValue(arg);
                        CommentInstr($"argument {i}");
                        WritePush(argValue);
                        espOffset += SizeOf(arg);
                        registerPool.FreeAll();
                        ++i;
                    }
                    // If the return method is by address, we pass the address
                    var returnMethod = GetReturnMethod(procTy.Return);
                    if (returnMethod == ReturnMethod.Address)
                    {
                        var resultAddr = CompileToAddress(call.Result);
                        var resultAddrReg = registerPool.Allocate(DataWidth.dword);
                        CommentInstr("return target address");
                        WriteInstr(X86Op.Lea, resultAddrReg, resultAddr);
                        WriteInstr(X86Op.Push, resultAddrReg);
                        espOffset += DataWidth.dword.Size;
                        registerPool.FreeAll();
                    }
                    // Do the call
                    var procedure = CompileSingleValue(call.Procedure);
                    WriteInstr(X86Op.Call, procedure);
                    // Restore stack
                    WriteInstr(X86Op.Add, Register.esp, new Operand.Literal(DataWidth.dword, espOffset));
                    // Writing the result back
                    if (returnMethod == ReturnMethod.None || returnMethod == ReturnMethod.Address)
                    {
                        // No-op
                        // For size == 0 there's nothing to copy, for size > 4 the value is already copied
                    }
                    else
                    {
                        CommentInstr("copy return value");
                        // Just store what's written in eax or eax:edx
                        var resultSize = SizeOf(proc.Return);
                        var result = CompileValue(call.Result);
                        if (result.Length == 1)
                        {
                            var eax = Register.AtSlot(0, DataWidth.GetFromSize(resultSize));
                            WriteInstr(X86Op.Mov, result[0], eax);
                        }
                        else
                        {
                            Debug.Assert(result.Length == 2);
                            var eax = Register.AtSlot(0, DataWidth.dword);
                            var edx = Register.AtSlot(2, DataWidth.GetFromSize(resultSize - 4));
                            WriteInstr(X86Op.Mov, result[0], eax);
                            WriteInstr(X86Op.Mov, result[1], edx);
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            break;

            case Instr.Ret ret:
            {
                // Writing the return value
                if (proc.CallConv == CallConv.Cdecl)
                {
                    var returnMethod = GetReturnMethod(ret.Value.Type);
                    if (returnMethod == ReturnMethod.None)
                    {
                        // No-op
                    }
                    else if (returnMethod == ReturnMethod.Eax || returnMethod == ReturnMethod.EaxEdx)
                    {
                        // We write in eax or the eax:edx pair
                        // TODO: Floats are returned in a different register!
                        // NOTE: Might be unnecessary allocation for edx
                        registerPool.Allocate(Register.eax, Register.edx);
                        var retValue = CompileValue(ret.Value);
                        if (retValue.Length == 1)
                        {
                            WriteInstr(X86Op.Mov, Register.eax, retValue[0]);
                        }
                        else
                        {
                            Debug.Assert(retValue.Length == 2);
                            WriteInstr(X86Op.Mov, Register.eax, retValue[0]);
                            WriteInstr(X86Op.Mov, Register.edx, retValue[1]);
                        }
                    }
                    else
                    {
                        // We receive a return address
                        // First we load that return address
                        var retAddrAddr = new Operand.Address(Register.ebp, 8);
                        var retAddr = registerPool.Allocate(DataWidth.dword);
                        CommentInstr("copy return value");
                        WriteInstr(X86Op.Mov, retAddr, retAddrAddr);
                        // Compile the value
                        var retValue = CompileValue(ret.Value);
                        // Copy
                        WriteCopy(retAddr, retValue);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
                // Write the epilogue, return
                WriteProcEpilogue(proc);
                CommentInstr(instr);
                WriteInstr(X86Op.Ret);
            }
            break;

            case Instr.Jmp jmp:
                WriteInstr(X86Op.Jmp, new Operand.Label(basicBlocks[jmp.Target]));
                break;

            case Instr.JmpIf jmpIf:
            {
                var condParts = CompileValue(jmpIf.Condition);
                Debug.Assert(condParts.Length > 0);
                // Initial condition is the first bytes
                var condHolder = registerPool.Allocate(condParts[0].GetWidth(sizeContext));
                WriteInstr(X86Op.Mov, condHolder, condParts[0]);
                // Remaining are or-ed to the result
                foreach (var item in condParts.Skip(1))
                {
                    WriteInstr(X86Op.Or, condHolder, item);
                }
                WriteInstr(X86Op.Test, condHolder, condHolder);
                WriteInstr(X86Op.Jne, new Operand.Label(basicBlocks[jmpIf.Then]));
                WriteInstr(X86Op.Jmp, new Operand.Label(basicBlocks[jmpIf.Else]));
            }
            break;

#if false
            case Instr.Store store:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var target = (Register)CompileValue(store.Target);
                var addr = new Operand.Address(target);
                var indirect = new Operand.Indirect(target.Width, addr);
                var source = CompileValue(store.Value);
                WriteInstr(X86Op.Mov, indirect, source);
            }
            break;

            case Instr.Load load:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var target = CompileValue(load.Result, true);
                var source = (Register)CompileValue(load.Address);
                var addr = new Operand.Address(source);
                var indirect = new Operand.Indirect(source.Width, addr);
                var immediate = registerPool.Allocate(DataWidth.dword);
                WriteInstr(X86Op.Mov, immediate, indirect);
                WriteInstr(X86Op.Mov, target, immediate);
            }
            break;

            case Instr.Alloc alloc:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var size = SizeOf(alloc.Allocated);
                WriteInstr(X86Op.Sub, Register.esp, size);
                var result = CompileValue(alloc.Result, true);
                WriteInstr(X86Op.Mov, result, Register.esp);
            }
            break;

            case Instr.Cmp cmp:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var target = CompileValue(cmp.Result, true);
                var left = CompileValue(cmp.Left);
                var right = CompileValue(cmp.Right);
                CleanseImmediates(ref left, ref right);
                // First we produce the operands and the flags
                WriteInstr(X86Op.Cmp, left, right);
                // Based on the comparison we need an x86 operation
                var op = cmp.Comparison switch
                {
                    Comparison.Eq => X86Op.Je,
                    Comparison.Ne => X86Op.Jne,
                    Comparison.Gr => X86Op.Jg,
                    Comparison.Le => X86Op.Jl,
                    Comparison.GrEq => X86Op.Jge,
                    Comparison.LeEq => X86Op.Jle,
                    _ => throw new NotImplementedException(),
                };
                // We need to branch to write the result
                var labelNameBase = GetUniqueName("WriteCmpResult");
                Debug.Assert(currentProcedure != null);
                var trueBB = new X86BasicBlock(currentProcedure, $"{labelNameBase}_T");
                var falseBB = new X86BasicBlock(currentProcedure, $"{labelNameBase}_F");
                var continueBB = new X86BasicBlock(currentProcedure, $"{labelNameBase}_C");
                // Do the branch to the true or false block
                WriteInstr(op, trueBB);
                WriteInstr(X86Op.Jmp, falseBB);
                // On true block, we write the truthy value then jump to the continuation
                currentBasicBlock = trueBB;
                WriteInstr(X86Op.Mov, target, 1);
                WriteInstr(X86Op.Jmp, continueBB);
                // On false block, we write the falsy value then jump to the continuation
                currentBasicBlock = falseBB;
                WriteInstr(X86Op.Mov, target, 0);
                WriteInstr(X86Op.Jmp, continueBB);
                // We continue writing on the continuation
                currentBasicBlock = continueBB;
                // Add all these basic blocks to the vurrent procedure
                Debug.Assert(currentProcedure != null);
                currentProcedure.BasicBlocks.Add(trueBB);
                currentProcedure.BasicBlocks.Add(falseBB);
                currentProcedure.BasicBlocks.Add(continueBB);
            }
            break;

            case Instr.Add:
            case Instr.Sub:
            {
                var arith = (ArithInstr)instr;
                // TODO: What if the operands don't fit in 32 bits?
                var target = CompileValue(arith.Result, true);
                var left = CompileValue(arith.Left);
                var right = CompileValue(arith.Right);
                // NOTE: We special-case pointer-arithmetic
                // TODO: Duplication
                if (arith.Left.Type is Type.Ptr leftPtr)
                {
                    ToRegister(ref right);
                    WriteInstr(X86Op.Mov, target, left);
                    WriteInstr(X86Op.Imul, right, SizeOf(leftPtr.Subtype));
                    WriteInstr(arith is Instr.Add ? X86Op.Add : X86Op.Sub, target, right);
                }
                else if (arith.Right.Type is Type.Ptr rightPtr)
                {
                    ToRegister(ref right);
                    WriteInstr(X86Op.Mov, target, left);
                    WriteInstr(X86Op.Imul, right, SizeOf(rightPtr.Subtype));
                    WriteInstr(arith is Instr.Add ? X86Op.Add : X86Op.Sub, target, right);
                }
                else
                {
                    WriteInstr(X86Op.Mov, target, left);
                    WriteInstr(arith is Instr.Add ? X86Op.Add : X86Op.Sub, target, right);
                }
            }
            break;

            case Instr.Mul mul:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var target = CompileValue(mul.Result, true);
                var left = CompileValue(mul.Left);
                var right = CompileValue(mul.Right);
                ToRegister(ref left);
                // TODO: Signed vs. unsigned?
                WriteInstr(X86Op.Imul, left, right);
                WriteInstr(X86Op.Mov, target, left);
            }
            break;

            case Instr.Div:
            case Instr.Mod:
            {
                var arith = (ArithInstr)instr;
                // NOTE: This is different!
                // TODO: What if the operands don't fit in 32 bits?
                registerPool.Allocate(Register.eax, Register.edx);
                var target = CompileValue(arith.Result, true);
                var left = CompileValue(arith.Left);
                var right = CompileValue(arith.Right);
                ToNonImmediate(ref right);
                WriteInstr(X86Op.Mov, Register.edx, 0);
                WriteInstr(X86Op.Mov, Register.eax, left);
                // TODO: Signed vs. unsigned?
                WriteInstr(X86Op.Idiv, right);
                WriteInstr(X86Op.Mov, target, arith is Instr.Div ? Register.eax : Register.edx);
            }
            break;

            // TODO: Duplications again....

            case Instr.BitAnd:
            case Instr.BitOr:
            case Instr.BitXor:
            {
                var bitw = (BitwiseInstr)instr;
                // TODO: What if the operands don't fit in 32 bits?
                var target = CompileValue(bitw.Result, true);
                var left = CompileValue(bitw.Left);
                var right = CompileValue(bitw.Right);
                WriteInstr(X86Op.Mov, target, left);
                var op = bitw switch
                {
                    Instr.BitAnd => X86Op.And,
                    Instr.BitOr => X86Op.Or,
                    Instr.BitXor => X86Op.Xor,
                    _ => throw new NotImplementedException(),
                };
                WriteInstr(op, target, right);
            }
            break;

            case Instr.Shl:
            case Instr.Shr:
            {
                var bitsh = (BitShiftInstr)instr;
                var target = CompileValue(bitsh.Result, true);
                var left = CompileValue(bitsh.Shifted);
                var right = CompileValue(bitsh.Amount);
                WriteInstr(X86Op.Mov, target, left);
                WriteInstr(bitsh is Instr.Shl ? X86Op.Shl : X86Op.Shr, target, right);
            }
            break;

            case Instr.ElementPtr elementPtr:
            {
                var target = CompileValue(elementPtr.Result, true);
                var value = CompileValue(elementPtr.Value);
                if (elementPtr.Value.Type is Type.Ptr ptrTy && ptrTy.Subtype is Type.Struct structTy)
                {
                    var index = elementPtr.Index.Value;
                    // Get offset, add it to the base address
                    var offset = OffsetOf(structTy.Definition, index);
                    WriteInstr(X86Op.Add, value, offset);
                    WriteInstr(X86Op.Mov, target, value);
                }
                else
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }
            break;

            case Instr.Cast cast:
            {
                // TODO: Arg sizes and such?
                if (cast.Target is Type.Ptr && cast.Value.Type is Type.Ptr)
                {
                    // Should be a no-op, simply copy
                    var target = CompileValue(cast.Result, true);
                    var src = CompileValue(cast.Value);
                    WriteInstr(X86Op.Mov, target, src);
                }
                else
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }
            break;
#endif

            default: throw new NotImplementedException();
            }
        }

        private void WriteCopy(Register targetBaseAddr, Operand[] values)
        {
            int offset = 0;
            foreach (var value in values)
            {
                var valueWidth = value.GetWidth(sizeContext);
                // Construct the current target address
                var targetAddr = new Operand.Address(targetBaseAddr, offset);
                var target = new Operand.Indirect(valueWidth, targetAddr);
                // Do the copy
                WriteMov(target, value);
                // Offset to the next target address
                offset += valueWidth.Size;
            }
        }

        private void WriteMov(Operand target, Operand source)
        {
            WriteInstr(X86Op.Mov, target, source);
        }

        private void WritePush(Operand[] ops)
        {
            foreach (var op in ops) WriteInstr(X86Op.Push, op);
        }

        private Operand CompileToAddress(Value value)
        {
            var res = CompileSingleValue(value, true);
            Debug.Assert(res is Operand.Address);
            return res;
        }

        private Operand CompileSingleValue(Value value, bool asLvalue = false)
        {
            var res = CompileValue(value, asLvalue);
            Debug.Assert(res.Length == 1);
            return res[0];
        }

        private Operand[] CompileValue(Value value, bool asLvalue = false)
        {
            Operand Lit(DataWidth dw, object obj) => new Operand.Literal(dw, obj);
            Operand[] Ops(params Operand[] ops) => ops;

            switch (value)
            {
            case Value.Int i:
            {
                if (asLvalue) throw new InvalidOperationException("An integer can't be an lvalue!");

                int byteCount = SizeOf(i);
                // This is less or equal to byteCount
                var origBytes = i.Value.ToByteArray();
                // We pad it with 0s for simplicity
                var bytes = origBytes.Concat(Enumerable.Repeat((byte)0, byteCount - origBytes.Length));
                // Now we collect the resulting operands
                var result = SplitData(byteCount, dataSize =>
                {
                    var bs = bytes.Take(dataSize).ToArray();
                    bytes = bytes.Skip(dataSize);
                    return Lit(DataWidth.GetFromSize(dataSize), BitConverter.ToInt32(bs));
                });
                return result.ToArray();
            }

            case ISymbol sym:
            {
                if (asLvalue) throw new InvalidOperationException("A symbol can't be an lvalue!");
                var symName = GetSymbolName(sym);
                return Ops(Lit(DataWidth.dword, symName));
            }

            case Lir.Register reg:
            {
                var regSize = SizeOf(reg);
                if (regSize == 0) return Ops();

                var initialOffset = registerOffsets[reg];
                // For lvalues, it's enough to just know the address, returning multiple values is not required
                if (asLvalue)
                {
                    var addr = new Operand.Address(Register.ebp, initialOffset);
                    return Ops(addr);
                }
                // For non-lvalues we might need to return multiple chunks of indirections
                int offset = 0;
                var result = SplitData(regSize, dataSize =>
                {
                    var width = DataWidth.GetFromSize(dataSize);
                    var newAddr = new Operand.Address(Register.ebp, initialOffset + offset);
                    offset += dataSize;
                    return new Operand.Indirect(width, newAddr);
                });
                return Ops(result.ToArray());
            }

            default: throw new NotImplementedException();
            }
        }

        private static IEnumerable<T> SplitData<T>(int byteCount, Func<int, T> func)
        {
            while (byteCount > 0)
            {
                // This basically just splits the number into 4, 2 and 1 byte chunks
                for (int dataSize = 4; ; dataSize /= 2)
                {
                    if (byteCount < dataSize) continue;
                    yield return func(dataSize);
                    byteCount -= dataSize;
                    break;
                }
            }
        }

        private void WriteProcPrologue(Proc _)
        {
            CommentInstr("prologue");
            WriteInstr(X86Op.Push, Register.ebp);
            WriteInstr(X86Op.Mov, Register.ebp, Register.esp);
        }

        private void WriteProcEpilogue(Proc _)
        {
            CommentInstr("epilogue");
            WriteInstr(X86Op.Mov, Register.esp, Register.ebp);
            WriteInstr(X86Op.Pop, Register.ebp);
        }

        private void CommentInstr(object comment) => nextComment = comment.ToString();

        private void WriteInstr(X86Op op, params Operand[] operands)
        {
            Debug.Assert(currentBasicBlock != null);
            var instr = new X86Instr(op, operands);
            if (nextComment != null)
            {
                instr.Comment = nextComment;
                nextComment = null;
            }
            currentBasicBlock.Instructions.Add(instr);
        }

        // TODO: Not the best solution...
        private int nameCnt = 0;
        private string GetUniqueName(string name) => $"{name}_{nameCnt++}";

        private static string GetSymbolName(ISymbol symbol) =>
               // For Cdecl procedures we assume an underscore prefix
               (symbol is Proc proc && proc.CallConv == CallConv.Cdecl)
            // For non-procedures too
            || !(symbol is Proc)
            ? $"_{symbol.Name}" : symbol.Name;

        private int OffsetOf(StructDef structDef, int fieldNo) => sizeContext.OffsetOf(structDef, fieldNo);
        private int SizeOf(Value value) => SizeOf(value.Type);
        private int SizeOf(Type type) => sizeContext.SizeOf(type);

        private ReturnMethod GetReturnMethod(Type returnType)
        {
            var resultSize = SizeOf(returnType);
            if (resultSize == 0) return ReturnMethod.None;
            if (resultSize <= 4) return ReturnMethod.Eax;
            // The size is > 4
            if (resultSize <= 8 && IsPrimitive(returnType)) return ReturnMethod.EaxEdx;
            return ReturnMethod.Address;
        }

        private static bool IsPrimitive(Type type) => type is Type.Int;
    }
}
