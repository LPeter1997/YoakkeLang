using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Symbols;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Services.Impl
{
    internal class SymbolService : ISymbolService
    {
        public IScope ContainingScope(Node node)
        {
            throw new NotImplementedException();
        }

        public ISymbol AssociatedSymbol(Node node)
        {
            throw new NotImplementedException();
        }
    }
}
