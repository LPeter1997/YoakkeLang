﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Yoakke.Compiler.Semantic.Types;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing
    public interface IDependencySystem
    {
        public string StandardLibraryPath { get; }

        public SymbolTable SymbolTable { get; }
        public Builder Builder { get; }
        public TypeTranslator TypeTranslator { get; }

        public Declaration.File LoadAst(string path);

        public Assembly? Compile(Declaration.File file, BuildStatus status);
        public void TypeCheck(Node node);
        public Type TypeOf(Expression expression);
        public Value Evaluate(Expression expression);
        public Value EvaluateConst(Declaration.Const constDecl);
        public Value EvaluateConst(Symbol.Const constSym);
        public Type EvaluateType(Expression expression);

        // TODO: Naming is horrible
        public Type ReferToConstTypeOf(params string[] pieces);
        public Value ReferToConst(params string[] pieces);
        public Type ReferToConstType(params string[] pieces);
    }
}
