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
        /// Reference to an <see cref="Extern"/> value.
        /// </summary>
        public record Extern : Value
        {
            /// <summary>
            /// The <see cref="Lir.Extern"/> value.
            /// </summary>
            public readonly Lir.Extern Value;

            public override Type Type => Value.Type;

            /// <summary>
            /// Initializes a new <see cref="Extern"/>.
            /// </summary>
            /// <param name="value">The <see cref="Lir.Extern"/> value.</param>
            public Extern(Lir.Extern value)
            {
                Value = value;
            }

            public override string ToString() => Value.Name;
        }
    }
}
