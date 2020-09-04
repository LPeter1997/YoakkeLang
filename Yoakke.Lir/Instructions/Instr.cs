using System.Collections.Generic;

namespace Yoakke.Lir.Instructions
{
    /// <summary>
    /// Base for every instruction.
    /// </summary>
    public abstract partial class Instr
    {
        /// <summary>
        /// True, if this instruction performs some kind of (conditional or unconditional)
        /// jump.
        /// </summary>
        public virtual bool IsJump => false;
        /// <summary>
        /// The <see cref="IEnumerable{IInstrArg}"/> of all of the arguments of this instruction.
        /// </summary>
        public abstract IEnumerable<IInstrArg> InstrArgs { get; }

        public abstract override string ToString();
    }
}
