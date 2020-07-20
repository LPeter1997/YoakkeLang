using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Compiler.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Defines and stores the <see cref="Scope"/> for every syntax <see cref="Node"/>.
    /// </summary>
    static class DeclareScope
    {
        /// <summary>
        /// Defines and stores the <see cref="Scope"/> for every syntax <see cref="Node"/>.
        /// <param name="symbolTable">The <see cref="SymbolTable"/> to use.</param>
        /// <param name="statement">The <see cref="Statement"/> to define the <see cref="Scope"/>s for.</param>
        /// </summary>
        public static void Declare(SymbolTable symbolTable, Statement statement)
        {
            statement.Scope = symbolTable.CurrentScope;

            switch (statement)
            {
            case Declaration.Program program:
                // Just loop through every declaration
                foreach (var decl in program.Declarations) Declare(symbolTable, decl);
                break;

            case Declaration.ConstDef constDef:
            {
                // For safety, declare in type
                if (constDef.Type != null) Declare(symbolTable, constDef.Type);
                // First declare everything in value
                Declare(symbolTable, constDef.Value);
            }
            break;

            case Statement.Return ret:
                // Declare in value, if needed
                if (ret.Value != null) Declare(symbolTable, ret.Value);
                break;

            case Statement.VarDef varDef:
                // Declare in type of needed
                if (varDef.Type != null) Declare(symbolTable, varDef.Type);
                // Declare in value
                Declare(symbolTable, varDef.Value);
                break;

            case Statement.Expression_ expression:
                // Declare in the expression
                Declare(symbolTable, expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Declare(SymbolTable symbolTable, Expression expression)
        {
            expression.Scope = symbolTable.CurrentScope;

            switch (expression)
            {
            case Expression.IntLit _:
            case Expression.BoolLit _:
            case Expression.StrLit _:
            case Expression.Ident _:
                // Nothing to declare, leaf nodes
                break;

            case Expression.DotPath dotPath:
                // Just declare in left, right-hand-side is just a token
                Declare(symbolTable, dotPath.Left);
                break;

            case Expression.StructType structType:
                symbolTable.PushScope(ScopeTag.None);
                // Declare in field types
                foreach (var field in structType.Fields) Declare(symbolTable, field.Type);
                // Declare in declarations
                foreach (var decl in structType.Declarations) Declare(symbolTable, decl);
                symbolTable.PopScope();
                break;

            case Expression.StructValue structValue:
                // Declare in the struct type expression
                Declare(symbolTable, structValue.StructType);
                // Declare in field values
                foreach (var field in structValue.Fields) Declare(symbolTable, field.Value);
                break;

            case Expression.ProcSignature procType:
                // Declare in parameter types
                foreach (var param in procType.Parameters) Declare(symbolTable, param.Type);
                // Declare in return type, if needed
                if (procType.ReturnType != null) Declare(symbolTable, procType.ReturnType);
                break;

            case Expression.ProcValue proc:
                // Processes introduce a scope for their signature
                symbolTable.PushScope(ScopeTag.Proc);
                // Declare in parameters
                foreach (var param in proc.Signature.Parameters) Declare(symbolTable, param.Type);
                // Declare in return-type
                if (proc.Signature.ReturnType != null) Declare(symbolTable, proc.Signature.ReturnType);
                // Declare in body
                Declare(symbolTable, proc.Body);
                symbolTable.PopScope();
                break;

            case Expression.Block block:
                // Blocks introduce a scope
                symbolTable.PushScope(ScopeTag.None);
                // Declare in each statement
                foreach (var stmt in block.Statements) Declare(symbolTable, stmt);
                // In return value too
                if (block.Value != null) Declare(symbolTable, block.Value);
                symbolTable.PopScope();
                break;

            case Expression.Call call:
                // Declare in called procedure
                Declare(symbolTable, call.Proc);
                // Declare in arguments
                foreach (var arg in call.Arguments) Declare(symbolTable, arg);
                break;

            case Expression.If iff:
                // Declare in condition and then
                Declare(symbolTable, iff.Condition);
                Declare(symbolTable, iff.Then);
                // Declare in else if needed
                if (iff.Else != null) Declare(symbolTable, iff.Else);
                break;

            case Expression.BinOp binOp:
                // Declare in both left and right
                Declare(symbolTable, binOp.Left);
                Declare(symbolTable, binOp.Right);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
