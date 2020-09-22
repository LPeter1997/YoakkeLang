using System;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    // TODO: We could make registers those fake enums too, like Comparison in the Cmp instruction.
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

    public static class RegisterUtils
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

        /// <summary>
        /// Retrieves a general index for this <see cref="Register"/>.
        /// </summary>
        /// <param name="register">The <see cref="Register"/> to retrieve the index for.</param>
        /// <param name="isHighBytes">Assigned true, if this <see cref="Register"/> was an iH 8 bit register,
        /// as that information is lost through the index.</param>
        /// <returns>The general index for the <see cref="Register"/>.</returns>
        public static int GetIndex(this Register register, out bool isHighBytes)
        {
            isHighBytes = false;
            int index = (int)register;
            if (index < 8)
            {
                // A, C, D or B low or high
                isHighBytes = index % 2 != 0;
                index = index / 2;
            }
            else if (index < 12)
            {
                // Remaining 8 bit register
                index = index - 4;
            }
            else if (index < 28)
            {
                // 16 or 32 bit register
                index = (index - 12) % 8;
            }
            else
            {
                // 64 bit
                index = index - 28;
            }
            return index;
        }

        /// <summary>
        /// <see cref="GetIndex(Register, out bool)"/>.
        /// </summary>
        public static int GetIndex(this Register register) => register.GetIndex(out bool _);

        private static readonly Register?[,] RegisterLUT = new Register?[16, 4]
            {
                { Register.Al , Register.Ax, Register.Eax, Register.Rax },
                { Register.Cl , Register.Cx, Register.Ecx, Register.Rcx },
                { Register.Dl , Register.Dx, Register.Edx, Register.Rdx },
                { Register.Bl , Register.Bx, Register.Ebx, Register.Rbx },
                { Register.Spl, Register.Sp, Register.Esp, Register.Rsp },
                { Register.Bpl, Register.Bp, Register.Ebp, Register.Rbp },
                { Register.Sil, Register.Si, Register.Esi, Register.Rsi },
                { Register.Dil, Register.Di, Register.Edi, Register.Rdi },
                { null        , null       , null        , Register.R8  },
                { null        , null       , null        , Register.R9  },
                { null        , null       , null        , Register.R10 },
                { null        , null       , null        , Register.R11 },
                { null        , null       , null        , Register.R12 },
                { null        , null       , null        , Register.R13 },
                { null        , null       , null        , Register.R14 },
                { null        , null       , null        , Register.R15 },
            };

        /// <summary>
        /// Gets a <see cref="Register"/> from an index and <see cref="DataWidth"/>.
        /// </summary>
        /// <param name="index">The general index that can be retrieved with <see cref="GetIndex(Register)"/>.</param>
        /// <param name="width">The desired <see cref="DataWidth"/>.</param>
        /// <returns>The <see cref="Register"/> at the given general index with the given width.</returns>
        public static Register FromIndexAndWidth(int index, DataWidth width) =>
            RegisterLUT[index, (int)width] ?? throw new InvalidOperationException();

        /// <summary>
        /// Transforms this <see cref="Register"/> to the equivalent one with the given <see cref="DataWidth"/>.
        /// </summary>
        /// <param name="register">The <see cref="Register"/> to transform.</param>
        /// <param name="width">The <see cref="DataWidth"/> to transform to.</param>
        /// <returns>The transformed eqivalent <see cref="Register"/> with the given <see cref="DataWidth"/>.</returns>
        public static Register ToWidth(this Register register, DataWidth width) =>
            FromIndexAndWidth(register.GetIndex(), width);
    }
}
