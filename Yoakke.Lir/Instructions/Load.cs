using System.Collections.Generic;
using Yoakke.Lir.Status;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Instructions
{
    partial class Instr
    {
        /// <summary>
        /// Load value from an address instruction.
        /// </summary>
        public class Load : ValueInstr
        {
            /// <summary>
            /// The address to load the <see cref="Value"/> from.
            /// </summary>
            public Value Source { get; set; }

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Result;
                    yield return Source;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="Load"/>.
            /// </summary>
            /// <param name="result">The <see cref="Register"/> to store the loaded value at.</param>
            /// <param name="address">The address to load the <see cref="Value"/> from.</param>
            public Load(Register result, Value address)
                : base(result)
            {
                Source = address;
            }

            public override string ToString() => $"{Result} = load {Source.ToValueString()}";

            public override void Validate(ValidationContext context)
            {
                if (!(Source.Type is Type.Ptr ptrTy))
                {
                    ReportValidationError(context, "The source address must be a pointer type!");
                    return; // NOTE: Not needed
                }
                if (!Result.Type.Equals(ptrTy.Subtype))
                {
                    ReportValidationError(context, "The result typemust be equal to the source address subtype!");
                }
            }
        }
    }
}
