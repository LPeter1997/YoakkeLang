using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.IR;
using Yoakke.Utils;

namespace Yoakke.Backend
{
    /// <summary>
    /// Code-generation for C.
    /// </summary>
    class CCodegen : ICodegen
    {
        public string Compile(Assembly assembly)
        {
            var result = new StringBuilder();
            result.Append("#include <stdint.h>\n\n");
            foreach (var proc in assembly.Procedures)
            {
                Declare(result, proc);
                result.Append('\n');
            }
            result.Append('\n');
            foreach (var proc in assembly.Procedures)
            {
                Compile(result, proc);
                result.Append('\n');
            }
            return result.ToString().Trim();
        }

        private void Declare(StringBuilder builder, Proc proc)
        {
            Compile(builder, proc.ReturnType);
            builder
                .Append(' ')
                .Append(proc.Name)
                .Append('(');
            proc.Parameters.Intertwine(param => Compile(builder, param.Type), () => builder.Append(", "));
            builder.Append(");");
        }

        private void Compile(StringBuilder builder, Proc proc)
        {
            Compile(builder, proc.ReturnType);
            builder
                .Append(' ')
                .Append(proc.Name)
                .Append("(");
            proc.Parameters.Intertwine(param =>
            {
                Compile(builder, param.Type);
                builder.Append(' ');
                Compile(builder, param);
            },
            () => builder.Append(", "));
            builder.Append(") {\n");
            foreach (var bb in proc.BasicBlocks) Compile(builder, bb);
            builder.Append("}\n");
        }

        private void Compile(StringBuilder builder, BasicBlock basicBlock)
        {
            builder.Append(basicBlock.Name).Append(":\n");
            foreach (var ins in basicBlock.Instructions)
            {
                builder.Append("    ");
                Compile(builder, ins);
                builder.Append(";\n");
            }
        }

        private void Compile(StringBuilder builder, Instruction instruction)
        {
            switch (instruction)
            {
            case AllocInstruction alloc:
                // T rX_value;
                // T* rX = &rX_value;
                Compile(builder, alloc.ElementType);
                builder
                    .Append(" r")
                    .Append(alloc.Value.Index)
                    .Append("_value;\n")
                    .Append("    ");
                Compile(builder, alloc.Value.Type);
                builder
                    .Append(" r")
                    .Append(alloc.Value.Index)
                    .Append(" = &")
                    .Append(" r")
                    .Append(alloc.Value.Index)
                    .Append("_value");
                break;

            case RetInstruction ret:
                builder.Append("return");
                if (ret.Value != null)
                {
                    builder.Append(' ');
                    Compile(builder, ret.Value);
                }
                break;

            default: throw new NotImplementedException();
            }    
        }

        private void Compile(StringBuilder builder, IR.Value value)
        {
            switch (value)
            {
            case RegisterValue regVal:
                builder.Append('r').Append(regVal.Index);
                break;

            case IntValue intVal:
                builder.Append(intVal.Value);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void Compile(StringBuilder builder, IR.Type type)
        {
            switch (type)
            {
            case VoidType voidType: 
                builder.Append("void");
                break;

            case IntType intType:
                builder.Append($"int{intType.Bits}_t");
                break;

            case PtrType ptrType:
                Compile(builder, ptrType.ElementType);
                builder.Append('*');
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
