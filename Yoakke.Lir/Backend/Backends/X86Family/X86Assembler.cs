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

        private ISet<Register> allRegs = new HashSet<Register> { Register.Eax, Register.Ecx, Register.Edx, Register.Ebx };
        private ISet<Register> occupiedRegs = new HashSet<Register>();

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
            currentBasicBlock = new X86BasicBlock();
            currentProcedure.BasicBlocks.Add(currentBasicBlock);
            WriteProcPrologue(proc);
            // Calculate space for locals
            var allocSize = registers.Sum(SizeOf);
            // Allocate space for the locals
            WriteInstr(X86Op.Sub, Register.Esp, allocSize);

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
            // TODO: Erase zero-size operands everywhere

            FreeOccupiedRegs();
            CommentInstr(instr);
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
                        OccupyRegister(Register.Eax);
                        var retValue = CompileValue(ret.Value);
                        WriteInstr(X86Op.Mov, Register.Eax, retValue);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                // Write the epilogue, return
                WriteProcEpilogue(proc);
                CommentInstr(instr);
                WriteInstr(X86Op.Ret);
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
                        WriteInstr(X86Op.Push, argValue);
                        espOffset += SizeOf(arg);
                        FreeOccupiedRegs();
                    }
                    // Do the call
                    var procedure = CompileValue(call.Procedure);
                    WriteInstr(X86Op.Call, procedure);
                    // Restore stack
                    WriteInstr(X86Op.Add, Register.Esp, espOffset);
                    // TODO: Only if size is fine
                    // Store value
                    var result = CompileValue(call.Result, true);
                    WriteInstr(X86Op.Mov, result, Register.Eax);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            break;

            case Instr.Jmp jmp:
                WriteInstr(X86Op.Jmp, basicBlocks[jmp.Target]);
                break;

            case Instr.JmpIf jmpIf:
            {
                // TODO: Size should matter! EAX won't always be corrct!
                var op = CompileValue(jmpIf.Condition);
                ToNonImmediate(ref op);
                WriteInstr(X86Op.Test, op, op);
                WriteInstr(X86Op.Jne, basicBlocks[jmpIf.Then]);
                WriteInstr(X86Op.Jmp, basicBlocks[jmpIf.Else]);
            }
            break;

            case Instr.Store store:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var target = (Operand.Register_)CompileValue(store.Target);
                var addr = new Operand.Address(target.Register);
                var indirect = new Operand.Indirect(target.Register.GetWidth(), addr);
                var source = CompileValue(store.Value);
                WriteInstr(X86Op.Mov, indirect, source);
            }
            break;

            case Instr.Load load:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var target = CompileValue(load.Result, true);
                var source = (Operand.Register_)CompileValue(load.Address);
                var addr = new Operand.Address(source.Register);
                var indirect = new Operand.Indirect(source.Register.GetWidth(), addr);
                var immediate = OccupyRegister();
                WriteInstr(X86Op.Mov, immediate, indirect);
                WriteInstr(X86Op.Mov, target, immediate);
            }
            break;

            case Instr.Alloc alloc:
            {
                // TODO: What if the operands don't fit in 32 bits?
                var size = SizeOf(alloc.Allocated);
                WriteInstr(X86Op.Sub, Register.Esp, size);
                var result = CompileValue(alloc.Result, true);
                WriteInstr(X86Op.Mov, result, Register.Esp);
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
                var trueBB = new X86BasicBlock($"{labelNameBase}_T");
                var falseBB = new X86BasicBlock($"{labelNameBase}_F");
                var continueBB = new X86BasicBlock($"{labelNameBase}_C");
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
                var eax = OccupyRegister(Register.Eax);
                var edx = OccupyRegister(Register.Edx);
                var target = CompileValue(arith.Result, true);
                var left = CompileValue(arith.Left);
                var right = CompileValue(arith.Right);
                ToNonImmediate(ref right);
                WriteInstr(X86Op.Mov, edx, 0);
                WriteInstr(X86Op.Mov, eax, left);
                // TODO: Signed vs. unsigned?
                WriteInstr(X86Op.Idiv, right);
                WriteInstr(X86Op.Mov, target, arith is Instr.Div ? eax : edx);
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

            default: throw new NotImplementedException();
            }
        }

        private static readonly object zeroSizeMarker = new object();
        private Operand CompileValue(Value value, bool asLvalue = false)
        {
            switch (value)
            {
            case Value.Int i:
                // TODO
                if (asLvalue) throw new NotImplementedException();
                return new Operand.Literal(i.Value);

            case ISymbol sym:
            {
                // TODO
                if (asLvalue) throw new NotImplementedException();
                var symName = GetSymbolName(sym);
                return new Operand.Literal(symName);
            }

            case Lir.Register reg:
            {
                var regSize = SizeOf(reg);
                if (regSize == 0) return new Operand.Literal(zeroSizeMarker);
                var offset = registerOffsets[reg];
                var addr = new Operand.Address(Register.Ebp, offset);
                var dataWidth = DataWidthUtils.FromByteSize(regSize);
                var result = new Operand.Indirect(dataWidth, addr);
                if (asLvalue)
                {
                    return result;
                }
                else
                {
                    // TODO: This is stupid for larger elements...
                    // Else we load it
                    var targetReg = OccupyRegister();
                    WriteInstr(X86Op.Mov, targetReg, result);
                    return targetReg;
                }
            }

            default: throw new NotImplementedException();
            }
        }

        private void CleanseImmediates(ref Operand left, ref Operand right)
        {
            // TODO: Operand size...
            if (left is Operand.Literal imm && right is Operand.Literal)
            {
                // Can't both be immediates!
                ToNonImmediate(ref left);
            }
        }

        private void ToNonImmediate(ref Operand op)
        {
            // TODO: Operand size...
            if (op is Operand.Literal imm)
            {
                op = OccupyRegister();
                WriteInstr(X86Op.Mov, op, imm);
            }
        }

        private void ToRegister(ref Operand op)
        {
            // TODO: Operand size...
            if (!(op is Operand.Register_))
            {
                var reg = OccupyRegister();
                WriteInstr(X86Op.Mov, reg, op);
                op = reg;
            }
        }

        private void WriteProcPrologue(Proc _)
        {
            CommentInstr("prologue");
            WriteInstr(X86Op.Push, Register.Ebp);
            WriteInstr(X86Op.Mov, Register.Ebp, Register.Esp);
        }

        private void WriteProcEpilogue(Proc _)
        {
            CommentInstr("epilogue");
            WriteInstr(X86Op.Mov, Register.Esp, Register.Ebp);
            WriteInstr(X86Op.Pop, Register.Ebp);
        }

        private void CommentInstr(object comment)
        {
            // if (nextComment != null) throw new InvalidOperationException();
            nextComment = comment.ToString();
        }

        private void WriteInstr(X86Op op, params object[] operands)
        {
            Debug.Assert(currentBasicBlock != null);
            var instr = new X86Instr(op, operands);
            if (nextComment != null)
            {
                instr.Comment = nextComment;
                nextComment = null;
            }
            if (!operands.Any(op => op is Operand.Literal l && l.Value == zeroSizeMarker))
            {
                currentBasicBlock.Instructions.Add(instr);
            }
        }

        // TODO: We could have a way to free up single registers too!
        // TODO: If we return Register type, it's more versitaile
        private Operand OccupyRegister()
        {
            foreach (var reg in allRegs)
            {
                if (!occupiedRegs.Contains(reg)) return OccupyRegister(reg);
            }
            throw new InvalidOperationException();
        }
        private Operand OccupyRegister(Register register)
        {
            if (occupiedRegs.Contains(register)) throw new InvalidOperationException();
            occupiedRegs.Add(register);
            return new Operand.Register_(register);
        }
        private void FreeOccupiedRegs() => occupiedRegs.Clear();

        // TODO: Not the best solution...
        private int nameCnt = 0;
        private string GetUniqueName(string name) => $"{currentProcedure?.Name}_{name}_{nameCnt++}";

        private static string GetSymbolName(ISymbol symbol) =>
               // For Cdecl procedures we assume an underscore prefix
               (symbol is Proc proc && proc.CallConv == CallConv.Cdecl)
            // For non-procedures too
            || !(symbol is Proc)
            ? $"_{symbol.Name}" : symbol.Name;

        private int OffsetOf(StructDef structDef, int fieldNo) => sizeContext.OffsetOf(structDef, fieldNo);
        private int SizeOf(Value value) => SizeOf(value.Type);
        private int SizeOf(Type type) => sizeContext.SizeOf(type);
    }
}
