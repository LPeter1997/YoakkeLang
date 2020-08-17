using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Instructions
{
    /// <summary>
    /// Base for every instruction.
    /// </summary>
    public abstract partial record Instr 
    {
        /// <summary>
        /// True, if this instruction performs some kind of (conditional or unconditional)
        /// jump.
        /// </summary>
        public virtual bool IsJump => false;

        public abstract override string ToString();
    }
}
