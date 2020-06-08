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
        // TODO: These contexts are really ugly
        // They should probably belong to the IR builder type.

        private class AssemblyContext
        {
            public readonly Dictionary<Symbol.Const, Value.Extern> Externals =
                new Dictionary<Symbol.Const, Value.Extern>();
        }

        private class ProcedureContext
        {
            public readonly Dictionary<Symbol.Variable, Value.Register> Variables = 
                new Dictionary<Symbol.Variable, Value.Register>();

            private int registerCount = 0;

            public Value.Register AllocateRegister(Symbol.Variable varSym, Type type)
            {
                var value = AllocateRegister(type);
                Variables.Add(varSym, value);
                return value;
            }

            public Value.Register AllocateRegister(Type type)
            {
                return new Value.Register(type, registerCount++);
            }
        }

        /// <summary>
        /// Compiles the given <see cref="ProgramDeclaration"/> into an IR <see cref="Assembly"/>.
        /// </summary>
        /// <param name="program">The <see cref="ProgramDeclaration"/> to compile.</param>
        /// <returns>The compiled IR <see cref="Assembly"/>.</returns>
        public static Assembly Compile(Declaration.Program program)
        {
            var assembly = new Assembly();
            var builder = new IrBuilder(assembly);
            var ctx = new AssemblyContext();

            Compile(builder, ctx, null, program);

            return assembly;
        }

        private static void CompileProcedure(IrBuilder builder, AssemblyContext asm, string name, Expression.Proc proc)
        {
            Assert.NonNull(proc.EvaluationType);
            proc.EvaluationType.AsProcedure(out _, out var semanticRetTy);
            var retTy = Compile(semanticRetTy);
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
                    var regMut = ctx.AllocateRegister(param.Symbol, new Type.Ptr(type));
                    builder.AddInstruction(new Instruction.Alloc(regMut));
                    builder.AddInstruction(new Instruction.Store(regMut, reg));
                }
                Compile(builder, asm, ctx, proc.Body);
            });
        }

        private static void Compile(IrBuilder builder, AssemblyContext asm, ProcedureContext? ctx, Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                foreach (var decl in program.Declarations) Compile(builder, asm, ctx, decl);
                break;

            case Declaration.ConstDef constDef:
            {
                Assert.NonNull(constDef.Symbol);
                Assert.NonNull(constDef.Symbol.Value);
                var value = constDef.Symbol.Value;
                
                if (value is Semantic.Value.Proc proc)
                {
                    CompileProcedure(builder, asm, constDef.Name.Value, proc.Node);
                }
                else if (value is Semantic.Value.ExternSymbol externSym)
                {
                    var externalType = Compile(externSym.Type);
                    var external = new Value.Extern(externalType, externSym.Name);
                    // Store it as a map from symbol to value, and define it in the assembly
                    builder.DeclareExternal(external);
                    asm.Externals.Add(constDef.Symbol, external);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            break;

            case Statement.Expression_ expr:
                Compile(builder, asm, ctx, expr.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static Value? Compile(IrBuilder builder, AssemblyContext asm, ProcedureContext? ctx, Expression expression)
        {
            switch (expression) 
            {
            case Expression.IntLit intLit:
            {
                Assert.NonNull(intLit.EvaluationType);
                var ty = (Type.Int)Compile(intLit.EvaluationType);
                return new Value.Int(ty, BigInteger.Parse(intLit.Token.Value));
            }

            case Expression.Ident ident:
            {
                Assert.NonNull(ident.Symbol);
                var symbol = ident.Symbol;
                switch (symbol)
                {
                case Symbol.Const constSym:
                    Assert.NonNull(constSym.Value);
                    return Compile(builder, asm, constSym.Value);

                case Symbol.Variable varSym:
                {
                    // rX = load ADDRESS
                    Assert.NonNull(ctx);
                    var varAddress = ctx.Variables[varSym];
                    var varType = ((Type.Ptr)varAddress.Type).ElementType;
                    var varValue = ctx.AllocateRegister(varType);
                    builder.AddInstruction(new Instruction.Load(varValue, varAddress));
                    return varValue;
                }

                default: throw new NotImplementedException();
                }
            }

            case Expression.Proc proc:
            {
                CompileProcedure(builder, asm, "anonymous", proc);
                // TODO: We need some kind of value for it
                throw new NotImplementedException();
            }

            case Expression.Block block:
            {
                foreach (var stmt in block.Statements) Compile(builder, asm, ctx, stmt);
                Value? retValue = block.Value == null
                                  ? null
                                  : Compile(builder, asm, ctx, block.Value);
                // TODO: block-evaluation does not necessarily return from the function!!!
                builder.AddInstruction(new Instruction.Ret(retValue));
                return retValue;
            }

            default: throw new NotImplementedException();
            }
        }

        private static Value Compile(IrBuilder builder, AssemblyContext asm, Semantic.Value value)
        {
            switch (value)
            {
            case Semantic.Value.Proc proc:
                CompileProcedure(builder, asm, "anonymous", proc.Node);
                throw new NotImplementedException();

            default: throw new NotImplementedException();
            }
        }

        private static Type Compile(Semantic.Type type)
        {
            if (Semantic.Type.Same(type, Semantic.Type.I32)) return Type.I32;
            if (Semantic.Type.Same(type, Semantic.Type.Unit)) return Type.Void_;

            if (type.IsProcedure())
            {
                type.AsProcedure(out var ps, out var ret);
                return new Type.Proc(ps.Select(Compile).ToList(), Compile(ret));
            }

            throw new NotImplementedException();
        }
    }
}
