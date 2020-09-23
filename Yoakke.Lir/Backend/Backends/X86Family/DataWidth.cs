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

        public static readonly DataWidth @byte = new Byte  { Index = 0, Repr = "byte" , Size = 1 };
        public static readonly DataWidth word  = new Word  { Index = 1, Repr = "word" , Size = 2 };
        public static readonly DataWidth dword = new Dword { Index = 2, Repr = "dword", Size = 4 };
        public static readonly DataWidth qword = new Qword { Index = 3, Repr = "qword", Size = 8 };

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
            1 => @byte,
            2 => word,
            4 => dword,
            8 => qword,
            _ => throw new ArgumentException("Invalid data width size!", nameof(byteSize)),
        };
    }
}
