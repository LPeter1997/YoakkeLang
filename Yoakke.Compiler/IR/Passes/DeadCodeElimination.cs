using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Compiler.IR;

namespace Yoakke.Compiler.IR.Passes
{
    /// <summary>
    /// A pass to remove dead code.
    /// </summary>
    class DeadCodeElimination : IPass
    {
        public bool Pass(Assembly assembly)
        {
            bool changed = false;
            foreach (var proc in assembly.Procedures) Pass(proc, ref changed);
            return changed;
        }

        private void Pass(Proc proc, ref bool changed)
        {
            // TODO: This is not a good approach to remove self-referential loops
            // Example:
            // label1:
            //     jmp label1
            // This promotes 'label1' to a jump target!
            // We would need a control-flow graph to properly deal with this!

            // First we collect each basic block that is a jump target
            // The first basic block is always a jump target
            var jumpTargets = new HashSet<BasicBlock> { proc.BasicBlocks[0] };
            foreach (var bb in proc.BasicBlocks) CollectJumpTargets(bb, jumpTargets);

            // Now eliminate the ones that aren't in the target set
            for (int i = 0; i < proc.BasicBlocks.Count;)
            {
                var bb = proc.BasicBlocks[i];
                if (jumpTargets.Contains(bb))
                {
                    // We spare this one
                    ++i;
                }
                else
                {
                    // Remove it
                    proc.BasicBlocks.RemoveAt(i);
                    changed = true;
                }
            }
        }

        private void CollectJumpTargets(BasicBlock basicBlock, HashSet<BasicBlock> jumpTargets)
        {
            foreach (var ins in basicBlock.Instructions) CollectJumpTargets(ins, jumpTargets);
        }

        private void CollectJumpTargets(Instruction ins, HashSet<BasicBlock> jumpTargets)
        {
            if (ins is Instruction.Jump jmp)
            {
                jumpTargets.Add(jmp.Target);
            }
            else if (ins is Instruction.JumpIf jmpIf)
            {
                jumpTargets.Add(jmpIf.Then);
                jumpTargets.Add(jmpIf.Else);
            }
        }
    }
}
