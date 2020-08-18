namespace Yoakke.Lir.Instructions
{
    /// <summary>
    /// Base for every instruction that yields some value in a <see cref="Register"/>.
    /// </summary>
    public abstract record ValueInstr : Instr
    {
        /// <summary>
        /// The <see cref="Register"/> storing the result of the instruction.
        /// </summary>
        public Register Result { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ValueInstr"/>.
        /// </summary>
        /// <param name="result">The <see cref="Register"/> storing the result of the instruction.</param>
        public ValueInstr(Register result)
        {
            Result = result;
        }
    }
}
