using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml.Schema;
using Yoakke.Compiler.Ast;
using Yoakke.Compiler.Syntax;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Semantic
{
    // TODO: Many semantic rules are redundant between here and TypeCheck.
    // Can we make it DRY?

    /// <summary>
    /// Does constant-evaluation at compile-time.
    /// </summary>
    static class ConstEval
    {
        /// <summary>
        /// Evaluates the given <see cref="Statement"/> at compile-time.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to evaluate.</param>
        public static void Evaluate(Statement statement)
        {
            var callStk = new Stack<StackFrame>();
            callStk.Push(new StackFrame());
            Evaluate(callStk, statement, true);
        }

        /// <summary>
        /// Evaluates the given <see cref="Expression"/> at compile-time.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The compile-time <see cref="Value"/> of the <see cref="Expression"/>.</returns>
        public static Value Evaluate(Expression expression)
        {
            var callStk = new Stack<StackFrame>();
            callStk.Push(new StackFrame());
            return Evaluate(callStk, expression, true, false);
        }

        /// <summary>
        /// Evaluates the given <see cref="Expression"/> at compile-time as a <see cref="Type"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The <see cref="Type"/> the <see cref="Expression"/> describes.</returns>
        public static Type EvaluateAsType(Expression expression)
        {
            var callStk = new Stack<StackFrame>();
            callStk.Push(new StackFrame());
            return EvaluateAsType(callStk, expression, true);
        }

        private class StackFrame
        {
            public Dictionary<Symbol.Variable, Value> Variables = new Dictionary<Symbol.Variable, Value>();
        }

        private class ReturnValue : Exception
        {
            public readonly Value Value;

            public ReturnValue(Value value)
            {
                Value = value;
            }
        }

        private static Value Evaluate(Stack<StackFrame> callStack, Expression expression, bool canCache, bool lvalue)
        {
            // NOTE: We need canCache because we can't cache *everything*. For example, we can't cache the values of
            // procedure bodies, as that would result in the same value for each evaluation of the procedure body, since once it's
            // cached, it will stay cached.
            // We could still cache, if we make caching dependent on the actual state, I.E. the call stack.
            // For now we just re-evaluate when we can't cache for sure.

            // Some simple cache-ing mechanism
            Value value;
            if (canCache)
            {
                if (expression.ConstantValue == null)
                {
                    expression.ConstantValue = EvaluateImpl(callStack, expression, canCache, lvalue);
                }
                value = lvalue ? expression.ConstantValue : expression.ConstantValue.Clone();
            }
            else
            {
                value = EvaluateImpl(callStack, expression, canCache, lvalue);
            }
            return value;
        }

        private static Type EvaluateAsType(Stack<StackFrame> callStack, Expression expression, bool canCache)
        {
            var value = Evaluate(callStack, expression, canCache, false);
            value.Type.UnifyWith(Type.Type_);
            return (Type)value;
        }

        private static void Evaluate(Stack<StackFrame> callStack, Statement statement, bool canCache)
        {
            switch (statement)
            {
            case Declaration.Program program:
                foreach (var decl in program.Declarations) Evaluate(callStack, decl, true);
                break;

            case Declaration.ConstDef constDef:
                Assert.NonNull(constDef.Symbol);
                // Evaluate the type, if there's any
                if (constDef.Type != null) EvaluateAsType(callStack, constDef.Type, true);
                // Evaluate the value
                if (constDef.Symbol.Value == null)
                {
                    // This is still unevaluated, evaluate and assign to symbol
                    // First we assign the symbol a pseudo-value to avoid problems with recursion
                    var pseudoValue = new Value.UnderEvaluation();
                    constDef.Symbol.Value = pseudoValue;
                    // Then we do the actual evaluation
                    constDef.Symbol.Value = Evaluate(callStack, constDef.Value, true, false);
                    // Finally we unify the types of the two values to not to lose inference information
                    constDef.Symbol.Value.Type.UnifyWith(pseudoValue.Type);
                }
                // If has a type, unify with value
                if (constDef.Type != null)
                {
                    Assert.NonNull(constDef.Type.ConstantValue);
                    Assert.NonNull(constDef.Value.ConstantValue);

                    var type = (Type)constDef.Type.ConstantValue;
                    type.UnifyWith(constDef.Value.ConstantValue.Type);
                }
                break;

            case Statement.Return ret:
                throw new ReturnValue(ret.Value == null ? new Value.Tuple() : Evaluate(callStack, ret.Value, canCache, false));

            case Statement.VarDef varDef:
                if (varDef.Type != null)
                {
                    // Evaluate type
                    var type = EvaluateAsType(callStack, varDef.Type, canCache);
                    // Evaluate value
                    var value = Evaluate(callStack, varDef.Value, canCache, false);
                    // Unify with type
                    type.UnifyWith(value.Type);
                    // Assign the variable
                    Assert.NonNull(varDef.Symbol);
                    callStack.Peek().Variables.Add(varDef.Symbol, value);
                }
                else
                {
                    // Evaluate value
                    var value = Evaluate(callStack, varDef.Value, canCache, false);
                    // Assign the variable
                    Assert.NonNull(varDef.Symbol);
                    callStack.Peek().Variables.Add(varDef.Symbol, value);
                }
                break;

            case Statement.Expression_ expression:
            {
                var value = Evaluate(callStack, expression.Expression, canCache, false);
                if (!expression.HasSemicolon) Type.Unit.UnifyWith(value.Type);
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        private static Value EvaluateImpl(Stack<StackFrame> callStack, Expression expression, bool canCache, bool lvalue)
        {
            switch (expression)
            {
            case Expression.IntLit intLit:
                if (lvalue) throw new NotImplementedException("Int literal can't be an lvalue!");
                // TODO: It should be a generic integer type!
                return new Value.Primitive<BigInteger>(Type.I32, BigInteger.Parse(intLit.Token.Value));

            case Expression.BoolLit boolLit:
                if (lvalue) throw new NotImplementedException("Bool literal can't be an lvalue!");
                return new Value.Primitive<bool>(Type.Bool, boolLit.Token.Type == TokenType.KwTrue);

            case Expression.StrLit strLit:
                if (lvalue) throw new NotImplementedException("String literal can't be an lvalue!");
                return new Value.Primitive<string>(Type.Str, strLit.Escape());

            case Expression.Ident ident:
                Assert.NonNull(ident.Symbol);
                // Depends on the symbol
                switch (ident.Symbol)
                {
                case Symbol.Const constSym:
                {
                    if (lvalue) throw new NotImplementedException("Constants can't be lvalues!");
                    // Symbol already has a value, return that
                    if (constSym.Value != null) return constSym.Value;
                    // No value, must be evaluatable
                    Assert.NonNull(constSym.Definition);
                    // TODO: Rethink these notes
                    // NOTE: We don't pass the call-stack on purpose. Constants shouldn't be related.
                    // NOTE: But maybe we should, since we now can stop cache-ing?
                    Evaluate(constSym.Definition);
                    Assert.NonNull(constSym.Value);
                    return constSym.Value;
                }

                // Simply look up the local
                case Symbol.Variable varSym:
                {
                    var frame = callStack.Peek();
                    if (lvalue)
                    {
                        // Wrap up the value in an lvalue
                        return new Value.Lvalue(
                            () => frame.Variables[varSym],
                            v => frame.Variables[varSym] = v);
                    }
                    else
                    {
                        // Simply return the value
                        return frame.Variables[varSym];
                    }
                }

                default: throw new NotImplementedException();
                }

            case Expression.DotPath dotPath:
            {
                // NOTE: This looks really-really bad
                // Switching to IR or some properly defined value-semantics could really help here
                var left = Evaluate(callStack, dotPath.Left, canCache, lvalue);
                Value.Struct? structureType = left is Value.Lvalue lval
                    ? lval.Getter() as Value.Struct
                    : left as Value.Struct;
                Type.Struct? structureValue = left as Type.Struct;
                if (structureType != null)
                {
                    if (!structureType.Fields.TryGetValue(dotPath.Right.Value, out var field))
                    {
                        // TODO
                        throw new NotImplementedException("No such field of struct!");
                    }
                    if (lvalue)
                    {
                        // We need to wrap
                        return new Value.Lvalue(
                            () => structureType.Fields[dotPath.Right.Value],
                            v => structureType.Fields[dotPath.Right.Value] = v);
                    }
                    else
                    {
                        return field;
                    }
                }
                else if (structureValue != null)
                {
                    if (lvalue) throw new NotImplementedException("Can't be lvalue!");

                    // TODO: Same todo as in TypeEval's

                    var symbol = (Symbol.Const)structureValue.Scope.Reference(dotPath.Right.Value);
                    return symbol.GetValue();
                }
                else
                {
                    // TODO
                    throw new NotImplementedException("Not a struct type on the left-hand-side of dot!");
                }
            }

            case Expression.StructType structType:
            {
                if (lvalue) throw new NotImplementedException("Struct types can't be lvalues!");
                var fields = structType.Fields.ToDictionary(
                    f => f.Name.Value,
                    f => EvaluateAsType(callStack, f.Type, canCache));

                // Evaluate every declaration
                //foreach (var decl in structType.Declarations) Evaluate(callStack, decl, canCache);

                // We need to find it's scope
                Scope? scope = null;
                // Try to grab it from a field
                if (structType.Fields.Count > 0) scope = structType.Fields[0].Type.Scope;
                // Try to grab it from a declaration
                else if (structType.Declarations.Count > 0) scope = structType.Declarations[0].Scope;
                // Doesn't matter, just create an empty one
                else scope = new Scope(ScopeTag.None, structType.Scope);

                Assert.NonNull(scope);
                return new Type.Struct(structType.Token, fields, scope);
            }

            case Expression.StructValue structValue:
            {
                if (lvalue) throw new NotImplementedException("Struct values can't be lvalues!");
                // Type-check
                TypeCheck.Check(structValue);
                // If the above didn't throw any errors, we are good to go
                // Get the struct type
                var structType = EvaluateAsType(callStack, structValue.StructType, canCache);
                var fields = structValue.Fields.ToDictionary(
                    f => f.Name.Value,
                    f => Evaluate(callStack, f.Value, canCache, false));
                return new Value.Struct(structType, fields);
            }

            case Expression.ProcType procType:
            {
                if (lvalue) throw new NotImplementedException("Procedure types can't be lvalues!");
                // Evaluate parameters
                var parameters = procType.ParameterTypes.Select(x => EvaluateAsType(callStack, x, canCache)).ToList();
                // Evaluate return type, if any
                var ret = procType.ReturnType == null ? Type.Unit : EvaluateAsType(callStack, procType.ReturnType, canCache);
                // Create the procedure type
                return new Type.Proc(parameters, ret);
            }

            case Expression.ProcValue proc:
            {
                if (lvalue) throw new NotImplementedException("Procedure values can't be lvalues!");
                // Evaluate parameters
                var parameters = proc.Parameters.Select(x => EvaluateAsType(callStack, x.Type, canCache)).ToList();
                // Evaluate return type, if any
                var ret = proc.ReturnType == null ? Type.Unit : EvaluateAsType(callStack, proc.ReturnType, canCache);
                // Get the body's scope, that's where we receive return types of explicit returns
                Assert.NonNull(proc.Body.Scope);
                var bodyScope = proc.Body.Scope;
                Debug.Assert(bodyScope.Tag.HasFlag(ScopeTag.Proc));
                ret.UnifyWith(bodyScope.ReturnType);
                // Type-check the body
                var bodyType = TypeEval.Evaluate(proc.Body);
                if (proc.Body is Expression.Block block && block.Value != null)
                {
                    // TODO: Same as in TypeCheck

                    // Unify with return type
                    ret.UnifyWith(bodyType);
                }
                // Create the procedure type
                var procType = new Type.Proc(parameters, ret);
                // Wrap it up in a value
                return new Value.Primitive<Expression.ProcValue>(procType, proc);
            }

            case Expression.Block block:
            {
                foreach (var stmt in block.Statements) Evaluate(callStack, stmt, canCache);
                return block.Value == null ? new Value.Tuple() : Evaluate(callStack, block.Value, canCache, lvalue);
            }

            case Expression.Call call:
            {
                // Evaluate the procedure and the arguments
                var proc = Evaluate(callStack, call.Proc, canCache, false);
                var args = call.Arguments.Select(x => Evaluate(callStack, x, canCache, false)).ToList();

                if (proc is Value.Primitive<Expression.ProcValue> procValue)
                {
                    // Create a type from the invocation
                    var argTypes = args.Select(x => x.Type).ToList();
                    // The return type is deduced with a type valiable
                    var retType = new Type.Variable();
                    // The call-site type
                    var callSiteProcType = new Type.Proc(argTypes, retType);
                    // Unify with the procedure type itself
                    procValue.Type.UnifyWith(callSiteProcType);
                    // Now actually call the procedure
                    callStack.Push(new StackFrame());
                    // Define arguments
                    foreach (var arg in args.Zip(procValue.Value.Parameters.Select(x => x.Symbol)))
                    {
                        Assert.NonNull(arg.Second);
                        callStack.Peek().Variables.Add(arg.Second, arg.First);
                    }
                    // Evaluate the body
                    Value? returnValue;
                    try
                    {
                        var value = Evaluate(callStack, procValue.Value.Body, false, lvalue);
                        returnValue = new Value.Tuple();
                    }
                    catch (ReturnValue ret)
                    {
                        returnValue = ret.Value;
                    }
                    // Now we need to do a special substitution to avoid dangling variables
                    returnValue = SubstituteVariables(returnValue, callStack.Peek().Variables);
                    callStack.Pop();
                    return returnValue;
                }
                else if (proc is Value.IntrinsicProc intrinsic)
                {
                    var argTypes = args.Select(x => x.Type).ToList();
                    // For now we ignore the return type on purpose
                    var retType = new Type.Variable();
                    // The call-site type
                    var callSiteProcType = new Type.Proc(argTypes, retType);
                    // Unify with the procedure type itself
                    intrinsic.Type.UnifyWith(callSiteProcType);
                    // Call it
                    return intrinsic.Function(args.ToList());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            case Expression.If iff:
            {
                // Evaluate condition
                var condition = Evaluate(callStack, iff.Condition, canCache, false);
                // Enforce bool condition
                Type.Bool.UnifyWith(condition.Type);
                var condValue = ((Value.Primitive<bool>)condition).Value;
                // We evaluate one, but only type-check the other
                if (condValue)
                {
                    // Evaluate then, type check else
                    var thenValue = Evaluate(callStack, iff.Then, canCache, lvalue);
                    if (iff.Else != null)
                    {
                        var elseType = TypeEval.Evaluate(iff.Else);
                        thenValue.Type.UnifyWith(elseType);
                    }
                    else
                    {
                        Type.Unit.UnifyWith(thenValue.Type);
                    }
                    return thenValue;
                }
                else if (iff.Else != null)
                {
                    var thenType = TypeEval.Evaluate(iff.Then);
                    // Evaluate else, type check then
                    var elseValue = Evaluate(callStack, iff.Else, canCache, lvalue);
                    thenType.UnifyWith(elseValue.Type);
                    return elseValue;
                }
                else
                {
                    var thenType = TypeEval.Evaluate(iff.Then);
                    Type.Unit.UnifyWith(thenType);
                }
                return new Value.Tuple();
            }

            case Expression.BinOp binOp:
                if (binOp.Operator.Type == TokenType.Assign)
                {
                    if (lvalue) throw new NotImplementedException("Assignment can't be lvalues!");

                    // We must receive our tricky type on the left-hand-side
                    var left = (Value.Lvalue)Evaluate(callStack, binOp.Left, canCache, true);
                    var right = Evaluate(callStack, binOp.Right, canCache, false);
                    left.Type.UnifyWith(right.Type);
                    // Set the value
                    left.Setter(right);
                    return right;
                }
                else
                {
                    // TODO
                    throw new NotImplementedException();
                }

            default: throw new NotImplementedException();
            }
        }

        private static Value SubstituteVariables(Value value, Dictionary<Symbol.Variable, Value> variables)
        {
            if (value is Type.Struct structType)
            {
                // This scope contains all of the constants
                var scope = structType.Scope;
                // We need to create a new scope that maked the current call stack's values constants
                var newScope = new Scope(scope.Tag, scope.Parent);
                // Here we define the current variables as constants
                foreach (var sym in variables) newScope.Define(new Symbol.Const(sym.Key.Name, sym.Value));
                // We copy out constants from the old scope
                var newConstants = new List<Declaration>();
                foreach (var sym in scope.Symbols)
                {
                    var constSym = (Symbol.Const)sym;
                    Assert.NonNull(constSym.Definition);
                    newConstants.Add(constSym.Definition.CloneDeclaration());
                }
                // We pack it up as a program so we can do batched semantic steps on them
                var constantBatch = new Declaration.Program(newConstants);
                // TODO: We could make this a separate function in Checks so we don't miss anything
                // We do the semantic steps on them
                var symbolTable = new SymbolTable();
                symbolTable.CurrentScope = newScope;
                DeclareScope.Declare(symbolTable, constantBatch);
                DeclareSymbol.Declare(constantBatch);
                DefineSymbol.Define(constantBatch);
                //TypeCheck.Check(constantBatch);
                // Wrap the new value up
                return new Type.Struct(
                    structType.Token,
                    structType.Fields.ToDictionary(kv => kv.Key, kv => kv.Value),
                    newScope);
            }
            else
            {
                // Just return it as-is
                return value;
            }
        }
    }
}
