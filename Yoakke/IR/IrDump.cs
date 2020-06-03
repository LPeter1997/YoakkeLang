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
            builder.Append("proc ");
            DumpType(builder, proc.ReturnType);
            builder
                .Append(' ')
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

            foreach (var ins in basicBlock.Instructions)
            {
                builder.Append("  ");
                DumpInstruction(builder, ins);
                builder.Append('\n');
            }
        }

        private static void DumpInstruction(StringBuilder builder, Instruction instruction)
        {
            if (instruction is RegisterInstruction reg)
            {
                builder
                    .Append('r')
                    .Append(reg.Register)
                    .Append(" = ");
            }

            switch (instruction)
            {
            case AllocInstruction alloc:
                builder.Append("alloc ");
                DumpType(builder, alloc.ElementType);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void DumpType(StringBuilder builder, Type type)
        {
            switch (type)
            {
            case VoidType voidType:
                builder.Append("void");
                break;

            case IntType intType:
                builder
                    .Append('i')
                    .Append(intType.Bits);
                break;

            case PtrType ptrType:
                builder.Append('*');
                DumpType(builder, ptrType.ElementType);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
