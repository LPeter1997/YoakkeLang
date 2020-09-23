using System;
using System.Collections.Generic;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// The different registers on X86.
    /// </summary>
    public abstract class Register : Operand
    {
        // The 4 base registers for 8, 16 and 32 bits

        public abstract class Accum : Register { }
        public abstract class Counter : Register { }
        public abstract class Data : Register { }
        public abstract class Base : Register { }
        public abstract class StackPtr : Register { }
        public abstract class StackBasePtr : Register { }
        public abstract class Source : Register { }
        public abstract class Dest : Register { }

        // 8 bit registers

        public class Al : Accum { }
        public class Ah : Accum { }
        public class Cl : Counter { }
        public class Ch : Counter { }
        public class Dl : Data { }
        public class Dh : Data { }
        public class Bl : Base { }
        public class Bh : Base { }
        public class Spl : StackPtr { }
        public class Bpl : StackBasePtr { }
        public class Sil : Source { }
        public class Dil : Dest { }

        // 16 bit registers

        public class Ax : Accum { }
        public class Cx : Counter { }
        public class Dx : Data { }
        public class Bx : Base { }
        public class Sp : StackPtr { }
        public class Bp : StackBasePtr { }
        public class Si : Source { }
        public class Di : Dest { }

        // 32 bit registers

        public class Eax : Accum { }
        public class Ecx : Counter { }
        public class Edx : Data { }
        public class Ebx : Base { }
        public class Esp : StackPtr { }
        public class Ebp : StackBasePtr { }
        public class Esi : Source { }
        public class Edi : Dest { }

        // 64 bit registers

        public class Rax : Accum { }
        public class Rcx : Counter { }
        public class Rdx : Data { }
        public class Rbx : Base { }
        public class Rsp : StackPtr { }
        public class Rbp : StackBasePtr { }
        public class Rsi : Source { }
        public class Rdi : Dest { }
        public class R8 : Register { }
        public class R9 : Register { }
        public class R10 : Register { }
        public class R11 : Register { }
        public class R12 : Register { }
        public class R13 : Register { }
        public class R14 : Register { }
        public class R15 : Register { }

        // Values

        public static readonly Register al  = new Al  { Index = 0 , Slot = 0, Width = DataWidth.@byte, Repr = "al" };
        public static readonly Register ah  = new Ah  { Index = 1 , Slot = 0, Width = DataWidth.@byte, Repr = "ah", IsHighBytes = true };
        public static readonly Register cl  = new Cl  { Index = 2 , Slot = 1, Width = DataWidth.@byte, Repr = "cl" };
        public static readonly Register ch  = new Ch  { Index = 3 , Slot = 1, Width = DataWidth.@byte, Repr = "ch", IsHighBytes = true };
        public static readonly Register dl  = new Dl  { Index = 4 , Slot = 2, Width = DataWidth.@byte, Repr = "dl" };
        public static readonly Register dh  = new Dh  { Index = 5 , Slot = 2, Width = DataWidth.@byte, Repr = "dh", IsHighBytes = true };
        public static readonly Register bl  = new Bl  { Index = 6 , Slot = 3, Width = DataWidth.@byte, Repr = "bl" };
        public static readonly Register bh  = new Bh  { Index = 7 , Slot = 3, Width = DataWidth.@byte, Repr = "bh", IsHighBytes = true };
        public static readonly Register spl = new Spl { Index = 8 , Slot = 4, Width = DataWidth.@byte, Repr = "spl" };
        public static readonly Register bpl = new Bpl { Index = 9 , Slot = 5, Width = DataWidth.@byte, Repr = "bpl" };
        public static readonly Register sil = new Sil { Index = 10, Slot = 6, Width = DataWidth.@byte, Repr = "sil" };
        public static readonly Register dil = new Dil { Index = 11, Slot = 7, Width = DataWidth.@byte, Repr = "dil" };

        public static readonly Register ax = new Ax   { Index = 12, Slot = 0, Width = DataWidth.word, Repr = "ax" };
        public static readonly Register cx = new Cx   { Index = 13, Slot = 1, Width = DataWidth.word, Repr = "cx" };
        public static readonly Register dx = new Dx   { Index = 14, Slot = 2, Width = DataWidth.word, Repr = "dx" };
        public static readonly Register bx = new Bx   { Index = 15, Slot = 3, Width = DataWidth.word, Repr = "bx" };
        public static readonly Register sp = new Sp   { Index = 16, Slot = 4, Width = DataWidth.word, Repr = "sp" };
        public static readonly Register bp = new Bp   { Index = 17, Slot = 5, Width = DataWidth.word, Repr = "bp" };
        public static readonly Register si = new Si   { Index = 18, Slot = 6, Width = DataWidth.word, Repr = "si" };
        public static readonly Register di = new Di   { Index = 19, Slot = 7, Width = DataWidth.word, Repr = "di" };

        public static readonly Register eax = new Eax { Index = 20, Slot = 0, Width = DataWidth.dword, Repr = "eax" };
        public static readonly Register ecx = new Ecx { Index = 21, Slot = 1, Width = DataWidth.dword, Repr = "ecx" };
        public static readonly Register edx = new Edx { Index = 22, Slot = 2, Width = DataWidth.dword, Repr = "edx" };
        public static readonly Register ebx = new Ebx { Index = 23, Slot = 3, Width = DataWidth.dword, Repr = "ebx" };
        public static readonly Register esp = new Esp { Index = 24, Slot = 4, Width = DataWidth.dword, Repr = "esp" };
        public static readonly Register ebp = new Ebp { Index = 25, Slot = 5, Width = DataWidth.dword, Repr = "ebp" };
        public static readonly Register esi = new Esi { Index = 26, Slot = 6, Width = DataWidth.dword, Repr = "esi" };
        public static readonly Register edi = new Edi { Index = 27, Slot = 7, Width = DataWidth.dword, Repr = "edi" };

        public static readonly Register rax = new Rax { Index = 28, Slot = 0 , Width = DataWidth.qword, Repr = "rax" };
        public static readonly Register rcx = new Rcx { Index = 29, Slot = 1 , Width = DataWidth.qword, Repr = "rcx" };
        public static readonly Register rdx = new Rdx { Index = 30, Slot = 2 , Width = DataWidth.qword, Repr = "rdx" };
        public static readonly Register rbx = new Rbx { Index = 31, Slot = 3 , Width = DataWidth.qword, Repr = "rbx" };
        public static readonly Register rsp = new Rsp { Index = 32, Slot = 4 , Width = DataWidth.qword, Repr = "rsp" };
        public static readonly Register rbp = new Rbp { Index = 33, Slot = 5 , Width = DataWidth.qword, Repr = "rbp" };
        public static readonly Register rsi = new Rsi { Index = 34, Slot = 6 , Width = DataWidth.qword, Repr = "rsi" };
        public static readonly Register rdi = new Rdi { Index = 35, Slot = 7 , Width = DataWidth.qword, Repr = "rdi" };
        public static readonly Register r8  = new R8  { Index = 36, Slot = 8 , Width = DataWidth.qword, Repr = "r8"  };
        public static readonly Register r9  = new R9  { Index = 37, Slot = 9 , Width = DataWidth.qword, Repr = "r9"  };
        public static readonly Register r10 = new R10 { Index = 38, Slot = 10, Width = DataWidth.qword, Repr = "r10" };
        public static readonly Register r11 = new R11 { Index = 39, Slot = 11, Width = DataWidth.qword, Repr = "r11" };
        public static readonly Register r12 = new R12 { Index = 40, Slot = 12, Width = DataWidth.qword, Repr = "r12" };
        public static readonly Register r13 = new R13 { Index = 41, Slot = 13, Width = DataWidth.qword, Repr = "r13" };
        public static readonly Register r14 = new R14 { Index = 42, Slot = 14, Width = DataWidth.qword, Repr = "r14" };
        public static readonly Register r15 = new R15 { Index = 43, Slot = 15, Width = DataWidth.qword, Repr = "r15" };

        public static readonly int SlotCount = 16;

        public static readonly IReadOnlyList<Register> All8 = new Register[]
        {
            al, ah, cl, ch, dl, dh, bl, bh, spl, bpl, sil, dil,
        };
        public static readonly IReadOnlyList<Register> All16 = new Register[]
        {
            ax, cx, dx, bx, sp, bp, si, di,
        };
        public static readonly IReadOnlyList<Register> All32 = new Register[]
        {
            eax, ecx, edx, ebx, esp, ebp, esi, edi,
        };
        public static readonly IReadOnlyList<Register> All64 = new Register[]
        {
            rax, rcx, rdx, rbx, rsp, rbp, rsi, rdi, 
            r8, r9, r10, r11, r12, r13, r14, r15,
        };

        /// <summary>
        /// A general slot index.
        /// </summary>
        public int Slot { get; private set; }
        /// <summary>
        /// The <see cref="DataWidth"/> of this <see cref="Register"/>.
        /// </summary>
        public DataWidth Width { get; private set; } = DataWidth.@byte;
        /// <summary>
        /// True, if this <see cref="Register"/> is a high-bytes part of another.
        /// </summary>
        public bool IsHighBytes { get; private set; }
        private int Index { get; set; }
        private string Repr { get; set; } = string.Empty;

        public static explicit operator int(Register r) => r.Index;

        public override string ToIntelSyntax(X86FormatOptions formatOptions) =>
            formatOptions.AllUpperCase ? Repr.ToUpper() : Repr.ToLower();

        /// <summary>
        /// Retrieves a collection of <see cref="Register"/>s with a given <see cref="DataWidth"/>.
        /// </summary>
        /// <param name="width">The <see cref="DataWidth"/> of the <see cref="Register"/> wanted.</param>
        /// <returns>All the <see cref="Register"/>s with the given <see cref="DataWidth"/>.</returns>
        public static IReadOnlyList<Register> All(DataWidth width) => width switch
        {
            DataWidth.Byte => All8,
            DataWidth.Word => All16,
            DataWidth.Dword => All32,
            DataWidth.Qword => All64,
            _ => throw new NotImplementedException(),
        };
    }
}
