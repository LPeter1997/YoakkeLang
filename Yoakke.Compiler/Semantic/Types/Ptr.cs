using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            public readonly Type Subtype;

            /// <summary>
            /// Initializes a new <see cref="Ptr"/>.
            /// </summary>
            /// <param name="subtype">The subtype that the pointer points to.</param>
            public Ptr(Type subtype)
            {
                Subtype = subtype;
            }

            public override bool Equals(Type? other) =>
                   other is Ptr p
                && Subtype.Equals(p.Subtype);
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Ptr), Subtype);
            public override string ToString() => $"*{Subtype}";
        }
    }
}
