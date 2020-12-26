using System;
using System.Linq;
using Yoakke.DataStructures;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Values
{
    partial class Value
    {
        /// <summary>
        /// Array value.
        /// </summary>
        public class Array : Value
        {
            /// <summary>
            /// The list of <see cref="Value"/>s in the <see cref="Array"/>.
            /// </summary>
            public readonly IValueList<Value> Values;

            public override Type Type { get; }

            /// <summary>
            /// Initializes a new <see cref="Array"/>.
            /// </summary>
            /// <param name="type">The array <see cref="Type"/>.</param>
            /// <param name="values">The <see cref="Value"/>s of the array.</param>
            public Array(Type type, IValueList<Value> values)
            {
                Type = type;
                Values = values;
            }

            public override bool Equals(Value? other) =>
                   other is Array a
                && Type.Equals(a.Type)
                && Values.Equals(a.Values);
            public override int GetHashCode() => HashCode.Combine(typeof(Array), Type, Values);

            public override Value Clone() =>
                new Array(Type, Values.Select(v => v.Clone()).ToList().AsValueList());

            public override string ToValueString() => $"{Type.ToTypeString()} " +
                $"[{string.Join(", ", Values.Select(v => v.ToValueString()))}]";
        }
    }
}
