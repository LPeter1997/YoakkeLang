using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Utils;

namespace Yoakke.IR
{
    /// <summary>
    /// Functionality to dump IR code as text.
    /// </summary>
    static class IrDump
    {
        /// <summary>
        /// Dumps the given IR <see cref="Assembly"/> as human-readable text.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to dump.</param>
        /// <returns>The string representation of the IR code.</returns>
        public static string Dump(Assembly assembly)
        {
            var result = new StringBuilder();
            foreach (var proc in assembly.Procedures)
            {
                DumpProc(result, proc);
                result.Append('\n');
            }
            return result.ToString().Trim();
        }

        private static void DumpProc(StringBuilder builder, Proc proc)
        {
            builder.Append("proc ");
            DumpType(builder, proc.ReturnType);
            builder
                .Append(' ')
                .Append(proc.Name)
                .Append('(');
            // Parameters
            proc.Parameters.Intertwine(param =>
            {
                DumpType(builder, param.Type);
                builder.Append(' ');
                DumpValue(builder, param);
            },
            () => builder.Append(", "));
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
            if (instruction is ValueInstruction value)
            {
                DumpValue(builder, value.Value);
                builder.Append(" = ");
            }

            switch (instruction)
            {
            case Instruction.Alloc alloc:
                builder.Append("alloc ");
                DumpType(builder, alloc.ElementType);
                break;

            case Instruction.Ret ret:
                builder.Append("ret");
                if (ret.Value != null)
                {
                    builder.Append(' ');
                    DumpValue(builder, ret.Value);
                }
                break;

            case Instruction.Store store:
                builder.Append("store ");
                DumpValue(builder, store.Target);
                builder.Append(", ");
                DumpValue(builder, store.Value);
                break;

            case Instruction.Load load:
                builder.Append("load ");
                DumpValue(builder, load.Source);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void DumpValue(StringBuilder builder, Value value)
        {
            switch (value)
            {
            case Value.Register reg:
                builder.Append('r').Append(reg.Index);
                break;

            case Value.Int intVal:
                builder.Append(intVal.Value);
                break;
            }
        }

        private static void DumpType(StringBuilder builder, Type type)
        {
            switch (type)
            {
            case IR.Type.Void _:
                builder.Append("void");
                break;

            case IR.Type.Int intType:
                builder.Append('i').Append(intType.Bits);
                break;

            case IR.Type.Ptr ptrType:
                builder.Append('*');
                DumpType(builder, ptrType.ElementType);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
