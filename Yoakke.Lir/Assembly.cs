using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Yoakke.Lir
{
    /// <summary>
    /// A compilation unit for the IR code.
    /// </summary>
    public class Assembly
    {
        /// <summary>
        /// The name of this <see cref="Assembly"/>.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The entry point of the <see cref="Assembly"/>.
        /// If null, the procedure named "main" will be chosen, or the singleton, if there's 
        /// only one procedure defined.
        /// </summary>
        public Proc? EntryPoint { get; set; }

        /// <summary>
        /// The <see cref="Extern"/>s the <see cref="Assembly"/> references.
        /// </summary>
        public readonly IList<Extern> Externals = new List<Extern>();
        /// <summary>
        /// The <see cref="Proc"/>s defined in this <see cref="Assembly"/>.
        /// </summary>
        public readonly IList<Proc> Procedures = new List<Proc>();

        /// <summary>
        /// All symbols this <see cref="Assembly"/> defines.
        /// </summary>
        public IEnumerable<ISymbol> Symbols => Externals.Cast<ISymbol>().Concat(Procedures);

        /// <summary>
        /// Returns all of the distinct external binary references in this <see cref="Assembly"/>.
        /// </summary>
        public IEnumerable<string> BinaryReferences =>
            Externals.Select(e => Path.GetFullPath(e.Path)).Distinct();

        /// <summary>
        /// Initializes a new <see cref="Assembly"/>.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        public Assembly(string name)
        {
            Name = name;
        }

        public override string ToString() => new StringBuilder()
            .AppendJoin('\n', Externals)
            .Append("\n\n")
            .AppendJoin("\n\n", Procedures)
            .ToString().Trim();
    }
}
