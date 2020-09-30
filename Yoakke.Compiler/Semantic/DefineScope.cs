using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// A semantic step to define the <see cref="Scope"/>s for each AST node that opens a new lexical scope.
    /// </summary>
    public class DefineScope
    {
        private SymbolTable symbolTable;
        private Scope currentScope;

        /// <summary>
        /// Initializes a new <see cref="DefineScope"/>.
        /// </summary>
        /// <param name="symbolTable">The <see cref="SymbolTable"/> to use.</param>
        public DefineScope(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
            currentScope = symbolTable.GlobalScope;
        }

        /// <summary>
        /// Defines <see cref="Scope"/>s for the given <see cref="Statement"/> and it's children.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to define inside.</param>
        public void Define(Statement statement)
        {
            switch (statement)
            {
            case Declaration.File file:
                foreach (var decl in file.Statements) Define(decl);
                break;

            case Declaration.Const cons:
                DefineNullable(cons.Type);
                Define(cons.Value);
                break;

            case Statement.Var var:
                DefineNullable(var.Type);
                DefineNullable(var.Value);
                break;

            case Statement.Return ret:
                DefineNullable(ret.Value);
                break;

            case Statement.Expression_ expr:
                Define(expr.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Defines <see cref="Scope"/>s for the given <see cref="Expression"/> and it's children.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to define inside.</param>
        public void Define(Expression expression)
        {
            switch (expression)
            {
            case Expression.Literal:
            case Expression.Identifier:
                // No-op
                break;

            case Expression.StructType sty:
                symbolTable.DefinedScope[sty] = PushScope(ScopeTag.None);
                foreach (var field in sty.Fields) Define(field.Type);
                PopScope();
                break;

            case Expression.StructValue sval:
                Define(sval.StructType);
                foreach (var field in sval.Fields) Define(field.Value);
                break;

            case Expression.ProcSignature sign:
                foreach (var param in sign.Parameters) Define(param.Type);
                DefineNullable(sign.Return);
                break;

            case Expression.Proc proc:
                symbolTable.DefinedScope[proc] = PushScope(ScopeTag.Proc);
                Define(proc.Signature);
                Define(proc.Body);
                PopScope();
                break;

            case Expression.Block block:
                symbolTable.DefinedScope[block] = PushScope(ScopeTag.None);
                foreach (var stmt in block.Statements) Define(stmt);
                DefineNullable(block.Value);
                PopScope();
                break;

            case Expression.Call call:
                Define(call.Procedure);
                foreach (var arg in call.Arguments) Define(arg);
                break;

            case Expression.If iff:
                Define(iff.Condition);
                Define(iff.Then);
                DefineNullable(iff.Else);
                break;

            case Expression.While whil:
                Define(whil.Condition);
                Define(whil.Body);
                break;

            case Expression.Binary bin:
                Define(bin.Left);
                Define(bin.Right);
                break;

            case Expression.DotPath dot:
                Define(dot.Left);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void DefineNullable(Expression? expr)
        {
            if (expr != null) Define(expr);
        }

        private Scope PushScope(ScopeTag scopeTag)
        {
            currentScope = new Scope(scopeTag, currentScope);
            return currentScope;
        }

        private void PopScope()
        {
            Debug.Assert(currentScope.Parent != null);
            currentScope = currentScope.Parent;
        }
    }
}
