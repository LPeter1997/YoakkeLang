using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Runtime
{
    internal class StructValue : Value
    {
        public readonly IValueList<Value> Values;
        public override Type Type { get; }

        public StructValue(Type type, IValueList<Value> values)
        {
            Type = type;
            Values = values;
        }

        public override bool Equals(Value? other) =>
               other is StructValue s
            && Type.Equals(s.Type)
            && Values.Equals(s.Values);
        public override int GetHashCode() => HashCode.Combine(typeof(StructValue), Type, Values);

        public override Value Clone() =>
            new StructValue(Type, Values.Select(v => v.Clone()).ToList().AsValueList());

        public override string ToValueString() => $"{Type.ToTypeString()} " +
            $"{{ {string.Join(", ", Values.Select(v => v.ToValueString()))} }}";
    }
}
