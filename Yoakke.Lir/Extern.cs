using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;

namespace Yoakke.Lir
{
    /// <summary>
    /// An external symbol.
    /// </summary>
    public class Extern
    {
        // TODO: Alignment
        /// <summary>
        /// The name of the external symbol.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The <see cref="Type"/> of the external symbol.
        /// </summary>
        public readonly Type Type;
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

        public override string ToString() =>
            $"extern {Type} {Name} [source = \"{Path}\"]";
    }
}
