using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeCheck : Visitor<object>
    {
        public IDependencySystem System { get; }

        public TypeCheck(IDependencySystem system)
        {
            System = system;
        }

        public void Check(Statement statement)
        {
            // TODO
        }
    }
}
