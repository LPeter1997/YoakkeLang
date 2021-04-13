﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Compiler.Services;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Symbols.Impl
{
    partial class Symbol
    {
        public class Const : Symbol
        {
            public override string Name { get; }
            public override Type Type => getType();
            public override IScope ContainingScope { get; }
            public override Node? Definition { get; }
            public Value Value => getValue();

            private Func<Value> getValue;
            private Func<Type> getType;

            public Const(IEvaluationService eval, ITypeService type, ISymbolTable symbolTable, Declaration.Const definition)
            {
                Name = definition.Name;
                Definition = definition;
                ContainingScope = symbolTable.ContainingScope(definition);
                getType = () => GetTypeOfConst(eval, type, definition);
                getValue = () => eval.Evaluate(definition.Value);
            }

            public Const(IScope scope, string name, Type type, Value value)
            {
                Name = name;
                ContainingScope = scope;
                getType = () => type;
                getValue = () => value;
            }

            private static Type GetTypeOfConst(IEvaluationService eval, ITypeService type, Declaration.Const definition)
            {
                if (definition.Type != null) return eval.EvaluateType(definition.Type);
                return type.TypeOf(definition.Value);
            }
        }
    }
}
