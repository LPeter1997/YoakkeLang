using System.Linq;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// The different operations an X86 instruction can perform.
    /// </summary>
    public enum X86Operation
    {
        Mov,
        Push,
        Pop,
        Ret,
        Call,
        Add,
        Sub,
        Test,
        Jne,
        Jmp,
    }

    /// <summary>
    /// A single X86 instruction.
    /// </summary>
    public record X86Instr(X86Operation Operation, params Operand[] Operands) : IX86Syntax
    {
        public X86Instr(X86Operation op, params object[] operands)
            : this(op, operands.Select(ToOperand).ToArray())
        {
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
