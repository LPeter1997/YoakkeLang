using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// The dependency system that incrementally compiles the result.
    /// </summary>
    public class DependencySystem : IDependencySystem
    {
        private Codegen codegen;
        private TypeChecker typeChecker;

        // TODO: Doc
        public DependencySystem()
        {
            codegen = new Codegen(this);
            typeChecker = new TypeChecker();
        }

        public Assembly Compile(Declaration.File file)
        {
            typeChecker.Check(file);
            return codegen.Generate(file);
        }

        public Value Evaluate(Expression expression)
        {
            typeChecker.Check(expression);
            var (assembly, proc) = codegen.Generate(expression);
            var vm = new VirtualMachine(assembly);
            return vm.Execute(proc, Enumerable.Empty<Value>());
        }

        public Semantic.Type EvaluateToType(Expression expression)
        {
            var value = Evaluate(expression);
            var user = (Value.User)value;
            return (Semantic.Type)user.Payload;
        }
    }
}
