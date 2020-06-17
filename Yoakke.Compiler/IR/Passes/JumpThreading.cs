using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.IR;

namespace Yoakke.Compiler.IR.Passes
{
    /// <summary>
    /// A pass that does jump threading optimization.
    /// </summary>
    class JumpThreading : IPass
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
            foreach (var ins in basicBlock.Instructions) Pass(ins, ref changed);
        }

        private void Pass(Instruction ins, ref bool changed)
        {
            if (ins is Instruction.Jump jmp)
            {
                var target = GetSingleJumpTarget(jmp.Target);
                if (target != null)
                {
                    changed = true;
                    jmp.Target = target;
                }
            }
            else if (ins is Instruction.JumpIf jmpIf)
            {
                var thenTarget = GetSingleJumpTarget(jmpIf.Then);
                if (thenTarget != null)
                {
                    changed = true;
                    jmpIf.Then = thenTarget;
                }
                var elseTarget = GetSingleJumpTarget(jmpIf.Else);
                if (elseTarget != null)
                {
                    changed = true;
                    jmpIf.Else = elseTarget;
                }
            }
        }

        private BasicBlock? GetSingleJumpTarget(BasicBlock bb) =>
            // If a basic block has a single unconditional jump instruction, it can be skipped
            (bb.Instructions.Count == 1 && bb.Instructions[0] is Instruction.Jump jmp)
            ? jmp.Target
            : null;
    }
}
