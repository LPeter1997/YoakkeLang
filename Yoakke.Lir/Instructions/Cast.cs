using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Instructions
{
    partial class Instr
    {
        /// <summary>
        /// Cast instruction.
        /// </summary>
        public class Cast : ValueInstr
        {
            /// <summary>
            /// The <see cref="Type"/> to cast to.
            /// </summary>
            public Type Target { get; set; }
            /// <summary>
            /// The <see cref="Value"/> to cast.
            /// </summary>
            public Value Value { get; set; }

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Result;
                    yield return Target;
                    yield return Value;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="Cast"/>.
            /// </summary>
            /// <param name="result">The <see cref="Register"/> to store the casted value at.</param>
            /// <param name="target"> The <see cref="Type"/> to cast to.</param>
            /// <param name="value">The <see cref="Value"/> to cast.</param>
            public Cast(Register result, Type target, Value value)
                : base(result)
            {
                Target = target;
                Value = value;
            }

            public override string ToString() => $"{Result} = cast {Target}, {Value.ToValueString()}";

            public override void Validate()
            {
                if (!Result.Type.Equals(Target))
                {
                    ThrowValidationException("The cast type must match the result storage type!");
                }
                if (Target is Type.Ptr && Value.Type is Type.Ptr)
                {
                    // Always allow that
                }
                else
                {
                    ThrowValidationException("No such cast!");
                }
            }
        }
    }
}
