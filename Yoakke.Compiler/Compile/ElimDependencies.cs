using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc
    public class ElimDependencies : Transformator
    {
        public IDependencySystem System { get; }

        private DependencyMap dependencyMap = new DependencyMap();

        public ElimDependencies(IDependencySystem system)
        {
            System = system;
        }

        public Declaration.File Elim(Declaration.File file)
        {
            var collector = new CollectDependencies(System);
            collector.Collect(file);
            dependencyMap = collector.DependencyMap;
            var result = (Declaration.File)Transform(file);
            System.ResetSymbolTable();
            SymbolResolution.Resolve(System.SymbolTable, result);
            //Console.WriteLine(result.Dump());
            return result;
        }

        protected override Node? Visit(Expression.Identifier ident) =>
            new Expression.Identifier(ident.ParseTreeNode, ident.Name);

        protected override Node? Visit(Expression.Proc proc)
        {
            if (dependencyMap.ProcDesugar.TryGetValue(proc, out var alt))
            {
                return Transform(alt);
            }
            return base.Visit(proc);
        }

        protected override Node? Visit(Expression.Call call)
        {
            if (dependencyMap.CallDesugar.TryGetValue(call, out var alt))
            {
                return Transform(alt);
            }
            return base.Visit(call);
        }
    }
}
