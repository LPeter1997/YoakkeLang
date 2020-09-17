﻿using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// Utility for building IR code in an <see cref="Assembly"/>.
    /// </summary>
    public class Builder
    {
        private class ProcContext
        {
            public int RegisterCount { get; set; }
        }

        /// <summary>
        /// The <see cref="UncheckedAssembly"/> the <see cref="Builder"/> works with.
        /// </summary>
        public readonly UncheckedAssembly Assembly;

        /// <summary>
        /// The currently built <see cref="Proc"/>.
        /// </summary>
        public Proc CurrentProc
        {
            get
            {
                // TODO
                if (currentProc is null) throw new NotImplementedException();
                return currentProc;
            }
            set
            {
                currentProc = value;
                currentBasicBlock = currentProc.BasicBlocks.Last();
            }
        }
        /// <summary>
        /// The currently built <see cref="BasicBlock"/>.
        /// </summary>
        public BasicBlock CurrentBasicBlock
        {
            get
            {
                // TODO
                if (currentBasicBlock is null) throw new NotImplementedException();
                return currentBasicBlock;
            }
            set
            {
                currentProc = Assembly.Procedures.First(proc => proc.BasicBlocks.Contains(value));
                currentBasicBlock = value;
            }
        }

        private Proc? currentProc;
        private BasicBlock? currentBasicBlock;
        private IDictionary<Proc, ProcContext> procContexts = new Dictionary<Proc, ProcContext>();

        /// <summary>
        /// Initializes a new <see cref="Builder"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="UncheckedAssembly"/> to build IR code in.</param>
        public Builder(UncheckedAssembly assembly)
        {
            Assembly = assembly;
        }

        /// <summary>
        /// Adds a new <see cref="Extern"/> symbol definition to the <see cref="Assembly"/>.
        /// </summary>
        /// <param name="name">The name of the external symbol.</param>
        /// <param name="type">The <see cref="Type"/> of the external symbol.</param>
        /// <param name="path">The path to the binary containing the external symbol.</param>
        /// <returns>The <see cref="Value"/> referring to the external symbol.</returns>
        public Value DefineExtern(string name, Type type, string path)
        {
            // TODO: Check name uniqueness
            var external = new Extern(name, type, path);
            Assembly.Externals.Add(external);
            return external;
        }

        // TODO: Return value instead
        public Proc DefineProc(string name)
        {
            // TODO: Check name uniqueness
            var proc = new Proc(name);
            currentProc = proc;
            Assembly.Procedures.Add(proc);
            DefineBasicBlock("begin");
            return proc;
        }

        // TODO: Doc
        // TODO: Allocate and assign implicitly?
        public Value DefineParameter(Type type)
        {
            var reg = AllocateRegister(type);
            CurrentProc.Parameters.Add(reg);
            return reg;
        }

        // TODO: Doc
        public BasicBlock DefineBasicBlock(string name)
        {
            var bb = new BasicBlock(name);
            currentBasicBlock = bb;
            CurrentProc.BasicBlocks.Add(bb);
            return bb;
        }

        // Instructions ////////////////////////////////////////////////////////

        // TODO: Doc
        public void AddInstruction(Instr instr) =>
            CurrentBasicBlock.Instructions.Add(instr);

        // TODO: Doc
        public void Ret(Value value) => AddInstruction(new Instr.Ret(value));

        // TODO: Doc
        public void Ret() => Ret(Value.Void_);

        // TODO: Doc
        public Value Call(Value procedure, IList<Value> arguments)
        {
            if (!(procedure.Type is Type.Proc procType))
            {
                throw new ArgumentException("The procedure value must have a procedure type!", nameof(procedure));
            }
            var resultReg = AllocateRegister(procType.Return);
            AddInstruction(new Instr.Call(resultReg, procedure, arguments));
            return resultReg;
        }

        // TODO: Doc
        public void Jmp(BasicBlock target) => AddInstruction(new Instr.Jmp(target));

        // TODO: Doc
        public void JmpIf(Value condition, BasicBlock then, BasicBlock els) =>
            AddInstruction(new Instr.JmpIf(condition, then, els));

        // TODO: Doc
        public Value Alloc(Type type)
        {
            var ptrType = new Type.Ptr(type);
            var resultReg = AllocateRegister(ptrType);
            AddInstruction(new Instr.Alloc(resultReg));
            return resultReg;
        }

        // TODO: Doc
        public Value Load(Value source)
        {
            if (!(source.Type is Type.Ptr ptrTy))
            {
                throw new ArgumentException("The source address must be a pointer type!", nameof(source));
            }
            var resultReg = AllocateRegister(ptrTy.Subtype);
            AddInstruction(new Instr.Load(resultReg, source));
            return resultReg;
        }

        // TODO: Doc
        public void Store(Value target, Value value) => AddInstruction(new Instr.Store(target, value));

        // TODO: Doc
        public Value Cmp(Comparison comparison, Value left, Value right)
        {
            // NOTE: Do we want an i32 here? What about an u1?
            var resultReg = AllocateRegister(Type.I32);
            AddInstruction(new Instr.Cmp(resultReg, comparison, left, right));
            return resultReg;
        }

        // TODO: Doc
        public Value CmpEq(Value left, Value right) => Cmp(Comparison.Eq_, left, right);

        // TODO: Doc
        public Value CmpNe(Value left, Value right) => Cmp(Comparison.Ne_, left, right);

        // TODO: Doc
        public Value CmpGr(Value left, Value right) => Cmp(Comparison.Gr_, left, right);

        // TODO: Doc
        public Value CmpLe(Value left, Value right) => Cmp(Comparison.Le_, left, right);

        // TODO: Doc
        public Value CmpLeEq(Value left, Value right) => Cmp(Comparison.LeEq_, left, right);

        // TODO: Doc
        public Value CmpGrEq(Value left, Value right) => Cmp(Comparison.GrEq_, left, right);

        // Internals

        private Register AllocateRegister(Type type)
        {
            if (!procContexts.TryGetValue(CurrentProc, out var ctx))
            {
                ctx = new ProcContext();
                procContexts.Add(CurrentProc, ctx);
            }
            return new Register(type, ctx.RegisterCount++);
        }
    }
}
