using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Services.Impl
{
    internal class TypeService : ITypeService
    {
        public Semantic.Types.Type TypeOf(Expression expression)
        {
            throw new NotImplementedException();
        }

        public bool IsTypeSafe(Node node)
        {
            throw new NotImplementedException();
        }
    }
}
