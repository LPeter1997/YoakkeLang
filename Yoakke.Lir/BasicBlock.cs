﻿using System.Collections.Generic;
using System.Linq;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Status;

namespace Yoakke.Lir
{
    /// <summary>
    /// A basic block inside a <see cref="Proc"/>.
    /// A basic block is a continuous list of instructions that only has a single entry point
    /// being the first instruction, and a single exit point being the last instruction.
    /// </summary>
    public class BasicBlock : IInstrArg, IValidate
    {
        internal static readonly BasicBlock Null = new BasicBlock(" <null> ");

        /// <summary>
        /// The <see cref="Proc"/> this <see cref="BasicBlock"/> belongs to.
        /// </summary>
        public Proc Proc { get; set; } = Proc.Null;
        /// <summary>
        /// The name of the <see cref="BasicBlock"/>.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The list of instructions this block contains.
        /// </summary>
        public readonly IList<Instr> Instructions = new List<Instr>();

        /// <summary>
        /// True, if this <see cref="BasicBlock"/> ends in a branch instruction.
        /// </summary>
        public bool EndsInBranch => Instructions.Count > 0 && Instructions.Last().IsBranch;

        /// <summary>
        /// Initializes a new <see cref="BasicBlock>.
        /// </summary>
        /// <param name="name">The name of the basic block.</param>
        public BasicBlock(string name)
        {
            Name = name;
        }

        public override string ToString() => 
            $"label {Name}:\n{string.Join('\n', Instructions.Select(i => $"    {i}"))}";

        public void Validate(BuildStatus status)
        {
            // Check emptyness
            if (Instructions.Count == 0)
            {
                ReportValidationError(status, "A basic block cannot be empty!");
            }
            // Check basic block assumptions
            if (Instructions.SkipLast(1).Any(ins => ins.IsBranch))
            {
                ReportValidationError(status, "A basic block can only contain jump or return instructions at the end!");
            }
            if (!EndsInBranch)
            {
                ReportValidationError(status, "A basic block must end in a jump or return instruction!");
            }
            // Check instructions
            foreach (var ins in Instructions)
            {
                if (ins.BasicBlock != this)
                {
                    var err = new ValidationError(ins, "The instruction is not linked to it's containing basic block!");
                    status.Report(err);
                }
                ins.Validate(status);
            }
        }

        private void ReportValidationError(BuildStatus status, string message)
        {
            status.Report(new ValidationError(this, message));
        }
    }
}
