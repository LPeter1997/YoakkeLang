using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Symbols.Impl
{
    internal abstract partial class Symbol : ISymbol
    {
        public abstract IScope ContainingScope { get; }
        public abstract IScope? DefinedScope { get; }
        public abstract string Name { get; }
        public abstract Node? Definition { get; }
    }
}
