using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// An X86 assembly.
    /// </summary>
    public class X86Assembly
    {
        /// <summary>
        /// The list of procedures this <see cref="X86Assembly"/> contains.
        /// </summary>
        public readonly IList<X86Proc> Procedures = new List<X86Proc>();
    }

    /// <summary>
    /// An X86 procedure. Essentially just a label, but some assemblers (like MASM) keep
    /// the concept of procedures.
    /// </summary>
    public class X86Proc
    {
        /// <summary>
        /// The name of this <see cref="X86Proc"/>.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The <see cref="Visibility"/> of this <see cref="X86Proc"/>.
        /// </summary>
        public Visibility Visibility { get; set; }
        /// <summary>
        /// The list of <see cref="X86BasicBlock"/> this <see cref="X86Proc"/> consists of.
        /// </summary>
        public readonly IList<X86BasicBlock> BasicBlocks = new List<X86BasicBlock>();

        public X86Proc(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// An X86 basic block, which is just label and instructions belonging to that label.
    /// </summary>
    public class X86BasicBlock
    {
        /// <summary>
        /// The name of this <see cref="X86BasicBlock"/>.
        /// </summary>
        public readonly string? Name;
        /// <summary>
        /// The list of instructions this <see cref="X86BasicBlock"/> consists of.
        /// </summary>
        public readonly IList<X86Instr> Instructions = new List<X86Instr>();

        public X86BasicBlock(string? name = null)
        {
            Name = name;
        }
    }
}
