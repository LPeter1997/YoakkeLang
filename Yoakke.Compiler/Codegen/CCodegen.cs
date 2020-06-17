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
        private NamingContext namingContext = new NamingContext(new Assembly());
        private StringBuilder builder = new StringBuilder();
        private StringBuilder typeDeclarations = new StringBuilder();
        private Dictionary<Type, string> compiledTypes = new Dictionary<Type, string>();

        public string Compile(NamingContext namingContext)
        {
            builder.Clear();
            typeDeclarations.Clear();
            this.namingContext = namingContext;
            return CompileAssembly();
        }

        private string CompileAssembly()
        {
            // We want the include to be on top
            Write(typeDeclarations, "#include <stdint.h>\n#include <stdbool.h>\n\n");
            foreach (var external in namingContext.Assembly.Externals)
            {
                DeclareExternal(external);
                Write(builder, '\n');
            }
            Write(builder, '\n');
            foreach (var proc in namingContext.Assembly.Procedures)
            {
                DeclareProc(proc);
                Write(builder, '\n');
            }
            Write(builder, '\n');
            foreach (var proc in namingContext.Assembly.Procedures)
            {
                CompileProc(proc);
                Write(builder, '\n');
            }
            typeDeclarations.Append('\n');
            return typeDeclarations.Append(builder).ToString().Trim();
        }

        private void DeclareExternal(Value.Extern external)
        {
            Write(builder, "extern ", external.Type, ' ', external.LinkName, ';');
        }

        private void DeclareProc(Proc proc)
        {
            Write(builder, proc.ReturnType, $" {namingContext.GetProcName(proc)}(");
            proc.Parameters.Intertwine(param => Write(builder, param.Type), () => Write(builder, ", "));
            Write(builder, ");");
        }

        private void CompileProc(Proc proc)
        {
            Write(builder, proc.ReturnType, $" {namingContext.GetProcName(proc)}(");
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
            Write(builder, namingContext.GetBasicBlockName(basicBlock), ":\n");
            foreach (var ins in basicBlock.Instructions)
            {
                Write(builder, "    ", ins, ";\n");
            }
        }

        private void ForwardDeclareVariable(Instruction instruction)
        {
            if (instruction is ValueInstruction vi)
            {
                // We don't print void types
                if (Type.Void_.EqualsNonNull(vi.Value.Type)) return;

                Write(builder, "    ", vi.Value.Type, ' ', vi.Value, ";\n");
            }

            switch (instruction)
            {
            case Instruction.Ret _:
            case Instruction.Store _:
            case Instruction.Load _:
            case Instruction.Call _:
            case Instruction.ElementPtr _:
            case Instruction.Jump _:
            case Instruction.JumpIf _:
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
            // TODO: Ignore voids?
            switch (instruction)
            {
            case Instruction.Alloc alloc:
                Write(builder, alloc.Value, " = &", alloc.Value, "_value");
                break;

            case Instruction.Ret ret:
                Write(builder, "return");
                // Only write return value if it's non-void
                if (!Type.Void_.EqualsNonNull(ret.Value.Type)) Write(builder, ' ', ret.Value);
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

            case Instruction.ElementPtr elementPtr:
            {
                var srcPtrTy = (Type.Ptr)elementPtr.Source.Type;
                if (srcPtrTy.ElementType is Type.Struct)
                {
                    if (!(elementPtr.Index.Type is Type.Int))
                    {
                        throw new NotImplementedException("Can't access a non-constant index field of a struct!");
                    }
                    Write(builder, elementPtr.Value, " = &", elementPtr.Source, "->f", elementPtr.Index);
                }
                else
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }
            break;

            case Instruction.Jump jump:
                Write(builder, "goto ", jump.Target);
                break;

            case Instruction.JumpIf jumpIf:
                Write(builder, "if (", jumpIf.Condition, ") goto ", jumpIf.Then, "; else goto ", jumpIf.Else);
                break;

            default: throw new NotImplementedException();
            }    
        }

        private void CompileValue(StringBuilder builder, Value value)
        {
            switch (value)
            {
            case Value.Void _: 
                throw new InvalidOperationException();

            case Value.Register regVal:
                Write(builder, 'r', regVal.Index);
                break;

            case Value.Int intVal:
                Write(builder, intVal.Value);
                break;

            case Value.Extern external:
                Write(builder, external.LinkName);
                break;

            case Proc procVal:
                Write(builder, namingContext.GetProcName(procVal));
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
                if (intType.Bits == 1 && !intType.Signed)
                {
                    // Special case, boolean
                    Write(builder, "bool");
                }
                else
                {
                    if (!intType.Signed) Write(builder, 'u');
                    Write(builder, $"int{intType.Bits}_t");
                }
                break;

            case Type.Ptr ptrType:
                Write(builder, ptrType.ElementType, '*');
                break;

            case Type.Proc procType:
            {
                if (!compiledTypes.TryGetValue(procType, out var typeName))
                {
                    // We need to declare it
                    // Allocate a name for it
                    typeName = namingContext.GetTypeName(procType);
                    // Register it
                    compiledTypes.Add(procType, typeName);
                    // Declare return type and params
                    var returnTypeBuilder = new StringBuilder();
                    CompileType(returnTypeBuilder, procType.ReturnType);
                    var paramTypesBuilder = new StringBuilder();
                    procType.Parameters.Intertwine(
                        param => Write(paramTypesBuilder, param),
                        () => Write(paramTypesBuilder, ", "));
                    // Write out the typedef
                    Write(typeDeclarations, "typedef ", returnTypeBuilder, $"(*{typeName})(", paramTypesBuilder, ");\n");
                }
                Write(builder, typeName);
            }
            break;

            case Type.Struct structType:
            {
                if (!compiledTypes.TryGetValue(structType, out var typeName))
                {
                    // We need to declare it
                    // Allocate a name for it
                    typeName = namingContext.GetTypeName(structType);
                    // Register it
                    compiledTypes.Add(structType, typeName);
                    // Write out the typedef
                    var structTypeBuilder = new StringBuilder();
                    Write(structTypeBuilder, "typedef struct ", typeName, " {\n");
                    int idx = 0;
                    foreach (var f in structType.Fields) Write(structTypeBuilder, "    ", f, " f", idx++, ";\n");
                    Write(structTypeBuilder, "} ", typeName, ";\n");
                    // Write it out
                    Write(typeDeclarations, structTypeBuilder);
                }
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

                case BasicBlock bb:
                    builder.Append(namingContext.GetBasicBlockName(bb));
                    break;

                default:
                    builder.Append(arg);
                    break;
                }
            }
        }
    }
}
