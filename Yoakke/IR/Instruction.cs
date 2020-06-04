using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    /// <summary>
    /// Base for all IR instructions.
    /// </summary>
    abstract class Instruction
    {
    }

    /// <summary>
    /// Base for any <see cref="Instruction"/> that stores it's value inside a register.
    /// </summary>
    abstract class ValueInstruction : Instruction
    {
        /// <summary>
        /// The register this <see cref="Instruction"/> returns it's value in.
        /// </summary>
        public RegisterValue Value { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ValueInstruction"/>.
        /// </summary>
        /// <param name="value">The register that the <see cref="Instruction"/> returns it's value in.</param>
        public ValueInstruction(RegisterValue value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// An <see cref="Instruction"/> to allocate memory on the stack.
    /// </summary>
    class AllocInstruction : ValueInstruction
    {
        /// <summary>
        /// The <see cref="Type"/> to allocate space for.
        /// </summary>
        public Type ElementType { get; }

        /// <summary>
        /// Initializes a new <see cref="AllocInstruction"/>.
        /// </summary>
        /// <param name="value">The register that'll contain the pointer to the allocated space.</param>
        public AllocInstruction(RegisterValue value) 
            : base(value)
        {
            if (value.Type is PtrType ptr)
            {
                ElementType = ptr.ElementType;
            }
            else
            {
                throw new ArgumentException("Allocation requires a pointer register type!", nameof(value));
            }
        }
    }

    /// <summary>
    /// Return from the current porcedure.
    /// </summary>
    class RetInstruction : Instruction
    {
        /// <summary>
        /// The return <see cref="Value"/>, if there's any.
        /// </summary>
        public Value? Value { get; set; }

        /// <summary>
        /// Initializes a new <see cref="RetInstruction"/>.
        /// </summary>
        /// <param name="value">The <see cref="Value"/> to return.</param>
        public RetInstruction(Value? value = null)
        {
            Value = value;
        }
    }
}
