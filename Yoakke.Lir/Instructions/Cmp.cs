using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Instructions
{
    /// <summary>
    /// Comparison types.
    /// </summary>
    public abstract class Comparison : IInstrArg
    {
        // Types

        public class Eq : Comparison {}
        public class Ne : Comparison {}
        public class Gr : Comparison {}
        public class Le : Comparison {}
        public class GrEq : Comparison {}
        public class LeEq : Comparison {}

        // Values

        public static readonly Comparison eq    = new Eq   { Index = 0, Repr = "eq"    };
        public static readonly Comparison ne    = new Ne   { Index = 1, Repr = "ne"    };
        public static readonly Comparison gr    = new Gr   { Index = 2, Repr = "gr"    };
        public static readonly Comparison le    = new Le   { Index = 3, Repr = "le"    };
        public static readonly Comparison gr_eq = new GrEq { Index = 4, Repr = "gr_eq" };
        public static readonly Comparison le_eq = new LeEq { Index = 5, Repr = "le_eq" };

        private int Index { get; set; }
        private string Repr { get; set; } = string.Empty;

        public override string ToString() => Repr;
        public static explicit operator int(Comparison c) => c.Index;

        /// <summary>
        /// The inverse <see cref="Comparison"/> of this one.
        /// </summary>
        public Comparison Inverse => this switch
        {
            Eq => ne,
            Ne => eq,
            Gr => le_eq,
            Le => gr_eq,
            GrEq => le,
            LeEq => gr,
            _ => throw new NotImplementedException(),
        };
    }

    partial class Instr
    {
        /// <summary>
        /// Comparison instruction.
        /// </summary>
        public class Cmp : ValueInstr
        {
            /// <summary>
            /// The <see cref="Comparison"/> kind to do.
            /// </summary>
            public Comparison Comparison { get; set; }
            /// <summary>
            /// The left-hand side of the comparison.
            /// </summary>
            public Value Left { get; set; }
            /// <summary>
            /// The right-hand side of the comparison.
            /// </summary>
            public Value Right { get; set; }

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Result;
                    yield return Comparison;
                    yield return Left;
                    yield return Right;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="Cmp"/>.
            /// </summary>
            /// <param name="result">The <see cref="Register"/> to store the comparison result at.</param>
            /// <param name="comparison">The <see cref="Comparison"/> to perform.</param>
            /// <param name="left">The left-hand side operand.</param>
            /// <param name="right">The right-hand side operand.</param>
            public Cmp(Register result, Comparison comparison, Value left, Value right)
                : base(result)
            {
                Comparison = comparison;
                Left = left;
                Right = right;
            }

            public override string ToString() => $"{Result} = cmp {Comparison} {Left}, {Right}";

            public override void Validate()
            {
                if (!(Result.Type is Type.Int))
                {
                    ThrowValidationException("Result type of comparison must be an integer!");
                }
                if (!(Left.Type.Equals(Right.Type)))
                {
                    ThrowValidationException("Type mismatch between operands!");
                }
                if (!(Left.Type is Type.Int && Right.Type is Type.Int))
                {
                    ThrowValidationException("Unsupported comparison types!");
                }
            }
        }
    }
}
