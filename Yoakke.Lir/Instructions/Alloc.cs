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
        }
    }
}
