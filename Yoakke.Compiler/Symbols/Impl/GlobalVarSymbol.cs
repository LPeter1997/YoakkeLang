using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Compiler.Services;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Symbols.Impl
{
    partial class Symbol
    {
        public class GlobalVar : VarSymbol
        {
            public GlobalVar(IEvaluationService eval, ITypeService type, ISymbolTable symbolTable, Statement.Var var) 
                : base(symbolTable, var.Name, () => GetTypeOfVar(eval, type, var), var)
            {
            }
        }
    }
}
