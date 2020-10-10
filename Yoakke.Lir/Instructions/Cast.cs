using System.Collections.Generic;
using Yoakke.Lir.Status;
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

            public override string ToString() => $"{Result} = cast {Target.ToTypeString()}, {Value.ToValueString()}";

            public override void Validate(BuildStatus status)
            {
                if (!Result.Type.Equals(Target))
                {
                    ReportValidationError(status, "The cast type must match the result storage type!");
                }
                if (
                    // Source type and target type match, basically a no-op
                       Target.Equals(Value.Type)
                    // Always allow pointer to pointer
                    || Target is Type.Ptr && Value.Type is Type.Ptr
                    // Target type is a user type, allow that for type transformations
                    || Target is Type.User)
                {
                }
                else
                {
                    ReportValidationError(status, "No such cast!");
                }
            }
        }
    }
}
