using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Symbols;
using Yoakke.Compiler.Symbols.Impl;
using Yoakke.DataStructures;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Internal
{
    internal class TypeTranslator
    {
        private Dictionary<(Semantic.Types.Type, string), int> fieldIndices = new Dictionary<(Semantic.Types.Type, string), int>();
        private int structCnt;
        //private ISymbolTable symbolTable;

        public TypeTranslator(/* ISymbolTable symbolTable */)
        {
            //this.symbolTable = symbolTable;
        }

        public Lir.Types.Type ToLirType(Semantic.Types.Type type) => type switch
        {
            Semantic.Types.Type.Prim prim => prim.Type,

            Semantic.Types.Type.Ptr ptr => new Lir.Types.Type.Ptr(ToLirType(ptr.Subtype)),

            Semantic.Types.Type.Proc proc =>
                new Lir.Types.Type.Proc(
                    Lir.CallConv.Cdecl,
                    ToLirType(proc.Return),
                    proc.Parameters.Select(param => ToLirType(param.Type)).ToList()),

            Semantic.Types.Type.Struct struc => ToLirStruct(struc),

            Semantic.Types.Type.Array array =>
                new Lir.Types.Type.Array(
                    ToLirType(array.ElementType),
                    array.Length),

            // NOTE: Edge-case
            // TODO: We'll need to yeet this after dependent types are gone
            Semantic.Types.Type.Dependent => Lir.Types.Type.User_,

            _ => throw new NotImplementedException(),
        };

        private Lir.Types.Type ToLirStruct(Semantic.Types.Type.Struct struc)
        {
            var fields = new ValueList<Lir.Types.Type>();
            int counter = 0;
            foreach (var field in struc.Fields)
            {
                fieldIndices[(struc, field.Key)] = counter;
                fields.Add(ToLirType(field.Value.Type.Value));
                ++counter;
            }
            var structName = $"struct{structCnt++}";
            var result = new Lir.Struct(structName);
            foreach (var field in fields) result.Fields.Add(field);
            return result;
        }

        public int FieldIndex(Semantic.Types.Type type, string fieldName) => fieldIndices[(type, fieldName)];

        public Semantic.Types.Type ToSemanticType(Lir.Values.Value value)
        {
            var user = (Lir.Values.Value.User)value;
            if (user.Payload is Semantic.Types.Type type) return type;
            if (user.Payload is Lir.Values.Value.Array array)
            {
                var tagType = ((Lir.Values.Value.User)array.Values[0]).Payload;
                var ctorTypes = array.Values.Skip(1);
                if (tagType is Expression.Unary ury
                    && ury.Operator == Expression.UnaryOp.PointerType)
                {
                    var payload = ((Lir.Values.Value.User)ctorTypes.First()).Payload;
                    return new Semantic.Types.Type.Ptr(ToSemanticType(ctorTypes.First()));
                }
                if (tagType is Expression.ArrayType)
                {
                    var ctorValues = ctorTypes.ToList();
                    var length = (Lir.Values.Value.Int)((Lir.Values.Value.User)ctorValues[0]).Payload;
                    var elementType = ToSemanticType(ctorValues[1]);
                    return new Semantic.Types.Type.Array(elementType, (int)length.Value);
                }
                // TODO: We need to clone the resulting struct
                // And finish this part
#if false
                if (tagType is Expression.StructType structType)
                {
                    // Define the scope that the struct will contain the symbols in
                    var parentScope = symbolTable.ContainingScope(structType);
                    IScope innerScope = new Scope.Struct(parentScope);
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
                    var cloner = new Cloner();
                    {
                        var decls = structType.Declarations.Select(cloner.Clone).ToList();
                        System.SymbolTable.CurrentScope = newSurroundingScope;
                        foreach (var decl in decls)
                        {
                            // TODO: not below
                            // NOTE: do we need to take care of compile status here?
                            SymbolResolution.Resolve(System.SymbolTable, decl);
                            // TODO: We screw upp associated constants with cloning!
                            // It was kind of a bad system anyway, so it's no biggie it's broken
                            // For now we type-check here to avoid crashing the test-suite
                            //System.TypeCheck(decl);
                        }
                    }
                    return new Semantic.Types.Type.Struct(
                        newSurroundingScope,
                        structType.KwStruct,
                        structType.Fields
                            .ToDictionary(
                                field => field.Name,
                                field =>
                                {
                                    var newType = cloner.Clone(field.Type);
                                    // TODO: note below
                                    // NOTE: do we need to take care of compile status here?
                                    SymbolResolution.Resolve(System.SymbolTable, newType);
                                    return new Semantic.Types.Type.Struct.Field(
                                        new DataStructures.Lazy<Semantic.Types.Type>(() => System.EvaluateType(newType)),
                                        field);
                                }));
                }
#endif
                if (tagType is Expression.ProcSignature procSign)
                {
                    var paramTypes = ctorTypes.SkipLast(1);
                    var retType = ctorTypes.Last();
                    return new Semantic.Types.Type.Proc(
                        paramTypes.Zip(procSign.Parameters).Select(ppair =>
                        {
                            //var pSym = symbolTable.AssociatedSymbol(ppair.Second);
                            var pType = ToSemanticType(ppair.First);
                            // TODO: Fix this
                            throw new NotImplementedException();
#pragma warning disable CS0162 // Unreachable code detected
                            return (Semantic.Types.Type.Proc.Param)null;
#pragma warning restore CS0162 // Unreachable code detected
                              // return new Semantic.Types.Type.Proc.Param(pSym, pType);
                        }).ToList(),
                        ToSemanticType(retType));
                }
            }

            throw new NotImplementedException();
        }
    }
}
