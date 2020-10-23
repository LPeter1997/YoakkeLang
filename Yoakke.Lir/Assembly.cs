using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Type = Yoakke.Lir.Types.Type;

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
        public Proc EntryPoint 
        { 
            get
            {
                // If it's explicitly given, use that
                if (entryPoint is not null) return entryPoint;
                // Search from the public ones
                var publicProcs = Procedures.Where(p => p.Visibility == Visibility.Public);
                // Try one named main
                var main = publicProcs.FirstOrDefault(p => p.Name == "main");
                if (main is not null) return main;
                // Try the first one
                var first = publicProcs.FirstOrDefault();
                if (first is not null) return first;

                throw new InvalidOperationException("No procedure is suitable as an entry point!");
            }
            set => entryPoint = value; 
        }
        private Proc? entryPoint;

        /// <summary>
        /// The prelude procedure of the <see cref="Assembly"/>.
        /// If not null, this will be ran before the main code to perform global initializations and such.
        /// </summary>
        public readonly Proc? Prelude;

        /// <summary>
        /// The <see cref="Extern"/>s the <see cref="Assembly"/> references.
        /// </summary>
        public readonly IReadOnlyList<Extern> Externals;
        /// <summary>
        /// The <see cref="Const"/>s the <see cref="Assembly"/> defines.
        /// </summary>
        public readonly IReadOnlyList<Const> Constants;
        /// <summary>
        /// The <see cref="Global"/>s the <see cref="Assembly"/> defines.
        /// </summary>
        public readonly IReadOnlyList<Global> Globals;
        /// <summary>
        /// The <see cref="Struct"/>s in this <see cref="Assembly"/>.
        /// </summary>
        public readonly IReadOnlyList<Struct> Structs;
        /// <summary>
        /// The <see cref="Proc"/>s defined in this <see cref="Assembly"/>.
        /// </summary>
        public readonly IReadOnlyList<Proc> Procedures;

        /// <summary>
        /// All symbols this <see cref="Assembly"/> defines.
        /// </summary>
        public IEnumerable<ISymbol> Symbols => 
            // NOTE: Cast returned an ISymbol? for some reason
            Externals
                .Select(sym => (ISymbol)sym)
                .Concat(Constants)
                .Concat(Globals)
                .Concat(Procedures);

        /// <summary>
        /// Returns all of the distinct external binary references in this <see cref="Assembly"/>.
        /// </summary>
        public IEnumerable<string> BinaryReferences =>
            // TODO: They should be distinct by absolute path?
            Externals.Select(e => e.Path).Distinct();

        internal Assembly(UncheckedAssembly uncheckedAssembly)
        {
            Name = uncheckedAssembly.Name;
            entryPoint = uncheckedAssembly.EntryPoint;
            Prelude = uncheckedAssembly.Prelude;
            Externals = uncheckedAssembly.Externals.ToArray();
            Constants = uncheckedAssembly.Constants.ToArray();
            Globals = uncheckedAssembly.Globals.ToArray();
            Structs = uncheckedAssembly.Structs.ToArray();
            Procedures = uncheckedAssembly.Procedures.ToArray();
        }

        public override string ToString() => new StringBuilder()
            .AppendJoin('\n', Externals)
            .Append("\n\n")
            .AppendJoin('\n', Constants)
            .Append("\n\n")
            .AppendJoin('\n', Globals)
            .Append("\n\n")
            .AppendJoin('\n', Structs)
            .Append("\n\n")
            .AppendJoin("\n\n", Procedures)
            .Replace("\n\n\n", "\n\n")
            .ToString()
            .Trim();

        /// <summary>
        /// Checks, if this <see cref="Assembly"/> contains <see cref="Value.User"/>s or <see cref="Type.User"/>s.
        /// If so, the <see cref="Assembly"/> is not appropriate for backend compilation and can only be executed in the VM.
        /// </summary>
        /// <returns>True, if the <see cref="Assembly"/> contains any <see cref="Value.User"/>s or <see cref="Type.User"/>s.</returns>
        public bool HasUserValues()
        {
            // Globals
            if (Globals.Any(g => g.Type.Equals(Type.User_))) return true;
            // Struct fields
            var structTypes = Structs.SelectMany(s => s.Fields);
            if (structTypes.Any(t => t.Equals(Type.User_))) return true;
            // Procedures
            if (Procedures.Any(p => p.HasUserValues())) return true;
            return false;
        }
    }
}
