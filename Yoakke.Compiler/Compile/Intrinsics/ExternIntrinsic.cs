using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Compile.Intrinsics
{
    // TODO: Doc
    public class ExternIntrinsic : Intrinsic
    {
        private class Impl : Intrinsic
        {
            public override Lir.Types.Type ReturnType { get; }
            public override Type Type { get; }

            public Impl(IDependencySystem system, Type returnType) 
                : base(system)
            {
                var paramSymbol = new Symbol.Var(null, "path", Symbol.VarKind.Param);
                var stringType = System.ReferToConstType("@c", "str");
                Type = new Type.Proc(true, new List<Type.Proc.Param> { new Type.Proc.Param(paramSymbol, stringType) }, returnType);
                ReturnType = System.TypeTranslator.ToLirType(returnType);
            }

            public override Value Execute(VirtualMachine vm, IEnumerable<Value> args)
            {
                // TODO: Check args
                var name = ReadString(vm, args.First());
                var external = new Lir.Extern(name, ReturnType, null); // TODO: Path?
                return external;
            }

            // TODO: Factor out utility to read string
            private static string ReadString(VirtualMachine vm, Value str)
            {
                // TODO: Proper errors
                var strStruct = (Value.Struct)str;
                var strPtr = strStruct.Values[0];
                var strLen = (int)((Value.Int)strStruct.Values[1]).Value;
                return vm.ReadUtf8FromMemory(strPtr, strLen);
            }
        }

        public class Undependent : Intrinsic
        {
            public override Lir.Types.Type ReturnType => Lir.Types.Type.User_;
            public override Type Type { get; }

            public Undependent(IDependencySystem system) 
                : base(system)
            {
                // TODO: Ease building these?
                var typeParamSymbol = new Symbol.Var(null, "T", Symbol.VarKind.Param);
                Type = new Type.Proc(
                    true,
                    new List<Type.Proc.Param> {
                        new Type.Proc.Param(typeParamSymbol, Type.Type_),
                    },
                    Type.Type_);
            }

            public override Value Execute(VirtualMachine vm, IEnumerable<Value> args)
            {
                // TODO: Check args
                var typeParam = System.TypeTranslator.ToSemanticType(args.First());
                // We need to create a struct type that contains the constants in the file
                // First we define the new scope
                System.SymbolTable.PushScope(Semantic.ScopeKind.Struct);
                var structScope = System.SymbolTable.CurrentScope;
                // Define things
                var impl = new Impl(System, typeParam);
                structScope.Define(new Symbol.Const("f", impl.Type, new Value.User(impl)));
                // Pop scope
                System.SymbolTable.PopScope();
                // Create the new type
                var moduleType = new Type.Struct(
                    new Syntax.Token(new Text.Span(), Syntax.TokenType.KwStruct, "struct"),
                    new Dictionary<string, Type>(),
                    structScope);
                // Wrap it, return it
                return new Value.User(moduleType);
            }
        }

        public override Lir.Types.Type ReturnType => Lir.Types.Type.User_;
        public override Type Type { get; }

        public ExternIntrinsic(IDependencySystem system)
            : base(system)
        {
            // TODO: Ease building these?
            var nameParamSymbol = new Symbol.Var(null, "name", Symbol.VarKind.Param);
            var typeParamSymbol = new Symbol.Var(null, "T", Symbol.VarKind.Param);
            var stringType = System.ReferToConstType("@c", "str");
            var typeType = Type.Type_;
            Type = new Type.Proc(
                true,
                new List<Type.Proc.Param> { 
                    new Type.Proc.Param(nameParamSymbol, stringType),
                    new Type.Proc.Param(typeParamSymbol, typeType),
                }, 
                new Type.Dependent(typeParamSymbol));
        }

        public override Value Execute(VirtualMachine vm, IEnumerable<Value> args)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override string NonDependentDesugar() => "@extern impl";
    }
}
