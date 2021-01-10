using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Yoakke.Compiler.Error;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Reporting.Render;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Yoakke.Syntax.Error;
using Yoakke.Text;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// The standard, global <see cref="IDependencySystem"/>.
    /// </summary>
    public class DependencySystem : IDependencySystem
    {
        public event IDependencySystem.CompileErrorEventHandler? CompileError;

        public string StandardLibraryPath { get; }

        public SymbolTable SymbolTable { get; private set; }
        public Builder Builder => codegen.Builder;
        public TypeTranslator TypeTranslator { get; }

        private Codegen codegen;
        private TypeEval typeEval;
        private TypeCheck typeCheck;

        private HashSet<Proc> tempEval = new HashSet<Proc>();
        private List<ICompileError> errors = new List<ICompileError>();

        public DependencySystem(string standardLibraryPath)
        {
            StandardLibraryPath = standardLibraryPath;

            SymbolTable = new SymbolTable(this);
            TypeTranslator = new TypeTranslator(this);
            codegen = new Codegen(this);
            typeEval = new TypeEval(this);
            typeCheck = new TypeCheck(this);

            SymbolTable.DefineBuiltinPrimitives();
            // Load prelude
            {
                // TODO: syntax status?
                var preludeAst = LoadAst("prelude.yk");
                // TODO: compile status?
                SymbolResolution.Resolve(SymbolTable, preludeAst);
            }
            SymbolTable.DefineBuiltinIntrinsics();

            CompileError += OnCompileError;
        }

        public void ReportCompileError(ICompileError compileError) =>
            CompileError?.Invoke(this, compileError);

        public Declaration.File LoadAst(string path)
        {
            path = GetFilePath(path);
            // To AST
            var src = File.ReadAllText(path);
            var srcFile = new SourceFile(path, src);
            var lexer = new Lexer(srcFile);
            lexer.SyntaxError += OnSyntaxError;
            var parser = new Parser(lexer.Lex());
            parser.SyntaxError += OnSyntaxError;
            var prg = parser.ParseFile();
            var ast = ParseTreeToAst.Convert(prg);
            ast = new Desugaring().Desugar(ast);
            TerminateAndDumpIfHasErrors();
            return ast;
        }

        private string GetFilePath(string path)
        {
            if (!File.Exists(path))
            {
                var stlPath = Path.Combine(StandardLibraryPath, path);
                if (!File.Exists(stlPath))
                {
                    // TODO
                    throw new NotImplementedException();
                }
                return stlPath;
            }
            return path;
        }

        // TODO: DO we still want nullable here?
        public Assembly? Compile(Declaration.File file)
        {
            var asm = codegen.Generate(file);
            // Erase the temporaries and things that have user-types in them
            asm.Procedures = asm.Procedures
                .Except(tempEval)
                .Where(proc => !proc.HasUserValues()).ToList();
            var checkedAsm = asm.Check();
            return checkedAsm;
        }

        public Type TypeOf(Expression expression) => typeEval.TypeOf(expression);

        public void TypeCheck(Node node)
        {
            typeCheck.Check(node);
            TerminateAndDumpIfHasErrors();
        }

        public Value Evaluate(Expression expression)
        {
            // TODO: We could mostly avoid special casing?
            // TODO: We need to capture local variables at evaluation for things like generics to work
            // TODO: It's also kinda expensive to just instantiate a new VM for the whole assembly
            // Can't we just track partially what this expression needs and include that?
            // We could also just have a VM that could build code incrementally
            // Like appending unknown procedures and such

            // It's an unknown expression we have to evaluate
            // We compile the expression into an evaluation procedure, run it through the VM and return the result
            var proc = codegen.GenerateEvaluationProc(expression);
            // We memorize the evaluation procedure so we can remove it from the final assembly
            tempEval.Add(proc);
            var asm = codegen.Builder.Assembly.Check();
            /*if (status.Errors.Count > 0)
            {
                // TODO: The compiled assembly might be incomplete!
                //throw new NotImplementedException();
            }*/
            var vm = new VirtualMachine(asm);
            return vm.Execute(proc, new Value[] { });
        }

        public Value EvaluateConst(Declaration.Const constDecl)
        {
            var symbol = (Symbol.Const)SymbolTable.DefinedSymbol(constDecl);
            // Check if there's a pre-stored value, if not, evaluate it
            if (symbol.Value == null)
            {
                symbol.Value = Evaluate(constDecl.Value);
            }
            return symbol.Value;
        }

        public Value EvaluateConst(Symbol.Const constSym)
        {
            if (constSym.Value != null) return constSym.Value;
            Debug.Assert(constSym.Definition != null);
            var constDecl = (Declaration.Const)constSym.Definition;
            return EvaluateConst(constDecl);
        }

        public Type EvaluateType(Expression expression)
        {
            var value = Evaluate(expression);
            return TypeTranslator.ToSemanticType(value);
        }

        public Type ReferToConstTypeOf(params string[] pieces)
        {
            var path = MakePathExpression(pieces);
            return TypeOf(path);
        }

        public Value ReferToConst(params string[] pieces)
        {
            // TODO: Hackish but works
            var path = MakePathExpression(pieces);
            return Evaluate(path);
        }

        public Type ReferToConstType(params string[] pieces)
        {
            var value = ReferToConst(pieces);
            return TypeTranslator.ToSemanticType(value);
        }

        private Expression MakePathExpression(params string[] pieces)
        {
            Debug.Assert(pieces.Length > 0);
            Expression result = new Expression.Identifier(null, pieces[0]);
            foreach (var piece in pieces.Skip(1))
            {
                result = new Expression.DotPath(null, result, piece);
            }
            // TODO: compile status?
            SymbolResolution.Resolve(SymbolTable, result);
            return result;
        }

        // TODO: Hack
        public void ResetSymbolTable()
        {
            SymbolTable = new SymbolTable(this);
            SymbolTable.DefineBuiltinPrimitives();
            // Load prelude
            {
                // TODO: Syntax status?
                var preludeAst = LoadAst("prelude.yk");
                // TODO: compile status?
                SymbolResolution.Resolve(SymbolTable, preludeAst);
            }
            SymbolTable.DefineBuiltinIntrinsics();
        }

        private void OnSyntaxError(object sender, ISyntaxError syntaxError) =>
            ReportCompileError(new SyntaxError(syntaxError));

        private void OnCompileError(IDependencySystem system, ICompileError compileError)
        {
            Debug.Assert(system == this);
            errors.Add(compileError);
        }

        private void TerminateAndDumpIfHasErrors()
        {
            if (errors.Count == 0) return;

            var diagRenderer = new TextDiagnosticRenderer { SyntaxHighlighter = new YoakkeReportingSyntaxHighlighter() };
            foreach (var err in errors) diagRenderer.Render(err.GetDiagnostic());
            Environment.Exit(1);
        }
    }
}
