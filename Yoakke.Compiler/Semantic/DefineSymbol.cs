using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Compiler.Ast;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Defines every symbol, also resolves each symbol reference.
    /// </summary>
    static class DefineSymbol
    {
        /// <summary>
        /// Defines the order-dependent symbols, resolves symbol references.
        /// </summary>
        /// <param name="statement">The statement to start the definitions from.</param>
        public static void Define(Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                // Just loop through every declaration
                foreach (var decl in program.Declarations) Define(decl);
                break;

            case Declaration.ConstDef constDef:
                // For safety, define in type
                if (constDef.Type != null) Define(constDef.Type);
                // Define everything in value
                Define(constDef.Value);
                break;

            case Statement.Return ret:
                // Define in value, if needed
                if (ret.Value != null) Define(ret.Value);
                break;

            case Statement.VarDef varDef:
                // Define in type of needed
                if (varDef.Type != null) Define(varDef.Type);
                // Define in value
                Define(varDef.Value);
                // Declare this symbol, store, add it to symbol table
                Assert.NonNull(varDef.Scope);
                varDef.Symbol = new Symbol.Variable(varDef.Name);
                varDef.Scope.Define(varDef.Symbol);
                break;

            case Statement.Expression_ expression:
                // Define in the expression
                Define(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Define(Expression expression)
        {
            switch (expression)
            {
            case Expression.IntLit _:
            case Expression.BoolLit _:
            case Expression.StrLit _:
                // Nothing to define, primitive literal
                break;

            case Expression.Ident identifier:
                // We want to resolve the referred symbol
                Assert.NonNull(identifier.Scope);
                identifier.Symbol = identifier.Scope.Reference(identifier.Token);
                break;

            case Expression.Intrinsic intrinsic:
                // Resolve the referred symbol
                // It must be an intrinsic symbol type
                Assert.NonNull(intrinsic.Scope);
                intrinsic.Symbol = (Symbol.Intrinsic)intrinsic.Scope.Reference(intrinsic.Token);
                break;

            case Expression.DotPath dotPath:
                // Just define in left, right-hand-side is just a token
                Define(dotPath.Left);
                break;

            case Expression.StructType structType:
                // Define in field types
                foreach (var field in structType.Fields) Define(field.Type);
                // Define in declarations
                foreach (var decl in structType.Declarations) Define(decl);
                break;

            case Expression.StructValue structValue:
                // Define in the struct type expression
                Define(structValue.StructType);
                // Define in field values
                foreach (var field in structValue.Fields) Define(field.Value);
                break;

            case Expression.ProcType procType:
                // Define inside each parameter type
                foreach (var param in procType.ParameterTypes) Define(param);
                // Define in return type, if needed
                if (procType.ReturnType != null) Define(procType.ReturnType);
                break;

            case Expression.ProcValue proc:
                // We define each parameter
                foreach (var param in proc.Parameters)
                {
                    // Also define inside each parameter type
                    Define(param.Type);
                    param.Symbol = new Symbol.Variable(param.Name);
                    // We use the type's scope to inject, as that's the same as the procedure's scope
                    Assert.NonNull(param.Type.Scope);
                    param.Type.Scope.Define(param.Symbol);
                }
                // Define in return-type
                if (proc.ReturnType != null) Define(proc.ReturnType);
                // Define in body
                Define(proc.Body);
                break;

            case Expression.Block block:
                // Define in each statement
                foreach (var stmt in block.Statements) Define(stmt);
                // In return value too
                if (block.Value != null) Define(block.Value);
                break;

            case Expression.Call call:
                // Define in the called expression
                Define(call.Proc);
                // Define in arguments
                foreach (var arg in call.Arguments) Define(arg);
                break;

            case Expression.If iff:
                // Define in condition and then
                Define(iff.Condition);
                Define(iff.Then);
                // Define in else if needed
                if (iff.Else != null) Define(iff.Else);
                break;

            case Expression.BinOp binOp:
                // Define in left and right
                Define(binOp.Left);
                Define(binOp.Right);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
