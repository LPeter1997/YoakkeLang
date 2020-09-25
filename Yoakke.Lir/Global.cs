using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// A global value definition.
    /// </summary>
    public class Global : Value, ISymbol
    {
        public override Type Type { get; }
        public string Name { get; }
        public Visibility Visibility { get; set; } = Visibility.Private;

        /// <summary>
        /// Initializes a new <see cref="Global"/>.
        /// </summary>
        /// <param name="name">The name of the external symbol.</param>
        /// <param name="type">The <see cref="Type"/> of the external symbol.</param>
        public Global(string name, Type type, string path)
        {
            Name = name;
            Type = type;
        }

        public override string ToValueString() => Name;
        public override string ToString() =>
            $"global {Type} {Name} [visibility = {Visibility}]";
        public override bool Equals(Value? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => HashCode.Combine(typeof(Global), Name);
        // NOTE: Makes no sense to clone this
        public override Value Clone() => this;

        public void Validate()
        {
            // No-op
        }
    }
}
