using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Internal;
using Yoakke.Compiler.Symbols;
using Yoakke.Dependency;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Services.Impl
{
    internal class SymbolService : ISymbolService
    {
#pragma warning disable CS8618
        [QueryGroup]
        public IEvaluationService Evaluation { get; set; }

        [QueryGroup]
        public ITypeService Type { get; set; }
#pragma warning restore CS8618

        public ISymbolTable GetSymbolTable(Node node) => SymbolResolution.Resolve(Evaluation, Type, node);
    }
}
