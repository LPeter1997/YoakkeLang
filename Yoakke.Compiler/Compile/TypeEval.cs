using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeEval : Visitor<Type>
    {
        public IDependencySystem System { get; }

        public TypeEval(IDependencySystem system)
        {
            System = system;
        }

        // Public interface ////////////////////////////////////////////////////

        public Type TypeOf(Expression expression) => VisitNonNull(expression);

        // Implementation details //////////////////////////////////////////////

        protected override Type? Visit(Expression.StructType sty) => Type.Type_;

        protected override Type? Visit(Expression.Literal lit) => lit.Type switch
        {
            TokenType.IntLiteral => Type.I32,
            TokenType.KwTrue or TokenType.KwFalse => Type.Bool,
            _ => throw new NotImplementedException(),
        };
    }
}
