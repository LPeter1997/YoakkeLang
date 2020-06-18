using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using Yoakke.Compiler.Ast;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Syntax;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.IR
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
        // TODO: This dict should really have IR types instead of semantic ones
        private Dictionary<(Semantic.Type, string), int> structFields = new Dictionary<(Semantic.Type, string), int>();

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

        private Value CompileStructValue(Semantic.Type semType, IEnumerable<(string, Value)> values)
        {
            // Compile the struct type
            var type = Compile(semType);
            // Allocate a register for the mutable value
            var mutReg = builder.AllocateRegister(new Type.Ptr(type), null);
            builder.AddInstruction(new Instruction.Alloc(mutReg));
            // Now store the fields
            foreach (var (name, value) in values)
            {
                // Allocate a register for the field pointer
                var ptrReg = builder.AllocateRegister(new Type.Ptr(value.Type), null);
                // Load the pointer of the proper field
                var index = new Value.Int(Type.I32, structFields[(semType, name)]);
                builder.AddInstruction(new Instruction.ElementPtr(ptrReg, mutReg, index));
                // Store the value
                builder.AddInstruction(new Instruction.Store(ptrReg, value));
            }
            // Load value
            var valueReg = builder.AllocateRegister(type, null);
            builder.AddInstruction(new Instruction.Load(valueReg, mutReg));
            return valueReg;
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
            // Nice lazy implementation
            case Expression.IntLit intLit:
                return Compile(ConstEval.Evaluate(intLit));
            case Expression.BoolLit boolLit:
                return Compile(ConstEval.Evaluate(boolLit));

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

            case Expression.DotPath dotPath:
            {
                // TODO: This is only for fields, this could be a constant or a type access!

                // TODO: We need to load this as an lvalue maybe?
                // It's more efficient and is a requirement for assignment!
                // (In theory it doesn't matter for rvalues, but always copies, but we can just copy the field)

                // Compile left-hand-side
                var left = Compile(dotPath.Left);
                // Get field index
                var leftSemanticType = TypeEval.Evaluate(dotPath.Left);
                var fieldIndex = structFields[(leftSemanticType, dotPath.Right.Value)];
                // Get field type
                var leftType = (Type.Struct)left.Type;
                var fieldType = leftType.Fields[fieldIndex];
                // Get the element and load it
                var fieldPtr = builder.AllocateRegister(new Type.Ptr(fieldType), null);
                var fieldVal = builder.AllocateRegister(fieldType, null);
                builder.AddInstruction(new Instruction.ElementPtr(fieldPtr, left, new Value.Int(Type.I32, fieldIndex)));
                builder.AddInstruction(new Instruction.Load(fieldVal, fieldPtr));
                return fieldVal;
            }

            case Expression.StructValue structValue:
            {
                var structTy = (Semantic.Type.Struct)ConstEval.EvaluateAsType(structValue.StructType);
                return CompileStructValue(structTy, 
                    structValue.Fields.Select(f => (f.Item1.Value, Compile(f.Item2))));
            }

            case Expression.Proc proc:
                return Assert.NonNullValue(CompileProcedure( proc));

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

            case Expression.If iff:
            {
                // First we allocate space for the return value
                var retType = Compile(TypeEval.Evaluate(iff.Then));
                var retPtr = builder.AllocateRegister(new Type.Ptr(retType), null);
                builder.AddInstruction(new Instruction.Alloc(retPtr));
                // We compile the condition
                var conditionValue = Compile(iff.Condition);
                var starterBasicBlock = builder.CurrentBasicBlock;
                // We create a then, an else and a finally basic block
                var thenBB = builder.CreateBasicBlock();
                var elseBB = builder.CreateBasicBlock();
                var finallyBB = builder.CreateBasicBlock();
                // From the start we need to conditionally jump to then or else
                builder.CurrentBasicBlock = starterBasicBlock;
                builder.AddInstruction(new Instruction.JumpIf(conditionValue, thenBB, elseBB));
                // We compile then
                builder.CurrentBasicBlock = thenBB;
                var thenValue = Compile(iff.Then);
                // Store it
                builder.AddInstruction(new Instruction.Store(retPtr, thenValue));
                // Jump to finally
                builder.AddInstruction(new Instruction.Jump(finallyBB));
                // We compile else
                builder.CurrentBasicBlock = elseBB;
                var elseValue = iff.Else == null ? Value.Void_ : Compile(iff.Else);
                // Store it
                builder.AddInstruction(new Instruction.Store(retPtr, elseValue));
                // Jump to finally
                builder.AddInstruction(new Instruction.Jump(finallyBB));
                // We continue from the finally block
                builder.CurrentBasicBlock = finallyBB;
                // Finally we load the stored value
                var retValue = builder.AllocateRegister(retType, null);
                builder.AddInstruction(new Instruction.Load(retValue, retPtr));
                return retValue;
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

            case Semantic.Value.Int i:
            {
                var ty = Compile(i.Type);
                return new Value.Int(ty, i.Value);
            }

            case Semantic.Value.Bool b:
                return new Value.Int(Type.Bool, b.Value ? 1 : 0);

            case Semantic.Value.Struct structure:
                return CompileStructValue(structure.Type, 
                    structure.Fields.Select(f => (f.Key, Compile(f.Value))));

            default: throw new NotImplementedException();
            }
        }

        private Type Compile(Semantic.Type type)
        {
            if (Semantic.Type.I32.EqualsNonNull(type)) return Type.I32;
            if (Semantic.Type.Bool.EqualsNonNull(type)) return Type.Bool;
            if (Semantic.Type.Unit.EqualsNonNull(type)) return Type.Void_;

            if (type is Semantic.Type.Proc proc)
            {
                var paramTypes = proc.Parameters.Select(Compile).ToList();
                var returnType = Compile(proc.Return);
                return CacheType(new Type.Proc(paramTypes, returnType));
            }

            if (type is Semantic.Type.Struct structure)
            {
                var fields = new List<Type>();
                int idx = 0;
                foreach (var field in structure.Fields)
                {
                    fields.Add(Compile(field.Value));
                    structFields[(type, field.Key)] = idx++;
                }
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
