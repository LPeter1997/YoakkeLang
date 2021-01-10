using System.Collections.Generic;
using Yoakke.Lir.Status;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Instructions
{
    // TODO: Option to rotate or not?

    /// <summary>
    /// Represents a bit-shift instruction.
    /// </summary>
    public abstract class BitShiftInstr : ValueInstr
    {
        /// <summary>
        /// The shifted value.
        /// </summary>
        public Value Shifted { get; set; }
        /// <summary>
        /// The shift amount.
        /// </summary>
        public Value Amount { get; set; }
        /// <summary>
        /// The keyword of this instruction.
        /// </summary>
        protected abstract string Keyword { get; }

        public override IEnumerable<IInstrArg> InstrArgs
        {
            get
            {
                yield return Result;
                yield return Shifted;
                yield return Amount;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="BitShiftInstr"/>.
        /// </summary>
        /// <param name="result">The <see cref="Register"/> to store the result in.</param>
        /// <param name="shifted">The shifted value.</param>
        /// <param name="amount">The shift amount.</param>
        public BitShiftInstr(Register result, Value shifted, Value amount)
            : base(result)
        {
            Shifted = shifted;
            Amount = amount;
        }

        public override string ToString() => 
            $"{Result} = {Keyword} {Shifted.ToValueString()}, {Amount.ToValueString()}";

        public override void Validate(ValidationContext context)
        {
            if (!Result.Type.Equals(Shifted.Type))
            {
                ReportValidationError(context, "The shifted type must match the result storage type!");
            }
            if (!(Shifted.Type is Type.Int && Amount.Type is Type.Int))
            {
                ReportValidationError(context, "The shift operands must be integers!");
            }
            // TODO: Make sure shift amount is unsigned?
        }
    }

    partial class Instr
    {
        /// <summary>
        /// Shift left instruction.
        /// </summary>
        public class Shl : BitShiftInstr
        {
            public Shl(Register result, Value shifted, Value amount)
                : base(result, shifted, amount)
            {
            }

            protected override string Keyword => "shl";
        }

        /// <summary>
        /// Shift right instruction.
        /// </summary>
        public class Shr : BitShiftInstr
        {
            public Shr(Register result, Value shifted, Value amount)
                : base(result, shifted, amount)
            {
            }

            protected override string Keyword => "shr";
        }
    }
}
