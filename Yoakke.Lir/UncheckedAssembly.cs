using System.Collections.Generic;
using System.Linq;
using Yoakke.Lir.Status;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// An <see cref="Assembly"/> that's mutable and is not in a validated state.
    /// </summary>
    public class UncheckedAssembly : IValidate
    {
        /// <summary>
        /// The name of this <see cref="Assembly"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The entry point of the assembly.
        /// If null, the procedure named "main" will be chosen, or the singleton, if there's 
        /// only one procedure defined.
        /// </summary>
        public Proc? EntryPoint { get; set; }

        /// <summary>
        /// The <see cref="Extern"/>s this assembly references.
        /// </summary>
        public readonly IList<Extern> Externals = new List<Extern>();
        /// <summary>
        /// The <see cref="Const"/>s this assembly defines.
        /// </summary>
        public readonly IList<Const> Constants = new List<Const>();
        /// <summary>
        /// The <see cref="Global"/>s this assembly defines.
        /// </summary>
        public readonly IList<Global> Globals = new List<Global>();
        /// <summary>
        /// The <see cref="Struct"/>s in this assembly.
        /// </summary>
        public readonly IList<Struct> Structs = new List<Struct>();
        /// <summary>
        /// The <see cref="Proc"/>s defined in this assembly.
        /// </summary>
        public readonly IList<Proc> Procedures = new List<Proc>();

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
        /// Initializes a new <see cref="UncheckedAssembly"/>.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        public UncheckedAssembly(string name)
        {
            Name = name;
        }

        public Assembly Check(BuildStatus status)
        {
            Validate(status);
            return new Assembly(this);
        }

        public void Validate(BuildStatus status)
        {
            // We check for user-types in externals
            foreach (var ext in Externals)
            {
                if (ext.Type.Equals(Type.User_))
                {
                    status.Report(new ValidationError(ext, "Externals can't be of user types!"));
                }
            }
            // We check name duplication for symbols
            var symbolNames = new HashSet<string>();
            foreach (var sym in Symbols)
            {
                if (!symbolNames.Add(sym.Name))
                {
                    status.Report(new ValidationError((IValidate)sym, "Symbol name already present in the assembly!"));
                }
            }
            // TODO: Check circularity for struct definitions?
            foreach (var proc in Procedures) proc.Validate(status);
        } 
    }
}
