using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Yoakke.IR
{
    /// <summary>
    /// Base for values during IR compilation.
    /// </summary>
    abstract class Value
    {
        /// <summary>
        /// The <see cref="Type"/> of this <see cref="Value"/>.
        /// </summary>
        public abstract Type Type { get; }
    }

    /// <summary>
    /// A <see cref="Value"/> that's being stored inside a register.
    /// </summary>
    class RegisterValue : Value
    {
        private Type type;
        public override Type Type => type;
        /// <summary>
        /// The index of the register.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Initializes a new <see cref="RegisterValue"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> the register needs to store.</param>
        /// <param name="index">The index of the register.</param>
        public RegisterValue(Type type, int index)
        {
            this.type = type;
            Index = index;
        }
    }

    /// <summary>
    /// An integral <see cref="Value"/>.
    /// </summary>
    class IntValue : Value
    {
        private IntType type;
        public override Type Type => type;
        /// <summary>
        /// The actual numberic value.
        /// </summary>
        public BigInteger Value { get; set; }

        /// <summary>
        /// Initializes a new <see cref="IntValue"/>.
        /// </summary>
        /// <param name="type">The <see cref="IntType"/> this integer value has.</param>
        /// <param name="value">The actual integer value itself.</param>
        public IntValue(IntType type, BigInteger value)
        {
            this.type = type;
            Value = value;
        }
    }
}
