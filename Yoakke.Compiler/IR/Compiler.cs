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
    class Compiler
    {
        /// <summary>
        /// Compiles the given <see cref="Declaration.Program"/> into an IR <see cref="Assembly"/>.
        /// </summary>
        /// <param name="program">The <see cref="Declaration.Program"/> to compile.</param>
        /// <returns>The compiled IR <see cref="Assembly"/>.</returns>
        public static Assembly Compile(Declaration.Program program)
        {
            var assembly = new Assembly();
            var builder = new IrBuilder(assembly);

            new Compiler(builder).Compile(program);

            return assembly;
        }

        private IrBuilder builder;
        private HashSet<Type> compiledTypes = new HashSet<Type>();

        private Compiler(IrBuilder builder) 
        {
            this.builder = builder;
        }

        private Proc? CompileProcedure(Expression.Proc proc)
        {
            // If it's already compiled, just return that
            if (builder.Globals.TryGetValue(proc, out var alreadyDefined)) return (Proc)alreadyDefined;

            // Get the semantic type of the procedure
            var semanticProcType = TypeEval.Evaluate(proc);
            // We need to find out, if this procedure fiddles with types or not
            if (semanticProcType.Contains(Semantic.Type.Type_))
            {
                // TODO: Not the best way to determine it, but for now this is a deal-breaker
                return null;
            }

            // Compile the type
            var procType = Compile(semanticProcType);
            // We begin our new procedure
            var compiledProc = builder.CreateProcBegin(procType, proc);
            // Allocate registers for the parameters
            foreach (var param in proc.Parameters)
            {
                Assert.NonNull(param.Symbol);
                Assert.NonNull(param.Symbol.Type);
                // Compile it's type
                var paramType = Compile(param.Symbol.Type);
                // Allocate a register for the received value
                // We won't ever refer to this ever again, so we don't associate it with a key
                var receiveReg = builder.AllocateParameter(paramType, null);
                // To make it mutable, we instantly allocate space on the stack and store the value there
                var storeReg = builder.AllocateRegister(new Type.Ptr(paramType), param.Symbol);
                // (in parameter list) ParamType rX
                // rY = alloc ParamType
                // store rY, rX
                builder.AddInstruction(new Instruction.Alloc(storeReg));
                builder.AddInstruction(new Instruction.Store(storeReg, receiveReg));
            }
            // Now we just compile the body
            var bodyValue = Compile(proc.Body);
            // We append the return statement
            builder.AddInstruction(new Instruction.Ret(bodyValue));
            // Compilation is done
            builder.CreateProcEnd();

            return compiledProc;
        }

        private Value CompileExtern(Semantic.Value.Extern external, Symbol symbol)
        {
            // If it's already compiled, just return that
            if (builder.Globals.TryGetValue(symbol, out var value)) return value;

            var externalType = Compile(external.Type);
            var createdValue = builder.CreateExtern(external.Name, externalType, symbol);

            return createdValue;
        }

        private void Compile(Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                foreach (var decl in program.Declarations) Compile(decl);
                break;

            case Declaration.ConstDef constDef:
            {
                Assert.NonNull(constDef.Symbol);
                Assert.NonNull(constDef.Symbol.Value);
                var value = constDef.Symbol.Value;
                
                if (value is Semantic.Value.Proc proc)
                {
                    var compiledProc = CompileProcedure(proc.Node);
                    if (compiledProc != null)
                    {
                        // TODO: This is not required, only if we export the function
                        compiledProc.LinkName = constDef.Name.Value;
                    }
                }
                else if (value is Semantic.Value.Extern external)
                {
                    CompileExtern(external, constDef.Symbol);
                }
                else
                {
                    // PASS
                }
            }
            break;

            case Statement.Expression_ expr:
                Compile(expr.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private Value Compile(Expression expression)
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
                    return Compile(constSym.Value);

                case Symbol.Variable varSym:
                {
                    // rX = load ADDRESS
                    var varAddress = builder.Locals[varSym];
                    var varType = ((Type.Ptr)varAddress.Type).ElementType;
                    var varValue = builder.AllocateRegister(varType, null);
                    builder.AddInstruction(new Instruction.Load(varValue, varAddress));
                    return varValue;
                }

                default: throw new NotImplementedException();
                }
            }

            case Expression.StructValue structValue:
            {
                // Compile the struct type
                var structTy = ConstEval.EvaluateAsType(structValue.StructType);
                var ty = Compile(structTy);
                // Allocate a register for the mutable value
                var mutReg = builder.AllocateRegister(new Type.Ptr(ty), null);
                builder.AddInstruction(new Instruction.Alloc(mutReg));
                // Now store the fields
                // TODO
                throw new NotImplementedException();
                // Load value
                var valueReg = builder.AllocateRegister(ty, null);
                builder.AddInstruction(new Instruction.Load(valueReg, mutReg));
                return valueReg;
            }

            case Expression.Proc proc:
            {
                CompileProcedure( proc);
                // TODO: We need some kind of value for it
                throw new NotImplementedException();
            }

            case Expression.Block block:
            {
                foreach (var stmt in block.Statements) Compile(stmt);
                Value retValue = block.Value == null
                                 ? Value.Void_
                                 : Compile(block.Value);
                return retValue;
            }

            case Expression.Call call:
            {
                var proc = Compile(call.Proc);
                var args = call.Arguments.Select(x => Compile(x)).ToList();
                var procTy = (Type.Proc)proc.Type;
                var retTy = procTy.ReturnType;
                var retRegister = builder.AllocateRegister(retTy, null);
                builder.AddInstruction(new Instruction.Call(retRegister, proc, args));
                return retRegister;
            }

            default: throw new NotImplementedException();
            }
        }

        private Value Compile(Semantic.Value value)
        {
            switch (value)
            {
            case Semantic.Value.Proc proc:
                return Assert.NonNullValue(CompileProcedure(proc.Node));

            case Semantic.Value.Extern external:
            {
                var ty = Compile(external.Type);
                return new Value.Extern(ty, external.Name);
            }

            default: throw new NotImplementedException();
            }
        }

        private Type Compile(Semantic.Type type)
        {
            if (Semantic.Type.I32.EqualsNonNull(type)) return Type.I32;
            if (Semantic.Type.Unit.EqualsNonNull(type)) return Type.Void_;

            if (type is Semantic.Type.Proc proc)
            {
                var paramTypes = proc.Parameters.Select(Compile).ToList();
                var returnType = Compile(proc.Return);
                return CacheType(new Type.Proc(paramTypes, returnType));
            }

            if (type is Semantic.Type.Struct structure)
            {
                var fields = structure.Fields.Values.Select(Compile).ToList();
                return CacheType(new Type.Struct(fields));
            }

            throw new NotImplementedException();
        }

        private Type CacheType(Type type)
        {
            if (compiledTypes.TryGetValue(type, out var alreadyPresent)) return alreadyPresent;
            compiledTypes.Add(type);
            return type;
        }
    }
}
