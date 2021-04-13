using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Symbols.Impl
{
    internal abstract partial class Symbol : ITypedSymbol
    {
        public abstract string Name { get; }
        public abstract Type Type { get; }
        public abstract IScope ContainingScope { get; }
        public abstract Node? Definition { get; }
    }
}
