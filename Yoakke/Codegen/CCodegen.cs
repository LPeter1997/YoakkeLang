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
        private StringBuilder typeDeclarations = new StringBuilder();
        private int typeCounter = 0;

        public string Compile(Assembly assembly)
        {
            return CompileAssembly(assembly);
        }

        private string CompileAssembly(Assembly assembly)
        {
            // We want the include to be on top
            Write(typeDeclarations, "#include <stdint.h>\n\n");
            foreach (var external in assembly.Externals)
            {
                DeclareExternal(external);
                Write(builder, '\n');
            }
            Write(builder, '\n');
            foreach (var proc in assembly.Procedures)
            {
                DeclareProc(proc);
                Write(builder, '\n');
            }
            Write(builder, '\n');
            foreach (var proc in assembly.Procedures)
            {
                CompileProc(proc);
                Write(builder, '\n');
            }
            typeDeclarations.Append('\n');
            return typeDeclarations.Append(builder).ToString().Trim();
        }

        private void DeclareExternal(Value.Extern external)
        {
            Write(builder, "extern ", external.Type, ' ', external.Name, ';');
        }

        private void DeclareProc(Proc proc)
        {
            Write(builder, proc.ReturnType, $" {proc.Name}(");
            proc.Parameters.Intertwine(param => Write(builder, param.Type), () => Write(builder, ", "));
            Write(builder, ");");
        }

        private void CompileProc(Proc proc)
        {
            Write(builder, proc.ReturnType, $" {proc.Name}(");
            proc.Parameters.Intertwine(
                param => Write(builder, param.Type, ' ', param),
                () => Write(builder, ", "));
            Write(builder, ") {\n");
            foreach (var bb in proc.BasicBlocks)
            {
                foreach (var ins in bb.Instructions) ForwardDeclareVariable(ins);
            }
            foreach (var bb in proc.BasicBlocks) CompileBasicBlock(bb);
            Write(builder, "}\n");
        }

        private void CompileBasicBlock(BasicBlock basicBlock)
        {
            Write(builder, basicBlock.Name, ":\n");
            foreach (var ins in basicBlock.Instructions)
            {
                Write(builder, "    ", ins, ";\n");
            }
        }

        private void ForwardDeclareVariable(Instruction instruction)
        {
            if (instruction is ValueInstruction vi)
            {
                Write(builder, "    ", vi.Value.Type, ' ', vi.Value, ";\n");
            }

            switch (instruction)
            {
            case Instruction.Ret _:
            case Instruction.Store _:
            case Instruction.Load _:
            case Instruction.Call _:
                break;

            case Instruction.Alloc alloc:
                // T rX_value;
                // T* rX = &rX_value;
                Write(builder, "    ", alloc.ElementType, ' ', alloc.Value, "_value;\n");
                break;

            default: throw new NotImplementedException();
            }
        }

        private void CompileInstruction(StringBuilder builder, Instruction instruction)
        {
            switch (instruction)
            {
            case Instruction.Alloc alloc:
                Write(builder, alloc.Value, " = &", alloc.Value, "_value");
                break;

            case Instruction.Ret ret:
                Write(builder, "return");
                if (ret.Value != null) Write(builder, ' ', ret.Value);
                break;

            case Instruction.Store store:
                Write(builder, '*', store.Target, " = ", store.Value);
                break;

            case Instruction.Load load:
                Write(builder, load.Value, " = *", load.Source);
                break;

            case Instruction.Call call:
                if (!Type.Void_.EqualsNonNull(call.Value.Type))
                {
                    Write(builder, call.Value, " = ");
                }
                Write(builder, call.Proc, '(');
                call.Arguments.Intertwine(x => Write(builder, x), () => Write(builder, ", "));
                Write(builder, ')');
                break;

            default: throw new NotImplementedException();
            }    
        }

        private void CompileValue(StringBuilder builder, Value value)
        {
            switch (value)
            {
            case Value.Register regVal:
                Write(builder, 'r', regVal.Index);
                break;

            case Value.Int intVal:
                Write(builder, intVal.Value);
                break;

            case Value.Proc procVal:
                Write(builder, procVal.Proc_.Name);
                break;

            case Value.Extern external:
                Write(builder, external.Name);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void CompileType(StringBuilder builder, Type type)
        {
            switch (type)
            {
            case Type.Void _: 
                Write(builder, "void");
                break;

            case Type.Int intType:
                if (!intType.Signed) Write(builder, 'u');
                Write(builder, $"int{intType.Bits}_t");
                break;

            case Type.Ptr ptrType:
                Write(builder, ptrType.ElementType, '*');
                break;

            case Type.Proc procType:
            {
                // We declare it and just refer to it
                // First declare return type and params, if any of them needs that
                var returnTypeBuilder = new StringBuilder();
                CompileType(returnTypeBuilder, procType.ReturnType);
                var paramTypesBuilder = new StringBuilder();
                procType.Parameters.Intertwine(
                    param => Write(paramTypesBuilder, param),
                    () => Write(paramTypesBuilder, ", "));
                // Now create our new type
                var typeName = $"func{typeCounter++}_t";
                Write(typeDeclarations, "typedef ", returnTypeBuilder, $"({typeName})(", paramTypesBuilder ,");\n");
                // Now write the built type's identifier to the correct place
                Write(builder, typeName);
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        private void Write(StringBuilder builder, params object[] args)
        {
            foreach (var arg in args)
            {
                switch (arg)
                {
                case Type t:
                    CompileType(builder, t);
                    break;

                case Value v:
                    CompileValue(builder, v);
                    break;

                case Instruction i:
                    CompileInstruction(builder, i);
                    break;

                default:
                    builder.Append(arg);
                    break;
                }
            }
        }
    }
}
