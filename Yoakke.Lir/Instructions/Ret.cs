using System.Collections.Generic;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Instructions
{
    partial class Instr
    {
        /// <summary>
        /// Return instruction.
        /// </summary>
        public class Ret : Instr
        {
            /// <summary>
            /// The returned <see cref="Value"/>.
            /// </summary>
            public Value Value { get; set; }

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Value;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="Ret"/>.
            /// </summary>
            /// <param name="value">The returned <see cref="Value"/>.</param>
            public Ret(Value value)
            {
                Value = value;
            }

            public override string ToString() => $"ret {Value}";
        }
    }
}
