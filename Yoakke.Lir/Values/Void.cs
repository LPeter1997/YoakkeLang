using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;

namespace Yoakke.Lir.Values
{
    partial record Value
    {
        /// <summary>
        /// Void value.
        /// </summary>
        public record Void : Value
        {
            public override Type Type => Type.Void_;

            /// <summary>
            /// Initializes a new <see cref="Void"/>.
            /// </summary>
            public Void()
            {
            }

            public override string ToValueString() => "void";
        }
    }
}
