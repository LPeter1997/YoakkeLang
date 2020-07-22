using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Compiler.Ast;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Declares every order-independent <see cref="Symbol"/>.
    /// </summary>
    static class DeclareSymbol
    {
        /// <summary>
        /// Declares every order-independent <see cref="Symbol"/> in it's respective <see cref="Scope"/>.
        /// </summary>
        /// <param name="statement">The statement to start the declarations from.</param>
        public static void Declare(Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                // Just loop through every declaration
                foreach (var decl in program.Declarations) Declare(decl);
                break;

            case Declaration.ConstDef constDef:
                // For safety, declare in type
                if (constDef.Type != null) Declare(constDef.Type);
                // First declare everything in value
                Declare(constDef.Value);
                // Declare this symbol, store it and add to the scope
                Assert.NonNull(constDef.Scope);
                constDef.Symbol = new Symbol.Const(constDef);
                constDef.Scope.Define(constDef.Symbol);
                break;

            case Statement.Return ret:
                // Declare in value, if needed
                if (ret.Value != null) Declare(ret.Value);
                break;

            case Statement.VarDef varDef:
                // Declare in type if needed
                if (varDef.Type != null) Declare(varDef.Type);
                // Declare in value if needed
                if (varDef.Value != null) Declare(varDef.Value);
                break;

            case Statement.Expression_ expression:
                // Declare in the expression
                Declare(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Declare(Expression expression)
        {
            switch (expression)
            {
            case Expression.IntLit _:
            case Expression.BoolLit _:
            case Expression.StrLit _:
            case Expression.Ident _:
            case Expression.VarType _:
                // Nothing to declare, leaf nodes
                break;

            case Expression.DotPath dotPath:
                // Just declare in left, right-hand-side is just a token
                Declare(dotPath.Left);
                break;

            case Expression.StructType structType:
                // Declare in field types
                foreach (var field in structType.Fields) Declare(field.Type);
                // Declare in declarations
                foreach (var decl in structType.Declarations) Declare(decl);
                break;

            case Expression.StructValue structValue:
                // Declare in the struct type expression
                Declare(structValue.StructType);
                // Declare in field values
                foreach (var field in structValue.Fields) Declare(field.Value);
                break;

            case Expression.ProcSignature procType:
                // Declare in parameter types
                foreach (var param in procType.Parameters) Declare(param.Type);
                // Declare in return type, if needed
                if (procType.ReturnType != null) Declare(procType.ReturnType);
                break;

            case Expression.ProcValue proc:
                // Declare in parameters
                foreach (var param in proc.Signature.Parameters) Declare(param.Type);
                // Declare in return-type
                if (proc.Signature.ReturnType != null) Declare(proc.Signature.ReturnType);
                // Declare in body
                Declare(proc.Body);
                break;

            case Expression.Block block:
                // Declare in each statement
                foreach (var stmt in block.Statements) Declare(stmt);
                // In return value too
                if (block.Value != null) Declare(block.Value);
                break;

            case Expression.Call call:
                // Declare in called procedure
                Declare(call.Proc);
                // Declare in arguments
                foreach (var arg in call.Arguments) Declare(arg);
                break;

            case Expression.If iff:
                // Declare in condition and then
                Declare(iff.Condition);
                Declare(iff.Then);
                // Declare in else if needed
                if (iff.Else != null) Declare(iff.Else);
                break;

            case Expression.While whil:
                // Declare in condition and body
                Declare(whil.Condition);
                Declare(whil.Body);
                break;

            case Expression.BinOp binOp:
                // Declare in both left and right
                Declare(binOp.Left);
                Declare(binOp.Right);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
