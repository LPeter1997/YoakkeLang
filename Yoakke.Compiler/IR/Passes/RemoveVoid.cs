using System.Collections.Generic;
using System.Text;
using Yoakke.IR;

namespace Yoakke.Compiler.IR.Passes
{
    class RemoveVoid : IPass
    {
        public bool Pass(Assembly assembly)
        {
            bool hasChanged = false;
            foreach (var proc in assembly.Procedures) Pass(proc, ref hasChanged);
            return hasChanged;
        }

        private void Pass(Proc proc, ref bool changed)
        {
            // TODO: Go through parameters
            // Now we go through blocks
            foreach (var bb in proc.BasicBlocks) Pass(bb, ref changed);
        }

        private void Pass(BasicBlock bb, ref bool changed)
        {
            for (int insIdx = 0; insIdx < bb.Instructions.Count;)
            {
                var ins = bb.Instructions[insIdx];
                if (ins is ValueInstruction vi && Type.Void_.EqualsNonNull(vi.Value.Type))
                {
                    // This instruction results in a void value
                    if (vi is Instruction.Call call)
                    {
                        // We can't erase a call because of possible side-effects
                        // TODO: Erase void args
                        // Since return-type is void, we mark that so
                        call.Value = Value.IgnoreRegister;
                        ++insIdx;
                    }
                    else
                    {
                        // Just remove it
                        bb.Instructions.RemoveAt(insIdx);
                    }
                    changed = true;
                }
                else if (ins is Instruction.Store store && Type.Void_.EqualsNonNull(store.Value.Type))
                {
                    // Trying to store a void value
                    bb.Instructions.RemoveAt(insIdx);
                    changed = true;
                }
                else if (ins is Instruction.Alloc alloc && Type.Void_.EqualsNonNull(alloc.ElementType))
                {
                    // Trying to allocate space for a void type
                    bb.Instructions.RemoveAt(insIdx);
                    changed = true;
                }
                else
                {
                    ++insIdx;
                }
            }
        }
    }
}
