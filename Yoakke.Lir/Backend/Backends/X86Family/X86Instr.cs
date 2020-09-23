using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

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
        Lea,
        Shl,
        Shr,
    }

    /// <summary>
    /// A single X86 instruction.
    /// </summary>
    public class X86Instr : IX86Syntax
    {
        public X86Op Operation { get; set; }
        public IList<Operand> Operands { get; set; }
        public string? Comment { get; set; }

        public X86Instr(X86Op op, params Operand[] operands)
        {
            Operation = op;
            Operands = operands.ToList();
        }

        public string ToIntelSyntax(X86FormatOptions formatOptions)
        {
            var instrName = Operation.ToString();
            instrName = formatOptions.AllUpperCase ? instrName.ToUpper() : instrName.ToLower();
            var instr = $"{instrName} {string.Join(", ", Operands.Select(o => o.ToIntelSyntax(formatOptions)))}";
            if (Comment == null) return instr;
            return formatOptions.CommentAbove ? $"; {Comment}\n    {instr}" : $"{instr} ; {Comment}";
        }
    }
}
