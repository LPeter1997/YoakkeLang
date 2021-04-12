using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Symbols.Impl
{
    partial class Symbol
    {
        public class GlobalVar : Symbol
        {
            public override IScope ContainingScope => throw new NotImplementedException();

            public override IScope? DefinedScope => throw new NotImplementedException();

            public override string Name => throw new NotImplementedException();

            public override Node? Definition => throw new NotImplementedException();
        }
    }
}
