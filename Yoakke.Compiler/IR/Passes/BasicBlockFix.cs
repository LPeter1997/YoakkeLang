using System.Diagnostics;

namespace Yoakke.Compiler.IR.Passes
{
    /// <summary>
    /// A pass to remove any <see cref="Instruction"/> after branches in <see cref="BasicBlock"/>s.
    /// </summary>
    class BasicBlockFix : IPass
    {
        public bool Pass(Assembly assembly)
        {
            bool hasChanged = false;
            foreach (var proc in assembly.Procedures) Pass(proc, ref hasChanged);
            return hasChanged;
        }

        private void Pass(Proc proc, ref bool changed)
        {
            // Now we go through blocks
            foreach (var bb in proc.BasicBlocks) Pass(bb, ref changed);
        }

        private void Pass(BasicBlock bb, ref bool changed)
        {
            // Find the index of the first jump
            int firstJump;
            for (firstJump = 0; firstJump < bb.Instructions.Count; ++firstJump)
            {
                if (bb.Instructions[firstJump].IsJump) break;
            }
            //Remove everything after
            Debug.Assert(firstJump < bb.Instructions.Count);
            var jumpsAfter = bb.Instructions.Count - (firstJump + 1);
            bb.Instructions.RemoveRange(firstJump + 1, jumpsAfter);
        }
    }
}
