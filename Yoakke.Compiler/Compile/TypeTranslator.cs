using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeTranslator
    {
        private Dictionary<(Semantic.Type, string), int> fieldIndices = new Dictionary<(Semantic.Type, string), int>();

        public Lir.Types.Type ToLirType(Semantic.Type type, Lir.Builder builder) => type switch
        {
            Semantic.Type.Prim prim => prim.Type,

            Semantic.Type.Ptr ptr => new Lir.Types.Type.Ptr(ToLirType(ptr.Subtype, builder)),

            Semantic.Type.Proc proc =>
                new Lir.Types.Type.Proc(
                    Lir.CallConv.Cdecl,
                    ToLirType(proc.Return, builder),
                    proc.Parameters.Select(param => ToLirType(param, builder)).ToList().AsValueList()),
            
            Semantic.Type.Struct struc => ToLirStruct(struc, builder),

            Semantic.Type.Array array => 
                new Lir.Types.Type.Array(
                    ToLirType(array.ElementType, builder),
                    array.Length),
            
            _ => throw new NotImplementedException(),
        };

        private Lir.Types.Type ToLirStruct(Semantic.Type.Struct struc, Lir.Builder builder)
        {
            var fields = new List<Lir.Types.Type>();
            int counter = 0;
            foreach (var field in struc.Fields)
            {
                fieldIndices[(struc, field.Key)] = counter;
                fields.Add(ToLirType(field.Value, builder));
                ++counter;
            }
            return builder.DefineStruct(fields);
        }

        public int FieldIndex(Semantic.Type type, string fieldName) => fieldIndices[(type, fieldName)];

        public static Semantic.Type ToSemanticType(Lir.Values.Value value)
        {
            if (value is Lir.Values.Value.User user)
            {
                if (user.Payload is Semantic.Type type) return type;
                if (user.Payload is Lir.Runtime.ArrayValue array)
                {
                    var tagType = ((Lir.Values.Value.User)array.Values[0]).Payload;
                    var ctorTypes = array.Values.Skip(1);
                    if (   tagType is Expression.Unary ury 
                        && ury.Operator == Expression.UnaryOperator.PointerType)
                    {
                        return new Semantic.Type.Ptr(ToSemanticType(ctorTypes.First()));
                    }
                    if (tagType is Expression.ArrayType)
                    {
                        var ctorValues = ctorTypes.ToList();
                        var length = (Lir.Values.Value.Int)ctorValues[0];
                        var elementType = ToSemanticType(ctorValues[1]);
                        return new Semantic.Type.Array(elementType, (int)length.Value);
                    }
                    if (tagType is Expression.StructType structType)
                    {
                        return new Semantic.Type.Struct(
                            structType.KwStruct,
                            structType.Fields
                                .Select(field => field.Name)
                                .Zip(ctorTypes.Select(ToSemanticType))
                                .ToDictionary(kv => kv.First, kv => kv.Second)
                                .AsValueDictionary());
                    }
                }
            }
            
            throw new NotImplementedException();
        }
    }
}
