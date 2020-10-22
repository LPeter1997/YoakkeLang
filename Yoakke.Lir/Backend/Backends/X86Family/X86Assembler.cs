using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private Proc? entryPoint;
        private Proc? prelude;
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
            entryPoint = assembly.EntryPoint;
            prelude = assembly.Prelude;
            // First we forward declare procedures and basic blocks
            ForwardDeclare(assembly);
            // Compile externals
            foreach (var ext in assembly.Externals) CompileExternal(ext);
            // Compile constants
            foreach (var c in assembly.Constants) CompileConstant(c);
            // Compile globals
            foreach (var glob in assembly.Globals) CompileGlobal(glob);
            // Compile procedures
            foreach (var proc in assembly.Procedures) CompileProc(proc);
            return result;
        }

        private void CompileExternal(Extern external)
        {
            var symName = GetSymbolName(external);
            result.Externals.Add(symName);
        }

        private void CompileConstant(Const constant)
        {
            var symName = GetSymbolName(constant);
            var bytes = ToByteArray(constant.Value);
            result.Constants.Add(new X86Const(symName, bytes));
        }

        private void CompileGlobal(Global global)
        {
            var symName = GetSymbolName(global);
            result.Globals.Add(new X86Global(symName, SizeOf(global.UnderlyingType)));
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
                        var resultSize = SizeOf(procTy.Return);
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
                WriteInstr(X86Op.Jmp, basicBlocks[jmp.Target]);
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
                WriteInstr(X86Op.Jne, basicBlocks[jmpIf.Then]);
                WriteInstr(X86Op.Jmp, basicBlocks[jmpIf.Else]);
            }
            break;

            case Instr.Store store:
            {
                var target = CompileToAddress(store.Target);
                var value = CompileValue(store.Value);
                var address = registerPool.Allocate(DataWidth.dword);
                WriteInstr(X86Op.Mov, address, target);
                WriteCopy(address, value);
            }
            break;

            case Instr.Load load:
            {
                var target = CompileToAddress(load.Result);
                var source = CompileToAddress(load.Source);
                var targetAddr = registerPool.Allocate(DataWidth.dword);
                var sourceAddr = registerPool.Allocate(DataWidth.dword);
                WriteInstr(X86Op.Lea, targetAddr, target);
                WriteInstr(X86Op.Mov, sourceAddr, source);
                WriteMemcopy(targetAddr, sourceAddr, SizeOf(load.Result));
            }
            break;

            case Instr.Alloc alloc:
            {
                var size = SizeOf(alloc.Allocated);
                WriteInstr(X86Op.Sub, Register.esp, new Operand.Literal(DataWidth.dword, size));
                var result = CompileToAddress(alloc.Result);
                WriteInstr(X86Op.Mov, result, Register.esp);
            }
            break;

            case Instr.ElementPtr elementPtr:
            {
                var target = CompileToAddress(elementPtr.Result);
                var value = CompileSingleValue(elementPtr.Value);
                // We need it in a register
                value = LoadToRegister(value);
                var structTy = (Struct)((Type.Ptr)elementPtr.Value.Type).Subtype;

                var index = elementPtr.Index.Value;
                // Get offset, add it to the base address
                var offset = OffsetOf(structTy, index);
                WriteInstr(X86Op.Add, value, new Operand.Literal(DataWidth.dword, offset));
                WriteMov(target, value);
            }
            break;

            case Instr.Cast cast:
            {
                if (cast.Target is Type.Ptr && cast.Value.Type is Type.Ptr)
                {
                    // Should be a no-op, simply copy
                    var target = CompileToAddress(cast.Result);
                    var src = CompileSingleValue(cast.Value);
                    WriteMov(target, src);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            break;

            // Size-dependent operations

            case Instr.Cmp cmp:
            {
                var targetParts = CompileValue(cmp.Result);
                var leftParts = CompileValue(cmp.Left);
                var rightParts = CompileValue(cmp.Right);
                Debug.Assert(leftParts.Length == rightParts.Length);
                var target = targetParts[0];
                var targetWidth = target.GetWidth(sizeContext);

                var truthy = new Operand.Literal(targetWidth, 1);
                var falsy = new Operand.Literal(targetWidth, 0);

                // We need to branch to write the result
                var labelNameBase = GetUniqueName("cmp_result");
                Debug.Assert(currentProcedure != null);
                var trueBB = new X86BasicBlock(currentProcedure, $"{labelNameBase}_T");
                var falseBB = new X86BasicBlock(currentProcedure, $"{labelNameBase}_F");
                var finallyBB = new X86BasicBlock(currentProcedure, $"{labelNameBase}_C");

                // Add all these basic blocks to the current procedure
                Debug.Assert(currentProcedure != null);
                currentProcedure.BasicBlocks.Add(trueBB);
                currentProcedure.BasicBlocks.Add(falseBB);
                currentProcedure.BasicBlocks.Add(finallyBB);

                bool signed = ((Type.Int)cmp.Left.Type).Signed;
                // For each part pair we compare
                // NOTE: It's important that we start from the most significant bytes here for the relational operators
                int i = 0;
                foreach (var (l, r) in leftParts.Zip(rightParts).Reverse())
                {
                    // NOTE: first && signed means that we are looking at the MSB, so in case of a signed
                    // comparison, we need to use the signed compare jumps, unsigned in any other case
                    // TL;DR: fisrt && signed means we need signed jump operation
                    bool first = i == 0;
                    bool last = i == leftParts.Length - 1;
                    i += 1;

                    // We need to do the comparison
                    var tmp = WriteNonImmOrMemoryInstr(X86Op.Cmp, l, r);
                    if (tmp != null) registerPool.Free(tmp);
                    if (last)
                    {
                        // If this was the last part to compare, we can just branch to the truthy part as the
                        // condition succeeded (or the falsy one on mismatch)
                        var inverseOp = ComparisonToJump(cmp.Comparison.Inverse, first && signed);
                        WriteInstr(inverseOp, falseBB);
                        WriteInstr(X86Op.Jmp, trueBB);
                    }
                    else
                    {
                        // These are not the last elements we compare
                        if (cmp.Comparison == Comparison.eq)
                        {
                            // We can jump to false on inequality
                            WriteInstr(X86Op.Jne, falseBB);
                        }
                        else if (cmp.Comparison == Comparison.ne)
                        {
                            // We can jump to true on inequality
                            WriteInstr(X86Op.Jne, trueBB);
                        }
                        else if (cmp.Comparison == Comparison.le || cmp.Comparison == Comparison.le_eq)
                        {
                            // If the part less, then we can jump to true, if greater, then to false
                            var lessOp = ComparisonToJump(Comparison.le, first && signed);
                            var greaterOp = ComparisonToJump(Comparison.gr, first && signed);
                            WriteInstr(lessOp, trueBB);
                            WriteInstr(greaterOp, falseBB);
                        }
                        else if (cmp.Comparison == Comparison.gr || cmp.Comparison == Comparison.gr_eq)
                        {
                            // If the part is greater, then we can jump to true, if less, then to false
                            var greaterOp = ComparisonToJump(Comparison.gr, first && signed);
                            var lessOp = ComparisonToJump(Comparison.le, first && signed);
                            WriteInstr(greaterOp, trueBB);
                            WriteInstr(lessOp, falseBB);
                        }
                    }
                }
                
                // On true block, we write the truthy value then jump to the continuation
                currentBasicBlock = trueBB;
                WriteInstr(X86Op.Mov, target, truthy);
                WriteInstr(X86Op.Jmp, finallyBB);
                // On false block, we write the falsy value then jump to the continuation
                currentBasicBlock = falseBB;
                WriteInstr(X86Op.Mov, target, falsy);
                WriteInstr(X86Op.Jmp, finallyBB);
                // We continue writing on the continuation
                currentBasicBlock = finallyBB;
            }
            break;

            case Instr.Add:
            case Instr.Sub:
            {
                var arith = (ArithInstr)instr;
                var target = CompileToAddress(arith.Result);

                if (arith.Left.Type is Type.Ptr leftPtr)
                {
                    // Pointer arithmetic
                    var left = CompileSingleValue(arith.Left);
                    var right = CompileSingleValue(arith.Right);
                    right = LoadToRegister(right);

                    WriteMov(target, left);
                    // We multiply the offset by the data size to get the expected behavior
                    WriteInstr(X86Op.Imul, right, new Operand.Literal(DataWidth.dword, SizeOf(leftPtr.Subtype)));
                    WriteInstr(arith is Instr.Add ? X86Op.Add : X86Op.Sub, target, right);
                }
                else if (arith.Left.Type is Type.Int && arith.Right.Type is Type.Int)
                {
                    // Integer arithmetic
                    var targetAddr = registerPool.Allocate(DataWidth.dword);
                    WriteInstr(X86Op.Lea, targetAddr, target);
                    var left = CompileValue(arith.Left);
                    var right = CompileValue(arith.Right);
                    Debug.Assert(left.Length == right.Length);

                    var firstOp = arith is Instr.Add ? X86Op.Add : X86Op.Sub;
                    var remOps = arith is Instr.Add ? X86Op.Adc : X86Op.Sbb;

                    int offset = 0;
                    bool first = true;
                    foreach (var (l, r) in left.Zip(right))
                    {
                        var width = l.GetWidth(sizeContext);
                        var addr = new Operand.Address(targetAddr, offset);
                        var displ = new Operand.Indirect(width, addr);
                        WriteMov(displ, l);
                        if (first)
                        {
                            // We use add or sub
                            WriteNonImmOrMemoryInstr(firstOp, displ, r);
                        }
                        else
                        {
                            // We use adc or sbb
                            WriteNonImmOrMemoryInstr(remOps, displ, r);
                        }

                        offset += width.Size;
                        first = false;
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            break;

            case Instr.Mul mul:
            {
                if (SizeOf(mul.Left.Type) > 4)
                {
                    // For now we skip this
                    throw new NotSupportedException("Multiplication of > 4 byte operands is not supported!");
                }
                var target = CompileToAddress(mul.Result);
                var left = CompileSingleValue(mul.Left);
                var right = CompileSingleValue(mul.Right);
                var tmp = registerPool.Allocate(DataWidth.dword);
                WriteMov(tmp, left);
                WriteInstr(X86Op.Imul, tmp, right);
                WriteMov(target, tmp);
            }
            break;

            case Instr.Div:
            case Instr.Mod:
            {
                var arith = (ArithInstr)instr;
                if (SizeOf(arith.Left.Type) > 4)
                {
                    // For now we skip this
                    throw new NotSupportedException("Division of > 4 byte operands is not supported!");
                }
                // NOTE: This is different from multiplication
                registerPool.Allocate(Register.eax, Register.edx);

                var target = CompileToAddress(arith.Result);
                var left = CompileSingleValue(arith.Left);
                var right = CompileSingleValue(arith.Right);
                
                right = LoadToRegister(right);
                WriteInstr(X86Op.Mov, Register.edx, new Operand.Literal(DataWidth.dword, 0));
                WriteInstr(X86Op.Mov, Register.eax, left);
                WriteInstr(X86Op.Idiv, right);
                WriteInstr(X86Op.Mov, target, arith is Instr.Div ? Register.eax : Register.edx);
            }
            break;

            case Instr.BitAnd:
            case Instr.BitOr:
            case Instr.BitXor:
            {
                var bitw = (BitwiseInstr)instr;

                var target = CompileToAddress(bitw.Result);
                var leftParts = CompileValue(bitw.Left);
                var rightParts = CompileValue(bitw.Right);

                var op = bitw switch
                {
                    Instr.BitAnd => X86Op.And,
                    Instr.BitOr => X86Op.Or,
                    Instr.BitXor => X86Op.Xor,
                    _ => throw new NotImplementedException(),
                };

                // We can just apply the operation for each part
                Debug.Assert(leftParts.Length == rightParts.Length);
                var targetAddr = registerPool.Allocate(DataWidth.dword);
                WriteInstr(X86Op.Lea, targetAddr, target);
                int offset = 0;
                foreach (var (l, r) in leftParts.Zip(rightParts))
                {
                    var width = l.GetWidth(sizeContext);
                    var addr = new Operand.Address(targetAddr, offset);
                    var displ = new Operand.Indirect(width, addr);
                    WriteMov(displ, l);
                    WriteInstr(op, displ, r);
                    offset += width.Size;
                }
            }
            break;

            case Instr.Shl:
            case Instr.Shr:
            {
                var bitsh = (BitShiftInstr)instr;
                if (SizeOf(bitsh.Shifted.Type) > 4 || SizeOf(bitsh.Amount.Type) > 4)
                {
                    // For now we skip this
                    throw new NotSupportedException("Shifting of > 4 byte operands is not supported!");
                }

                var target = CompileToAddress(bitsh.Result);
                var left = CompileSingleValue(bitsh.Shifted);
                var right = CompileSingleValue(bitsh.Amount);
                left = LoadToRegister(left);
                WriteInstr(bitsh is Instr.Shl ? X86Op.Shl : X86Op.Shr, left, right);
                WriteMov(target, left);
            }
            break;

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

        private void WriteMemcopy(Register targetBaseAddr, Register sourceBaseAddr, int bytes)
        {
            int offset = 0;
            SplitData(bytes, chunkSize =>
            {
                var width = DataWidth.GetFromSize(chunkSize);
                var targetAddr = new Operand.Address(targetBaseAddr, offset);
                var sourceAddr = new Operand.Address(sourceBaseAddr, offset);
                var source = new Operand.Indirect(width, sourceAddr);
                WriteMov(targetAddr, source);
                offset += chunkSize;
            });
        }

        private bool IsIndirect(Operand o) => 
            o is Operand.Address || o is Operand.Indirect;

        private void WriteMov(Operand target, Operand source)
        {
            if (IsIndirect(target) && IsIndirect(source))
            {
                // We can't copy memory to memory on x86
                var width = source.GetWidth(sizeContext);
                var tmp = registerPool.Allocate(width);
                WriteInstr(X86Op.Mov, tmp, source);
                WriteInstr(X86Op.Mov, target, tmp);
                registerPool.Free(tmp);
            }
            else
            {
                WriteInstr(X86Op.Mov, target, source);
            }
        }

        private Register? WriteNonImmOrMemoryInstr(X86Op op, Operand left, Operand right)
        {
            if (left is Operand.Literal && right is Operand.Literal)
            {
                // One of the arguments needs to go to a register
                var tmp = registerPool.Allocate(left.GetWidth(sizeContext));
                WriteInstr(X86Op.Mov, tmp, left);
                WriteInstr(op, tmp, right);
                return tmp;
            }
            else if (IsIndirect(left) && IsIndirect(right))
            {
                // Both can't be memory
                var tmp = registerPool.Allocate(right.GetWidth(sizeContext));
                WriteInstr(X86Op.Mov, tmp, right);
                WriteInstr(op, left, tmp);
                registerPool.Free(tmp);
            }
            else
            {
                WriteInstr(op, left, right);
            }
            return null;
        }

        private Register LoadToRegister(Operand op)
        {
            if (op is Register r) return r;
            var reg = registerPool.Allocate(op.GetWidth(sizeContext));
            WriteInstr(X86Op.Mov, reg, op);
            return reg;
        }

        private void WritePush(Operand[] ops)
        {
            foreach (var op in ops) WriteInstr(X86Op.Push, op);
        }

        private Operand CompileToAddress(Value value)
        {
            var res = CompileSingleValue(value, true);
            //Debug.Assert(res is Operand.Address || res is Operand.Symbol);
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
            static Operand Lit(DataWidth dw, object obj) => new Operand.Literal(dw, obj);
            static Operand[] Ops(params Operand[] ops) => ops;

            switch (value)
            {
            case Value.Int i:
            {
                if (asLvalue) throw new InvalidOperationException("An integer can't be an lvalue!");

                int byteCount = SizeOf(i);
                // This is less or equal to byteCount
                var origBytes = i.Value.AsSpan().ToArray();
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

            case Const:
            case Global:
            {
                var symName = GetSymbolName((ISymbol)value);
                var addr = new Operand.Symbol(symName, 0);
                //if (asLvalue) return Ops(addr);
                // Non-lvalue, load address
                var result = registerPool.Allocate(DataWidth.dword);
                WriteInstr(X86Op.Lea, result, addr);
                return Ops(result);
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

        private static void SplitData(int byteCount, Action<int> action)
        {
            // NOTE: We force evaluation
            foreach (var _ in SplitData(byteCount, x => { action(x); return 0; })) ;
        }

        private void WriteProcPrologue(Proc proc)
        {
            CommentInstr("prologue");
            WriteInstr(X86Op.Push, Register.ebp);
            WriteInstr(X86Op.Mov, Register.ebp, Register.esp);
            // Call the prelude procedure, if this is the entry point
            if (ReferenceEquals(entryPoint, proc) && prelude != null)
            {
                var x86proc = procs[prelude];
                WriteInstr(X86Op.Call, x86proc);
            }
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

        private int OffsetOf(Struct structDef, int fieldNo) => sizeContext.OffsetOf(structDef, fieldNo);
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

        private static byte[] ToByteArray(Value value) => value switch
        {
            Value.Int i => i.Value.AsSpan().ToArray(),
            Value.Array a => a.Values.SelectMany(v => ToByteArray(v)).ToArray(),
            _ => throw new NotImplementedException(),
        };

        private static bool IsPrimitive(Type type) => type is Type.Int;

        private static X86Op ComparisonToJump(Comparison cmp, bool signed) => cmp switch
        {
            Comparison.Eq => X86Op.Je,
            Comparison.Ne => X86Op.Jne,
            Comparison.Gr => signed ? X86Op.Jg : X86Op.Ja,
            Comparison.Le => signed ? X86Op.Jl : X86Op.Jb,
            Comparison.GrEq => signed ? X86Op.Jge : X86Op.Jae,
            Comparison.LeEq => signed ? X86Op.Jle : X86Op.Jbe,
            _ => throw new NotImplementedException(),
        };
    }
}
