﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    static class EvaluateConst
    {
        public static Value Evaluate(Expression expression)
        {
            switch (expression)
            {
            // TODO: Int literal value
            case IntLiteralExpression intLit: throw new NotImplementedException();

            case IdentifierExpression ident:
            {
                Assert.NonNull(ident.Symbol);
                // We are in constant evaluation, the symbol must be a constant
                if (ident.Symbol is ConstSymbol symbol)
                {
                    if (symbol.Value == null)
                    {
                        // If value is null, it hasn't been calculated yet
                        Assert.NonNull(symbol.Definition);
                        symbol.Value = Evaluate(symbol.Definition.Value);
                    }
                    // Value is surely calculated here
                    return symbol.Value;
                }
                throw new NotImplementedException("Non-constant symbol referenced in a constant expression!");
            }

            case ProcExpression proc:
            {
                var paramTypes = proc.Parameters.Select(x => EvaluateToType(x.Type)).ToList();
                var returnType = proc.ReturnType == null ? Type.Unit : EvaluateToType(proc.ReturnType);
                var procType =  Type.Procedure(paramTypes, returnType);
                return new ProcValue(proc, procType);
            }

            // TODO: Evaluate statements, block
            case BlockExpression block: throw new NotImplementedException();

            default: throw new NotImplementedException();
            }
        }

        private static Type EvaluateToType(Expression expression)
        {
            var val = Evaluate(expression);
            Unifier.Unify(val.Type, Type.Type_);
            var paramVal = val as TypeValue;
            Assert.NonNull(paramVal);
            return paramVal.Value;
        }
    }
}