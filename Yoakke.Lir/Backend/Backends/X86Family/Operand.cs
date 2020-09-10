using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// Represents a scaled index for X86 addressing.
    /// </summary>
    public record ScaledIndex
    {
        public readonly Register Index;
        public readonly int Scale;

        public ScaledIndex(Register index, int scale)
        {
            if (scale != 1 && scale != 2 && scale != 4 && scale != 8)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "The scale must be 1, 2, 4 or 8!");
            }
            Index = index;
            Scale = scale;
        }

        public void Deconstruct(out Register index, out int scale)
        {
            index = Index;
            scale = Scale;
        }
    }

    /// <summary>
    /// Operand description base for X86 instructions.
    /// </summary>
    public abstract record Operand : IX86Syntax
    {
        public abstract string ToIntelSyntax();

        /// <summary>
        /// Some literal constant.
        /// </summary>
        public record Literal(object Value) : Operand
        {
            public override string ToIntelSyntax() => Value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// A register access.
        /// </summary>
        public record Register_(Register Register) : Operand
        {
            public override string ToIntelSyntax() => Register.ToString().ToLower();
        }

        /// <summary>
        /// Addressing.
        /// </summary>
        public record Address : Operand
        {
            /// <summary>
            /// The base address register.
            /// </summary>
            public readonly Register? Base;
            /// <summary>
            /// A scaled offset.
            /// </summary>
            public readonly ScaledIndex? ScaledIndex;
            /// <summary>
            /// A displacement constant.
            /// </summary>
            public readonly int Displacement;

            public Address(Register? @base = null, ScaledIndex? scaledIndex = null, int displacement = 0)
            {
                Base = @base;
                ScaledIndex = scaledIndex;
                Displacement = displacement;
            }

            public Address(int displacement)
                : this(null, null, displacement)
            {
            }

            public Address(Register @base, int displacement)
                : this(@base, null, displacement)
            {
            }

            public Address(ScaledIndex scaledIndex, int displacement = 0)
                : this(null, scaledIndex, displacement)
            {
            }

            private static string R(Register? r) => 
                r == null ? string.Empty : r.Value.ToString().ToLower();
            public override string ToIntelSyntax() => (Base, ScaledIndex, Displacement)switch
            {
                (null      , null               , int d) => $"[{d}]",
                (Register b, null               , 0    ) => $"[{R(b)}]",
                (Register b, null               , int d) => $"[{R(b)} + {d}]",
                (null      , (Register i, int s), 0    ) => $"[{R(i)} * {s}]",
                (null      , (Register i, int s), int d) => $"[{R(i)} * {s} + {d}]",
                (Register b, (Register i, int s), 0    ) => $"[{R(b)} + {R(i)} * {s}]",
                (Register b, (Register i, int s), int d) => $"[{R(b)} + {R(i)} * {s} + {d}]",
            };
        }

        /// <summary>
        /// Indirect access through a memory address.
        /// </summary>
        public record Indirect(DataWidth Width, Address Address_) : Operand
        {
            public override string ToIntelSyntax() => $"{Width.ToString().ToUpper()} PTR {Address_.ToIntelSyntax()}";
        }
    }
}
