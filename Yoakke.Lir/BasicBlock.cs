using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Instructions;

namespace Yoakke.Lir
{
    /// <summary>
    /// A basic block inside a <see cref="Proc"/>.
    /// A basic block is a continuous list of instructions that only has a single entry point
    /// being the first instruction, and a single exit point being the last instruction.
    /// </summary>
    public class BasicBlock
    {
        /// <summary>
        /// The name of the <see cref="BasicBlock"/>.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The list of instructions this block contains.
        /// </summary>
        public readonly IList<Instr> Instructions = new List<Instr>();

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
    }
}
