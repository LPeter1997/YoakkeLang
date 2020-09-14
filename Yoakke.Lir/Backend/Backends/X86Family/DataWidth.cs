using System;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// The different widths primitive data can be on X86.
    /// </summary>
    public enum DataWidth
    {
        Byte,
        Word,
        Dword,
        Qword,
    }

    public static class DataWidthUtils
    {
        /// <summary>
        /// Gets the size of the given <see cref="DataWidth"/>.
        /// </summary>
        /// <param name="dataWidth">The <see cref="DataWidth"/> to get the size of.</param>
        /// <returns>The size of the <see cref="DataWidth"/> in bytes.</returns>
        public static int GetByteSize(this DataWidth dataWidth) => dataWidth switch
        {
            DataWidth.Byte  => 1,
            DataWidth.Word  => 2,
            DataWidth.Dword => 4,
            DataWidth.Qword => 8,
            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Gets the <see cref="DataWidth"/> for the given size.
        /// </summary>
        /// <param name="byteSize">The size in bytes.</param>
        /// <returns>The <see cref="DataWidth"/> for the size.</returns>
        public static DataWidth FromByteSize(int byteSize) => byteSize switch
        {
            1 => DataWidth.Byte ,
            2 => DataWidth.Word ,
            4 => DataWidth.Dword,
            8 => DataWidth.Qword,
            _ => throw new ArgumentException("Invalid data width size!", nameof(byteSize)),
        };
    }
}
