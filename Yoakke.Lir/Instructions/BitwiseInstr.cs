using System.Collections.Generic;
using Yoakke.Lir.Status;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Instructions
{
    /// <summary>
    /// Represents some bitwise instruction.
    /// </summary>
    public abstract class BitwiseInstr : ValueInstr
    {
        /// <summary>
        /// The left-hand side of the bitwise operation.
        /// </summary>
        public Value Left { get; set; }
        /// <summary>
        /// The right-hand side of the bitwise operation.
        /// </summary>
        public Value Right { get; set; }
        /// <summary>
        /// The keyword of this instruction.
        /// </summary>
        protected abstract string Keyword { get; }

        public override IEnumerable<IInstrArg> InstrArgs
        {
            get
            {
                yield return Result;
                yield return Left;
                yield return Right;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="BitwiseInstr"/>.
        /// </summary>
        /// <param name="result">The <see cref="Register"/> to store the result in.</param>
        /// <param name="left">The left-hand side operand.</param>
        /// <param name="right">The right-hand side operand.</param>
        public BitwiseInstr(Register result, Value left, Value right)
            : base(result)
        {
            Left = left;
            Right = right;
        }

        public override string ToString() => 
            $"{Result} = {Keyword} {Left.ToValueString()}, {Right.ToValueString()}";

        public override void Validate(BuildStatus status)
        {
            if (!Left.Type.Equals(Right.Type))
            {
                ReportValidationError(status, "Operand type mismatch!");
            }
            if (!Result.Type.Equals(Left.Type))
            {
                ReportValidationError(status, "The bitwise result does not match the result storage type!");
            }
            if (Left.Type is Type.Int)
            {
                // Good
            }
            else
            {
                ReportValidationError(status, "Unsupported operand type!");
            }
        }
    }

    partial class Instr
    {
        /// <summary>
        /// Bitwise and instruction.
        /// </summary>
        public class BitAnd : BitwiseInstr
        {
            public BitAnd(Register result, Value left, Value right)
                : base(result, left, right)
            {
            }

            protected override string Keyword => "bitand";
        }

        /// <summary>
        /// Bitwise or instruction.
        /// </summary>
        public class BitOr : BitwiseInstr
        {
            public BitOr(Register result, Value left, Value right)
                : base(result, left, right)
            {
            }

            protected override string Keyword => "bitor";
        }

        /// <summary>
        /// Bitwise xor instruction.
        /// </summary>
        public class BitXor : BitwiseInstr
        {
            public BitXor(Register result, Value left, Value right)
                : base(result, left, right)
            {
            }

            protected override string Keyword => "bitxor";
        }
    }
}
