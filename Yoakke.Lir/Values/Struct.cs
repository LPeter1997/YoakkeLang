using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Lir.Types;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Values
{
    partial class Value
    {
        /// <summary>
        /// Struct value.
        /// </summary>
        public class Struct : Value
        {
            /// <summary>
            /// The <see cref="Value"/>s the <see cref="Struct"/> consists of.
            /// </summary>
            public readonly IValueList<Value> Values;
         
            public override Type Type { get; }

            /// <summary>
            /// Initializes a new <see cref="Struct"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> os the struct.</param>
            /// <param name="values">The list of <see cref="Value"/>s.</param>
            public Struct(Type type, IValueList<Value> values)
            {
                Type = type;
                Values = values;
            }

            public override bool Equals(Value? other) =>
                   other is Struct s
                && Type.Equals(s.Type)
                && Values.Equals(s.Values);
            public override int GetHashCode() => HashCode.Combine(typeof(Struct), Type, Values);

            public override Value Clone() =>
                new Struct(Type, Values.Select(v => v.Clone()).ToList().AsValueList());

            public override string ToValueString() => $"{Type.ToTypeString()} " +
                $"{{ {string.Join(", ", Values.Select(v => v.ToValueString()))} }}";
        }
    }
}
