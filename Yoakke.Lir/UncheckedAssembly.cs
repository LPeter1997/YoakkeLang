using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Types;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// An <see cref="Assembly"/> that's mutable and is not in a validated state.
    /// </summary>
    public class UncheckedAssembly
    {
        /// <summary>
        /// The name of this <see cref="Assembly"/>.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The <see cref="Extern"/>s this assembly references.
        /// </summary>
        public readonly IList<Extern> Externals = new List<Extern>();
        /// <summary>
        /// The <see cref="Proc"/>s defined in this assembly.
        /// </summary>
        public readonly IList<Proc> Procedures = new List<Proc>();

        /// <summary>
        /// The entry point of the assembly.
        /// If null, the procedure named "main" will be chosen, or the singleton, if there's 
        /// only one procedure defined.
        /// </summary>
        public Proc? EntryPoint { get; set; }

        /// <summary>
        /// Initializes a new <see cref="UncheckedAssembly"/>.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        public UncheckedAssembly(string name)
        {
            Name = name;
        }

        public Assembly Check()
        {
            foreach (var proc in Procedures) Check(proc);
            return new Assembly(this);
        }

        private void Check(Proc proc)
        {
            foreach (var bb in proc.BasicBlocks) Check(proc, bb);
        }

        private void Check(Proc proc, BasicBlock basicBlock)
        {
            if (basicBlock.Instructions.Count == 0)
            {
                // TODO: A basic block can't be empty!
                throw new InvalidOperationException();
            }
            if (basicBlock.Instructions.SkipLast(1).Any(ins => ins.IsBranch))
            {
                // TODO: A basic block can't contain a branch instruction apart from the last instruction
                throw new InvalidOperationException();
            }
            if (!basicBlock.Instructions.Last().IsBranch)
            {
                // TODO: A basic block must end in a branch
                throw new InvalidOperationException();
            }
            // Check instructions
            foreach (var ins in basicBlock.Instructions) Check(proc, ins);
        }

        private void Check(Proc proc, Instr instr)
        {
            switch (instr)
            {
            case Instr.Alloc alloc:
            {
                if (!(alloc.Result.Type is Type.Ptr resultPtr))
                {
                    // TODO: Result must be a pointer
                    throw new InvalidOperationException();
                }
                if (!resultPtr.Subtype.Equals(alloc.Allocated))
                {
                    // TODO: Type mismatch
                    throw new InvalidOperationException();
                }
            }
            break;

            case Instr.Call call:
            {
                if (!(call.Procedure.Type is Type.Proc procTy))
                {
                    // TODO: Must have procedure type
                    throw new InvalidOperationException();
                }
                if (!call.Result.Type.Equals(procTy.Return))
                {
                    // TODO: Return type must match with storage
                    throw new InvalidOperationException();
                }
                if (!procTy.Parameters.SequenceEqual(call.Arguments.Select(arg => arg.Type)))
                {
                    // TODO: Argument types must match
                    throw new InvalidOperationException();
                }
            }
            break;

            case Instr.Cmp cmp:
            {
                if (!(cmp.Result.Type is Type.Int))
                {
                    // TODO: Result must be an int
                    throw new InvalidOperationException();
                }
                if (!(cmp.Left.Type is Type.Int && cmp.Right.Type is Type.Int))
                {
                    // TODO: Unsupported types
                    throw new InvalidOperationException();
                }
            }
            break;

            case Instr.Jmp jmp:
            {
                if (!proc.BasicBlocks.Contains(jmp.Target))
                {
                    // TODO: Can't jump between procedures
                    throw new InvalidOperationException();
                }
            }
            break;

            case Instr.JmpIf jmpIf:
            {
                if (!(jmpIf.Condition.Type is Type.Int))
                {
                    // TODO: Condition must be an integer
                    throw new InvalidOperationException();
                }
                if (!proc.BasicBlocks.Contains(jmpIf.Then) || !proc.BasicBlocks.Contains(jmpIf.Else))
                {
                    // TODO: Can't jump between procedures
                    throw new InvalidOperationException();
                }
            }
            break;

            case Instr.Load load:
            {
                if (!(load.Address.Type is Type.Ptr srcPtr))
                {
                    // TODO: Source address must be pointer
                    throw new InvalidOperationException();
                }
                if (!load.Result.Type.Equals(srcPtr.Subtype))
                {
                    // TODO: Result must hold dereferenced ptr type
                    throw new InvalidOperationException();
                }
            }
            break;

            case Instr.Ret ret:
            {
                if (!proc.Return.Equals(ret.Value.Type))
                {
                    // TODO: Return type mismatch
                    throw new InvalidOperationException();
                }
            }
            break;

            case Instr.Store store:
            {
                if (!(store.Target.Type is Type.Ptr targetPtr))
                {
                    // TODO: Target address must be pointer
                    throw new InvalidOperationException();
                }
                if (!store.Value.Type.Equals(targetPtr.Subtype))
                {
                    // TODO: Value must be pointer subtype
                    throw new InvalidOperationException();
                }
            }
            break;

            default: throw new NotImplementedException();
            }
        }        
    }
}
