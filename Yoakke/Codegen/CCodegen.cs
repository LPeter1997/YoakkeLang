using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.IR;
using Yoakke.Utils;
using Type = Yoakke.IR.Type;

namespace Yoakke.Backend
{
    /// <summary>
    /// Code-generation for C.
    /// </summary>
    class CCodegen : ICodegen
    {
        private StringBuilder builder = new StringBuilder();

        public string Compile(Assembly assembly)
        {
            return CompileAssembly(assembly);
        }

        private string CompileAssembly(Assembly assembly)
        {
            Write("#include <stdint.h>\n\n");
            foreach (var proc in assembly.Procedures)
            {
                DeclareProc(proc);
                Write('\n');
            }
            Write('\n');
            foreach (var proc in assembly.Procedures)
            {
                CompileProc(proc);
                Write('\n');
            }
            return builder.ToString().Trim();
        }

        private void DeclareProc(Proc proc)
        {
            Write(proc.ReturnType, $" {proc.Name}(");
            proc.Parameters.Intertwine(param => Write(param.Type), () => Write(", "));
            Write(");");
        }

        private void CompileProc(Proc proc)
        {
            Write(proc.ReturnType, $" {proc.Name}(");
            proc.Parameters.Intertwine(
                param => Write(param.Type, ' ', param),
                () => Write(", "));
            Write(") {\n");
            foreach (var bb in proc.BasicBlocks)
            {
                foreach (var ins in bb.Instructions) ForwardDeclareVariable(ins);
            }
            foreach (var bb in proc.BasicBlocks) CompileBasicBlock(bb);
            Write("}\n");
        }

        private void CompileBasicBlock(BasicBlock basicBlock)
        {
            Write(basicBlock.Name, ":\n");
            foreach (var ins in basicBlock.Instructions)
            {
                Write("    ", ins, ";\n");
            }
        }

        private void ForwardDeclareVariable(Instruction instruction)
        {
            if (instruction is ValueInstruction vi)
            {
                Write("    ", vi.Value.Type, ' ', vi.Value, ";\n");
            }

            switch (instruction)
            {
            case Instruction.Ret _:
            case Instruction.Store _:
            case Instruction.Load _:
                break;

            case Instruction.Alloc alloc:
                // T rX_value;
                // T* rX = &rX_value;
                Write("    ", alloc.ElementType, ' ', alloc.Value, "_value;\n");
                break;

            default: throw new NotImplementedException();
            }
        }

        private void CompileInstruction(Instruction instruction)
        {
            switch (instruction)
            {
            case Instruction.Alloc alloc:
                Write(alloc.Value, " = &", alloc.Value, "_value");
                break;

            case Instruction.Ret ret:
                Write("return");
                if (ret.Value != null) Write(' ', ret.Value);
                break;

            case Instruction.Store store:
                Write('*', store.Target, " = ", store.Value);
                break;

            case Instruction.Load load:
                Write(load.Value, " = *", load.Source);
                break;

            default: throw new NotImplementedException();
            }    
        }

        private void CompileValue(Value value)
        {
            switch (value)
            {
            case Value.Register regVal:
                Write('r', regVal.Index);
                break;

            case Value.Int intVal:
                Write(intVal.Value);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void CompileType(Type type)
        {
            switch (type)
            {
            case Type.Void _: 
                Write("void");
                break;

            case Type.Int intType:
                if (!intType.Signed) Write('u');
                Write($"int{intType.Bits}_t");
                break;

            case Type.Ptr ptrType:
                Write(ptrType.ElementType, '*');
                break;

            default: throw new NotImplementedException();
            }
        }

        private void Write(params object[] args)
        {
            foreach (var arg in args)
            {
                switch (arg)
                {
                case Type t:
                    CompileType(t);
                    break;

                case Value v:
                    CompileValue(v);
                    break;

                case Instruction i:
                    CompileInstruction(i);
                    break;

                default:
                    builder.Append(arg);
                    break;
                }
            }
        }
    }
}
