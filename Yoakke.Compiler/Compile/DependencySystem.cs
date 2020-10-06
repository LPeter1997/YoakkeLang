using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Lir;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// The standard, global <see cref="IDependencySystem"/>.
    /// </summary>
    public class DependencySystem : IDependencySystem
    {
        public SymbolTable SymbolTable { get; }

        private Codegen codegen;
        private Dictionary<Symbol.Const, Value> constValues = new Dictionary<Symbol.Const, Value>();
        private string? procNameHint = null;

        public DependencySystem(SymbolTable symbolTable)
        {
            SymbolTable = symbolTable;
            codegen = new Codegen(this);
        }

        public Assembly? Compile(Declaration.File file, BuildStatus status)
        {
            var asm = codegen.Generate(file, status);
            if (status.Errors.Count > 0) return null;
            return asm;
        }

        public Type TypeOf(Expression expression)
        {
            // TODO
            if (expression is Expression.Proc)
            {
                return new Semantic.Type.Proc(new ValueList<Type> { }, Type.I32);
            }
            if (expression is Expression.If || expression is Expression.Call)
            {
                return Semantic.Type.I32;
            }
            if (expression is Expression.Literal lit)
            {
                if (lit.Type == TokenType.IntLiteral) return Semantic.Type.I32;
                if (lit.Type == TokenType.KwTrue) return Semantic.Type.Bool;
                if (lit.Type == TokenType.KwFalse) return Semantic.Type.Bool;
            }
            throw new NotImplementedException();
        }

        public void TypeCheck(Statement statement)
        {
            // TODO: Assume correct
        }

        public Value Evaluate(Expression expression)
        {
            // TODO: A local context should be passed that's used for cacheing!
            if (expression is Expression.Proc procExpr)
            {
                Debug.Assert(procNameHint != null);
                var procName = procNameHint;
                procNameHint = null;
                return codegen.Generate(procExpr, procName);
            }
            throw new NotImplementedException();
        }

        public Value EvaluateConst(Declaration.Const constDecl)
        {
            var symbol = (Symbol.Const)SymbolTable.DefinedSymbol(constDecl);
            // Check if there's a pre-stored value
            if (symbol.Value != null) return symbol.Value;
            // We need to evaluate based on the definition
            // Check if it's cached
            if (!constValues.TryGetValue(symbol, out var value))
            {
                // Not cached, evaluate and then cache
                if (constDecl.Value is Expression.Proc)
                {
                    procNameHint = constDecl.Name;
                }
                value = Evaluate(constDecl.Value);
                constValues.Add(symbol, value);
            }
            return value;
        }

        public Type EvaluateType(Expression expression)
        {
            if (expression is Expression.Identifier ident)
            {
                if (ident.Name == "i32") return Semantic.Type.I32;
            }
            throw new NotImplementedException();
        }

        public Lir.Types.Type TranslateToLirType(Type type) => type switch
        {
            Type.Prim prim => prim.Type,

            _ => throw new NotImplementedException(),
        };
    }
}
