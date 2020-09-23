using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// A simple utility to allocate and free registers and keep track of them.
    /// </summary>
    public class RegisterPool
    {
        private readonly Register?[,] slots = new Register[Register.SlotCount, 2];

        private ref Register? Reg(Register reg) => ref slots[reg.Slot, reg.IsHighBytes ? 1 : 0];

        /// <summary>
        /// Checks if a given <see cref="Register"/> is free to use.
        /// </summary>
        /// <param name="reg">The <see cref="Register"/> to check.</param>
        /// <returns>True, if the <see cref="Register"/> is free to use.</returns>
        public bool IsFree(Register reg)
        {
            var s1reg = slots[reg.Slot, 0];
            var s2reg = slots[reg.Slot, 1];
            if (s1reg != null && s2reg != null) return false;
            if (s1reg != null) return !s1reg.IsOverlapping(reg);
            if (s2reg != null) return !s2reg.IsOverlapping(reg);
            return false;
        }

        /// <summary>
        /// Allocates the given <see cref="Register"/> for use.
        /// </summary>
        /// <param name="reg">The <see cref="Register"/> to allocate.</param>
        public void Allocate(Register reg)
        {
            if (!IsFree(reg)) throw new ArgumentException("The given register is already allocated!", nameof(reg));
            Reg(reg) = reg;
        }

        /// <summary>
        /// Same as <see cref="Allocate(Register)"/>, but for multiple <see cref="Register"/>s at once.
        /// </summary>
        public void Allocate(params Register[] regs)
        {
            foreach (var reg in regs) Allocate(reg);
        }

        /// <summary>
        /// Allocates a free <see cref="Register"/> with the given <see cref="DataWidth"/>.
        /// </summary>
        /// <param name="width">The required <see cref="DataWidth"/> of the <see cref="Register"/>.</param>
        /// <returns>The allocated <see cref="Register"/>.</returns>
        public Register Allocate(DataWidth width)
        {
            // We filter for allowed registers only (not sp, bp, si, di)
            foreach (var reg in Register.All(width).Where(r => r.Slot >= 4 && r.Slot < 8))
            {
                if (IsFree(reg))
                {
                    Allocate(reg);
                    return reg;
                }
            }
            throw new InvalidOperationException("No free register remaining to allocate!");
        }

        /// <summary>
        /// Frees the given <see cref="Register"/>.
        /// </summary>
        /// <param name="reg">The <see cref="Register"/> to free.</param>
        public void Free(Register reg)
        {
            Reg(reg) = null;
        }

        /// <summary>
        /// Same as <see cref="Free(Register)"/>, but for multiple <see cref="Register"/>s.
        /// </summary>
        public void Free(params Register[] regs)
        {
            foreach (var reg in regs) Free(reg);
        }

        /// <summary>
        /// Frees all <see cref="Register"/>s.
        /// </summary>
        public void FreeAll()
        {
            for (int i = 0; i < slots.GetLength(0); ++i)
            {
                for (int j = 0; j < slots.GetLength(1); ++j) slots[i, j] = null;
            }
        }
    }
}
