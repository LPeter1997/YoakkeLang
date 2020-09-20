using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Runtime
{
    internal class ChunkValue : Value
    {
        public override Type Type => throw new NotImplementedException();

        public override Value Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(Value? other)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override string ToValueString()
        {
            throw new NotImplementedException();
        }
    }
}
