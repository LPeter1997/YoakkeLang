using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc
    public class ElimDependentCalls : Transformator
    {
        public IDependencySystem System { get; }

        public ElimDependentCalls(IDependencySystem system)
        {
            System = system;
        }

        public Declaration.File Elim(Declaration.File file) => (Declaration.File)Transform(file);

        protected override Node? Visit(Expression.Identifier ident) =>
            new Expression.Identifier(ident.ParseTreeNode, ident.Name);

        protected override Node? Visit(Expression.Proc proc)
        {
            var procType = (Type.Proc)System.TypeOf(proc);
            if (procType.DependentTypes().Any())
            {
                // Separate dependent args from dependee ones
                var dependeeSymbols = procType.DependentTypes().Select(dt => (Symbol)dt.Symbol).ToHashSet();
                var dependeeArgs = proc.Signature.Parameters
                    .Where(p => dependeeSymbols.Contains(System.SymbolTable.DefinedSymbol(p)))
                    .ToList();
                var dependentArgs = proc.Signature.Parameters
                    .Where(p => !dependeeArgs.Contains(p))
                    .ToList();

                /*
                Desugar
                    proc(dependee args..., dependent args...) -> Ret {
                        body
                    }
                Into
                    proc(dependee args...) -> type {
                        struct { 
                            const f = proc(dependent args...) -> Ret {
                                body
                            };
                        }
                    }
                */

                var result = new Expression.Proc(
                    null,
                    new Expression.ProcSignature(
                        null,
                        dependeeArgs.Select(Transform).ToArray(),
                        new Expression.Identifier(null, "type")),
                    new Expression.StructType(
                        null,
                        new Syntax.Token(new Text.Span(), Syntax.TokenType.KwStruct, "struct"),
                        new Expression.StructType.Field[] { },
                        new Statement[]
                        {
                            new Declaration.Const(
                                null,
                                "f",
                                null,
                                new Expression.Proc(
                                    null,
                                    new Expression.ProcSignature(
                                        null,
                                        dependentArgs.Select(Transform).ToArray(),
                                        TransformNullable(proc.Signature.Return)),
                                    Transform(proc.Body)))
                        }));

                new DefineScope(System.SymbolTable).Define(result);
                new DeclareSymbol(System.SymbolTable).Declare(result);
                new ResolveSymbol(System.SymbolTable).Resolve(result);

                return result;
            }
            else
            {
                return base.Visit(proc);
            }
        }
    }
}
