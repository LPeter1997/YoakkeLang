using System;
using System.Collections.Generic;
using System.Drawing;
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
    public class DependentProcArgs
    {
        public readonly IList<int> DependeeArgs;
        public readonly IList<int> DependentArgs;

        public DependentProcArgs(IList<int> dependeeInds, IList<int> dependentInds)
        {
            DependeeArgs = dependeeInds;
            DependentArgs = dependentInds;
        }
    }

    // TODO: Doc
    public class ElimDependentProcs : Transformator
    {
        public IDependencySystem System { get; }

        public readonly IDictionary<Expression.Proc, DependentProcArgs> ProcArgMaps 
            = new Dictionary<Expression.Proc, DependentProcArgs>();

        public ElimDependentProcs(IDependencySystem system)
        {
            System = system;
        }

        public Declaration.File Elim(Declaration.File file) => (Declaration.File)VisitNonNull(file);

        protected override Node? Visit(Expression.Identifier ident) =>
            new Expression.Identifier(ident.ParseTreeNode, ident.Name);

        protected override Expression Visit(Expression.Proc proc)
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

                var description = new DependentProcArgs(
                    dependeeArgsWithIndices.Select(pi => pi.Index).ToList(),
                    dependentArgsWithIndices.Select(pi => pi.Index).ToList());

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

                ProcArgMaps.Add(result, description);

                return result;
            }
            else
            {
                return new Expression.Proc(
                    proc.ParseTreeNode,
                    (Expression.ProcSignature)Transform(proc.Signature),
                    Transform(proc.Body));
            }
        }
    }
}
