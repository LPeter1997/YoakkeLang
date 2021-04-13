using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Compiler.Services;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Symbols.Impl
{
    internal abstract class VarSymbol : Symbol
    {
        public override string Name { get; }
        public override Type Type => getType();
        public override IScope ContainingScope { get; }
        public override Node? Definition { get; }

        private Func<Type> getType;

        protected VarSymbol(ISymbolTable symbolTable, string name, Func<Type> getType, Node definition)
        {
            Name = name;
            ContainingScope = symbolTable.ContainingScope(definition);
            Definition = definition;
            this.getType = getType;
        }

        protected static Type GetTypeOfVar(IEvaluationService eval, ITypeService type, Statement.Var var)
        {
            if (var.Type != null) return eval.EvaluateType(var.Type);
            Debug.Assert(var.Value != null);
            return type.TypeOf(var.Value);
        }
    }
}
