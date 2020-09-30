using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// A semantic pass to check static typing rules.
    /// </summary>
    public class TypeChecker : Visitor<object>
    {
        // TODO: Doc
        public void Check(Statement statement)
        {
            throw new NotImplementedException();
        }

        // TODO: Doc
        public void Check(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
