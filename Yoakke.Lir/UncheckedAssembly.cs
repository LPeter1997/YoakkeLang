using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// The prelude procedure of the assembly.
        /// If not null, this will be ran before the main code to perform global initializations and such.
        /// </summary>
        public Proc? Prelude { get; set; }

        /// <summary>
        /// The <see cref="Extern"/>s this assembly references.
        /// </summary>
        public IList<Extern> Externals { get; set; } = new List<Extern>();
        /// <summary>
        /// The <see cref="Const"/>s this assembly defines.
        /// </summary>
        public IList<Const> Constants { get; set; } = new List<Const>();
        /// <summary>
        /// The <see cref="Global"/>s this assembly defines.
        /// </summary>
        public IList<Global> Globals { get; set; } = new List<Global>();
        /// <summary>
        /// The <see cref="Struct"/>s in this assembly.
        /// </summary>
        public IList<Struct> Structs { get; set; } = new List<Struct>();
        /// <summary>
        /// The <see cref="Proc"/>s defined in this assembly.
        /// </summary>
        public IList<Proc> Procedures { get; set; } = new List<Proc>();

        // TODO: Doc
        public event ValidationContext.ValidationErrorEventHandler? ValidationError;

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

        // TODO: Doc
        public IEnumerable<Extern> FindAllExternalsUsed() => Procedures
            .SelectMany(p => p.BasicBlocks)
            .SelectMany(bb => bb.Instructions)
            .SelectMany(ins => ins.InstrArgs)
            .OfType<Extern>();

        public Assembly Check()
        {
            var context = new ValidationContext(this);
            context.ValidationError += ValidationError;
            Validate(context);
            // TODO: This should be done elsewhere, but add externals
            Externals = Externals.Concat(FindAllExternalsUsed()).Distinct().ToList();
            return new Assembly(this);
        }

        public void Validate(ValidationContext context)
        {
            // We check for user-types in externals
            foreach (var ext in Externals)
            {
                if (ext.Type.Equals(Type.User_))
                {
                    context.Report(new ValidationError(context, ext, "Externals can't be of user types!"));
                }
            }
            // We check name duplication for symbols
            var symbolNames = new HashSet<string>();
            foreach (var sym in Symbols)
            {
                if (!symbolNames.Add(sym.Name))
                {
                    context.Report(new ValidationError(context, (IValidate)sym, "Symbol name already present in the assembly!"));
                }
            }
            // TODO: Check circularity for struct definitions?
            foreach (var proc in Procedures) proc.Validate(context);
        } 
    }
}
