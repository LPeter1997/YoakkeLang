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

        public static readonly Register Al_  = new Al  { Index = 0 , Slot = 0, Width = DataWidth.Byte_, Repr = "al" };
        public static readonly Register Ah_  = new Ah  { Index = 1 , Slot = 0, Width = DataWidth.Byte_, Repr = "ah", IsHighBytes = true };
        public static readonly Register Cl_  = new Cl  { Index = 2 , Slot = 1, Width = DataWidth.Byte_, Repr = "cl" };
        public static readonly Register Ch_  = new Ch  { Index = 3 , Slot = 1, Width = DataWidth.Byte_, Repr = "ch", IsHighBytes = true };
        public static readonly Register Dl_  = new Dl  { Index = 4 , Slot = 2, Width = DataWidth.Byte_, Repr = "dl" };
        public static readonly Register Dh_  = new Dh  { Index = 5 , Slot = 2, Width = DataWidth.Byte_, Repr = "dh", IsHighBytes = true };
        public static readonly Register Bl_  = new Bl  { Index = 6 , Slot = 3, Width = DataWidth.Byte_, Repr = "bl" };
        public static readonly Register Bh_  = new Bh  { Index = 7 , Slot = 3, Width = DataWidth.Byte_, Repr = "bh", IsHighBytes = true };
        public static readonly Register Spl_ = new Spl { Index = 8 , Slot = 4, Width = DataWidth.Byte_, Repr = "spl" };
        public static readonly Register Bpl_ = new Bpl { Index = 9 , Slot = 5, Width = DataWidth.Byte_, Repr = "bpl" };
        public static readonly Register Sil_ = new Sil { Index = 10, Slot = 6, Width = DataWidth.Byte_, Repr = "sil" };
        public static readonly Register Dil_ = new Dil { Index = 11, Slot = 7, Width = DataWidth.Byte_, Repr = "dil" };

        public static readonly Register Ax_ = new Ax   { Index = 12, Slot = 0, Width = DataWidth.Word_, Repr = "ax" };
        public static readonly Register Cx_ = new Cx   { Index = 13, Slot = 1, Width = DataWidth.Word_, Repr = "cx" };
        public static readonly Register Dx_ = new Dx   { Index = 14, Slot = 2, Width = DataWidth.Word_, Repr = "dx" };
        public static readonly Register Bx_ = new Bx   { Index = 15, Slot = 3, Width = DataWidth.Word_, Repr = "bx" };
        public static readonly Register Sp_ = new Sp   { Index = 16, Slot = 4, Width = DataWidth.Word_, Repr = "sp" };
        public static readonly Register Bp_ = new Bp   { Index = 17, Slot = 5, Width = DataWidth.Word_, Repr = "bp" };
        public static readonly Register Si_ = new Si   { Index = 18, Slot = 6, Width = DataWidth.Word_, Repr = "si" };
        public static readonly Register Di_ = new Di   { Index = 19, Slot = 7, Width = DataWidth.Word_, Repr = "di" };

        public static readonly Register Eax_ = new Eax { Index = 20, Slot = 0, Width = DataWidth.Dword_, Repr = "eax" };
        public static readonly Register Ecx_ = new Ecx { Index = 21, Slot = 1, Width = DataWidth.Dword_, Repr = "ecx" };
        public static readonly Register Edx_ = new Edx { Index = 22, Slot = 2, Width = DataWidth.Dword_, Repr = "edx" };
        public static readonly Register Ebx_ = new Ebx { Index = 23, Slot = 3, Width = DataWidth.Dword_, Repr = "ebx" };
        public static readonly Register Esp_ = new Esp { Index = 24, Slot = 4, Width = DataWidth.Dword_, Repr = "esp" };
        public static readonly Register Ebp_ = new Ebp { Index = 25, Slot = 5, Width = DataWidth.Dword_, Repr = "ebp" };
        public static readonly Register Esi_ = new Esi { Index = 26, Slot = 6, Width = DataWidth.Dword_, Repr = "esi" };
        public static readonly Register Edi_ = new Edi { Index = 27, Slot = 7, Width = DataWidth.Dword_, Repr = "edi" };

        public static readonly Register Rax_ = new Rax { Index = 28, Slot = 0 , Width = DataWidth.Qword_, Repr = "rax" };
        public static readonly Register Rcx_ = new Rcx { Index = 29, Slot = 1 , Width = DataWidth.Qword_, Repr = "rcx" };
        public static readonly Register Rdx_ = new Rdx { Index = 30, Slot = 2 , Width = DataWidth.Qword_, Repr = "rdx" };
        public static readonly Register Rbx_ = new Rbx { Index = 31, Slot = 3 , Width = DataWidth.Qword_, Repr = "rbx" };
        public static readonly Register Rsp_ = new Rsp { Index = 32, Slot = 4 , Width = DataWidth.Qword_, Repr = "rsp" };
        public static readonly Register Rbp_ = new Rbp { Index = 33, Slot = 5 , Width = DataWidth.Qword_, Repr = "rbp" };
        public static readonly Register Rsi_ = new Rsi { Index = 34, Slot = 6 , Width = DataWidth.Qword_, Repr = "rsi" };
        public static readonly Register Rdi_ = new Rdi { Index = 35, Slot = 7 , Width = DataWidth.Qword_, Repr = "rdi" };
        public static readonly Register R8_  = new R8  { Index = 36, Slot = 8 , Width = DataWidth.Qword_, Repr = "r8" };
        public static readonly Register R9_  = new R9  { Index = 37, Slot = 9 , Width = DataWidth.Qword_, Repr = "r9" };
        public static readonly Register R10_ = new R10 { Index = 38, Slot = 10, Width = DataWidth.Qword_, Repr = "r10" };
        public static readonly Register R11_ = new R11 { Index = 39, Slot = 11, Width = DataWidth.Qword_, Repr = "r11" };
        public static readonly Register R12_ = new R12 { Index = 40, Slot = 12, Width = DataWidth.Qword_, Repr = "r12" };
        public static readonly Register R13_ = new R13 { Index = 41, Slot = 13, Width = DataWidth.Qword_, Repr = "r13" };
        public static readonly Register R14_ = new R14 { Index = 42, Slot = 14, Width = DataWidth.Qword_, Repr = "r14" };
        public static readonly Register R15_ = new R15 { Index = 43, Slot = 15, Width = DataWidth.Qword_, Repr = "r15" };

        public static readonly IReadOnlyList<Register> All8 = new Register[]
        {
            Al_, Ah_, Cl_, Ch_, Dl_, Dh_, Bl_, Bh_, Spl_, Bpl_, Sil_, Dil_,
        };
        public static readonly IReadOnlyList<Register> All16 = new Register[]
        {
            Ax_, Cx_, Dx_, Bx_, Sp_, Bp_, Si_, Di_,
        };
        public static readonly IReadOnlyList<Register> All32 = new Register[]
        {
            Eax_, Ecx_, Edx_, Ebx_, Esp_, Ebp_, Esi_, Edi_,
        };
        public static readonly IReadOnlyList<Register> All64 = new Register[]
        {
            Rax_, Rcx_, Rdx_, Rbx_, Rsp_, Rbp_, Rsi_, Rdi_, 
            R8_, R9_, R10_, R11_, R12_, R13_, R14_, R15_,
        };

        /// <summary>
        /// A general slot index.
        /// </summary>
        public int Slot { get; private set; }
        /// <summary>
        /// The <see cref="DataWidth"/> of this <see cref="Register"/>.
        /// </summary>
        public DataWidth Width { get; private set; } = DataWidth.Byte_;
        /// <summary>
        /// True, if this <see cref="Register"/> is a high-bytes part of another.
        /// </summary>
        public bool IsHighBytes { get; private set; }
        private int Index { get; set; }
        private string Repr { get; set; } = string.Empty;

        public static explicit operator int(Register r) => r.Index;

        public override string ToIntelSyntax(X86FormatOptions formatOptions) =>
            formatOptions.AllUpperCase ? Repr.ToUpper() : Repr.ToLower();
    }
}
