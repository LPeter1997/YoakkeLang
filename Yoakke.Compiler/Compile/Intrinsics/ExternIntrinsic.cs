using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Values;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Compile.Intrinsics
{
    // TODO: Doc
    public class ExternIntrinsic : Intrinsic
    {
        public override Lir.Types.Type ReturnType { get; }
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
            ReturnType = Lir.Types.Type.User_;
        }

        public override Value Execute(VirtualMachine vm, IEnumerable<Value> args)
        {
            // TODO
            throw new NotImplementedException();
        }

        // TODO: Factor out utility to read string
        private string GetFileName(VirtualMachine vm, IEnumerable<Value> args)
        {
            // TODO: Proper errors
            var fileNameStr = (Value.Struct)args.First();
            var fileNamePtr = fileNameStr.Values[0];
            var fileNameLen = (int)((Value.Int)fileNameStr.Values[1]).Value;
            return vm.ReadUtf8FromMemory(fileNamePtr, fileNameLen);
        }

        public override string NonDependentDesugar() => "@extern impl";
    }
}
