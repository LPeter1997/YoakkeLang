using System.Collections.Generic;
using System.Linq;

namespace Yoakke.Compiler.IR.Passes
{
    /// <summary>
    /// A pass to merge <see cref="BasicBlock"/>s.
    /// </summary>
    class BlockMerging : IPass
    {
        public bool Pass(Assembly assembly)
        {
            bool changed = false;
            foreach (var proc in assembly.Procedures) Pass(proc, ref changed);
            return changed;
        }

        private void Pass(Proc proc, ref bool changed)
        {
            // We count how many jumps go into a given basic block
            var targetCount = proc.BasicBlocks.ToDictionary(bb => bb, _ => 0);
            // The first block can't be moved, we cheat with that
            targetCount[proc.BasicBlocks[0]] = 2;
            // Collect the count
            foreach (var bb in proc.BasicBlocks) CountJumpTargets(bb, targetCount);

            // Now we go through and do the optimization
            foreach (var bb in proc.BasicBlocks) MergeBlocks(bb, targetCount, ref changed);
        }

        private void CountJumpTargets(BasicBlock basicBlock, Dictionary<BasicBlock, int> targetCount)
        {
            var lastIns = basicBlock.Instructions.Last();
            CountJumpTargets(lastIns, targetCount);
        }

        private void CountJumpTargets(Instruction ins, Dictionary<BasicBlock, int> targetCount)
        {
            if (ins is Instruction.Jump jmp)
            {
                // Has a single basic block to mark
                targetCount[jmp.Target]++;
            }
            else if (ins is Instruction.JumpIf jmpIf)
            {
                // Has two blocks to mark
                targetCount[jmpIf.Then]++;
                targetCount[jmpIf.Else]++;
            }
        }

        private void MergeBlocks(BasicBlock bb, Dictionary<BasicBlock, int> targetCount, ref bool changed)
        {
            var lastIns = bb.Instructions.Last();
            if (lastIns is Instruction.Jump jmp && targetCount[jmp.Target] == 1 && jmp.Target != bb)
            {
                // The last instruction is a non-conditional jump and we are the only ones jumping there
                // AND this is not the same block! This is very important to not to fall into an infinite growth!
                // We can merge that block into here
                var target = jmp.Target;
                bb.Instructions.RemoveAt(bb.Instructions.Count - 1);
                foreach (var ins in target.Instructions) bb.Instructions.Add(ins);
                changed = true;
            }
        }
    }
}
