using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Services;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Symbols.Impl
{
    partial class Symbol
    {
        public class LocalVar : VarSymbol
        {
            private static int unnamedCnt = 0;

            public LocalVar(IEvaluationService eval, ITypeService type, ISymbolTable symbolTable, Statement.Var var)
                : base(symbolTable, var.Name, () => GetTypeOfVar(eval, type, var), var)
            {
            }

            public LocalVar(IEvaluationService eval, ISymbolTable symbolTable, Expression.ProcSignature.Parameter param)
                : base(symbolTable, param.Name ?? $"unnamed_{unnamedCnt++}", () => eval.EvaluateType(param.Type), param)
            {
            }
        }
    }
}
