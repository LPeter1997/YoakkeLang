using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// The different operations an X86 instruction can perform.
    /// </summary>
    public enum Operation
    {
        Mov,
        Push,
        Pop,
        Ret,
        Call,
        Add,
        Sub,
    }

    /// <summary>
    /// A single X86 instruction.
    /// </summary>
    public record X86Instr(Operation Operation, params Operand[] Operands) : IX86Syntax
    {
        public string ToIntelSyntax() =>
            $"{Operation.ToString().ToLower()} {string.Join(", ", Operands.Select(o => o.ToIntelSyntax()))}";
    }
}
