using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Lir;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// The standard, global <see cref="IDependencySystem"/>.
    /// </summary>
    public class DependencySystem : IDependencySystem
    {
        public SymbolTable SymbolTable { get; }

        private Codegen codegen;
        private TypeEval typeEval;
        private TypeCheck typeCheck;

        private HashSet<Proc> tempEval = new HashSet<Proc>();

        public DependencySystem(SymbolTable symbolTable)
        {
            SymbolTable = symbolTable;
            codegen = new Codegen(this);
            typeEval = new TypeEval(this);
            typeCheck = new TypeCheck(this);
        }

        public Assembly? Compile(Declaration.File file, BuildStatus status)
        {
            var asm = codegen.Generate(file);
            // Erase the temporaries
            asm.Procedures = asm.Procedures.Except(tempEval).ToList();
            var checkedAsm = asm.Check(status);
            if (status.Errors.Count > 0)
            {
                // TODO: Debug dump
                Console.WriteLine(asm);
                return null;
            }
            return checkedAsm;
        }

        public Type TypeOf(Expression expression) => typeEval.TypeOf(expression);
        public void TypeCheck(Statement statement) => typeCheck.Check(statement);

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
            var status = new BuildStatus();
            var asm = codegen.Builder.Assembly.Check(status);
            if (status.Errors.Count > 0)
            {
                // TODO: The compiled assembly might be incomplete!
                //throw new NotImplementedException();
            }
            var vm = new VirtualMachine(asm);
            return vm.Execute(proc, new Value[] { });
        }

        public Value EvaluateConst(Declaration.Const constDecl)
        {
            var symbol = (Symbol.Const)SymbolTable.DefinedSymbol(constDecl);
            // Check if there's a pre-stored value, if not, evaluate it
            if (symbol.Value == null)
            {
                codegen.HintName(constDecl.Name);
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
    }
}
