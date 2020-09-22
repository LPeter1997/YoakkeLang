using System;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// The different widths primitive data can be on X86.
    /// </summary>
    public abstract class DataWidth
    {
        // Types

        public sealed class Byte : DataWidth {}
        public sealed class Word : DataWidth {}
        public sealed class Dword : DataWidth {}
        public sealed class Qword : DataWidth {}

        // Values

        public static readonly DataWidth Byte_  = new Byte  { Index = 0, Repr = "byte" , Size = 1 };
        public static readonly DataWidth Word_  = new Word  { Index = 1, Repr = "word" , Size = 2 };
        public static readonly DataWidth Dword_ = new Dword { Index = 2, Repr = "dword", Size = 4 };
        public static readonly DataWidth Qword_ = new Qword { Index = 3, Repr = "qword", Size = 8 };

        /// <summary>
        /// The size of this <see cref="DataWidth"/> in bytes.
        /// </summary>
        public int Size { get; private set; }
        private int Index { get; set; }
        private string Repr { get; set; } = string.Empty;

        public override string ToString() => Repr;
        public static explicit operator int(DataWidth dw) => dw.Index;

        /// <summary>
        /// Gets the <see cref="DataWidth"/> for the given size.
        /// </summary>
        /// <param name="byteSize">The size in bytes.</param>
        /// <returns>The <see cref="DataWidth"/> for the size.</returns>
        public static DataWidth GetFromSize(int byteSize) => byteSize switch
        {
            1 => Byte_,
            2 => Word_,
            4 => Dword_,
            8 => Qword_,
            _ => throw new ArgumentException("Invalid data width size!", nameof(byteSize)),
        };
    }
}
