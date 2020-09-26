using System.Collections.Generic;
using Yoakke.Lir.Types;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// An X86 assembly.
    /// </summary>
    public class X86Assembly
    {
        /// <summary>
        /// The list of externals this <see cref="X86Assembly"/> contains.
        /// </summary>
        public readonly IList<string> Externals = new List<string>();
        /// <summary>
        /// The list of constants this <see cref="X86Assembly"/> contains.
        /// </summary>
        public readonly IList<X86Const> Constants = new List<X86Const>();
        /// <summary>
        /// The list of globals this <see cref="X86Assembly"/> contains.
        /// </summary>
        public readonly IList<X86Global> Globals = new List<X86Global>();
        /// <summary>
        /// The list of procedures this <see cref="X86Assembly"/> contains.
        /// </summary>
        public readonly IList<X86Proc> Procedures = new List<X86Proc>();
    }

    /// <summary>
    /// An X86 constant.
    /// </summary>
    public class X86Const
    {
        /// <summary>
        /// The name of this <see cref="X86Const"/>.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The value of this constant in bytes.
        /// </summary>
        public readonly byte[] Bytes;
        /// <summary>
        /// The <see cref="Visibility"/> of this <see cref="X86Const"/>.
        /// </summary>
        public Visibility Visibility { get; set; }

        public X86Const(string name, byte[] bytes)
        {
            Name = name;
            Bytes = bytes;
        }
    }

    /// <summary>
    /// An X86 global mutable.
    /// </summary>
    public class X86Global
    {
        /// <summary>
        /// The name of this <see cref="X86Global"/>.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The size of this <see cref="X86Global"/> in bytes.
        /// </summary>
        public readonly int Size;
        /// <summary>
        /// The <see cref="Visibility"/> of this <see cref="X86Global"/>.
        /// </summary>
        public Visibility Visibility { get; set; }

        public X86Global(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }

    /// <summary>
    /// An X86 procedure. Essentially just a label, but some assemblers (like MASM) keep
    /// the concept of procedures.
    /// </summary>
    public class X86Proc : Operand
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

        // We print the name here
        public override string ToIntelSyntax(X86FormatOptions formatOptions) => Name;

        // NOTE: This is a pointer, requires a pointer size
        public override DataWidth GetWidth(SizeContext sizeContext) =>
            DataWidth.GetFromSize(sizeContext.PointerSize);
    }

    /// <summary>
    /// An X86 basic block, which is just label and instructions belonging to that label.
    /// </summary>
    public class X86BasicBlock : Operand
    {
        /// <summary>
        /// The procedure this <see cref="X86BasicBlock"/> belongs to.
        /// </summary>
        public readonly X86Proc Proc;
        /// <summary>
        /// The name of this <see cref="X86BasicBlock"/>.
        /// </summary>
        public readonly string? Name;
        /// <summary>
        /// The list of instructions this <see cref="X86BasicBlock"/> consists of.
        /// </summary>
        public readonly IList<X86Instr> Instructions = new List<X86Instr>();

        public X86BasicBlock(X86Proc proc, string? name = null)
        {
            Proc = proc;
            Name = name;
        }

        // We print the label name here
        public override string ToIntelSyntax(X86FormatOptions formatOptions)
        {
            if (Name == null) return string.Empty;
            return $"{Proc.Name}{formatOptions.SpecialSeparator}{Name}";
        }

        // NOTE: This is a pointer, requires a pointer size
        public override DataWidth GetWidth(SizeContext sizeContext) => 
            DataWidth.GetFromSize(sizeContext.PointerSize);
    }
}
