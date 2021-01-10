using System.Collections.Generic;
using Yoakke.Lir.Status;

namespace Yoakke.Lir.Instructions
{
    partial class Instr
    {
        /// <summary>
        /// Unconditional jump instruction.
        /// </summary>
        public class Jmp : Instr
        {
            /// <summary>
            /// The <see cref="BasicBlock"/> to jump to.
            /// </summary>
            public BasicBlock Target { get; set; }

            public override bool IsBranch => true;

            public override IEnumerable<IInstrArg> InstrArgs
            {
                get
                {
                    yield return Target;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="Jmp"/>.
            /// </summary>
            /// <param name="target">The <see cref="BasicBlock"/> to jump to.</param>
            public Jmp(BasicBlock target)
            {
                Target = target;
            }

            public override string ToString() => $"jmp {Target.Name}";

            public override void Validate(ValidationContext context)
            {
                if (Target.Proc != BasicBlock.Proc)
                {
                    ReportValidationError(context, "Cross-procedure jump is illegal!");
                }
            }
        }
    }
}
