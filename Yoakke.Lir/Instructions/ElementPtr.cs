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
        /// Subelement pointer calculation instruction.
        /// </summary>
        public class ElementPtr : ValueInstr
        {
            /// <summary>
            /// The <see cref="Value"/> to get the element pointer of.
            /// </summary>
            public Value Value { get; set; }
            // TODO: Maybe we should have a (int | Value) type here? Statically more valid.
            /// <summary>
            /// The index of the element.
            /// </summary>
            public Value Index { get; set; }

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Result;
                    yield return Value;
                    yield return Index;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="ElementPtr"/>.
            /// </summary>
            /// <param name="result">The <see cref="Register"/> to store the element pointer at.</param>
            /// <param name="value">The value to get the element pointer for.</param>
            /// <param name="index">The index of the element.</param>
            public ElementPtr(Register result, Value value, Value index)
                : base(result)
            {
                Value = value;
                Index = index;
            }

            public override string ToString() => $"{Result} = elementptr {Value}, {Index}";
        }
    }
}
