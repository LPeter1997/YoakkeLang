using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Types
{
    /// <summary>
    /// A utility class to calculate sizes and offsets.
    /// </summary>
    public class SizeContext
    {
        /// <summary>
        /// The size of pointers in bytes.
        /// </summary>
        public int PointerSize { get; set; }

        /// <summary>
        /// Calculates the size of a <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to calculate the size of.</param>
        /// <returns>The size in bytes.</returns>
        public int SizeOf(Type type) => type switch
        {
            Type.Void => 0,
            Type.Int i => NextPow2((i.Bits + 7) / 8),
            Type.Proc => PointerSize,
            Type.Ptr => PointerSize,
            Type.Struct s => s.Definition.Fields.Sum(SizeOf),
            Type.Array a => a.Size * SizeOf(a.Subtype),
            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Calculates the offset of a field in the given struct.
        /// </summary>
        /// <param name="structDef">The <see cref="StructDef"/> to calculate the offset in.</param>
        /// <param name="fieldIndex">The index of the field.</param>
        /// <returns>The offset in bytes.</returns>
        public int OffsetOf(StructDef structDef, int fieldIndex) =>
            structDef.Fields.Take(fieldIndex).Sum(SizeOf);

        private static int NextPow2(int n)
        {
            int result = 1;
            while (result < n) result = result << 1;
            return result;
        }
    }
}
