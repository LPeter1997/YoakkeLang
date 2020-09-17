﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// For assembling for X86.
    /// </summary>
    public class X86Assembler
    {
        private X86Assembly result = new X86Assembly();
        private X86Proc? currentProcedure;
        private X86BasicBlock? currentBasicBlock;

        private IDictionary<BasicBlock, X86BasicBlock> basicBlocks = new Dictionary<BasicBlock, X86BasicBlock>();
        private IDictionary<Proc, X86Proc> procs = new Dictionary<Proc, X86Proc>();
        private IDictionary<Lir.Register, int> registerOffsets = new Dictionary<Lir.Register, int>();

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
                    basicBlocks[bb] = new X86BasicBlock($"{x86proc.Name}.{bb.Name}");
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
            // Collect parameter offsets, they are the other way in reverse order
            // Start from 4 because of the top one being the return address
            int offset = 4;
            foreach (var r in proc.Parameters)
            {
                offset += SizeOf(r.Type);
                registerOffsets[r] = offset;
            }
            // Let's collect each local offset relative to EBP
            offset = 0;
            foreach (var r in registers)
            {
                offset -= SizeOf(r.Type);
                registerOffsets[r] = offset;
            }

            // We need to write the prologue in an initial basic block
            currentBasicBlock = new X86BasicBlock();
            currentProcedure.BasicBlocks.Add(currentBasicBlock);
            WriteProcPrologue(proc);
            // Calculate space for locals
            var allocSize = registers.Select(r => SizeOf(r.Type)).Sum();
            // Allocate space for the locals
            WriteInstr(X86Operation.Sub, Register.Esp, allocSize);

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
            switch (instr)
            {
            case Instr.Ret ret:
            {
                var valueSize = SizeOf(ret.Value);
                // Return value
                if (valueSize > 0)
                {
                    // Store return value
                    if (proc.CallConv == CallConv.Cdecl && ret.Value.Type is Type.Int && SizeOf(ret.Value) <= 4)
                    {
                        // We can return integral values with at most 32 bits in EAX
                        var retValue = CompileValue(ret.Value);
                        WriteInstr(X86Operation.Mov, Register.Eax, retValue);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                // Write the epilogue, return
                WriteProcEpilogue(proc);
                WriteInstr(X86Operation.Ret);
            }
            break;

            case Instr.Call call:
            {
                // TODO: Proper error
                if (!(call.Procedure.Type is Type.Proc procTy))
                {
                    throw new InvalidOperationException();
                }
                if (procTy.CallConv == CallConv.Cdecl)
                {
                    // Push arguments backwards, track the offset of esp
                    var espOffset = 0;
                    foreach (var arg in call.Arguments.Reverse())
                    {
                        var argValue = CompileValue(arg);
                        WriteInstr(X86Operation.Push, argValue);
                        espOffset += SizeOf(arg.Type);
                    }
                    // Do the call
                    var procedure = CompileValue(call.Procedure);
                    WriteInstr(X86Operation.Call, procedure);
                    // Restore stack
                    WriteInstr(X86Operation.Add, Register.Esp, espOffset);
                    // TODO: Only if size is fine
                    // Store value
                    var result = CompileValue(call.Result);
                    WriteInstr(X86Operation.Mov, result, Register.Eax);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            break;

            case Instr.Jmp jmp:
                WriteInstr(X86Operation.Jmp, basicBlocks[jmp.Target]);
                break;

            case Instr.JmpIf jmpIf:
            {
                var op = CompileValue(jmpIf.Condition);
                // TODO: Size should matter! EAX won't always be corrct!
                WriteInstr(X86Operation.Mov, Register.Eax, op);
                WriteInstr(X86Operation.Test, Register.Eax, Register.Eax);
                WriteInstr(X86Operation.Jne, basicBlocks[jmpIf.Then]);
                WriteInstr(X86Operation.Jmp, basicBlocks[jmpIf.Else]);
            }
            break;

            case Instr.Store store:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var target = CompileValue(store.Target, true);
                var source = CompileValue(store.Value);
                WriteInstr(X86Operation.Mov, Register.Eax, source);
                WriteInstr(X86Operation.Mov, target, Register.Eax);
            }
            break;

            case Instr.Load load:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var target = CompileValue(load.Result);
                var source = CompileValue(load.Address, true);
                WriteInstr(X86Operation.Mov, Register.Eax, source);
                WriteInstr(X86Operation.Mov, target, Register.Eax);
            }
            break;

            case Instr.Alloc alloc:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var size = SizeOf(alloc.Allocated);
                WriteInstr(X86Operation.Sub, Register.Esp, size);
                var allocResult = CompileValue(alloc.Result);
                WriteInstr(X86Operation.Mov, allocResult, Register.Esp);
            }
            break;

            case Instr.Cmp cmp:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var target = CompileValue(cmp.Result, true);
                var left = CompileValue(cmp.Left);
                var right = CompileValue(cmp.Right);
                // First we produce the operands and the flags
                WriteInstr(X86Operation.Mov, Register.Eax, left);
                WriteInstr(X86Operation.Cmp, Register.Eax, right);
                // Based on the comparison we need an x86 operation
                var op = cmp.Comparison switch
                {
                    Comparison.Eq => X86Operation.Je,
                    Comparison.Ne => X86Operation.Jne,
                    Comparison.Gr => X86Operation.Jg,
                    Comparison.Le => X86Operation.Jl,
                    Comparison.GrEq => X86Operation.Jge,
                    Comparison.LeEq => X86Operation.Jle,
                    _ => throw new NotImplementedException(),
                };
                // We need to branch to write the result
                var labelNameBase = GetUniqueName("WriteCmpResult");
                var trueBB = new X86BasicBlock($"{labelNameBase}_T");
                var falseBB = new X86BasicBlock($"{labelNameBase}_F");
                var continueBB = new X86BasicBlock($"{labelNameBase}_C");
                // Do the branch to the true or false block
                WriteInstr(op, trueBB);
                WriteInstr(X86Operation.Jmp, falseBB);
                // On true block, we write the truthy value then jump to the continuation
                currentBasicBlock = trueBB;
                WriteInstr(X86Operation.Mov, target, 1);
                WriteInstr(X86Operation.Jmp, continueBB);
                // On false block, we write the falsy value then jump to the continuation
                currentBasicBlock = falseBB;
                WriteInstr(X86Operation.Mov, target, 0);
                WriteInstr(X86Operation.Jmp, continueBB);
                // We continue writing on the continuation
                currentBasicBlock = continueBB;
                // Add all these basic blocks to the vurrent procedure
                Debug.Assert(currentProcedure != null);
                currentProcedure.BasicBlocks.Add(trueBB);
                currentProcedure.BasicBlocks.Add(falseBB);
                currentProcedure.BasicBlocks.Add(continueBB);
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        private Operand CompileValue(Value value, bool asIndirect = false)
        {
            switch (value)
            {
            case Value.Int i:
                // TODO
                if (asIndirect) throw new NotImplementedException();
                return new Operand.Literal(i.Value);

            case ISymbol sym:
            {
                // TODO
                if (asIndirect) throw new NotImplementedException();

                var symName = GetSymbolName(sym);
                return new Operand.Literal(symName);
            }

            case Lir.Register reg:
            {
                var offset = registerOffsets[reg];
                // TODO: Maybe we do need lvalue / rvalue here for reads and writes?
                var result = new Operand.Address(Register.Ebp, offset);
                if (asIndirect)
                {
                    var dataWidth = DataWidthUtils.FromByteSize(SizeOf(reg.Type));
                    return new Operand.Indirect(dataWidth, result);
                }
                return result;
            }

            default: throw new NotImplementedException();
            }
        }

        private void WriteProcPrologue(Proc _)
        {
            WriteInstr(X86Operation.Push, Register.Ebp);
            WriteInstr(X86Operation.Mov, Register.Ebp, Register.Esp);
        }

        private void WriteProcEpilogue(Proc _)
        {
            WriteInstr(X86Operation.Mov, Register.Esp, Register.Ebp);
            WriteInstr(X86Operation.Pop, Register.Ebp);
        }

        private void WriteInstr(X86Operation op, params object[] operands)
        {
            Debug.Assert(currentBasicBlock != null);
            currentBasicBlock.Instructions.Add(new X86Instr(op, operands));
        }

        // TODO: Not the best solution...
        private int nameCnt = 0;
        private string GetUniqueName(string name) => $"{currentProcedure?.Name}_{name}_{nameCnt++}";

        private static string GetSymbolName(ISymbol symbol) =>
               // For Cdecl procedures we assume an underscore prefix
               (symbol is Proc proc && proc.CallConv == CallConv.Cdecl)
            // For non-procedures too
            || !(symbol is Proc)
            ? $"_{symbol.Name}" : symbol.Name;

        private static int SizeOf(Value value) => SizeOf(value.Type);
        private static int SizeOf(Type type) => type switch
        {
            Type.Void _ => 0,
            Type.Ptr _ => 4,
            // First we round up to bytes, then make sure it's a power of 2
            Type.Int i => NextPow2((i.Bits + 7) / 8),
            _ => throw new NotImplementedException(),
        };

        private static int NextPow2(int n)
        {
            int result = 1;
            while (result < n) result = result << 1;
            return result;
        }
    }
}
