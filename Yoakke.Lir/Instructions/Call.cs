using System.Collections.Generic;
using System.Linq;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Instructions
{

    partial class Instr
    {
        /// <summary>
        /// Call instruction.
        /// </summary>
        public class Call : ValueInstr
        {
            /// <summary>
            /// The procedure <see cref="Value"/> to call.
            /// </summary>
            public Value Procedure { get; set; }
            /// <summary>
            /// The call's argument list.
            /// </summary>
            public IList<Value> Arguments { get; set; }

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Result;
                    yield return Procedure;
                    foreach (var arg in Arguments) yield return arg;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="Call"/>.
            /// </summary>
            /// <param name="result">The result <see cref="Register"/> to store the results in.</param>
            /// <param name="procedure">The procedure <see cref="Value"/> to call.</param>
            /// <param name="arguments">The argument <see cref="Value"/>s to call the procedure with.</param>
            public Call(Register result, Value procedure, IList<Value> arguments) 
                : base(result)
            {
                // TODO: Type check?
                Procedure = procedure;
                Arguments = arguments;
            }

            public override string ToString() => 
                $"{Result} = call {Procedure.ToValueString()}" +
                $"({string.Join(", ", Arguments.Select(arg => arg.ToValueString()))})";
        }
    }
}
