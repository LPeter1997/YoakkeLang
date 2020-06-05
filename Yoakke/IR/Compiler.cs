using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using Yoakke.Ast;
using Yoakke.Semantic;
using Yoakke.Utils;

namespace Yoakke.IR
{
    /// <summary>
    /// Compiles the AST into IR code.
    /// </summary>
    static class Compiler
    {
        private class ProcedureContext
        {
            public readonly Dictionary<VariableSymbol, RegisterValue> Variables = new Dictionary<VariableSymbol, RegisterValue>();

            private int registerCount = 0;

            public RegisterValue AllocateRegister(VariableSymbol varSym, Type type)
            {
                var value = AllocateRegister(type);
                Variables.Add(varSym, value);
                return value;
            }

            public RegisterValue AllocateRegister(Type type)
            {
                return new RegisterValue(type, registerCount++);
            }
        }

        /// <summary>
        /// Compiles the given <see cref="ProgramDeclaration"/> into an IR <see cref="Assembly"/>.
        /// </summary>
        /// <param name="program">The <see cref="ProgramDeclaration"/> to compile.</param>
        /// <returns>The compiled IR <see cref="Assembly"/>.</returns>
        public static Assembly Compile(ProgramDeclaration program)
        {
            var assembly = new Assembly();
            var builder = new IrBuilder(assembly);

            Compile(builder, null, program);

            return assembly;
        }

        private static void CompileProcedure(IrBuilder builder, string name, ProcExpression proc)
        {
            Assert.NonNull(proc.EvaluationType);
            var procTy = (TypeConstructor)proc.EvaluationType.Substitution;
            Debug.Assert(procTy.Name == "procedure");
            var retTy = Compile(procTy.Subtypes.Last());
            builder.CreateProc(name, retTy, () =>
            {
                // New context for this procedure compilation
                var ctx = new ProcedureContext();
                // Allocate registers for the parameters
                foreach (var param in proc.Parameters)
                {
                    Assert.NonNull(param.Symbol);
                    Assert.NonNull(param.Symbol.Type);
                    // Get type, get a register for it
                    var type = Compile(param.Symbol.Type);
                    var reg = ctx.AllocateRegister(type);
                    // Insert the register as a parameter
                    builder.CurrentProc.Parameters.Add(reg);
                    // Store and load it to make it mutable
                    // (in parameter list) ParamType rX
                    // rY = alloc ParamType
                    // store rY, rX
                    var regMut = ctx.AllocateRegister(param.Symbol, Type.Ptr(type));
                    builder.AddInstruction(new AllocInstruction(regMut));
                    builder.AddInstruction(new StoreInstruction(regMut, reg));
                }
                Compile(builder, ctx, proc.Body);
            });
        }

        private static void Compile(IrBuilder builder, ProcedureContext? ctx, Statement statement)
        {
            switch (statement)
            {
            case ProgramDeclaration program:
                foreach (var decl in program.Declarations) Compile(builder, ctx, decl);
                break;

            case ConstDefinition constDef:
            {
                Assert.NonNull(constDef.Symbol);
                Assert.NonNull(constDef.Symbol.Value);
                var value = constDef.Symbol.Value;
                
                if (value is ProcValue proc)
                {
                    CompileProcedure(builder, constDef.Name.Value, proc.Node);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            break;

            case ExpressionStatement expr:
                Compile(builder, ctx, expr.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static Value? Compile(IrBuilder builder, ProcedureContext? ctx, Expression expression)
        {
            switch (expression) 
            {
            case IntLiteralExpression intLit:
            {
                Assert.NonNull(intLit.EvaluationType);
                var ty = (IntType)Compile(intLit.EvaluationType);
                return new IntValue(ty, BigInteger.Parse(intLit.Token.Value));
            }

            case IdentifierExpression ident:
            {
                Assert.NonNull(ident.Symbol);
                var symbol = ident.Symbol;
                switch (symbol)
                {
                case ConstSymbol constSym:
                    Assert.NonNull(constSym.Value);
                    return Compile(builder, constSym.Value);

                case VariableSymbol varSym:
                {
                    // rX = load ADDRESS
                    Assert.NonNull(ctx);
                    var varAddress = ctx.Variables[varSym];
                    var varType = ((PtrType)varAddress.Type).ElementType;
                    var varValue = ctx.AllocateRegister(varType);
                    builder.AddInstruction(new LoadInstruction(varValue, varAddress));
                    return varValue;
                }

                default: throw new NotImplementedException();
                }
            }

            case ProcExpression proc:
            {
                CompileProcedure(builder, "anonymous", proc);
                // TODO: We need some kind of value for it
                throw new NotImplementedException();
            }

            case BlockExpression block:
            {
                foreach (var stmt in block.Statements) Compile(builder, ctx, stmt);
                Value? retValue = block.Value == null
                                  ? null
                                  : Compile(builder, ctx, block.Value);
                // TODO: block-evaluation does not necessarily return from the function!!!
                builder.AddInstruction(new RetInstruction(retValue));
                return retValue;
            }

            default: throw new NotImplementedException();
            }
        }

        private static Value Compile(IrBuilder builder, Semantic.Value value)
        {
            switch (value)
            {
            case ProcValue proc:
                CompileProcedure(builder, "anonymous", proc.Node);
                throw new NotImplementedException();

            default: throw new NotImplementedException();
            }
        }

        private static Type Compile(Semantic.Type type)
        {
            type = type.Substitution;
            if (type == Semantic.Type.I32) return Type.I32;
            if (type == Semantic.Type.Unit) return Type.Void;

            throw new NotImplementedException();
        }
    }
}
