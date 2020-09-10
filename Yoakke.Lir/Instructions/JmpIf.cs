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
        /// Conditional jump instruction.
        /// </summary>
        public class JmpIf : Instr
        {
            /// <summary>
            /// The condition.
            /// </summary>
            public Value Condition { get; set; }
            /// <summary>
            /// The <see cref="BasicBlock"/> to jump to on a truthy condition.
            /// </summary>
            public BasicBlock Then { get; set; }
            /// <summary>
            /// The <see cref="BasicBlock"/> to jump to on a falsy condition.
            /// </summary>
            public BasicBlock Else { get; set; }

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Condition;
                    yield return Then;
                    yield return Else;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="JmpIf"/>.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="then">The <see cref="BasicBlock"/> to jump to on a truthy condition.</param>
            /// <param name="els">The <see cref="BasicBlock"/> to jump to on a falsy condition.</param>
            public JmpIf(Value condition, BasicBlock then, BasicBlock els)
            {
                Condition = condition;
                Then = then;
                Else = els;
            }

            public override string ToString() => 
                $"jmpif {Condition.ToValueString()}, {Then.Name}, {Else.Name}";
        }
    }
}
