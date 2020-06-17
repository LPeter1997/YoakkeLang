using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.IR;

namespace Yoakke.Compiler.IR.Passes
{
    /// <summary>
    /// A pass to do constant folding.
    /// </summary>
    class ConstantFolding : IPass
    {
        public bool Pass(Assembly assembly)
        {
            bool changed = false;
            foreach (var proc in assembly.Procedures) Pass(proc, ref changed);
            return changed;
        }

        private void Pass(Proc proc, ref bool changed)
        {
            foreach (var bb in proc.BasicBlocks) Pass(bb, ref changed);
        }

        private void Pass(BasicBlock basicBlock, ref bool changed)
        {
            for (int i = 0; i < basicBlock.Instructions.Count; ++i)
            {
                var ins = basicBlock.Instructions[i];
                var newIns = ChangeInstruction(ins, ref changed);
                if (newIns != null)
                {
                    basicBlock.Instructions[i] = newIns;
                    changed = true;
                }
            }
        }

        private Instruction? ChangeInstruction(Instruction ins, ref bool changed)
        {
            switch (ins)
            {
            case Instruction.JumpIf jumpIf:
                // If we know the condition, we can change this into a jump
                if (jumpIf.Condition is Value.Int i)
                {
                    changed = true;
                    return new Instruction.Jump(i.Value == 1 ? jumpIf.Then : jumpIf.Else);
                }
                return null;

            default:
                return null;
            }
        }
    }
}
