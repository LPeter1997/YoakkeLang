using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Services.Impl
{
    internal class TypeService : ITypeService
    {
        public Type TypeOf(Expression expression)
        {
            throw new NotImplementedException();
        }

        public bool IsTypeSafe(Node node)
        {
            throw new NotImplementedException();
        }

        Type ITypeService.ToSemanticType(Lir.Types.Type type)
        {
            throw new NotImplementedException();
        }

        Lir.Types.Type ITypeService.ToLirType(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
