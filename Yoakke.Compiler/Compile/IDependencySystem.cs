using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public interface IDependencySystem
    {
        public SymbolTable SymbolTable { get; }

        public Assembly? Compile(Declaration.File file, BuildStatus status);
        public void TypeCheck(Statement statement);
        public Type TypeOf(Expression expression);
        public Value Evaluate(Expression expression);
        public Value EvaluateConst(Declaration.Const constDecl);
        public Value EvaluateConst(Symbol.Const constSym);
        public Type EvaluateType(Expression expression);
        public int FieldIndex(Type.Struct structType, string name);
        public void SetVarType(Symbol.Var varSym, Type type);
    }
}
