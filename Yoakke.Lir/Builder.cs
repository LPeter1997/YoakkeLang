using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.DataStructures;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Runtime;
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
            public int BasicBlockCount { get; set; }
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
                if (currentProc is null) throw new InvalidOperationException("There's no procedure defined yet!");
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
                if (currentBasicBlock is null) throw new InvalidOperationException("There's no procedure defined yet!");
                return currentBasicBlock;
            }
            set
            {
                currentProc = Assembly.Procedures.First(proc => proc.BasicBlocks.Contains(value));
                currentBasicBlock = value;
            }
        }
        /// <summary>
        /// The integer <see cref="Type"/> used for boolean results.
        /// </summary>
        public Type.Int BoolResult { get; set; } = Type.I32;

        private HashSet<Struct> structDefs = new HashSet<Struct>();
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
            var external = new Extern(name, type, path);
            Assembly.Externals.Add(external);
            return external;
        }

        /// <summary>
        /// Adds a new <see cref="Const"/> symbol definition to the <see cref="Assembly"/>.
        /// </summary>
        /// <param name="name">The name of the constant symbol.</param>
        /// <param name="value">The <see cref="Value"/> of the constant..</param>
        /// <returns>The <see cref="Value"/> referring to the constant.</returns>
        public Value DefineConst(string name, Value value)
        {
            var constant = new Const(name, value);
            Assembly.Constants.Add(constant);
            return constant;
        }

        /// <summary>
        /// Adds a new <see cref="Global"/> symbol definition to the <see cref="Assembly"/>.
        /// </summary>
        /// <param name="name">The name of the global symbol.</param>
        /// <param name="type">The <see cref="Type"/> of the global symbol.</param>
        /// <returns>The <see cref="Value"/> referring to the global symbol.</returns>
        public Value DefineGlobal(string name, Type type)
        {
            var global = new Global(name, type);
            Assembly.Globals.Add(global);
            return global;
        }

        /// <summary>
        /// Adds a new <see cref="Struct"/> to the <see cref="Assembly"/>.
        /// </summary>
        /// <param name="types">The <see cref="Type"/>s of the struct fields.</param>
        /// <returns>The <see cref="Type"/> created from the <see cref="Struct"/>.</returns>
        public Type DefineStruct(IEnumerable<Type> types)
        {
            // First we construct the type
            var name = $"type{structDefs.Count}";
            var structDef = new Struct(name);
            foreach (var t in types) structDef.Fields.Add(t);
            // Check if it matches any existing type
            if (structDefs.TryGetValue(structDef, out var existingStructTy))
            {
                // Just return the existing one
                return existingStructTy;
            }
            // We add it as new
            structDefs.Add(structDef);
            Assembly.Structs.Add(structDef);
            return structDef;
        }

        /// <summary>
        /// Defines a new procedure in the <see cref="Assembly"/>.
        /// </summary>
        /// <param name="name">The name of the procedure.</param>
        /// <returns>The created <see cref="Proc"/>.</returns>
        public Proc DefineProc(string name)
        {
            var proc = new Proc(name);
            currentProc = proc;
            Assembly.Procedures.Add(proc);
            DefineBasicBlock("begin");
            return proc;
        }

        /// <summary>
        /// Defines a new parameter in the current procedure.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the parameter.</param>
        /// <returns>The read-only <see cref="Value"/> of the new parameter.</returns>
        public Value DefineParameter(Type type)
        {
            var reg = AllocateRegister(type);
            CurrentProc.Parameters.Add(reg);
            return reg;
        }

        /// <summary>
        /// Defines a new <see cref="BasicBlock"/> in the current procedure.
        /// </summary>
        /// <param name="name">The suggested name of the basic block.</param>
        /// <returns>The defined <see cref="BasicBlock"/>.</returns>
        public BasicBlock DefineBasicBlock(string name)
        {
            name = GetUniqueBasicBlockName(name);
            var bb = new BasicBlock(name);
            // We want to insert it after the current basic block to keep the natural flow of generated code
            var insertIndex = 0;
            if (currentBasicBlock != null) insertIndex = CurrentProc.BasicBlocks.IndexOf(currentBasicBlock) + 1;

            currentBasicBlock = bb;
            // Link both ways
            bb.Proc = CurrentProc;
            CurrentProc.BasicBlocks.Insert(insertIndex, bb);
            return bb;
        }

        /// <summary>
        /// Removes the given <see cref="BasicBlock"/>.
        /// </summary>
        /// <param name="bb">The <see cref="BasicBlock"/> to remove.</param>
        public void RemoveBasicBlock(BasicBlock bb)
        {
            bb.Proc.BasicBlocks.Remove(bb);
            // We need to find another current basic block
            if (ReferenceEquals(bb, currentBasicBlock)) currentBasicBlock = CurrentProc.BasicBlocks.Last();
        }

        /// <summary>
        /// Executes some <see cref="Action{Builder}"/> while being inside the prelude procedure.
        /// Useful for defining initialization code.
        /// </summary>
        /// <param name="action">The <see cref="Action{Builder}"/> to build code with within the prelude 
        /// procedure.</param>
        public void WithPrelude(Action<Builder> action) => 
            WithSubcontext(b =>
            {
                // If there's no prelude function defined, define it
                if (Assembly.Prelude == null)
                {
                    Assembly.Prelude = DefineProc("prelude");
                }
                else
                // Set it as the current procedure
                CurrentProc = Assembly.Prelude;
                // If the last instruction was a 'ret', delete it
                {
                    var instructions = CurrentBasicBlock.Instructions;
                    if (instructions.Count > 0 && instructions.Last() is Instr.Ret)
                    {
                        instructions.RemoveAt(instructions.Count - 1);
                    }
                }
                // Allow the user to do the things inside
                action(this);
                // Add back the return instruction
                Ret();
            });

        /// <summary>
        /// Executes some <see cref="Action{Builder}"/>, restoring the original state (<see cref="CurrentProc"/> and
        /// <see cref="currentBasicBlock"/>) afterwards.
        /// </summary>
        /// <param name="action">The <see cref="Action{Builder}"/> to execute.</param>
        public void WithSubcontext(Action<Builder> action)
        {
            // Save state
            var lastProc = currentProc;
            var lastBasicBlock = currentBasicBlock;
            // Call the action
            action(this);
            // Reset state
            currentProc = lastProc;
            currentBasicBlock = lastBasicBlock;
        }

        // Instructions ////////////////////////////////////////////////////////

        /// <summary>
        /// Adds a new <see cref="Instr"/> to the current <see cref="BasicBlock"/>.
        /// </summary>
        /// <param name="instr">The <see cref="Instr"/> to add.</param>
        public void AddInstr(Instr instr)
        {
            // Link both ways
            instr.BasicBlock = CurrentBasicBlock;
            CurrentBasicBlock.Instructions.Add(instr);
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Ret"/>.
        /// </summary>
        /// <param name="value">The value to return.</param>
        public void Ret(Value value) => AddInstr(new Instr.Ret(value));

        /// <summary>
        /// Same as <see cref="Ret(Value)"/>, but returns with void.
        /// </summary>
        public void Ret() => Ret(Value.Void_);

        /// <summary>
        /// Adds a new <see cref="Instr.Call"/>.
        /// </summary>
        /// <param name="procedure">The procedure to call.</param>
        /// <param name="arguments">The arguments to call the procedure with.</param>
        /// <returns>The return <see cref="Value"/> of the call.</returns>
        public Value Call(Value procedure, IList<Value> arguments)
        {
            // Special case for user procedures
            if (procedure is Value.User userValue && userValue.Payload is IUserProc userProc)
            {
                return Call(userProc, arguments);
            }
            if (!(procedure.Type is Type.Proc procType))
            {
                throw new ArgumentException("The procedure value must be of a procedure type!", nameof(procedure));
            }
            var resultReg = AllocateRegister(procType.Return);
            AddInstr(new Instr.Call(resultReg, procedure, arguments));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Call"/> to an <see cref="IUserProc"/>.
        /// </summary>
        /// <param name="userProc">The <see cref="IUserProc"/> to call.</param>
        /// <param name="arguments">The arguments to call the procedure with.</param>
        /// <returns>The return <see cref="Value"/> of the call.</returns>
        public Value Call(IUserProc userProc, IList<Value> arguments)
        {
            var resultReg = AllocateRegister(userProc.ReturnType);
            AddInstr(new Instr.Call(resultReg, new Value.User(userProc), arguments));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Jmp"/>.
        /// </summary>
        /// <param name="target">The basic block to jump to.</param>
        public void Jmp(BasicBlock target) => AddInstr(new Instr.Jmp(target));

        /// <summary>
        /// Adds a new <see cref="Instr.JmpIf"/>.
        /// </summary>
        /// <param name="condition">The jump condition.</param>
        /// <param name="then">The basic block to jump to if the condition is truthy.</param>
        /// <param name="els">The basic block to jump to if the condition is falsy.</param>
        public void JmpIf(Value condition, BasicBlock then, BasicBlock els) =>
            AddInstr(new Instr.JmpIf(condition, then, els));

        /// <summary>
        /// Adds a new <see cref="Instr.Alloc"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to allocate space for.</param>
        /// <returns>The pointer to the allocated space.</returns>
        public Value Alloc(Type type)
        {
            var ptrType = new Type.Ptr(type);
            var resultReg = AllocateRegister(ptrType);
            AddInstr(new Instr.Alloc(resultReg));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Load"/>.
        /// </summary>
        /// <param name="source">The source pointer to load the value from.</param>
        /// <returns>The loaded <see cref="Value"/>.</returns>
        public Value Load(Value source)
        {
            if (!(source.Type is Type.Ptr ptrTy))
            {
                throw new ArgumentException("The source address must be a pointer type!", nameof(source));
            }
            var resultReg = AllocateRegister(ptrTy.Subtype);
            AddInstr(new Instr.Load(resultReg, source));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Store"/>.
        /// </summary>
        /// <param name="target">The pointer to write the <see cref="Value"/> to.</param>
        /// <param name="value">The <see cref="Value"/> to write.</param>
        public void Store(Value target, Value value) => AddInstr(new Instr.Store(target, value));

        /// <summary>
        /// Adds a new <see cref="Instr.Cmp"/>.
        /// </summary>
        /// <param name="comparison">The kind of comparison to do.</param>
        /// <param name="left">The left-hand side <see cref="Value"/> of the comparison.</param>
        /// <param name="right">The right-hand side <see cref="Value"/> of the comparison.</param>
        /// <returns>The comparison result.</returns>
        public Value Cmp(Comparison comparison, Value left, Value right)
        {
            var resultReg = AllocateRegister(BoolResult);
            AddInstr(new Instr.Cmp(resultReg, comparison, left, right));
            return resultReg;
        }

        /// <summary>
        /// Writes an equality <see cref="Cmp(Comparison, Value, Value)"/>.
        /// </summary>
        public Value CmpEq(Value left, Value right) => Cmp(Comparison.eq, left, right);

        /// <summary>
        /// Writes an inequality <see cref="Cmp(Comparison, Value, Value)"/>.
        /// </summary>
        public Value CmpNe(Value left, Value right) => Cmp(Comparison.ne, left, right);

        /// <summary>
        /// Writes a greater-than <see cref="Cmp(Comparison, Value, Value)"/>.
        /// </summary>
        public Value CmpGr(Value left, Value right) => Cmp(Comparison.gr, left, right);

        /// <summary>
        /// Writes a less-than <see cref="Cmp(Comparison, Value, Value)"/>.
        /// </summary>
        public Value CmpLe(Value left, Value right) => Cmp(Comparison.le, left, right);

        /// <summary>
        /// Writes a less-or-equals <see cref="Cmp(Comparison, Value, Value)"/>.
        /// </summary>
        public Value CmpLeEq(Value left, Value right) => Cmp(Comparison.le_eq, left, right);

        /// <summary>
        /// Writes a greater-or-equals <see cref="Cmp(Comparison, Value, Value)"/>.
        /// </summary>
        public Value CmpGrEq(Value left, Value right) => Cmp(Comparison.gr_eq, left, right);

        /// <summary>
        /// Adds a new <see cref="Instr.Add"/>.
        /// </summary>
        /// <param name="left">The left-hand side of the addition.</param>
        /// <param name="right">The right-hand side of the addition.</param>
        /// <returns>The result of the addition.</returns>
        public Value Add(Value left, Value right)
        {
            var resultReg = AllocateRegister(ArithInstr.CommonArithmeticType(left.Type, right.Type));
            AddInstr(new Instr.Add(resultReg, left, right));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Sub"/>.
        /// </summary>
        /// <param name="left">The left-hand side of the subtraction.</param>
        /// <param name="right">The right-hand side of the subtraction.</param>
        /// <returns>The result of the subtraction.</returns>
        public Value Sub(Value left, Value right)
        {
            var resultReg = AllocateRegister(ArithInstr.CommonArithmeticType(left.Type, right.Type));
            AddInstr(new Instr.Sub(resultReg, left, right));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Mul"/>.
        /// </summary>
        /// <param name="left">The left-hand side of the multiplication.</param>
        /// <param name="right">The right-hand side of the multiplication.</param>
        /// <returns>The result of the multiplication.</returns>
        public Value Mul(Value left, Value right)
        {
            var resultReg = AllocateRegister(ArithInstr.CommonArithmeticType(left.Type, right.Type));
            AddInstr(new Instr.Mul(resultReg, left, right));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Div"/>.
        /// </summary>
        /// <param name="left">The left-hand side of the division.</param>
        /// <param name="right">The right-hand side of the division.</param>
        /// <returns>The result of the division.</returns>
        public Value Div(Value left, Value right)
        {
            var resultReg = AllocateRegister(ArithInstr.CommonArithmeticType(left.Type, right.Type));
            AddInstr(new Instr.Div(resultReg, left, right));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Mod"/>.
        /// </summary>
        /// <param name="left">The left-hand side of the modulo.</param>
        /// <param name="right">The right-hand side of the modulo.</param>
        /// <returns>The result of the modulo.</returns>
        public Value Mod(Value left, Value right)
        {
            var resultReg = AllocateRegister(ArithInstr.CommonArithmeticType(left.Type, right.Type));
            AddInstr(new Instr.Mod(resultReg, left, right));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.BitAnd"/>.
        /// </summary>
        /// <param name="left">The left-hand side of the bitwise-and.</param>
        /// <param name="right">The right-hand side of the bitwise-and.</param>
        /// <returns>The result of the bitwise-and.</returns>
        public Value BitAnd(Value left, Value right)
        {
            var resultReg = AllocateRegister(left.Type);
            AddInstr(new Instr.BitAnd(resultReg, left, right));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.BitOr"/>.
        /// </summary>
        /// <param name="left">The left-hand side of the bitwise-or.</param>
        /// <param name="right">The right-hand side of the bitwise-or.</param>
        /// <returns>The result of the bitwise-or.</returns>
        public Value BitOr(Value left, Value right)
        {
            var resultReg = AllocateRegister(left.Type);
            AddInstr(new Instr.BitOr(resultReg, left, right));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.BitXor"/>.
        /// </summary>
        /// <param name="left">The left-hand side of the bitwise-xor.</param>
        /// <param name="right">The right-hand side of the bitwise-xor.</param>
        /// <returns>The result of the bitwise-xor.</returns>
        public Value BitXor(Value left, Value right)
        {
            var resultReg = AllocateRegister(left.Type);
            AddInstr(new Instr.BitXor(resultReg, left, right));
            return resultReg;
        }

        /// <summary>
        /// Adds a bitwise negate instruction.
        /// Implements it as XOR-ing with all 1s.
        /// </summary>
        /// <param name="value">The value to bitwise-negate.</param>
        /// <returns>The result of the bitwise-negation.</returns>
        public Value BitNot(Value value)
        {
            // NOTE: We do a bit-xor
            var intType = (Type.Int)value.Type;;
            var allOnes = intType.NewValue(intType.Signed ? BigInt.AllOnes(true, intType.Bits) : intType.MaxValue);
            return BitXor(value, allOnes);
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Shl"/>.
        /// </summary>
        /// <param name="shifted">The value to shift left.</param>
        /// <param name="amount">The amount to shift left with.</param>
        /// <returns>The result of the left-shift</returns>
        public Value Shl(Value shifted, Value amount)
        {
            var resultReg = AllocateRegister(shifted.Type);
            AddInstr(new Instr.Shl(resultReg, shifted, amount));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Shr"/>.
        /// </summary>
        /// <param name="shifted">The value to shift right.</param>
        /// <param name="amount">The amount to shift right with.</param>
        /// <returns>The result of the right-shift</returns>
        public Value Shr(Value shifted, Value amount)
        {
            var resultReg = AllocateRegister(shifted.Type);
            AddInstr(new Instr.Shr(resultReg, shifted, amount));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.ElementPtr"/>.
        /// </summary>
        /// <param name="value">The base pointer to get the result relative to.</param>
        /// <param name="index">The index of the struct field to calculate the pointer of.</param>
        /// <returns>The pointer to the struct field.</returns>
        public Value ElementPtr(Value value, int index)
        {
            var resultPtrTy = Instr.ElementPtr.AccessedSubtype(value.Type, index);
            var resultReg = AllocateRegister(resultPtrTy);
            AddInstr(new Instr.ElementPtr(resultReg, value, index));
            return resultReg;
        }

        /// <summary>
        /// Adds a new <see cref="Instr.Cast"/>.
        /// </summary>
        /// <param name="target">The target <see cref="Type"/> to cast the <see cref="Value"/> to.</param>
        /// <param name="value">The <see cref="Value"/> to cast.</param>
        /// <returns>The casted <see cref="Value"/>.</returns>
        public Value Cast(Type target, Value value)
        {
            var resultReg = AllocateRegister(target);
            AddInstr(new Instr.Cast(resultReg, target, value));
            return resultReg;
        }

        // Internals

        private string GetUniqueBasicBlockName(string name)
        {
            var ctx = GetCurrentProcContext();
            return $"bb{ctx.BasicBlockCount++}_{name}";
        }

        private Register AllocateRegister(Type type)
        {
            var ctx = GetCurrentProcContext();
            return new Register(type, ctx.RegisterCount++);
        }

        private ProcContext GetCurrentProcContext()
        {
            if (!procContexts.TryGetValue(CurrentProc, out var ctx))
            {
                ctx = new ProcContext();
                procContexts.Add(CurrentProc, ctx);
            }
            return ctx;
        }
    }
}
