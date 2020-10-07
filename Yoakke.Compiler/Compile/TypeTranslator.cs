using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeTranslator
    {
        public Lir.Types.Type ToLirType(Semantic.Type type, Lir.Builder builder) => type switch
        {
            Semantic.Type.Prim prim => prim.Type,
            Semantic.Type.Proc proc =>
                new Lir.Types.Type.Proc(
                    Lir.CallConv.Cdecl,
                    ToLirType(proc.Return, builder),
                    proc.Parameters.Select(param => ToLirType(param, builder)).ToList().AsValueList()
                ),
            _ => throw new NotImplementedException(),
        };

        public int FieldIndex(Semantic.Type type, string fieldName)
        {
            throw new NotImplementedException();
        }

        public static Semantic.Type ToSemanticType(Lir.Values.Value value)
        {
            if (value is Lir.Values.Value.User user)
            {
                if (user.Payload is Semantic.Type type) return type;
            }
            throw new NotImplementedException();
        }
    }
}
