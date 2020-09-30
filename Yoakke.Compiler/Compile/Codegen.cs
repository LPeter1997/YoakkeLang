using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// A code-generator that works together with a <see cref="IDependencySystem"/> to compile
    /// parts of the AST.
    /// </summary>
    public class Codegen
    {
        private IDependencySystem dependencySystem;

        // TODO: Doc
        public Codegen(IDependencySystem dependencySystem)
        {
            this.dependencySystem = dependencySystem;
        }

        // TODO: Doc
        public Assembly Generate(Declaration.File file)
        {
            // TODO
            throw new NotImplementedException();
        }

        // TODO: Doc
        public (Assembly, Proc) Generate(Expression expression)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
