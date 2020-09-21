using System.Collections.Generic;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Instructions
{
    partial class Instr
    {
        /// <summary>
        /// Stack allocation instruction.
        /// </summary>
        public class Alloc : ValueInstr
        {
            /// <summary>
            /// The allocated <see cref="Type"/>.
            /// </summary>
            public Type Allocated => ((Type.Ptr)Result.Type).Subtype;

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Result;
                    yield return Allocated;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="Alloc"/>.
            /// </summary>
            /// <param name="result">The <see cref="Register"/> to store the allocated address at.</param>
            public Alloc(Register result)
                : base(result)
            {
            }

            public override string ToString() => $"{Result} = alloc {Allocated}";

            public override void Validate()
            {
                if (!(Result.Type is Type.Ptr resultPtr))
                {
                    ThrowValidationException("Result type must be a pointer!");
                    return; // NOTE: Unnecessary
                }
                if (!resultPtr.Subtype.Equals(Allocated))
                {
                    ThrowValidationException("Type mismatch between the allocated type and the storage pointer's subtype!");
                }
            }
        }
    }
}
