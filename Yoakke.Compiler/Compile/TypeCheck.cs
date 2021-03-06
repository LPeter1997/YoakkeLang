﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Yoakke.Compiler.Error;
using Yoakke.Compiler.Semantic;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;
using IParseTreeElement = Yoakke.Syntax.ParseTree.IParseTreeElement;
using Yoakke.DataStructures;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeCheck : Visitor<object>
    {
        public IDependencySystem System { get; }

        // TODO: Very yucky context pattern here too...
        private Type? currentProcReturnType = null;
        private Expression? currentProcSignature = null;

        public TypeCheck(IDependencySystem system)
        {
            System = system;
        }

        public void Check(Node node) => Visit(node);

        protected override object? Visit(Declaration.Const cons) => WithCurrentProcReturnType(null, null, () =>
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
                    System.Report(new TypeMismatchError(consType, symbol.Type)
                    {
                        Defined = cons.Type.ParseTreeNode,
                        Wrong = cons.Value.ParseTreeNode,
                        Context = "constant definition",
                    });
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
                        System.Report(new TypeMismatchError(inferredType, valueType)
                        {
                            Defined = var.Type?.ParseTreeNode,
                            Wrong = var.Value.ParseTreeNode,
                            Context = "variable definition",
                        });
                    }
                }
            }
            Debug.Assert(inferredType != null);
            symbol.Type = inferredType;
            return null;
        }

        protected override object? Visit(Statement.Return ret)
        {
            Debug.Assert(currentProcReturnType != null);

            base.Visit(ret);
            Type retType = ret.Value == null ? Type.Unit : System.TypeOf(ret.Value);
            if (!currentProcReturnType.Equals(retType))
            {
                // Error, work out what we know
                Debug.Assert(currentProcSignature != null);
                var signature = (Syntax.ParseTree.Expression.ProcSignature?)currentProcSignature.ParseTreeNode;
                var definition = (Syntax.ParseTree.IParseTreeElement?)signature?.Return ?? signature?.CloseParen;
                var wrong = ((Node?)FindDeepestReturnValue(ret.Value) ?? ret)?.ParseTreeNode;
                System.Report(new TypeMismatchError(currentProcReturnType, retType)
                {
                    Defined = definition,
                    Wrong = wrong,
                    Context = "return value",
                    ImplicitlyDefined = signature != null && signature.Return == null,
                });
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
                    System.Report(new ExpectedTypeError(Type.Unit, evalType)
                    {
                        Context = "statement",
                        Place = FindDeepestReturnValue(expr.Expression)?.ParseTreeNode,
                        Note = "implicit returns can only appear as the last expressions in the block"
                    });
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
            Debug.Assert(proc.Signature.Return != null);
            var returnType = System.EvaluateType(proc.Signature.Return);
            // We type-check with this return type
            WithCurrentProcReturnType(returnType, proc.Signature, () => Visit(proc.Body));
            return null;
        }

        protected override object? Visit(Expression.If iff)
        {
            base.Visit(iff);
            // Condition must be bool
            var conditionType = System.TypeOf(iff.Condition);
            if (!conditionType.Equals(Type.Bool))
            {
                System.Report(new ExpectedTypeError(Type.Bool, conditionType)
                {
                    Context = "if condition",
                    Place = iff.Condition.ParseTreeNode,
                });
            }
            // If there is an else branch, body types should match
            if (iff.Else != null)
            {
                var thenType = System.TypeOf(iff.Then);
                var elseType = System.TypeOf(iff.Else);
                if (!thenType.Equals(elseType))
                {
                    var thenRet = FindDeepestReturnValue(iff.Then)?.ParseTreeNode;
                    var elseRet = FindDeepestReturnValue(iff.Else)?.ParseTreeNode;
                    System.Report(new TypeMismatchError(thenType, elseType)
                    {
                        Context = "if expression",
                        Defined = thenRet,
                        Wrong = elseRet,
                    });
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
                System.Report(new ExpectedTypeError(Type.Bool, conditionType)
                {
                    Context = "while condition",
                    Place = whil.Condition.ParseTreeNode,
                });
            }
            // Body must be unit
            var bodyType = System.TypeOf(whil.Body);
            if (!bodyType.Equals(Type.Unit))
            {
                System.Report(new ExpectedTypeError(Type.Unit, bodyType)
                {
                    Context = "while block",
                    Place = FindDeepestReturnValue(whil.Body)?.ParseTreeNode,
                    Note = "implicit returns cannot appear in while statements"
                });
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
                var token = ((Syntax.ParseTree.Expression.DotPath?)dot.ParseTreeNode)?.Right;
                var _ = token == null 
                    ? leftValue.DefinedScope.Reference(dot.Right, System)
                    : leftValue.DefinedScope.Reference(token, System);
            }
            else if (leftType is Type.Struct structType)
            {
                // Field access
                if (!(structType.Fields.ContainsKey(dot.Right)))
                {
                    var rightIdent = (dot.ParseTreeNode as Syntax.ParseTree.Expression.DotPath)?.Right;
                    var err = rightIdent == null 
                        ? new UndefinedSymbolError(dot.Right) 
                        : new UndefinedSymbolError(rightIdent);
                    err.Context = "field access";
                    err.SimilarExistingNames = structType.Fields.Keys
                        .Where(f => StringMetrics.OptimalStringAlignmentDistance(f, dot.Right) <= 2);
                    System.Report(err);
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
            // Check if arguments match parameters
            if (procType.Parameters.Count == call.Arguments.Count)
            {
                foreach (var (param, arg) in procType.Parameters.Zip(call.Arguments))
                {
                    var paramType = param.Type;
                    var argType = System.TypeOf(arg);
                    if (!paramType.Equals(argType))
                    {
                        System.Report(new TypeMismatchError(paramType, argType)
                        {
                            Context = "procedure call",
                            Defined = param.Symbol.Definition?.ParseTreeNode,
                            Wrong = arg.ParseTreeNode,
                        });
                    }
                }
            }
            else
            {
                // Argument count mismatch
                System.Report(new ArgCountMismatchError(procType.Parameters.Count, call.Arguments.Count)
                {
                    // TODO: Add definition
                    Wrong = call.ParseTreeNode,
                });
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
                    var leftIdent = bin.Left as Expression.Identifier;
                    var leftSymbol = leftIdent == null ? null : System.SymbolTable.ReferredSymbol(leftIdent);
                    System.Report(new TypeMismatchError(leftType, rightType)
                    {
                        Context = "assignment",
                        Defined = leftSymbol?.Definition?.ParseTreeNode,
                        Wrong = bin.Right.ParseTreeNode,
                    });
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

        protected override object? Visit(Expression.StructType sty)
        {
            // NOTE: We don't visit declarations here to avoid recursion
            foreach (var field in sty.Fields) Visit(field);
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
            var alreadyInitialized = new Dictionary<string, IParseTreeElement?>();
            foreach (var field in sval.Fields)
            {
                if (!remainingFields.Remove(field.Name, out var declaredField))
                {
                    // Either already initialized, or unknown field
                    if (structType.Fields.ContainsKey(field.Name))
                    {
                        System.Report(new DoubleInitializationError(structType, field.Name)
                        {
                            TypeInitializer = sval.StructType?.ParseTreeNode,
                            FirstInitialized = alreadyInitialized[field.Name],
                            SecondInitialized = field.ParseTreeNode,
                        });
                    }
                    else
                    {
                        System.Report(new UnknownInitializedFieldError(structType, field.Name)
                        {
                            TypeInitializer = sval.StructType?.ParseTreeNode,
                            UnknownInitialized = field.ParseTreeNode,
                            SimilarExistingNames = remainingFields.Keys
                                .Where(n => StringMetrics.OptimalStringAlignmentDistance(n, field.Name) <= 2),
                        });
                    }
                }
                else
                {
                    // Types need to match
                    var assignedType = System.TypeOf(field.Value);
                    if (!assignedType.Equals(declaredField.Type.Value))
                    {
                        System.Report(new TypeMismatchError(declaredField.Type.Value, assignedType)
                        { 
                            Context = "struct value initialization",
                            Defined = declaredField.Definition?.ParseTreeNode,
                            Wrong = field.ParseTreeNode,
                        });
                    }
                    alreadyInitialized.Add(field.Name, field.ParseTreeNode);
                }
            }
            if (remainingFields.Count > 0)
            {
                System.Report(new MissingInitializationError(structType, remainingFields.Keys)
                {
                    TypeInitializer = sval.StructType?.ParseTreeNode,
                });
            }
            return null;
        }

        // TODO: Eww
        private object? WithCurrentProcReturnType(Type? procReturnType, Expression? procSignature, Action action)
        {
            var lastProcReturnType = currentProcReturnType;
            var lastProcSignature = currentProcSignature;
            currentProcReturnType = procReturnType;
            currentProcSignature = procSignature;
            action();
            currentProcReturnType = lastProcReturnType;
            currentProcSignature = lastProcSignature;
            return null;
        }

        private static Expression? FindDeepestReturnValue(Expression? expr) => expr switch
        {
            null => null,
            Expression.Block block => FindDeepestReturnValue(block.Value),
            Expression.If iff => FindDeepestReturnValue(iff.Then) ?? FindDeepestReturnValue(iff.Else) ?? iff,
            _ => expr,
        };
    }
}
