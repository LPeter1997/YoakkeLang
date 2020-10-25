using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Syntax.Ast;
using SemaType = Yoakke.Compiler.Semantic.Types.Type;
using LirType = Yoakke.Lir.Types.Type;
using Yoakke.Lir.Values;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public class TypeTranslator
    {
        public IDependencySystem System { get; }

        private Dictionary<(SemaType, string), int> fieldIndices = new Dictionary<(SemaType, string), int>();

        public TypeTranslator(IDependencySystem system)
        {
            System = system;
        }

        public LirType ToLirType(SemaType type) => type switch
        {
            SemaType.Prim prim => prim.Type,

            SemaType.Ptr ptr => new LirType.Ptr(ToLirType(ptr.Subtype)),

            SemaType.Proc proc =>
                new LirType.Proc(
                    Lir.CallConv.Cdecl,
                    ToLirType(proc.Return),
                    proc.Parameters.Select(param => ToLirType(param.Type)).ToList()),

            SemaType.Struct struc => ToLirStruct(struc),

            SemaType.Array array => 
                new LirType.Array(
                    ToLirType(array.ElementType),
                    array.Length),
            
            _ => throw new NotImplementedException(),
        };

        private LirType ToLirStruct(SemaType.Struct struc)
        {
            var fields = new List<LirType>();
            int counter = 0;
            foreach (var field in struc.Fields)
            {
                fieldIndices[(struc, field.Key)] = counter;
                fields.Add(ToLirType(field.Value));
                ++counter;
            }
            return System.Builder.DefineStruct(fields);
        }

        public int FieldIndex(SemaType type, string fieldName) => fieldIndices[(type, fieldName)];

        public SemaType ToSemanticType(Lir.Values.Value value)
        {
            if (value is Value.User user)
            {
                if (user.Payload is SemaType type) return type;
                if (user.Payload is Value.Array array)
                {
                    var tagType = ((Value.User)array.Values[0]).Payload;
                    var ctorTypes = array.Values.Skip(1);
                    if (tagType is Expression.Unary ury
                        && ury.Operator == Expression.UnaryOp.PointerType)
                    {
                        return new SemaType.Ptr(ToSemanticType(ctorTypes.First()));
                    }
                    if (tagType is Expression.ArrayType)
                    {
                        var ctorValues = ctorTypes.ToList();
                        var length = (Value.Int)((Value.User)ctorValues[0]).Payload;
                        var elementType = ToSemanticType(ctorValues[1]);
                        return new SemaType.Array(elementType, (int)length.Value);
                    }
                    if (tagType is Expression.StructType structType)
                    {
                        // NOTE: This will not be correct for generics!
                        Semantic.Scope? scope = null;
                        if (structType.Declarations.Count > 0)
                        {
                            scope = System.SymbolTable.ContainingScope(structType.Declarations.First());
                        }
                        return new SemaType.Struct(
                            structType.KwStruct,
                            structType.Fields
                                .Select(field => field.Name)
                                .Zip(ctorTypes.Select(ToSemanticType))
                                .ToDictionary(kv => kv.First, kv => kv.Second),
                            scope);
                    }
                    if (tagType is Expression.ProcSignature procSign)
                    {
                        var paramTypes = ctorTypes.SkipLast(1);
                        var retType = ctorTypes.Last();
                        return new SemaType.Proc(
                            paramTypes.Zip(procSign.Parameters).Select(ppair => 
                            {
                                var pName = ppair.Second.Name;
                                var pType = ToSemanticType(ppair.First);
                                return new SemaType.Proc.Param(pName, pType);
                            }).ToList(),
                            ToSemanticType(retType));
                    }
                }
            }
            
            throw new NotImplementedException();
        }
    }
}
