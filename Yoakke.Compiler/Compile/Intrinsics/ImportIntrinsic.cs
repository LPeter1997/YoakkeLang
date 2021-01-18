using System.Collections.Generic;
using System.Linq;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Values;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Compile.Intrinsics
{
    /// <summary>
    /// An <see cref="Intrinsic"/> for importing other files.
    /// </summary>
    public class ImportIntrinsic : Intrinsic
    {
        public override Lir.Types.Type ReturnType { get; }
        public override Type Type { get; }

        public ImportIntrinsic(IDependencySystem system)
            : base(system)
        {
            var paramSymbol = new Symbol.Var(null, "path", Symbol.VarKind.Param);
            var stringType = System.ReferToConstType("@c", "str");
            Type = new Type.Proc(true, new List<Type.Proc.Param>{ new Type.Proc.Param(paramSymbol, stringType) }, Type.Type_);
            ReturnType = Lir.Types.Type.User_;
        }

        public override Value Execute(VirtualMachine vm, IEnumerable<Value> args)
        {
            var fileName = GetFileName(vm, args);
            // TODO: Skip if already imported?
            // This could be part of the system so it could just return the proper scope to wrap
            // Or the intrinsic itself could cache already loaded in values
            // Load in file
            // TODO: syntax status?
            var ast = System.LoadAst(fileName);
            // We need to create a struct type that contains the constants in the file
            // First we define the new scope
            System.SymbolTable.PushScope(Semantic.ScopeKind.Struct);
            var structScope = System.SymbolTable.CurrentScope;
            // Define things
            // TODO: compile status?
            SymbolResolution.Resolve(System.SymbolTable, ast);
            // Pop scope
            System.SymbolTable.PopScope();
            // Create the new type
            var moduleType = new Type.Struct(
                structScope,
                new Syntax.Token(new Text.Span(), Syntax.TokenType.KwStruct, "struct"),
                new Dictionary<string, Type.Struct.Field>());
            // Wrap it, return it
            return new Value.User(moduleType);
        }

        // TODO: Factor out utility to read string
        private static string GetFileName(VirtualMachine vm, IEnumerable<Value> args)
        {
            // TODO: Proper errors
            var fileNameStr = (Value.Struct)args.First();
            var fileNamePtr = fileNameStr.Values[0];
            var fileNameLen = (int)((Value.Int)fileNameStr.Values[1]).Value;
            return vm.ReadUtf8FromMemory(fileNamePtr, fileNameLen);
        }
    }
}
