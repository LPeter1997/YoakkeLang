using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// The different operations an X86 instruction can perform.
    /// </summary>
    public enum X86Op
    {
        Mov,
        Push,
        Pop,
        Ret,
        Call,
        Add,
        Sub,
        Imul,
        Idiv,
        Test,
        Je,
        Jne,
        Jg,
        Jl,
        Jge,
        Jle,
        Jmp,
        Cmp,
        And,
        Or,
        Xor,
    }

    /// <summary>
    /// A single X86 instruction.
    /// </summary>
    public class X86Instr : IX86Syntax
    {
        public X86Op Operation { get; set; }
        public IList<Operand> Operands { get; set; }

        public X86Instr(X86Op op, params object[] operands)
        {
            Operation = op;
            Operands = operands.Select(ToOperand).ToList();
        }

        public string ToIntelSyntax() =>
            $"{Operation.ToString().ToLower()} {string.Join(", ", Operands.Select(o => o.ToIntelSyntax()))}";

        private static Operand ToOperand(object obj) => obj switch
        {
            Register r => new Operand.Register_(r),
            Operand o => o,
            _ => new Operand.Literal(obj),
        };
    }
}
