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
using Yoakke.Syntax;
using Yoakke.Compiler.Semantic;

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

            // NOTE: Edge-case
            SemaType.Dependent => LirType.User_,
            
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
                        // Inner scope
                        Semantic.Scope? scope = null;
                        if (structType.Declarations.Count > 0)
                        {
                            scope = System.SymbolTable.ContainingScope(structType.Declarations.First());
                        }
                        // Do the new surrounding scope thing
                        var containingScope = System.SymbolTable.ContainingScope(structType);
                        var newSurroundingScope = new Semantic.Scope(Semantic.ScopeKind.Struct, containingScope);
                        var symbols = (Value.Array)((Value.User)ctorTypes.First()).Payload;
                        foreach (var symbolAndValuePair in symbols.Values.Select(v => (Value.Array)((Value.User)v).Payload))
                        {
                            var symbol = (Semantic.Symbol.Var)((Value.User)symbolAndValuePair.Values[0]).Payload;
                            var symbolValue = symbolAndValuePair.Values[1];
                            Debug.Assert(symbol.Type != null);
                            newSurroundingScope.Define(new Semantic.Symbol.Const(symbol.Name, symbol.Type, symbolValue));
                        }
                        {
                            var cloner = new Cloner();
                            var decls = structType.Declarations.Select(cloner.Clone).ToList();
                            System.SymbolTable.CurrentScope = newSurroundingScope;
                            foreach (var decl in decls)
                            {
                                new DefineScope(System.SymbolTable).Define(decl);
                                new DeclareSymbol(System.SymbolTable).Declare(decl);
                                new ResolveSymbol(System.SymbolTable).Resolve(decl);
                            }
                        }
                        return new SemaType.Struct(
                            structType.KwStruct,
                            structType.Fields
                                .Select(field => field.Name)
                                .Zip(ctorTypes.Skip(1).Select(ToSemanticType))
                                .ToDictionary(kv => kv.First, kv => kv.Second),
                            newSurroundingScope);
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
