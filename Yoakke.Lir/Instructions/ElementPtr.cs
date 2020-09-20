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
        // Wrapper type to be type safe
        public class Int : IInstrArg
        {
            public int Value { get; set; }
        }

        /// <summary>
        /// Subelement pointer calculation instruction.
        /// </summary>
        public class ElementPtr : ValueInstr
        {
            /// <summary>
            /// The <see cref="Value"/> to get the element pointer of.
            /// </summary>
            public Value Value { get; set; }
            /// <summary>
            /// The index of the element.
            /// </summary>
            public Int Index { get; set; }

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
            public ElementPtr(Register result, Value value, int index)
                : base(result)
            {
                Value = value;
                Index = new Int { Value = index };
            }

            public override string ToString() => $"{Result} = elementptr {Value}, {Index.Value}";
        }
    }
}
