using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;

namespace Yoakke.Compiler.Semantic.Types
{
    partial class Type
    {
        /// <summary>
        /// A pointer <see cref="Type"/>.
        /// </summary>
        public class Ptr : Type
        {
            /// <summary>
            /// The subtype that the pointer points to.
            /// </summary>
            public readonly Lazy<Type> Subtype;

            /// <summary>
            /// Initializes a new <see cref="Ptr"/>.
            /// </summary>
            /// <param name="subtype">The subtype that the pointer points to.</param>
            public Ptr(Lazy<Type> subtype)
            {
                Subtype = subtype;
            }

            public override bool Equals(Type? other) =>
                   other is Ptr p
                && Subtype.Value.Equals(p.Subtype.Value);
            public override int GetHashCode() =>
                System.HashCode.Combine(typeof(Ptr), Subtype);
            public override string ToString() => $"*{Subtype}";
        }
    }
}
