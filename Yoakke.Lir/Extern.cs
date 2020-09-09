using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;

namespace Yoakke.Lir
{
    /// <summary>
    /// An external symbol.
    /// </summary>
    public record Extern : Value, ISymbol
    {
        public override Type Type { get; }
        public string Name { get; }
        public Visibility Visibility { get; set; } = Visibility.Public;

        /// <summary>
        /// The path of the binary the symbol originates from.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Initializes a new <see cref="Extern"/>.
        /// </summary>
        /// <param name="name">The name of the external symbol.</param>
        /// <param name="type">The <see cref="Type"/> of the external symbol.</param>
        /// <param name="path">The path of the binary the symbol originates from.</param>
        public Extern(string name, Type type, string path)
        {
            Name = name;
            Type = type;
            Path = path;
        }

        public override string ToValueString() => Name;

        public override string ToString() =>
            $"extern {Type} {Name} [source = \"{Path}\"]";
    }
}
