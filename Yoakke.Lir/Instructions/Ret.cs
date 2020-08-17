﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Instructions
{
    partial record Instr
    {
        /// <summary>
        /// Return instruction.
        /// </summary>
        public record Ret : Instr
        {
            /// <summary>
            /// The returned <see cref="Value"/>.
            /// </summary>
            public Value Value { get; set; }

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