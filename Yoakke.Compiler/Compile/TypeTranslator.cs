using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeTranslator
    {
        public static Lir.Types.Type ToLirType(Semantic.Type type, Lir.Builder builder) => type switch
        {
            Semantic.Type.Prim prim => prim.Type,
            _ => throw new NotImplementedException(),
        };

        public static Semantic.Type ToSemanticType(Lir.Values.Value value)
        {
            throw new NotImplementedException();
        }
    }
}
