using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yoakke.IR
{
    /// <summary>
    /// Represents a translation unit for the IR code.
    /// </summary>
    class Assembly
    {
        /// <summary>
        /// The list of external symbols.
        /// </summary>
        public readonly List<Value.Extern> Externals = new List<Value.Extern>();
        /// <summary>
        /// The compiled <see cref="Proc"/>s.
        /// </summary>
        public readonly List<Proc> Procedures = new List<Proc>();
    }

    /// <summary>
    /// A single procedure compiled to IR.
    /// </summary>
    class Proc : Value
    {
        /// <summary>
        /// The name of the procedure.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The parameters of the procedure.
        /// </summary>
        public readonly List<Register> Parameters = new List<Register>();
        /// <summary>
        /// The parameter <see cref="Type"/>s of the procedure.
        /// </summary>
        public List<Type> ParameterTypes => type.Parameters;
        /// <summary>
        /// The return <see cref="Type"/> of the procedure.
        /// </summary>
        public Type ReturnType => type.ReturnType;
        /// <summary>
        /// The list of <see cref="BasicBlock"/>s this procedure consists of.
        /// </summary>
        public readonly List<BasicBlock> BasicBlocks = new List<BasicBlock>();

        private Type.Proc type;
        public override Type Type => type;

        /// <summary>
        /// Initializes a new <see cref="Proc"/>.
        /// </summary>
        /// <param name="name">The name of the procedure. Assumed to be globally unique.</param>
        /// <param name="parameters">The parameter <see cref="Value.Register"/>s of this procedure.</param>
        /// <param name="returnType">The <see cref="Type"/> the procedure returns with.</param>
        public Proc(string name, Type type)
        {
            this.type = (Type.Proc)type;
            Name = name;
        }
    }

    /// <summary>
    /// A single basic-block inside a <see cref="Proc"/>. A basic block is a sequence of instructions,
    /// with a single entry point being the first instruction, and a single exit point being the last instruction.
    /// </summary>
    class BasicBlock
    {
        /// <summary>
        /// The name of the <see cref="BasicBlock"/>.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The <see cref="Instruction"/>s contain within this block.
        /// </summary>
        public readonly List<Instruction> Instructions = new List<Instruction>();

        /// <summary>
        /// Initializes a new <see cref="BasicBlock"/>.
        /// </summary>
        /// <param name="name">The name of this <see cref="BasicBlock"/>. Assumed to be locally unique.</param>
        public BasicBlock(string name)
        {
            Name = name;
        }
    }
}
