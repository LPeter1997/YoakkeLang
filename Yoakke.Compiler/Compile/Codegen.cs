using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Lir;
using Yoakke.Lir.Status;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc the whole thing

    public class Codegen : Visitor<Value>
    {
        private Builder builder;
        private Dictionary<Symbol, Value> variablesToRegisters;

        public Codegen(Builder builder)
        {
            this.builder = builder;
            variablesToRegisters = new Dictionary<Semantic.Symbol, Value>();
        }

        public Codegen()
            : this(new Builder(new UncheckedAssembly(string.Empty)))
        {
        }

        // External interface //////////////////////////////////////////////////

        private void TypeCheck(Statement statement)
        {
            // TODO: Assume good
            throw new NotImplementedException();
        }

        private Semantic.Type TypeOf(Expression expression)
        {
            // TODO
            throw new NotImplementedException();
        }

        private Value EvaluateConst(Declaration.Const constDecl)
        {
            // TODO
            throw new NotImplementedException();
        }

        private Semantic.Type EvaluateToType(Expression expression)
        {
            // TODO
            throw new NotImplementedException();
        }

        private Lir.Types.Type TranslateToLirType(Semantic.Type type)
        {
            // TODO
            throw new NotImplementedException();
        }

        private SymbolTable SymbolTable => throw new NotImplementedException();

        // Public interface ////////////////////////////////////////////////////

        public Assembly Generate(Declaration.File file, BuildStatus status)
        {
            // Rename the assembly
            var parseTreeNode = (Syntax.ParseTree.Declaration.File?)file.ParseTreeNode;
            var fileName = parseTreeNode?.Name ?? "unnamed";
            builder.Assembly.Name = fileName;
            // For something to be compiled, it has to be type-checked
            TypeCheck(file);
            // If the type-checking succeeded, we can compile
            Visit(file);
            // We close the prelude function
            if (builder.Assembly.Prelude != null)
            {
                builder.WithPrelude(b => b.Ret());
            }
            // Then we check the assembly
            var asm = builder.Assembly.Check(status);
            // We are done
            return asm;
        }

        public Proc Generate(Expression.Proc procExpr) => (Proc)VisitNonNull(procExpr);

        public Proc GenerateEvaluationProc(Expression expr)
        {
            Proc? procValue = null;
            builder.WithSubcontext(b =>
            {
                procValue = builder.DefineProc($"expr_eval_{builder.Assembly.Procedures.Count}");
                // We need the return type
                var returnType = TypeOf(expr);
                procValue.Return = TranslateToLirType(returnType);
                // Compile and return the body
                var result = VisitNonNull(expr);
                builder.Ret(result);
            });
            Debug.Assert(procValue != null);
            return procValue;
        }

        // Actual code-generation //////////////////////////////////////////////

        protected override Value? Visit(Declaration.Const cons) => EvaluateConst(cons);

        protected override Value? Visit(Statement.Var var)
        {
            // Figure out variable type
            Semantic.Type type;
            if (var.Value != null)
            {
                type = TypeOf(var.Value);
            }
            else
            {
                Debug.Assert(var.Type != null);
                type = EvaluateToType(var.Type);
            }
            // Globals and locals are very different
            Value varSpace;
            if (SymbolTable.IsGlobal(var))
            {
                // Global variable
                varSpace = builder.DefineGlobal(var.Name, TranslateToLirType(type));
                if (var.Value != null)
                {
                    // Assign initial value in the startup code
                    builder.WithPrelude(b => 
                    {
                        var initialValue = VisitNonNull(var.Value);
                        b.Store(varSpace, initialValue);
                    });
                }
            }
            else
            {
                // Local variable
                // Allocate space
                varSpace = builder.Alloc(TranslateToLirType(type));
                if (var.Value != null)
                {
                    // We also need to assign the value
                    var value = VisitNonNull(var.Value);
                    builder.Store(varSpace, value);
                }
            }
            // Associate with symbol
            var symbol = SymbolTable.DefinedSymbol(var);
            variablesToRegisters.Add(symbol, varSpace);
            return null;
        }

        protected override Value? Visit(Statement.Return ret)
        {
            if (ret.Value == null)
            {
                // No return value
                builder.Ret();
            }
            else
            {
                // We also need to compile the return value
                var value = VisitNonNull(ret.Value);
                builder.Ret(value);
            }
            return null;
        }

        protected override Value? Visit(Expression.Proc proc)
        {
            Proc? procVal = null;
            builder.WithSubcontext(b =>
            {
                procVal = builder.DefineProc("unnamed");
                // We need the return type
                var returnType = Semantic.Type.Unit;
                if (proc.Signature.Return != null)
                {
                    returnType = EvaluateToType(proc.Signature.Return);
                }
                procVal.Return = TranslateToLirType(returnType);
                // We need to compile parameters
                foreach (var param in proc.Signature.Parameters)
                {
                    // Get the parameter type, define it in the Lir code
                    var paramType = EvaluateToType(param.Type);
                    var lirParamType = TranslateToLirType(paramType);
                    var paramValue = builder.DefineParameter(lirParamType);
                    // We make parameters mutable by making them allocate space on the stack and refer to that space
                    var paramSpace = builder.Alloc(lirParamType);
                    // Copy the initial value
                    builder.Store(paramSpace, paramValue);
                    if (param.Name != null)
                    {
                        // It has a symbol, we store the allocated space associated
                        var symbol = SymbolTable.DefinedSymbol(param);
                        variablesToRegisters.Add(symbol, paramSpace);
                    }
                }
                // Now we can compile the body
                Visit(proc.Body);
            });
            Debug.Assert(procVal != null);
            return procVal;
        }

        protected override Value? Visit(Expression.Block block)
        {
            // We just compile statements and the optional value
            foreach (var stmt in block.Statements) Visit(stmt);
            return block.Value == null ? null : Visit(block.Value);
        }

        protected override Value? Visit(Expression.If iff)
        {
            if (iff.Else == null)
            {
                // No chance for a return value
                builder.IfThen(
                    condition: b => VisitNonNull(iff.Condition),
                    then: b => Visit(iff.Then));
                return null;
            }
            else
            {
                var retType = TypeOf(iff);
                if (retType.Equals(Semantic.Type.Unit))
                {
                    // There is no return value
                    builder.IfThenElse(
                        condition: b => VisitNonNull(iff.Condition),
                        then: b => Visit(iff.Then),
                        @else: b => Visit(iff.Else));
                    return null;
                }
                else
                {
                    // There is a return value we need to take care of
                    // First we allocate space for the return value
                    var retSpace = builder.Alloc(TranslateToLirType(retType));
                    // Compile it, storing the results in the respective blocks
                    builder.IfThenElse(
                        condition: b => VisitNonNull(iff.Condition),
                        then: b =>
                        {
                            var result = VisitNonNull(iff.Then);
                            b.Store(retSpace, result);
                        },
                        @else: b =>
                        {
                            var result = VisitNonNull(iff.Else);
                            b.Store(retSpace, result);
                        });
                    // Load up the result
                    return builder.Load(retSpace);
                }
            }
        }

        protected override Value? Visit(Expression.While whil)
        {
            builder.While(
                condition: b => VisitNonNull(whil.Condition),
                body: b => Visit(whil.Body));
            return null;
        }

        protected override Value? Visit(Expression.Identifier ident)
        {
            // Get the referred symbol
            var symbol = SymbolTable.ReferredSymbol(ident);
            // Check what kind of symbol it is
            if (symbol is Symbol.Var)
            {
                // Handle the variable
                var reg = variablesToRegisters[symbol];
                // Load the value
                return builder.Load(reg);
            }
            else
            {
                var constSymbol = (Symbol.Const)symbol;
                // If there's a value assigned, just return that
                if (constSymbol.Value != null) return constSymbol.Value;
                // Otherwise we need to calculate it from the definition
                Debug.Assert(constSymbol.Definition != null);
                return EvaluateConst((Declaration.Const)constSymbol.Definition);
            }
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
            var proc = VisitNonNull(call.Procedure);
            // Then the args
            var args = call.Arguments.Select(arg => VisitNonNull(arg)).ToList();
            // And write the call
            return builder.Call(proc, args);
        }

        protected override Value? Visit(Expression.Binary bin)
        {
            if (bin.Operator == TokenType.Assign)
            {
                var left = Lvalue(bin.Left);
                var right = VisitNonNull(bin.Right);
                // Write out the store
                builder.Store(left, right);
                return null;
            }
            else
            {
                // TODO: Do proper type-checking, for now we blindly assume builtin operations
                // Here we need to handle the case when there's a user-defined operator!
                var left = VisitNonNull(bin.Left);
                var right = VisitNonNull(bin.Right);
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

        private Value Lvalue(Expression expression)
        {
            switch (expression)
            {
            case Expression.Identifier ident:
            {
                var symbol = SymbolTable.ReferredSymbol(ident);
                return variablesToRegisters[symbol];
            }

            default: throw new NotImplementedException();
            }
        }
    }
}
