using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;

namespace Yoakke.Lir.Runtime
{
    internal record PtrValue : Value
    {
        public Value? Value { get; set; }
        public int Offset { get; set; }

        public override Type Type { get; }

        public PtrValue(Type type)
        {
            Type = type;
        }

        // TODO
        public override string ToValueString() => "<some ptr>";
    }
}
