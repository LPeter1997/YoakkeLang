using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeEval : Visitor<Type>
    {
        public IDependencySystem System { get; }

        public TypeEval(IDependencySystem system)
        {
            System = system;
        }

        // Public interface ////////////////////////////////////////////////////

        public Type TypeOf(Expression expression) => VisitNonNull(expression);

        // Implementation details //////////////////////////////////////////////

        protected override Type? Visit(Expression.StructType sty) => Type.Type_;

        protected override Type? Visit(Expression.Literal lit) => lit.Type switch
        {
            TokenType.IntLiteral => Type.I32,
            TokenType.KwTrue or TokenType.KwFalse => Type.Bool,
            _ => throw new NotImplementedException(),
        };

        protected override Type? Visit(Expression.Identifier ident)
        {
            var symbol = System.SymbolTable.ReferredSymbol(ident);
            if (symbol is Symbol.Const constSym)
            {
                if (constSym.Type != null) return constSym.Type;
                Debug.Assert(constSym.Definition != null);
                var definition = (Declaration.Const)constSym.Definition;
                return TypeOf(definition.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected override Type? Visit(Expression.Proc proc)
        {
            var signature = proc.Signature;
            // Evaluate parameter types
            var paramTypes = signature.Parameters.Select(param => System.EvaluateType(param.Type));
            // Evaluate return type, default to unit, if none provided
            var returnType = Type.Unit;
            if (signature.Return != null) returnType = System.EvaluateType(signature.Return);
            // Construct the procedure type
            return new Type.Proc(paramTypes.ToList().AsValueList(), returnType);
        }
    }
}
