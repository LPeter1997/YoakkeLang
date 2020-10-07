using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeCheck : Visitor<object>
    {
        public IDependencySystem System { get; }

        public TypeCheck(IDependencySystem system)
        {
            System = system;
        }

        public void Check(Node node) => Visit(node);

        protected override object? Visit(Statement.Var var)
        {
            var symbol = (Symbol.Var)System.SymbolTable.DefinedSymbol(var);
            Type? inferredType = null;
            if (var.Type != null)
            {
                // We have a type declaration
                inferredType = System.EvaluateType(var.Type);
            }
            if (var.Value != null)
            {
                // We have an initializer value
                var valueType = System.TypeOf(var.Value);
                if (inferredType == null)
                {
                    // No declared type
                    inferredType = valueType;
                }
                else
                {
                    // The delared type must match the value type
                    if (!inferredType.Equals(valueType))
                    {
                        // TODO
                        throw new NotImplementedException($"Type mismatch '{inferredType}' vs '{valueType}'!");
                    }
                }
            }
            Debug.Assert(inferredType != null);
            symbol.Type = inferredType;
            return null;
        }

        protected override object? Visit(Expression.Proc proc)
        {
            // TODO: This is missing a few things, just a start
            foreach (var param in proc.Signature.Parameters)
            {
                var symbol = (Symbol.Var)System.SymbolTable.DefinedSymbol(param);
                symbol.Type = System.EvaluateType(param.Type);
            }
            Visit(proc.Body);
            return null;
        }

        // TODO: Do the rest
    }
}
