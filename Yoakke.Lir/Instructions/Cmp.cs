using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;

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

        public static readonly Comparison Eq_   = new Eq   { Index = 0, Repr = "eq" };
        public static readonly Comparison Ne_   = new Ne   { Index = 1, Repr = "ne" };
        public static readonly Comparison Gr_   = new Gr   { Index = 2, Repr = "gr" };
        public static readonly Comparison Le_   = new Le   { Index = 3, Repr = "le" };
        public static readonly Comparison GrEq_ = new GrEq { Index = 4, Repr = "gr_eq" };
        public static readonly Comparison LeEq_ = new LeEq { Index = 5, Repr = "le_eq" };

        private int Index { get; set; }
        private string Repr { get; set; } = string.Empty;

        public override string ToString() => Repr;
        public static explicit operator int(Comparison c) => c.Index;
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
                if (!(Left.Type is Type.Int && Right.Type is Type.Int))
                {
                    ThrowValidationException("Unsupported comparison types!");
                }
            }
        }
    }
}
