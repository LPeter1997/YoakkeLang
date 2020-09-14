using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            public Value Address { get; set; }

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Result;
                    yield return Address;
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
                Address = address;
            }

            public override string ToString() => $"{Result} = load {Address}";
        }
    }
}
