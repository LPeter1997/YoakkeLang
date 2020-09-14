using System.Numerics;

namespace Yoakke.Compiler.IR
{
    // Constants

    partial class Value
    {
        public static readonly Register IgnoreRegister = new Register(Type.Void_, -1);
        public static readonly Value Void_ = new Void();
    }

    /// <summary>
    /// Base for values during IR compilation.
    /// </summary>
    public abstract partial class Value
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
        /// A <see cref="Value"/> representing no-value.
        /// </summary>
        public class Void : Value
        {
            public override Type Type => Type.Void_;
        }

        /// <summary>
        /// A <see cref="Value"/> that's being stored inside a register.
        /// </summary>
        public class Register : Value
        {
            /// <summary>
            /// The index of the register.
            /// </summary>
            public int Index { get; set; }

            private Type type;
            public override Type Type => type;

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
            /// <summary>
            /// The actual numberic value.
            /// </summary>
            public BigInteger Value { get; set; }

            private Type.Int type;
            public override Type Type => type;

            /// <summary>
            /// Initializes a new <see cref="Int"/>.
            /// </summary>
            /// <param name="type">The <see cref="IntType"/> this integer value has.</param>
            /// <param name="value">The actual integer value itself.</param>
            public Int(Type type, BigInteger value)
            {
                this.type = (Type.Int)type;
                Value = value;
            }
        }

        /// <summary>
        /// An external symbol <see cref="Value"/>.
        /// </summary>
        public class Extern : Value
        {
            /// <summary>
            /// The link name of the external symbol.
            /// </summary>
            public readonly string LinkName;

            private Type type;
            public override Type Type => type;

            /// <summary>
            /// Initializes a new <see cref="Extern"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> of the external.</param>
            /// <param name="name">The name of the external.</param>
            public Extern(Type type, string name)
            {
                this.type = type;
                LinkName = name;
            }
        }
    }
}
