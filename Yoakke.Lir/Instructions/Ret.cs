﻿using System.Collections.Generic;
using Yoakke.Lir.Status;
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

            public override bool IsBranch => true;

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

            public override string ToString() => $"ret {Value.ToValueString()}";

            public override void Validate(ValidationContext context)
            {
                if (!BasicBlock.Proc.Return.Equals(Value.Type))
                {
                    ReportValidationError(context, "Return type mismatch!");
                }
            }
        }
    }
}
