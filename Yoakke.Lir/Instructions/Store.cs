using System.Collections.Generic;
using Yoakke.Lir.Status;
using Yoakke.Lir.Types;
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

            public override string ToString() => $"store {Target.ToValueString()}, {Value.ToValueString()}";

            public override void Validate(BuildStatus status)
            {
                if (!(Target.Type is Type.Ptr targetPtr))
                {
                    ReportValidationError(status, "Target address must be of a pointer type!");
                    return; // NOTE: Not needed
                }
                if (!Value.Type.Equals(targetPtr.Subtype))
                {
                    ReportValidationError(status, "The stored value must match the source pointer's subtype!");
                }
            }
        }
    }
}
