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
        /// Reference to a <see cref="Lir.Register"/>.
        /// </summary>
        public record Register(Lir.Register Value) : Value
        {
            public override Type Type => Value.Type;

            public override string ToString() => $"r{Value.Index}";
        }
    }
}
