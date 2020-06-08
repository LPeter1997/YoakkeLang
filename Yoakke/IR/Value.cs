﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Yoakke.IR
{
    /// <summary>
    /// Base for values during IR compilation.
    /// </summary>
    abstract partial class Value
    {
        /// <summary>
        /// The <see cref="Type"/> of this <see cref="Value"/>.
        /// </summary>
        public abstract Type Type { get; }
    }

    // Variants

    partial class Value
    {
        /// <summary>
        /// A <see cref="Value"/> that's being stored inside a register.
        /// </summary>
        public class Register : Value
        {
            private Type type;
            public override Type Type => type;
            /// <summary>
            /// The index of the register.
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Register"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> the register needs to store.</param>
            /// <param name="index">The index of the register.</param>
            public Register(Type type, int index)
            {
                this.type = type;
                Index = index;
            }
        }

        /// <summary>
        /// An integral <see cref="Value"/>.
        /// </summary>
        public class Int : Value
        {
            private Type.Int type;
            public override Type Type => type;
            /// <summary>
            /// The actual numberic value.
            /// </summary>
            public BigInteger Value { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Int"/>.
            /// </summary>
            /// <param name="type">The <see cref="IntType"/> this integer value has.</param>
            /// <param name="value">The actual integer value itself.</param>
            public Int(Type.Int type, BigInteger value)
            {
                this.type = type;
                Value = value;
            }
        }

        /// <summary>
        /// An external symbol <see cref="Value"/>.
        /// </summary>
        public class Extern : Value
        {
            private Type type;
            public override Type Type => type;
            /// <summary>
            /// The name of the external symbol.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// Initializes a new <see cref="Extern"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> of the external.</param>
            /// <param name="name">The name of the external.</param>
            public Extern(Type type, string name)
            {
                this.type = type;
                Name = name;
            }
        }

        /// <summary>
        /// A procedure <see cref="Value"/>.
        /// </summary>
        public class Proc : Value
        {
            private Type type;
            public override Type Type => type;
            /// <summary>
            /// The <see cref="IR.Proc"/> itself.
            /// </summary>
            public readonly IR.Proc Proc_;

            /// <summary>
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="proc">The <see cref="IR.Proc"/> to wrap as a value.</param>
            public Proc(IR.Proc proc)
            {
                Proc_ = proc;
                type = new Type.Proc(proc.Parameters.Select(x => x.Type).ToList(), proc.ReturnType);
            }
        }
    }
}
