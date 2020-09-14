using System;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// The different registers on X86.
    /// </summary>
    public enum Register
    {
        // 8 bit
        Al, Ah, Cl, Ch, Dl, Dh, Bl, Bh, Spl, Bpl, Sil, Dil,
        // 16 bit
        Ax, Cx, Dx, Bx, Sp, Bp, Si, Di,
        // 32 bit
        Eax, Ecx, Edx, Ebx, Esp, Ebp, Esi, Edi,
        // 64 bit
        Rax, Rcx, Rdx, Rbx, Rsp, Rbp, Rsi, Rdi, R8, R9, R10, R11, R12, R13, R14, R15,
    }

    public static class RegisterExtensions
    {
        /// <summary>
        /// Gets the <see cref="DataWidth"/> for the given <see cref="Register"/>.
        /// </summary>
        /// <param name="register">The <see cref="Register"/> to get the <see cref="DataWidth"/> for.</param>
        /// <returns>The <see cref="DataWidth"/> of the <see cref="Register"/>.</returns>
        public static DataWidth GetWidth(this Register register) => (int)register switch
        {
            int n when n < 12 => DataWidth.Byte ,
            int n when n < 20 => DataWidth.Word ,
            int n when n < 28 => DataWidth.Dword,
            int n when n < 44 => DataWidth.Qword,
            _ => throw new NotImplementedException(),
        };
    }
}
