using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Internal;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Compiler.Symbols;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Services.Impl
{
    internal class TypeService : ITypeService
    {
        private TypeTranslator typeTranslator = new TypeTranslator();

        public Type TypeOf(Expression expression)
        {
            throw new NotImplementedException();
        }

        public bool IsTypeSafe(Node node)
        {
            throw new NotImplementedException();
        }

        Type ITypeService.ToSemanticType(Lir.Values.Value value) => typeTranslator.ToSemanticType(value);
        Lir.Types.Type ITypeService.ToLirType(Type type) => typeTranslator.ToLirType(type);
    }
}
