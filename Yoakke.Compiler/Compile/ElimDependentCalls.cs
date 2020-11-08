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
    public class ElimDependentCalls
    {
        public IDependencySystem System { get; }

        public ElimDependentCalls(IDependencySystem system)
        {
            System = system;
        }

        public Expression Elim(Expression.Proc proc)
        {
            var procType = (Type.Proc)System.TypeOf(proc);
            if (procType.DependentTypes().Any())
            {
                // Separate dependent args from dependee ones
                var dependeeSymbols = procType.DependentTypes().Select(dt => (Symbol)dt.Symbol).ToHashSet();
                var dependeeArgsWithIndices = proc.Signature.Parameters
                    .Select((p, i) => (Param: p, Index: i))
                    .Where(pi => dependeeSymbols.Contains(System.SymbolTable.DefinedSymbol(pi.Param)))
                    .ToList();
                var dependentArgsWithIndices = proc.Signature.Parameters
                    .Select((p, i) => (Param: p, Index: i))
                    .Except(dependeeArgsWithIndices)
                    .ToList();

                var dependeeArgs = dependeeArgsWithIndices.Select(pi => pi.Param).ToArray();
                var dependentArgs = dependentArgsWithIndices.Select(pi => pi.Param).ToArray();

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
                        dependeeArgs,
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
                                        dependentArgs,
                                        proc.Signature.Return),
                                    proc.Body))
                        }));

                new DefineScope(System.SymbolTable).Define(result);
                new DeclareSymbol(System.SymbolTable).Declare(result);
                new ResolveSymbol(System.SymbolTable).Resolve(result);

                return result;
            }
            else
            {
                return proc;
            }
        }
    }
}
