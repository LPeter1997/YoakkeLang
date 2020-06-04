using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Compiles the given <see cref="ProgramDeclaration"/> into an IR <see cref="Assembly"/>.
        /// </summary>
        /// <param name="program">The <see cref="ProgramDeclaration"/> to compile.</param>
        /// <returns>The compiled IR <see cref="Assembly"/>.</returns>
        public static Assembly Compile(ProgramDeclaration program)
        {
            var assembly = new Assembly();
            var builder = new IrBuilder(assembly);

            Compile(builder, program);

            return assembly;
        }

        private static void CompileProcedure(IrBuilder builder, string name, ProcExpression proc)
        {
            Assert.NonNull(proc.EvaluationType);
            var procTy = (TypeConstructor)proc.EvaluationType.Substitution;
            if (procTy.Name != "procedure") throw new InvalidOperationException("The type of procedure is not a procedure type!");
            var retTy = Compile(procTy.Subtypes.Last());
            builder.CreateProc(name, retTy, () =>
            {
                Compile(builder, proc.Body);
            });
        }

        private static void Compile(IrBuilder builder, Statement statement)
        {
            switch (statement)
            {
            case ProgramDeclaration program:
                foreach (var decl in program.Declarations) Compile(builder, decl);
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
                Compile(builder, expr.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static Value? Compile(IrBuilder builder, Expression expression)
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
                if (symbol is ConstSymbol constSym)
                {
                    Assert.NonNull(constSym.Value);
                    return Compile(builder, constSym.Value);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            case ProcExpression proc:
            {
                CompileProcedure(builder, "anonymous", proc);
                throw new NotImplementedException();
            }

            case BlockExpression block:
            {
                foreach (var stmt in block.Statements) Compile(builder, stmt);
                Value? retValue = block.Value == null
                                  ? null
                                  : Compile(builder, block.Value);
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
            if (type == Semantic.Type.I32) return Type.I32;
            if (type == Semantic.Type.Unit) return Type.Void;

            throw new NotImplementedException();
        }
    }
}
