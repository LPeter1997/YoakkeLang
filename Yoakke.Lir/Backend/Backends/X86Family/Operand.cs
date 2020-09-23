using System;
using System.Net.Http.Headers;
using Yoakke.Lir.Types;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// Represents a scaled index for X86 addressing.
    /// </summary>
    public struct ScaledIndex
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
    public abstract class Operand : IX86Syntax
    {
        public abstract DataWidth GetWidth(SizeContext sizeContext);
        public abstract string ToIntelSyntax(X86FormatOptions formatOptions);

        /// <summary>
        /// A label to a procedure or basic block.
        /// </summary>
        public class Label : Operand
        {
            /// <summary>
            /// The procedure or basic block the label refers to.
            /// </summary>
            public readonly object Target;

            public Label(X86Proc proc)
            {
                Target = proc;
            }

            public Label(X86BasicBlock basicBlock)
            {
                Target = basicBlock;
            }

            public override DataWidth GetWidth(SizeContext sizeContext) => 
                DataWidth.GetFromSize(sizeContext.PointerSize);

            public override string ToIntelSyntax(X86FormatOptions formatOptions) => Target switch
            {
                X86BasicBlock bb => bb.GetLabelName(formatOptions),
                X86Proc p => p.Name,
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// Some literal constant.
        /// </summary>
        public class Literal : Operand
        {
            public readonly DataWidth Width;
            public readonly object Value;

            public Literal(DataWidth width, object value)
            {
                Width = width;
                Value = value;
            }

            public override DataWidth GetWidth(SizeContext sizeContext) => Width;

            public override string ToIntelSyntax(X86FormatOptions formatOptions) => 
                Value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Addressing.
        /// </summary>
        public class Address : Operand
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

            public override DataWidth GetWidth(SizeContext sizeContext) =>
                DataWidth.GetFromSize(sizeContext.PointerSize);

            private static string R(X86FormatOptions f, Register r)
            {
                var res = r.ToIntelSyntax(f);
                return f.AllUpperCase ? res.ToUpper() : res.ToLower();
            }
            private static string N(int n) => n > 0 ? $"+ {n}" : $"- {-n}";
            public override string ToIntelSyntax(X86FormatOptions f) => (Base, ScaledIndex, Displacement) switch
            {
                (null      , null               , int d) => $"[{d}]",
                (Register b, null               , 0    ) => $"[{R(f, b)}]",
                (Register b, null               , int d) => $"[{R(f, b)} {N(d)}]",
                (null      , (Register i, int s), 0    ) => $"[{R(f, i)} * {s}]",
                (null      , (Register i, int s), int d) => $"[{R(f, i)} * {s} {N(d)}]",
                (Register b, (Register i, int s), 0    ) => $"[{R(f, b)} + {R(f, i)} * {s}]",
                (Register b, (Register i, int s), int d) => $"[{R(f, b)} + {R(f, i)} * {s} {N(d)}]",
            };
        }

        /// <summary>
        /// Indirect access through a memory address.
        /// </summary>
        public class Indirect : Operand
        {
            public readonly DataWidth Width;
            public readonly Address Address_;

            public Indirect(DataWidth width, Address addr)
            {
                Width = width;
                Address_ = addr;
            }

            public override DataWidth GetWidth(SizeContext sizeContext) => Width;

            public override string ToIntelSyntax(X86FormatOptions formatOptions) => 
                // TODO: The PTR keyword is not necessarily correct!
                $"{Width.ToString().ToUpper()} PTR {Address_.ToIntelSyntax(formatOptions)}";
        }
    }
}
