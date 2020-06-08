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
        /// <param name="assembly">The <see cref="Assembly"/> to dump.</param>
        /// <returns>The string representation of the IR code.</returns>
        public static string Dump(Assembly assembly)
        {
            return new IrDump().DumpAssembly(assembly);
        }

        private StringBuilder builder = new StringBuilder();

        private IrDump() { }

        private string DumpAssembly(Assembly assembly)
        {
            foreach (var external in assembly.Externals)
            {
                DumpExternalDeclaration(external.Name, external.Type);
                Write(";\n");
            }
            Write('\n');
            foreach (var proc in assembly.Procedures)
            {
                DumpProc(proc);
                Write('\n');
            }
            return builder.ToString().Trim();
        }

        private void DumpExternalDeclaration(string name, Type type)
        {
            Write("extern ", type, $" {name}");
        }

        private void DumpProc(Proc proc)
        {
            Write("proc ", proc.ReturnType, $" {proc.Name}(");
            // Parameters
            proc.Parameters.Intertwine(
                param => Write(param.Type, ' ', param),
                () => Write(", "));
            Write("):\n");

            foreach (var bb in proc.BasicBlocks) DumpBasicBlock(bb);
        }

        private void DumpBasicBlock(BasicBlock basicBlock)
        {
            Write($"{basicBlock.Name}:\n");

            foreach (var ins in basicBlock.Instructions)
            {
                Write("  ", ins, '\n');
            }
        }

        private void DumpInstruction(Instruction instruction)
        {
            if (instruction is ValueInstruction value)
            {
                Write(value.Value, " = ");
            }

            switch (instruction)
            {
            case Instruction.Alloc alloc:
                Write("alloc ", alloc.ElementType);
                break;

            case Instruction.Ret ret:
                Write("ret");
                if (ret.Value != null) Write(' ', ret.Value);
                break;

            case Instruction.Store store:
                Write("store ", store.Target, ", ", store.Value);
                break;

            case Instruction.Load load:
                Write("load ", load.Source);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void DumpValue(Value value)
        {
            switch (value)
            {
            case Value.Register reg:
                Write($"r{reg.Index}");
                break;

            case Value.Int intVal:
                Write(intVal.Value);
                break;
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
