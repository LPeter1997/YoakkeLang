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
        public IDependencySystem System { get; }

        private Dictionary<(Semantic.Types.Type, string), int> fieldIndices = new Dictionary<(Semantic.Types.Type, string), int>();

        public TypeTranslator(IDependencySystem system)
        {
            System = system;
        }

        public Lir.Types.Type ToLirType(Semantic.Types.Type type, Lir.Builder builder) => type switch
        {
            Semantic.Types.Type.Prim prim => prim.Type,

            Semantic.Types.Type.Ptr ptr => new Lir.Types.Type.Ptr(ToLirType(ptr.Subtype, builder)),

            Semantic.Types.Type.Proc proc =>
                new Lir.Types.Type.Proc(
                    Lir.CallConv.Cdecl,
                    ToLirType(proc.Return, builder),
                    proc.Parameters.Select(param => ToLirType(param.Type, builder)).ToList().AsValueList()),
            
            Semantic.Types.Type.Struct struc => ToLirStruct(struc, builder),

            Semantic.Types.Type.Array array => 
                new Lir.Types.Type.Array(
                    ToLirType(array.ElementType, builder),
                    array.Length),
            
            _ => throw new NotImplementedException(),
        };

        private Lir.Types.Type ToLirStruct(Semantic.Types.Type.Struct struc, Lir.Builder builder)
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

        public int FieldIndex(Semantic.Types.Type type, string fieldName) => fieldIndices[(type, fieldName)];

        public Semantic.Types.Type ToSemanticType(Lir.Values.Value value)
        {
            if (value is Lir.Values.Value.User user)
            {
                if (user.Payload is Semantic.Types.Type type) return type;
                if (user.Payload is Lir.Values.Value.Array array)
                {
                    var tagType = ((Lir.Values.Value.User)array.Values[0]).Payload;
                    var ctorTypes = array.Values.Skip(1);
                    if (tagType is Expression.Unary ury
                        && ury.Operator == Expression.UnaryOp.PointerType)
                    {
                        return new Semantic.Types.Type.Ptr(ToSemanticType(ctorTypes.First()));
                    }
                    if (tagType is Expression.ArrayType)
                    {
                        var ctorValues = ctorTypes.ToList();
                        var length = (Lir.Values.Value.Int)((Lir.Values.Value.User)ctorValues[0]).Payload;
                        var elementType = ToSemanticType(ctorValues[1]);
                        return new Semantic.Types.Type.Array(elementType, (int)length.Value);
                    }
                    if (tagType is Expression.StructType structType)
                    {
                        // NOTE: This will not be correct for generics!
                        Semantic.Scope? scope = null;
                        if (structType.Declarations.Count > 0)
                        {
                            scope = System.SymbolTable.ContainingScope(structType.Declarations.First());
                        }
                        return new Semantic.Types.Type.Struct(
                            structType.KwStruct,
                            structType.Fields
                                .Select(field => field.Name)
                                .Zip(ctorTypes.Select(ToSemanticType))
                                .ToDictionary(kv => kv.First, kv => kv.Second)
                                .AsValueDictionary(),
                            scope);
                    }
                    if (tagType is Expression.ProcSignature procSign)
                    {
                        var paramTypes = ctorTypes.SkipLast(1);
                        var retType = ctorTypes.Last();
                        return new Semantic.Types.Type.Proc(
                            paramTypes.Select(p => 
                            { 
                                if (p is Lir.Values.Value.User user && user.Payload is Lir.Values.Value.Array arr)
                                {
                                    var name = (string)((Lir.Values.Value.User)arr.Values[0]).Payload;
                                    var ty = ToSemanticType(arr.Values[1]);
                                    return new Semantic.Types.Type.Proc.Param(name, ty);
                                }
                                else
                                {
                                    return new Semantic.Types.Type.Proc.Param(null, ToSemanticType(p));
                                }
                            }).ToList().AsValueList(),
                            ToSemanticType(retType));
                    }
                }
            }
            
            throw new NotImplementedException();
        }
    }
}
