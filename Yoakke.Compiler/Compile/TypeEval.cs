using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// An object to resolve the type of values in the AST.
    /// </summary>
    public class TypeEval : Visitor<Type>
    {
        // TODO: Doc
        public Type TypeOf(Expression expression)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
