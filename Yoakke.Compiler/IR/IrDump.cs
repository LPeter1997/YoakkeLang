using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.IR
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
        private StringBuilder typeDeclarations = new StringBuilder();
        private Dictionary<Type, string> compiledTypes = new Dictionary<Type, string>();
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
                Write(builder, ";\n");
            }

            Write(builder, '\n');

            // Then the procedures
            foreach (var proc in namingContext.Assembly.Procedures)
            {
                namingContext.NewLocals();
                DumpProc(proc);
                Write(builder, '\n');
            }

            return typeDeclarations.Append(builder).Append('\n').ToString().Trim();
        }

        private void DumpExternal(string name, Type type)
        {
            Write(builder, "extern ", type, $" {name}");
        }

        private void DumpProc(Proc proc)
        {
            Write(builder, "proc ", proc.ReturnType, $" {namingContext.GetProcName(proc)}(");
            // Parameters
            proc.Parameters.Intertwine(
                param => Write(builder, param.Type, ' ', param),
                () => Write(builder, ", "));
            Write(builder, "):\n");

            foreach (var bb in proc.BasicBlocks) DumpBasicBlock(bb);
        }

        private void DumpBasicBlock(BasicBlock basicBlock)
        {
            Write(builder, $"{namingContext.GetBasicBlockName(basicBlock)}:\n");

            foreach (var ins in basicBlock.Instructions)
            {
                Write(builder, "  ", ins, '\n');
            }
        }

        private void DumpInstruction(StringBuilder builder, Instruction instruction)
        {
            if (instruction is ValueInstruction value)
            {
                // Check if call value should be ignored
                if (!(value is Instruction.Call call && call.Value == Value.IgnoreRegister))
                {
                    Write(builder, value.Value, " = ");
                }
            }

            switch (instruction)
            {
            case Instruction.Alloc alloc:
                Write(builder, "alloc ", alloc.ElementType);
                break;

            case Instruction.Ret ret:
                Write(builder, "ret");
                // Only write return value if it's non-void
                if (!Type.Void_.EqualsNonNull(ret.Value.Type)) Write(builder, ' ', ret.Value);
                break;

            case Instruction.Store store:
                Write(builder, "store ", store.Target, ", ", store.Value);
                break;

            case Instruction.Load load:
                Write(builder, "load ", load.Source);
                break;

            case Instruction.Call call:
                Write(builder, "call ", call.Proc, '(');
                call.Arguments.Intertwine(x => Write(builder, x), () => Write(builder, ", "));
                Write(builder, ')');
                break;

            case Instruction.ElementPtr elementPtr:
                Write(builder, "elementptr ", elementPtr.Source, ", ", elementPtr.Index);
                break;

            case Instruction.Jump jump:
                Write(builder, "jump ", jump.Target);
                break;

            case Instruction.JumpIf jumpIf:
                Write(builder, "jumpif ", jumpIf.Condition, ", ", jumpIf.Then, ", ", jumpIf.Else);
                break;

            case Instruction.IAdd add:
                Write(builder, "iadd ", add.Type, ", ", add.Left, ", ", add.Right);
                break;

            case Instruction.IMul mul:
                Write(builder, "imul ", mul.Type, ", ", mul.Left, ", ", mul.Right);
                break;

            case Instruction.ILess less:
                Write(builder, "iless ", less.Type, ", ", less.Left, ", ", less.Right);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void DumpValue(StringBuilder builder, Value value)
        {
            switch (value)
            {
            case Value.Void _:
                Write(builder, "void");
                break;

            case Value.Register reg:
                Write(builder, namingContext.GetRegisterName(reg));
                break;

            case Value.Int intVal:
                Write(builder, intVal.Value);
                break;

            case Proc proc:
                Write(builder, namingContext.GetProcName(proc));
                break;

            case Value.Extern external:
                Write(builder, external.LinkName);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void DumpType(StringBuilder builder, Type type)
        {
            switch (type)
            {
            case Type.Void _:
                Write(builder, "void");
                break;

            case Type.Int i:
                Write(builder, $"{(i.Signed ? 'i' : 'u')}{i.Bits}");
                break;

            case Type.Ptr ptrType:
                Write(builder, '*', ptrType.ElementType);
                break;

            case Type.Proc p:
                Write(builder, "proc(");
                p.Parameters.Intertwine(param => Write(builder, param), () => Write(builder, ", "));
                Write(builder, ") -> ", p.ReturnType);
                break;

            case Type.Struct s:
            {
                if (!compiledTypes.TryGetValue(s, out var name))
                {
                    // First time seeing this type, add it to the declaration
                    // Allocate a name for it
                    name = namingContext.GetTypeName(s);
                    // Append it here
                    compiledTypes.Add(s, name);
                    // Add it to the declarations
                    Write(typeDeclarations, name, " = struct { ");
                    s.Fields.Intertwine(
                        t => Write(typeDeclarations, t),
                        () => Write(typeDeclarations, ", "));
                    Write(typeDeclarations, " }\n");
                }
                // Write it to the output
                Write(builder, name);
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
                    DumpType(builder, t);
                    break;

                case Value v:
                    DumpValue(builder, v);
                    break;

                case Instruction i:
                    DumpInstruction(builder, i);
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
