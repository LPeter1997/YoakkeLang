using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Utils;

namespace Yoakke.IR
{
    /// <summary>
    /// Functionality to dump IR code as text.
    /// </summary>
    class IrDump
    {
        /// <summary>
        /// Dumps the given IR <see cref="Assembly"/> as human-readable text.
        /// </summary>
        /// <param name="namingContext">The <see cref="NamingContext"/> of the <see cref="Assembly"/> to dump.</param>
        /// <returns>The string representation of the IR code.</returns>
        public static string Dump(NamingContext namingContext)
        {
            return new IrDump(namingContext).DumpAssembly();
        }

        private StringBuilder builder = new StringBuilder();
        private NamingContext namingContext;

        private IrDump(NamingContext namingContext) 
        {
            this.namingContext = namingContext;
        }

        private string DumpAssembly()
        {
            // Dump the externals
            foreach (var external in namingContext.Assembly.Externals)
            {
                DumpExternal(external.LinkName, external.Type);
                Write(";\n");
            }

            Write('\n');

            // Then the procedures
            foreach (var proc in namingContext.Assembly.Procedures)
            {
                namingContext.NewLocals();
                DumpProc(proc);
                Write('\n');
            }

            return builder.ToString().Trim();
        }

        private void DumpExternal(string name, Type type)
        {
            Write("extern ", type, $" {name}");
        }

        private void DumpProc(Proc proc)
        {
            Write("proc ", proc.ReturnType, $" {namingContext.GetProcName(proc)}(");
            // Parameters
            proc.Parameters.Intertwine(
                param => Write(param.Type, ' ', param),
                () => Write(", "));
            Write("):\n");

            foreach (var bb in proc.BasicBlocks) DumpBasicBlock(bb);
        }

        private void DumpBasicBlock(BasicBlock basicBlock)
        {
            Write($"{namingContext.GetBasicBlockName(basicBlock)}:\n");

            foreach (var ins in basicBlock.Instructions)
            {
                Write("  ", ins, '\n');
            }
        }

        private void DumpInstruction(Instruction instruction)
        {
            if (instruction is ValueInstruction value)
            {
                // If it's a call and is a void return, don't bother writing the assignee
                if (!(instruction is Instruction.Call call) || !Type.Void_.EqualsNonNull(call.Value.Type))
                {
                    Write(value.Value, " = ");
                }
            }

            switch (instruction)
            {
            case Instruction.Alloc alloc:
                Write("alloc ", alloc.ElementType);
                break;

            case Instruction.Ret ret:
                Write("ret");
                // Only write return value if it's non-void
                if (!Type.Void_.EqualsNonNull(ret.Value.Type)) Write(' ', ret.Value);
                break;

            case Instruction.Store store:
                Write("store ", store.Target, ", ", store.Value);
                break;

            case Instruction.Load load:
                Write("load ", load.Source);
                break;

            case Instruction.Call call:
                Write("call ", call.Proc, '(');
                call.Arguments.Intertwine(x => Write(x), () => Write(", "));
                Write(')');
                break;

            default: throw new NotImplementedException();
            }
        }

        private void DumpValue(Value value)
        {
            switch (value)
            {
            case Value.Void _:
                break;

            case Value.Register reg:
                Write(namingContext.GetRegisterName(reg));
                break;

            case Value.Int intVal:
                Write(intVal.Value);
                break;

            case Proc proc:
                Write(namingContext.GetProcName(proc));
                break;

            case Value.Extern external:
                Write(external.LinkName);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void DumpType(Type type)
        {
            switch (type)
            {
            case Type.Void _:
                Write("void");
                break;

            case Type.Int i:
                Write($"{(i.Signed ? 'i' : 'u')}{i.Bits}");
                break;

            case Type.Ptr ptrType:
                Write('*', ptrType.ElementType);
                break;

            case Type.Proc p:
                Write("proc(");
                p.Parameters.Intertwine(param => Write(param), () => Write(", "));
                Write(") -> ", p.ReturnType);
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
                    DumpType(t);
                    break;

                case Value v:
                    DumpValue(v);
                    break;

                case Instruction i:
                    DumpInstruction(i);
                    break;

                default:
                    builder.Append(arg);
                    break;
                }
            }
        }
    }
}
