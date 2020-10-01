using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
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
                // TODO: We publish all procedures for now
                proc.Visibility = Visibility.Public;
            }
            return null;
        }

        protected override Value? Visit(Statement.Var var)
        {
            // Figure out variable type
            Semantic.Type type;
            if (var.Value != null)
            {
                type = system.TypeOf(var.Value);
            }
            else
            {
                Debug.Assert(var.Type != null);
                type = system.EvaluateToType(var.Type);
            }
            // Allocate space
            var varSpace = builder.Alloc(system.TranslateToLirType(type));
            if (var.Value != null)
            {
                // We also need to assign the value
                var value = NonNull(Visit(var.Value));
                builder.Store(varSpace, value);
            }
            // Associate with symbol
            var symbol = system.DefinedSymbolFor(var);
            variablesToRegisters.Add(symbol, varSpace);
            return null;
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

        protected override Value? Visit(Expression.Block block)
        {
            foreach (var stmt in block.Statements) Visit(stmt);
            return block.Value == null ? null : Visit(block.Value);
        }

        protected override Value? Visit(Expression.If iff)
        {
            if (iff.Else == null)
            {
                // No chance for a return value
                builder.IfThen(
                    condition: b => NonNull(Visit(iff.Condition)),
                    then: b => Visit(iff.Then));
                return null;
            }
            else
            {
                // First we allocate space for the return value
                var retType = system.TypeOf(iff);
                var retSpace = builder.Alloc(system.TranslateToLirType(retType));
                // Compile it, storing the results in the respective blocks
                builder.IfThenElse(
                    condition: b => NonNull(Visit(iff.Condition)),
                    then: b => 
                    {
                        var result = NonNull(Visit(iff.Then));
                        b.Store(retSpace, result);
                    },
                    @else: b =>
                    {
                        var result = NonNull(Visit(iff.Else));
                        b.Store(retSpace, result);
                    });
                // Load up the result
                return builder.Load(retSpace);
            }
        }

        protected override Value? Visit(Expression.While whil)
        {
            builder.While(
                condition: b => NonNull(Visit(whil.Condition)),
                body: b => Visit(whil.Body));
            return null;
        }

        protected override Value? Visit(Expression.Identifier ident)
        {
            // Look up the register arrociated with the symbol
            var symbol = system.ReferredSymbolFor(ident);
            var reg = variablesToRegisters[symbol];
            // Load the value
            return builder.Load(reg);
        }

        protected override Value? Visit(Expression.Literal lit) => lit.Type switch
        {
            TokenType.IntLiteral => Lir.Types.Type.I32.NewValue(int.Parse(lit.Value)),
            TokenType.KwTrue => Lir.Types.Type.I32.NewValue(1),
            TokenType.KwFalse => Lir.Types.Type.I32.NewValue(0),

            _ => throw new NotImplementedException(),
        };

        protected override Value? Visit(Expression.Call call)
        {
            // Simply compile the procedure
            var proc = NonNull(Visit(call.Procedure));
            // Then the args
            var args = call.Arguments.Select(arg => NonNull(Visit(arg))).ToList();
            // And write the call
            return builder.Call(proc, args);
        }

        protected override Value? Visit(Expression.Binary bin)
        {
            if (bin.Operator == TokenType.Assign)
            {
                var left = Lvalue(bin.Left);
                var right = NonNull(Visit(bin.Right));
                // Write out the store
                builder.Store(left, right);
                return null;
            }
            else
            {
                // TODO: Do proper type-checking, for now we blindly assume builtin operations
                // Here we need to handle the case when there's a user-defined operator!
                var left = NonNull(Visit(bin.Left));
                var right = NonNull(Visit(bin.Right));
                return bin.Operator switch
                {
                    TokenType.Add      => builder.Add(left, right),
                    TokenType.Subtract => builder.Sub(left, right),
                    TokenType.Multiply => builder.Mul(left, right),
                    TokenType.Divide   => builder.Div(left, right),
                    TokenType.Modulo   => builder.Mod(left, right),

                    TokenType.Equal        => builder.CmpEq(left, right),
                    TokenType.NotEqual     => builder.CmpNe(left, right),
                    TokenType.Greater      => builder.CmpGr(left, right),
                    TokenType.Less         => builder.CmpLe(left, right),
                    TokenType.GreaterEqual => builder.CmpGrEq(left, right),
                    TokenType.LessEqual    => builder.CmpLeEq(left, right),

                    _ => throw new NotImplementedException(),
                };
            }
        }

        // TODO: StructType, StructValue, ProcSignature, DotPath

        private Value Lvalue(Expression expression) => expression switch
        {
            Expression.Identifier ident => variablesToRegisters[system.ReferredSymbolFor(ident)],

            _ => throw new NotSupportedException(),
        };

        private static T NonNull<T>(T? value) where T : class
        {
            Debug.Assert(value != null);
            return value;
        }
    }
}
