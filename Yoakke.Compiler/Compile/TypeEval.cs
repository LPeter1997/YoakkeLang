﻿using System;
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
        protected override Type? Visit(Expression.ArrayType aty) => Type.Type_;
        protected override Type? Visit(Expression.ProcSignature sign) => Type.Type_;

        protected override Type? Visit(Expression.Literal lit) => lit.Type switch
        {
            Expression.LiteralType.Integer => Type.I32,
            Expression.LiteralType.Bool => Type.Bool,
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
            if (   bin.Operator == Expression.BinaryOperator.Assign 
                || Expression.CompoundBinaryOperators.ContainsKey(bin.Operator))
            {
                return Type.Unit;
            }

            switch (bin.Operator)
            {
            case Expression.BinaryOperator.And:
            case Expression.BinaryOperator.Or:
            case Expression.BinaryOperator.Less:
            case Expression.BinaryOperator.LessEqual:
            case Expression.BinaryOperator.Greater:
            case Expression.BinaryOperator.GreaterEqual:
            case Expression.BinaryOperator.Equals:
            case Expression.BinaryOperator.NotEquals:
                return Type.Bool;

            default: return TypeOf(bin.Left);
            }
        }

        protected override Type? Visit(Expression.Unary ury)
        {
            switch (ury.Operator)
            {
            case Expression.UnaryOperator.Ponote:
            case Expression.UnaryOperator.Negate:
            case Expression.UnaryOperator.Not:
                return TypeOf(ury.Operand);

            case Expression.UnaryOperator.AddressOf:
                return new Type.Ptr(TypeOf(ury.Operand));

            case Expression.UnaryOperator.PointerType:
                return Type.Type_;

            case Expression.UnaryOperator.Dereference:
            {
                var ptrType = (Type.Ptr)TypeOf(ury.Operand);
                return ptrType.Subtype;
            }

            default: throw new NotImplementedException();
            }
        }

        protected override Type? Visit(Expression.StructValue sval) =>
            System.EvaluateType(sval.StructType);

        protected override Type? Visit(Expression.DotPath dot)
        {
            var leftType = (Type.Struct)TypeOf(dot.Left);
            return leftType.Fields[dot.Right];
        }
    }
}
