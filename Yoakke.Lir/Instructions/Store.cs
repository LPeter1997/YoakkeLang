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
        /// Store a value at an address instruction.
        /// </summary>
        public class Store : Instr
        {
            /// <summary>
            /// The address to store the <see cref="Value"/> at.
            /// </summary>
            public Value Target { get; set; }
            /// <summary>
            /// The <see cref="Value"/> to store.
            /// </summary>
            public Value Value { get; set; }

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Target;
                    yield return Value;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="Store"/>.
            /// </summary>
            /// <param name="target">The address to store the value at.</param>
            /// <param name="value">The <see cref="Value"/> to store.</param>
            public Store(Value target, Value value)
            {
                Target = target;
                Value = value;
            }

            public override string ToString() => $"store {Target}, {Value}";
        }
    }
}
