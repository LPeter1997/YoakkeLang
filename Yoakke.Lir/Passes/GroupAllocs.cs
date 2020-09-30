using System.Collections.Generic;
using System.Linq;
using Yoakke.Lir.Instructions;

namespace Yoakke.Lir.Passes
{
    /// <summary>
    /// A <see cref="ICodePass"/> to group allocations to the front of a procedure.
    /// </summary>
    public class GroupAllocs : ICodePass
    {
        public bool IsSinglePass => true;

        public void Pass(Assembly assembly, out bool changed)
        {
            changed = false;
            foreach (var proc in assembly.Procedures)
            {
                // NOTE: The order is important here, || is lazy!
                changed = Pass(proc) || changed;
            }
        }

        private bool Pass(Proc proc)
        {
            // First weed out the allocations from everywhere
            var allocs = RemoveAllocs(proc);
            if (allocs.Count == 0)
            {
                // In case there are no allocations, we don't bother
                return false;
            }
            // NOTE: We need a new basic block at the beginning, where we do our allocations, then
            // jump to the actual starting block. This is to avoid double-allocations when we jump 
            // back to the beginning block, because it's a loop!
            var preludeBB = new BasicBlock($"bb_{proc.BasicBlocks.Count}_prelude");
            foreach (var instr in allocs) preludeBB.Instructions.Add(instr);
            preludeBB.Instructions.Add(new Instr.Jmp(proc.BasicBlocks.First()));
            // Now we insert our block
            preludeBB.Proc = proc;
            proc.BasicBlocks.Insert(0, preludeBB);
            // We modified
            return true;
        }

        private IList<Instr> RemoveAllocs(Proc proc)
        {
            var result = new List<Instr>();
            foreach (var bb in proc.BasicBlocks) RemoveAllocs(result, bb);
            return result;
        }

        private void RemoveAllocs(IList<Instr> allocs, BasicBlock basicBlock)
        {
            for (int i = 0; i < basicBlock.Instructions.Count;)
            {
                var ins = basicBlock.Instructions[i];
                if (ins is Instr.Alloc)
                {
                    allocs.Add(ins);
                    basicBlock.Instructions.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }
    }
}
