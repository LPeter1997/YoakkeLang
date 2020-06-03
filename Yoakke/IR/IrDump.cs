using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    static class IrDump
    {
        public static string Dump(Assembly assembly)
        {
            var result = new StringBuilder();
            foreach (var proc in assembly.Procedures)
            {
                DumpProc(result, proc);
                result.Append('\n');
            }
            return result.ToString();
        }

        private static void DumpProc(StringBuilder builder, Proc proc)
        {
            builder
                .Append("proc ")
                .Append(proc.Name)
                .Append('(');
            // TODO: Parameters
            builder.Append("):\n");

            foreach (var bb in proc.BasicBlocks) DumpBasicBlock(builder, bb);
        }

        private static void DumpBasicBlock(StringBuilder builder, BasicBlock basicBlock)
        {
            builder
                .Append(basicBlock.Name)
                .Append(":\n");
            // TODO: Instructions
        }
    }
}
