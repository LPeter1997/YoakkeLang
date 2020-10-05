using System;
using System.Collections.Generic;
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

        public DependencySystem(SymbolTable symbolTable)
        {
            SymbolTable = symbolTable;
        }

        public Assembly? Compile(Declaration.File file, BuildStatus status)
        {
            var codegen = new Codegen(this);
            var asm = codegen.Generate(file, status);
            if (status.Errors.Count > 0) return null;
            return asm;
        }

        public void TypeCheck(Statement statement)
        {
            // TODO: Assume correct
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

        public Value Evaluate(Expression expression)
        {
            // TODO: A local context should be passed that's used for cacheing!
            throw new NotImplementedException();
        }

        public Type EvaluateToType(Expression expression)
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
