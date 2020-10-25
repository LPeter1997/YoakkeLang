using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeCheck : Visitor<object>
    {
        public IDependencySystem System { get; }

        private Type? currentProcReturnType = null;

        public TypeCheck(IDependencySystem system)
        {
            System = system;
        }

        public void Check(Node node) => Visit(node);

        protected override object? Visit(Declaration.Const cons) => WithCurrentProcReturnType(null, () =>
        {
            // Type-check type and value
            base.Visit(cons);
            // First we assign the type to the symbol
            var symbol = (Symbol.Const)System.SymbolTable.DefinedSymbol(cons);
            symbol.Type = System.TypeOf(cons.Value);
            if (cons.Type != null)
            {
                // There's an explicit type, make sure they match
                var consType = System.EvaluateType(cons.Type);
                if (!symbol.Type.Equals(consType))
                {
                    // TODO
                    throw new NotImplementedException($"Type mismatch '{consType}' vs '{symbol.Type}'!");
                }
            }
        });

        protected override object? Visit(Statement.Var var)
        {
            base.Visit(var);
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

        protected override object? Visit(Statement.Return ret)
        {
            if (currentProcReturnType == null)
            {
                // TODO
                throw new NotImplementedException("Return at illegal position!");
            }
            base.Visit(ret);
            Type retType = ret.Value == null ? Type.Unit : System.TypeOf(ret.Value);
            if (!currentProcReturnType.Equals(retType))
            {
                // TODO
                throw new NotImplementedException($"Return type mismatch! Declared '{currentProcReturnType}' but got '{retType}'!");
            }
            return null;
        }

        protected override object? Visit(Statement.Expression_ expr)
        {
            base.Visit(expr);
            var evalType = System.TypeOf(expr.Expression);
            if (!expr.HasSemicolon)
            {
                // An expression in a statement's position must produce a unit type, if it's not semicolon terminated
                if (!evalType.Equals(Type.Unit))
                {
                    throw new NotImplementedException("A statement can't evaluate to anything other than unit!");
                }
            }
            return null;
        }

        protected override object? Visit(Expression.Proc proc)
        {
            Visit(proc.Signature);
            // Evaluate parameter types, assign their return types
            foreach (var param in proc.Signature.Parameters)
            {
                var symbol = (Symbol.Var)System.SymbolTable.DefinedSymbol(param);
                symbol.Type = System.EvaluateType(param.Type);
            }
            // Deduce return type
            var returnType = Type.Unit;
            if (proc.Signature.Return != null) returnType = System.EvaluateType(proc.Signature.Return);
            // We type-check with this return type
            WithCurrentProcReturnType(returnType, () => Visit(proc.Body));
            return null;
        }

        protected override object? Visit(Expression.If iff)
        {
            base.Visit(iff);
            // Condition must be bool
            var conditionType = System.TypeOf(iff.Condition);
            if (!conditionType.Equals(Type.Bool))
            {
                // TODO
                throw new NotImplementedException("Condition must be boolean!");
            }
            // If there is an else branch, body types should match
            if (iff.Else != null)
            {
                var thenType = System.TypeOf(iff.Then);
                var elseType = System.TypeOf(iff.Else);
                if (!thenType.Equals(elseType))
                {
                    // TODO
                    throw new NotImplementedException("If-else branch mismatch!");
                }
            }
            return null;
        }

        protected override object? Visit(Expression.While whil)
        {
            base.Visit(whil);
            // Condition must be bool
            var conditionType = System.TypeOf(whil.Condition);
            if (!conditionType.Equals(Type.Bool))
            {
                // TODO
                throw new NotImplementedException("Condition must be boolean!");
            }
            // Body must be unit
            var bodyType = System.TypeOf(whil.Body);
            if (!bodyType.Equals(Type.Unit))
            {
                // TODO
                throw new NotImplementedException("While body must be unit!");
            }
            return null;
        }

        protected override object? Visit(Expression.DotPath dot)
        {
            base.Visit(dot);
            var leftType = System.TypeOf(dot.Left);
            if (leftType.Equals(Type.Type_))
            {
                // Static member access, we need the type value itself
                var leftValue = System.EvaluateType(dot.Left);
                if (leftValue.DefinedScope == null)
                {
                    // TODO
                    throw new NotImplementedException();
                }
                leftValue.DefinedScope.Reference(dot.Right);
            }
            else if (leftType is Type.Struct structType)
            {
                // Field access
                if (!(structType.Fields.ContainsKey(dot.Right)))
                {
                    // TODO
                    throw new NotImplementedException($"Non-existing field '{dot.Right}'!");
                }
            }
            else
            {
                // TODO
                throw new NotImplementedException();
            }
            return null;
        }

        protected override object? Visit(Expression.Call call)
        {
            base.Visit(call);
            // Check if the called thing is even a procedure
            var calledType = System.TypeOf(call.Procedure);
            if (!(calledType is Type.Proc procType))
            {
                // TODO
                throw new NotImplementedException("Can't call non-procedure!");
            }
            // Check if arguments match
            var argTypes = call.Arguments.Select(arg => System.TypeOf(arg));
            if (!procType.Parameters.Select(p => p.Type).SequenceEqual(argTypes))
            {
                // TODO
                throw new NotImplementedException("Call argument types mismatch!");
            }
            return null;
        }

        protected override object? Visit(Expression.Subscript sub)
        {
            base.Visit(sub);
            // Check if the indexed thing is even an array
            var indexedType = System.TypeOf(sub.Array);
            if (!(indexedType is Type.Array arrayType))
            {
                // TODO
                throw new NotImplementedException("Can't index non-array!");
            }
            // Check if the index is an integer
            var indexType = System.TypeOf(sub.Index);
            if (!(indexType is Type.Prim prim && prim.Type is Lir.Types.Type.Int))
            {
                // TODO
                throw new NotImplementedException("Can't index with non-integer!");
            }
            return null;
        }

        protected override object? Visit(Expression.Binary bin)
        {
            base.Visit(bin);
            if (   bin.Operator == Expression.BinOp.Assign 
                || Expression.CompoundBinaryOperators.ContainsKey(bin.Operator))
            {
                // For assignment the sides must match
                var leftType = System.TypeOf(bin.Left);
                var rightType = System.TypeOf(bin.Right);
                if (!leftType.Equals(rightType))
                {
                    // TODO
                    throw new NotImplementedException($"Assignment type mismatch '{leftType}' vs '{rightType}'!");
                }
            }
            else
            {
                // TODO
            }
            return null;
        }

        protected override object? Visit(Expression.Unary ury)
        {
            base.Visit(ury);
            // TODO: Implement everything
            // For now we assume correct usage
            switch (ury.Operator)
            {
            case Expression.UnaryOp.Dereference:
            {
                var subType = System.TypeOf(ury.Operand);
                if (!(subType is Type.Ptr))
                {
                    // TODO
                    throw new NotImplementedException("Can't deref non-pointer!");
                }
            }
            break;

            default:
            { 
                // TODO
            }
            break;
            }
            return null;
        }

        protected override object? Visit(Expression.StructValue sval)
        {
            base.Visit(sval);
            // Check if it's even a struct type
            var instanceType = System.EvaluateType(sval.StructType);
            if (!(instanceType is Type.Struct structType))
            {
                // TODO
                throw new NotImplementedException("Can't instantiate non-struct!");
            }
            // Check if all fields are instantiated exactly once and with their proper type
            var remainingFields = structType.Fields.ToDictionary(f => f.Key, f => f.Value);
            foreach (var field in sval.Fields)
            {
                if (!remainingFields.Remove(field.Name, out var declaredType))
                {
                    // Either already initialized, or unknown field
                    if (structType.Fields.ContainsKey(field.Name))
                    {
                        // Double-init
                        // TODO
                        throw new NotImplementedException($"Field '{field.Name}' is double-initialized!");
                    }
                    else
                    {
                        // Unknown field
                        // TODO
                        throw new NotImplementedException($"Unknown field '{field.Name}'!");
                    }
                }
                else
                {
                    // Types need to match
                    var assignedType = System.TypeOf(field.Value);
                    if (!assignedType.Equals(declaredType))
                    {
                        // TODO
                        throw new NotImplementedException($"Field type mismatch '{declaredType}' vs '{assignedType}'!");
                    }
                }
            }
            if (remainingFields.Count > 0)
            {
                // TODO
                throw new NotImplementedException($"{remainingFields.Count} fields remaining uninitialized!");
            }
            return null;
        }

        private object? WithCurrentProcReturnType(Type? procReturnType, Action action)
        {
            var lastProcReturnType = currentProcReturnType;
            currentProcReturnType = procReturnType;
            action();
            currentProcReturnType = lastProcReturnType;
            return null;
        }
    }
}
