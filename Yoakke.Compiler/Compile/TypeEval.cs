using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

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
        protected override Type? Visit(Expression.ArrayType aty) => Type.Type_;
        protected override Type? Visit(Expression.ProcSignature sign) => Type.Type_;

        protected override Type? Visit(Expression.Literal lit) => lit.Type switch
        {
            Expression.LitType.Integer => Type.I32,
            Expression.LitType.Bool => Type.Bool,
            Expression.LitType.String => System.ReferToConstType("@c", "str"),
            _ => throw new NotImplementedException(),
        };

        protected override Type? Visit(Expression.Identifier ident)
        {
            var symbol = System.SymbolTable.ReferredSymbol(ident);
            return TypeOfSymbol(symbol);
        }

        protected override Type? Visit(Expression.Proc proc) =>
            System.EvaluateType(proc.Signature);

        protected override Type? Visit(Expression.If iff) => TypeOf(iff.Then);

        protected override Type? Visit(Expression.While whil) => Type.Unit;

        protected override Type? Visit(Expression.Block block) =>
            block.Value == null ? Type.Unit : TypeOf(block.Value);

        protected override Type? Visit(Expression.Call call)
        {
            var procType = (Type.Proc)TypeOf(call.Procedure);
            return procType.Return;
        }

        protected override Type? Visit(Expression.Subscript sub)
        {
            var arrayType = (Type.Array)TypeOf(sub.Array);
            return arrayType.ElementType;
        }

        protected override Type? Visit(Expression.Binary bin)
        {
            // TODO: Just a basic assumption for now
            if (   bin.Operator == Expression.BinOp.Assign 
                || Expression.CompoundBinaryOperators.ContainsKey(bin.Operator))
            {
                return Type.Unit;
            }

            switch (bin.Operator)
            {
            case Expression.BinOp.And:
            case Expression.BinOp.Or:
            case Expression.BinOp.Less:
            case Expression.BinOp.LessEqual:
            case Expression.BinOp.Greater:
            case Expression.BinOp.GreaterEqual:
            case Expression.BinOp.Equals:
            case Expression.BinOp.NotEquals:
                return Type.Bool;

            default: return TypeOf(bin.Left);
            }
        }

        protected override Type? Visit(Expression.Unary ury)
        {
            switch (ury.Operator)
            {
            case Expression.UnaryOp.Ponote:
            case Expression.UnaryOp.Negate:
            case Expression.UnaryOp.Not:
                return TypeOf(ury.Operand);

            case Expression.UnaryOp.AddressOf:
                return new Type.Ptr(new DataStructures.Lazy<Type>(() => TypeOf(ury.Operand)));

            case Expression.UnaryOp.PointerType:
                return Type.Type_;

            case Expression.UnaryOp.Dereference:
            {
                var ptrType = (Type.Ptr)TypeOf(ury.Operand);
                return ptrType.Subtype.Value;
            }

            default: throw new NotImplementedException();
            }
        }

        protected override Type? Visit(Expression.StructValue sval) =>
            System.EvaluateType(sval.StructType);

        protected override Type? Visit(Expression.DotPath dot)
        {
            var leftType = TypeOf(dot.Left);
            if (leftType.Equals(Type.Type_))
            {
                // Static member access
                var leftValue = System.EvaluateType(dot.Left);
                var referredSymbol = leftValue.DefinedScope.Reference(dot.Right);
                return TypeOfSymbol(referredSymbol);
            }
            else if (leftType is Type.Struct structType)
            {
                // Field access
                return structType.Fields[dot.Right];
            }
            else
            {
                // TODO
                throw new NotImplementedException();
            }
        }

        private Type TypeOfSymbol(Symbol symbol)
        {
            if (symbol is Symbol.Const constSym)
            {
                if (constSym.Type != null) return constSym.Type;
                Debug.Assert(constSym.Definition != null);
                var definition = (Declaration.Const)constSym.Definition;
                return TypeOf(definition.Value);
            }
            else if (symbol is Symbol.Var varSym)
            {
                Debug.Assert(varSym.Type != null);
                return varSym.Type;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
