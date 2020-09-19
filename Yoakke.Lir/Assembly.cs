using System;
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
        public readonly IReadOnlyList<Extern> Externals;
        /// <summary>
        /// The <see cref="StructDef"/>s in this <see cref="Assembly"/>.
        /// </summary>
        public readonly IReadOnlyList<StructDef> Structs;
        /// <summary>
        /// The <see cref="Proc"/>s defined in this <see cref="Assembly"/>.
        /// </summary>
        public readonly IReadOnlyList<Proc> Procedures;

        /// <summary>
        /// All symbols this <see cref="Assembly"/> defines.
        /// </summary>
        public IEnumerable<ISymbol> Symbols => 
            // NOTE: Cast returned an ISymbol? for some reason
            Externals.Select(sym => (ISymbol)sym).Concat(Procedures);

        /// <summary>
        /// Returns all of the distinct external binary references in this <see cref="Assembly"/>.
        /// </summary>
        public IEnumerable<string> BinaryReferences =>
            Externals.Select(e => Path.GetFullPath(e.Path)).Distinct();

        internal Assembly(UncheckedAssembly uncheckedAssembly)
        {
            Name = uncheckedAssembly.Name;
            EntryPoint = uncheckedAssembly.EntryPoint;
            Externals = uncheckedAssembly.Externals.ToArray();
            Structs = uncheckedAssembly.Structs.ToArray();
            Procedures = uncheckedAssembly.Procedures.ToArray();
        }

        public override string ToString() => new StringBuilder()
            .AppendJoin('\n', Externals)
            .Append("\n\n")
            .AppendJoin('\n', Structs)
            .Append("\n\n")
            .AppendJoin("\n\n", Procedures)
            .Replace("\n\n\n", "\n\n")
            .ToString()
            .Trim();
    }
}
