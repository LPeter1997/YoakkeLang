using System.Collections.Generic;

namespace Yoakke.Lir.Instructions
{
    /// <summary>
    /// Base for every instruction.
    /// </summary>
    public abstract partial class Instr : IValidate
    {
        /// <summary>
        /// The <see cref="BasicBlock"/> this <see cref="Instr"/> belongs to.
        /// </summary>
        public BasicBlock BasicBlock { get; set; } = BasicBlock.Null;
        /// <summary>
        /// True, if this instruction affects the control flow by jumping.
        /// </summary>
        public virtual bool IsBranch => false;
        /// <summary>
        /// The <see cref="IEnumerable{IInstrArg}"/> of all of the arguments of this instruction.
        /// </summary>
        public abstract IEnumerable<IInstrArg> InstrArgs { get; }

        public abstract override string ToString();

        public abstract void Validate();

        protected void ThrowValidationException(string message)
        {
            throw new ValidationException(this, message);
        }
    }
}
