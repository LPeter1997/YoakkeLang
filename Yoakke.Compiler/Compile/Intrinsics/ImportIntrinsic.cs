using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;
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
            var stringType = System.ReferToConstType("@c", "str");
            Type = new Type.Proc(new ValueList<Type.Proc.Param>{ new Type.Proc.Param(null, stringType) }, Type.Type_);
            ReturnType = Lir.Types.Type.User_;
        }

        public override Value Execute(VirtualMachine vm, IEnumerable<Value> args)
        {
            var fileName = GetFileName(vm, args);
            // TODO: Skip if already imported?
            // This could be part of the system so it could just return the proper scope to wrap
            // Or the intrinsic itself could cache already loaded in values
            // Load in file
            var ast = System.LoadAst(fileName);
            // We need to create a struct type that contains the constants in the file
            // First we define the new scope
            System.SymbolTable.PushScope(Semantic.ScopeKind.Struct);
            var structScope = System.SymbolTable.CurrentScope;
            // Define things
            new DefineScope(System.SymbolTable).Define(ast);
            new DeclareSymbol(System.SymbolTable).Declare(ast);
            new ResolveSymbol(System.SymbolTable).Resolve(ast);
            // Pop scope
            System.SymbolTable.PopScope();
            // Create the new type
            var moduleType = new Type.Struct(
                new Syntax.Token(new Text.Span(), Syntax.TokenType.KwStruct, "struct"),
                new ValueDictionary<string, Type>(),
                structScope);
            // Wrap it, return it
            return new Value.User(moduleType);
        }

        private string GetFileName(VirtualMachine vm, IEnumerable<Value> args)
        {
            // TODO: Proper errors
            var fileNameStr = (Value.Struct)args.First();
            var fileNamePtr = fileNameStr.Values[0];
            var fileNameLen = (int)((Value.Int)fileNameStr.Values[1]).Value;
            return vm.ReadUtf8FromMemory(fileNamePtr, fileNameLen);
        }
    }
}
