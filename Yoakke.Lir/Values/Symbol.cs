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
        /// Reference to an <see cref="ISymbol"/>.
        /// </summary>
        public record Symbol : Value
        {
            /// <summary>
            /// The <see cref="ISymbol"/> value.
            /// </summary>
            public readonly ISymbol Value;

            public override Type Type => Value.Type;

            /// <summary>
            /// Initializes a new <see cref="Symbol"/>.
            /// </summary>
            /// <param name="value">The <see cref="ISymbol"/> value.</param>
            public Symbol(ISymbol value)
            {
                Value = value;
            }

            public override string ToString() => Value.Name;
        }
    }
}
