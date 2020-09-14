using System.Collections.Generic;

namespace Yoakke.Compiler.IR
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
        /// The linking name to export this procedure with, if any.
        /// </summary>
        public string? LinkName { get; set; }
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
        /// <param name="type">The <see cref="Type"/> of the procedure.</param>
        public Proc(Type type)
        {
            this.type = (Type.Proc)type;
        }
    }

    /// <summary>
    /// A single basic-block inside a <see cref="Proc"/>. A basic block is a sequence of instructions,
    /// with a single entry point being the first instruction, and a single exit point being the last instruction.
    /// </summary>
    public class BasicBlock
    {
        /// <summary>
        /// The <see cref="Instruction"/>s contain within this block.
        /// </summary>
        public readonly List<Instruction> Instructions = new List<Instruction>();
    }
}
