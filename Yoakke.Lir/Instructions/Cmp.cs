using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Instructions
{
    /// <summary>
    /// Comparison types.
    /// </summary>
    public abstract class Comparison : IInstrArg
    {
        // Types

        public sealed class Eq : Comparison { protected override string Repr => "eq"; }
        public sealed class Neq : Comparison { protected override string Repr => "neq"; }
        public sealed class Gr : Comparison { protected override string Repr => "gr"; }
        public sealed class Le : Comparison { protected override string Repr => "le"; }
        public sealed class GrEq : Comparison { protected override string Repr => "gr_eq"; }
        public sealed class LeEq : Comparison { protected override string Repr => "le_eq"; }

        // Values

        public static readonly Comparison Eq_ = new Eq();
        public static readonly Comparison Neq_ = new Neq();
        public static readonly Comparison Gr_ = new Gr();
        public static readonly Comparison Le_ = new Le();
        public static readonly Comparison GrEq_ = new GrEq();
        public static readonly Comparison LeEq_ = new LeEq();

        protected abstract string Repr { get; }

        public override string ToString() => Repr;
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
        }
    }
}
