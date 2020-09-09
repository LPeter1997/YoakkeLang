using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public static class DataWidthExtensions
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
    }
}
