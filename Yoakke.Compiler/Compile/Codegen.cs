using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// Generates code for given portions of the code.
    /// </summary>
    public class Codegen : Visitor<Value>
    {
        private IDependencySystem system;
        private Builder builder;
        private Dictionary<Semantic.Symbol, Value> variablesToRegisters;

        /// <summary>
        /// Initializes a new <see cref="Codegen"/>.
        /// </summary>
        /// <param name="system">The <see cref="IDependencySystem"/> to use for type-checking and resolving compile-time
        /// expressions.</param>
        public Codegen(IDependencySystem system)
        {
            this.system = system;
            builder = new Builder(new UncheckedAssembly(string.Empty));
            variablesToRegisters = new Dictionary<Semantic.Symbol, Value>();
        }

        /// <summary>
        /// Generates a Lir <see cref="Assembly"/> for the given file node.
        /// </summary>
        /// <param name="file">The file node to generate code for.</param>
        /// <param name="status">The <see cref="BuildStatus"/> to write errors to.</param>
        /// <returns>The generated <see cref="Assembly"/>.</returns>
        public Assembly Generate(Declaration.File file, BuildStatus status)
        {
            // Rename the assembly
            var parseTreeNode = (Syntax.ParseTree.Declaration.File?)file.ParseTreeNode;
            var fileName = parseTreeNode?.Name ?? "unnamed";
            builder.Assembly.Name = fileName;

            // For something to be compiled, it has to be type-checked
            system.TypeCheck(file);
            // If the type-checking succeeded, we can compile
            Visit(file);
            // Then we check the assembly
            var asm = builder.Assembly.Check(status);
            // We are done
            return asm;
        }

        protected override Value? Visit(Declaration.Const cons)
        {
            // If constants are procedures and they contain no user types, we compile them as procedures
            var constType = system.TypeOf(cons.Value);
            if (constType is Semantic.Type.Proc)
            {
                // It's a procedure, we want to compile it
                var proc = (Proc)NonNull(Visit(cons.Value));
                // Rename it properly
                proc.Name = cons.Name;
            }
            return null;
        }

        protected override Value? Visit(Expression.Proc proc)
        {
            var procVal = builder.DefineProc("unnamed");
            // We need the return type
            var returnType = Semantic.Type.Unit;
            if (proc.Signature.Return != null)
            { 
                returnType = system.EvaluateToType(proc.Signature.Return);
            }
            procVal.Return = system.TranslateToLirType(returnType);
            // We need to compile parameters
            foreach (var param in proc.Signature.Parameters)
            {
                // Get the parameter type, define it in the Lir code
                var paramType = system.EvaluateToType(param.Type);
                var lirParamType = system.TranslateToLirType(paramType);
                var paramValue = builder.DefineParameter(lirParamType);
                // We make parameters mutable by making them allocate space on the stack and refer to that space
                var paramSpace = builder.Alloc(lirParamType);
                // Copy the initial value
                builder.Store(paramSpace, paramValue);
                if (param.Name != null)
                {
                    // It has a symbol, we store the allocated space associated
                    var symbol = system.DefinedSymbolFor(param);
                    variablesToRegisters.Add(symbol, paramSpace);
                }
            }
            // Now we can compile the body
            Visit(proc.Body);
            return procVal;
        }

        protected override Value? Visit(Statement.Return ret)
        {
            if (ret.Value == null)
            {
                builder.Ret();
            }
            else
            {
                var value = NonNull(Visit(ret.Value));
                builder.Ret(value);
            }
            return null;
        }

        protected override Value? Visit(Expression.Block block)
        {
            foreach (var stmt in block.Statements) Visit(stmt);
            return block.Value == null ? null : Visit(block.Value);
        }

        protected override Value? Visit(Expression.Literal lit) => lit.Type switch
        {
            TokenType.IntLiteral => Lir.Types.Type.I32.NewValue(int.Parse(lit.Value)),

            _ => throw new NotImplementedException(),
        };

        private static T NonNull<T>(T? value) where T : class
        {
            Debug.Assert(value != null);
            return value;
        }
    }
}
