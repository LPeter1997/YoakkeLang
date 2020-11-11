using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        private class ProcContext
        {
            public IDictionary<Symbol, Value> Variables { get; set; } = new Dictionary<Symbol, Value>();
        }

        public IDependencySystem System { get; }
        public Builder Builder { get; }

        private CollectDependencies elimDepProcs;
        private Dictionary<Expression.Proc, Proc> compiledProcs = new Dictionary<Expression.Proc, Proc>();
        private ProcContext globalContext = new ProcContext();
        private ProcContext context = new ProcContext();
        private string? nameHint;
        private int constCnt;

        public Codegen(IDependencySystem system, Builder builder)
        {
            System = system;
            Builder = builder;
            elimDepProcs = new CollectDependencies(system);
        }

        public Codegen(IDependencySystem system)
            : this(system, new Builder(new UncheckedAssembly(string.Empty)))
        {
        }

        // External interface //////////////////////////////////////////////////

        private SymbolTable SymbolTable => System.SymbolTable;

        private void TypeCheck(Node node) => System.TypeCheck(node);
        private Semantic.Types.Type TypeOf(Expression expression) => System.TypeOf(expression);
        private Value EvaluateConst(Declaration.Const constDecl) => System.EvaluateConst(constDecl);
        private Value EvaluateConst(Symbol.Const constSym) => System.EvaluateConst(constSym);
        private Semantic.Types.Type EvaluateType(Expression expression) => System.EvaluateType(expression);
        private TypeTranslator TypeTranslator => System.TypeTranslator;
        private int FieldIndex(Semantic.Types.Type sty, string name) => TypeTranslator.FieldIndex(sty, name);
        private Lir.Types.Type TranslateToLirType(Semantic.Types.Type type) => TypeTranslator.ToLirType(type);

        // Public interface ////////////////////////////////////////////////////

        public void HintName(string name) => nameHint = name;

        public UncheckedAssembly Generate(Declaration.File file)
        {
            // Rename the assembly
            var parseTreeNode = (Syntax.ParseTree.Declaration.File?)file.ParseTreeNode;
            var fileName = parseTreeNode?.Name ?? "unnamed";
            Builder.Assembly.Name = fileName;
            file = new ElimDependencies(System).Elim(file);
            // Eliminate dependent procedures
            //file = elimDepProcs.Elim(file);
            /*new DefineScope(SymbolTable).Define(file);
            new DeclareSymbol(SymbolTable).Declare(file);
            new ResolveSymbol(SymbolTable).Resolve(file);*/
            // For something to be compiled, it has to be type-checked
            TypeCheck(file);
            // If the type-checking succeeded, we can compile
            Visit(file);
            return Builder.Assembly;
        }

        public Proc GenerateEvaluationProc(Expression expr)
        {
            TypeCheck(expr);
            Proc? procValue = null;
            WithSubcontext(() =>
            {
                procValue = Builder.DefineProc($"expr_eval_{Builder.Assembly.Procedures.Count}");
                // We need the return type
                var returnType = TypeOf(expr);
                procValue.Return = TranslateToLirType(returnType);
                // Compile and return the body
                var result = VisitNonNull(expr);
                Builder.Ret(result);
            });
            Debug.Assert(procValue != null);
            return procValue;
        }

        [DebuggerStepThrough]
        private void WithSubcontext(Action action)
        {
            var oldContext = context;
            context = new ProcContext();
            Builder.WithSubcontext(v => action());
            context = oldContext;
        }

        // Actual code-generation //////////////////////////////////////////////

        protected override Value? Visit(Declaration.Const cons)
        {
            var constType = System.TypeOf(cons.Value);
            if (constType is Semantic.Types.Type.Proc procType && procType.GetDependency() != null)
            {
                // We don't compile this, contains dependent types
                return null;
            }
            EvaluateConst(cons);
            return null;
        }

        protected override Value? Visit(Statement.Var var)
        {
            var symbol = (Symbol.Var)SymbolTable.DefinedSymbol(var);
            Debug.Assert(symbol.Type != null);
            var type = symbol.Type;
            // Globals and locals are very different
            Value varSpace;
            if (symbol.Kind == Symbol.VarKind.Global)
            {
                // Global variable
                varSpace = Builder.DefineGlobal(var.Name, TranslateToLirType(type));
                if (var.Value != null)
                {
                    // Assign initial value in the startup code
                    Builder.WithPrelude(b =>
                    {
                        var initialValue = VisitNonNull(var.Value);
                        b.Store(varSpace, initialValue);
                    });
                }
                // Associate with symbol
                globalContext.Variables.Add(symbol, varSpace);
            }
            else
            {
                // Local variable
                // Allocate space
                varSpace = Builder.Alloc(TranslateToLirType(type));
                if (var.Value != null)
                {
                    // We also need to assign the value
                    var value = VisitNonNull(var.Value);
                    Builder.Store(varSpace, value);
                }
                // Associate with symbol
                context.Variables.Add(symbol, varSpace);
            }
            return null;
        }

        protected override Value? Visit(Statement.Return ret)
        {
            if (ret.Value == null)
            {
                // No return value
                Builder.Ret();
            }
            else
            {
                // We also need to compile the return value
                var value = VisitNonNull(ret.Value);
                Builder.Ret(value);
            }
            return null;
        }

        protected override Value? Visit(Expression.Proc proc)
        {
            // It it's cached, just return that
            if (compiledProcs.TryGetValue(proc, out var procVal)) return procVal;
            WithSubcontext(() =>
            {
                procVal = Builder.DefineProc(nameHint ?? $"unnamed_proc_{Builder.Assembly.Procedures.Count}");
                nameHint = null;
                // Add it to the cache here to get ready for recursion
                compiledProcs.Add(proc, procVal);
                // For now we make every procedure public
                procVal.Visibility = Visibility.Public;
                // We need the return type
                Debug.Assert(proc.Signature.Return != null);
                var returnType = EvaluateType(proc.Signature.Return);
                procVal.Return = TranslateToLirType(returnType);
                // We need to compile parameters
                foreach (var param in proc.Signature.Parameters)
                {
                    // Get the parameter type, define it in the Lir code
                    var paramType = EvaluateType(param.Type);
                    var lirParamType = TranslateToLirType(paramType);
                    var paramValue = Builder.DefineParameter(lirParamType);
                    // We make parameters mutable by making them allocate space on the stack and refer to that space
                    var paramSpace = Builder.Alloc(lirParamType);
                    // Copy the initial value
                    Builder.Store(paramSpace, paramValue);
                    if (param.Name != null)
                    {
                        // It has a symbol, we store the allocated space associated
                        var symbol = SymbolTable.DefinedSymbol(param);
                        context.Variables.Add(symbol, paramSpace);
                    }
                }
                // Now we can compile the body
                Visit(proc.Body);
                // Add a return, if there's none and the return-type is unit
                if (!Builder.CurrentBasicBlock.EndsInBranch && returnType.Equals(Semantic.Types.Type.Unit))
                {
                    Builder.Ret();
                }
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
                Builder.IfThen(
                    condition: b => VisitNonNull(iff.Condition),
                    then: b => Visit(iff.Then));
                return null;
            }
            else
            {
                var retType = TypeOf(iff);
                if (retType.Equals(Semantic.Types.Type.Unit))
                {
                    // There is no return value
                    Builder.IfThenElse(
                        condition: b => VisitNonNull(iff.Condition),
                        then: b => Visit(iff.Then),
                        @else: b => Visit(iff.Else));
                    return null;
                }
                else
                {
                    // There is a return value we need to take care of
                    // First we allocate space for the return value
                    var retSpace = Builder.Alloc(TranslateToLirType(retType));
                    // Compile it, storing the results in the respective blocks
                    Builder.IfThenElse(
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
                    return Builder.Load(retSpace);
                }
            }
        }

        protected override Value? Visit(Expression.While whil)
        {
            Builder.While(
                condition: b => VisitNonNull(whil.Condition),
                body: b => Visit(whil.Body));
            return null;
        }

        protected override Value? Visit(Expression.Identifier ident)
        {
            // Get the referred symbol
            var symbol = SymbolTable.ReferredSymbol(ident);
            return CompileSymbol(symbol);
        }

        protected override Value? Visit(Expression.Literal lit) => lit.Type switch
        {
            Expression.LitType.Integer => Lir.Types.Type.I32.NewValue(int.Parse(lit.Value)),
            Expression.LitType.Bool => Lir.Types.Type.I32.NewValue(lit.Value == "true" ? 1 : 0),
            Expression.LitType.String => CompileStringLit(lit.Value),
            _ => throw new NotImplementedException(),
        };

        private Value CompileStringLit(string str)
        {
            // Cut off quotes
            str = str.Substring(1, str.Length - 2);
            // TODO: Escape
            // Null-terminate
            str += '\0';
            // Encode as UTF8
            var utf8bytes = Encoding.UTF8.GetBytes(str);
            // Convert to lir type and value
            var arrayType = new Lir.Types.Type.Array(Lir.Types.Type.U8, utf8bytes.Length);
            var arrayValue = new Value.Array(
                arrayType, 
                utf8bytes.Select(b => (Value)Lir.Types.Type.U8.NewValue(b)).ToList().AsValueList());
            // Define the new constant
            var constant = Builder.DefineConst($"yk_str_const_{constCnt++}", arrayValue);
            // Figure out the struct type
            var structType = System.ReferToConstType("@c", "str");
            var lirStructType = TranslateToLirType(structType);
            // Instantiate the struct
            var structSpace = Builder.InitStruct(lirStructType, new KeyValuePair<int, Value>[] 
            { 
                new KeyValuePair<int, Value>(0, Builder.Cast(new Lir.Types.Type.Ptr(Lir.Types.Type.U8), constant)),
                new KeyValuePair<int, Value>(1, Lir.Types.Type.U32.NewValue(utf8bytes.Length - 1)),
            });
            // Load it
            return Builder.Load(structSpace);
        }

        protected override Value? Visit(Expression.Call call)
        {
            // Simply compile the procedure
            var proc = VisitNonNull(call.Procedure);
            // Then the args
            var args = call.Arguments.Select(arg => VisitNonNull(arg)).ToList();
            // And write the call
            return Builder.Call(proc, args);
        }

        protected override Value? Visit(Expression.Subscript sub)
        {
            var arrayType = (Semantic.Types.Type.Array)TypeOf(sub.Array);
            var elementType = TranslateToLirType(arrayType.ElementType);
            var array = Builder.Cast(new Lir.Types.Type.Ptr(elementType), Lvalue(sub.Array));
            var index = VisitNonNull(sub.Index);
            return Builder.Load(Builder.Add(array, index));
        }

        protected override Value? Visit(Expression.Binary bin)
        {
            if (bin.Operator == Expression.BinOp.Assign)
            {
                var left = Lvalue(bin.Left);
                var right = VisitNonNull(bin.Right);
                // Write out the store
                Builder.Store(left, right);
                return null;
            }
            else if (Expression.CompoundBinaryOperators.TryGetValue(bin.Operator, out var op))
            {
                // TODO: Do proper type-checking, for now we blindly assume builtin operations
                // Here we need to handle the case when there's a user-defined operator!
                var leftPlace = Lvalue(bin.Left);
                var left = Builder.Load(leftPlace);
                var right = VisitNonNull(bin.Right);
                var result = op switch
                {
                    Expression.BinOp.Add => Builder.Add(left, right),
                    Expression.BinOp.Subtract => Builder.Sub(left, right),
                    Expression.BinOp.Multiply => Builder.Mul(left, right),
                    Expression.BinOp.Divide => Builder.Div(left, right),
                    Expression.BinOp.Modulo => Builder.Mod(left, right),

                    _ => throw new NotImplementedException(),
                };
                Builder.Store(leftPlace, result);
                return null;
            }
            else if (bin.Operator == Expression.BinOp.And || bin.Operator == Expression.BinOp.Or)
            {
                // TODO: Do proper type-checking, for now we blindly assume builtin operations
                // NOTE: Different case as these are lazy operators
                if (bin.Operator == Expression.BinOp.And)
                {
                    return Builder.LazyAnd(b => VisitNonNull(bin.Left), b => VisitNonNull(bin.Right));
                }
                else
                {
                    return Builder.LazyOr(b => VisitNonNull(bin.Left), b => VisitNonNull(bin.Right));
                }
            }
            else
            {
                // TODO: Do proper type-checking, for now we blindly assume builtin operations
                // Here we need to handle the case when there's a user-defined operator!
                var left = VisitNonNull(bin.Left);
                var right = VisitNonNull(bin.Right);
                return bin.Operator switch
                {
                    Expression.BinOp.Add      => Builder.Add(left, right),
                    Expression.BinOp.Subtract => Builder.Sub(left, right),
                    Expression.BinOp.Multiply => Builder.Mul(left, right),
                    Expression.BinOp.Divide   => Builder.Div(left, right),
                    Expression.BinOp.Modulo   => Builder.Mod(left, right),

                    Expression.BinOp.LeftShift  => Builder.Shl(left, right),
                    Expression.BinOp.RightShift => Builder.Shr(left, right),

                    Expression.BinOp.BitAnd => Builder.BitAnd(left, right),
                    Expression.BinOp.BitOr  => Builder.BitOr(left, right),
                    Expression.BinOp.BitXor => Builder.BitXor(left, right),

                    Expression.BinOp.Equals       => Builder.CmpEq(left, right),
                    Expression.BinOp.NotEquals    => Builder.CmpNe(left, right),
                    Expression.BinOp.Greater      => Builder.CmpGr(left, right),
                    Expression.BinOp.Less         => Builder.CmpLe(left, right),
                    Expression.BinOp.GreaterEqual => Builder.CmpGrEq(left, right),
                    Expression.BinOp.LessEqual    => Builder.CmpLeEq(left, right),

                    _ => throw new NotImplementedException(),
                };
            }
        }

        protected override Value? Visit(Expression.Unary ury)
        {
            switch (ury.Operator)
            {
            case Expression.UnaryOp.Ponote:
                // No-op for numbers
                return VisitNonNull(ury.Operand);

            case Expression.UnaryOp.Negate:
            {
                // Find the integer type
                var intType = (Lir.Types.Type.Int)((Semantic.Types.Type.Prim)TypeOf(ury.Operand)).Type;
                Debug.Assert(intType.Signed);
                // Multiply by -1
                var sub = VisitNonNull(ury.Operand);
                return Builder.Mul(sub, intType.NewValue(-1));
            }

            case Expression.UnaryOp.Not:
            {
                // Either bitwise or bool not
                var subType = TypeOf(ury.Operand);
                var sub = VisitNonNull(ury.Operand);
                if (subType.Equals(Semantic.Types.Type.Bool))
                {
                    // Bool-not
                    var intType = (Lir.Types.Type.Int)((Semantic.Types.Type.Prim)subType).Type;
                    return Builder.BitXor(sub, intType.NewValue(1));
                }
                else
                {
                    // Assume bitwise not
                    return Builder.BitNot(sub);
                }
            }

            // Address-of
            case Expression.UnaryOp.AddressOf: 
                return Lvalue(ury.Operand);

            // Pointer type construction
            case Expression.UnaryOp.PointerType:
            {
                // The first element will be pointer to this expression
                // The second one will be the subtype
                var arrayValues = new List<Value>();
                arrayValues.Add(new Value.User(ury));
                arrayValues.Add(VisitNonNull(ury.Operand));
                var arraySpace = Builder.InitArray(Lir.Types.Type.User_, arrayValues.ToArray());
                // We cast it to a singular user type
                return Builder.Cast(Lir.Types.Type.User_, Builder.Load(arraySpace));
            }

            case Expression.UnaryOp.Dereference:
                return Builder.Load(VisitNonNull(ury.Operand));

            default: throw new NotImplementedException();
            }
        }

        protected override Value? Visit(Expression.DotPath dot)
        {
            var leftType = TypeOf(dot.Left);
            if (leftType.Equals(Semantic.Types.Type.Type_))
            {
                // Static member access
                var leftValue = System.EvaluateType(dot.Left);
                Debug.Assert(leftValue.DefinedScope != null);
                var symbol = leftValue.DefinedScope.Reference(dot.Right);
                return CompileSymbol(symbol);
            }
            else if (leftType is Semantic.Types.Type.Struct sty)
            {
                var left = VisitNonNull(dot.Left);
                // Since we need a pointer, we need to write the struct to memory
                var leftSpace = Builder.Alloc(TranslateToLirType(leftType));
                Builder.Store(leftSpace, left);
                // We need the field index
                var index = FieldIndex(sty, dot.Right);
                // Get the proper pointer
                var ptr = Builder.ElementPtr(leftSpace, index);
                // Load it
                return Builder.Load(ptr);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected override Value? Visit(Expression.ProcSignature sign)
        {
            // The first element will be a pointer to this expression
            // From the second it's the parameter types
            // Finally the return type
            var arrayValues = new List<Value>();
            arrayValues.Add(new Value.User(sign));
            var paramTypes = sign.Parameters
                .Select(p => Builder.Cast(Lir.Types.Type.User_, VisitNonNull(p.Type)));
            arrayValues.AddRange(paramTypes);
            // Return type
            Debug.Assert(sign.Return != null);
            arrayValues.Add(VisitNonNull(sign.Return));
            var arraySpace = Builder.InitArray(Lir.Types.Type.User_, arrayValues.ToArray());
            // We cast it to a singular user type
            return Builder.Cast(Lir.Types.Type.User_, Builder.Load(arraySpace));
        }

        protected override Value? Visit(Expression.ArrayType aty)
        {
            // The first element will be a pointer to this expression
            // The second one will be the length
            // The third will be the array element type
            var arrayValues = new List<Value>();
            arrayValues.Add(new Value.User(aty));
            arrayValues.Add(Builder.Cast(Lir.Types.Type.User_, VisitNonNull(aty.Length)));
            arrayValues.Add(VisitNonNull(aty.ElementType));
            var arraySpace = Builder.InitArray(Lir.Types.Type.User_, arrayValues.ToArray());
            // We cast it to a singular user type
            return Builder.Cast(Lir.Types.Type.User_, Builder.Load(arraySpace));
        }

        protected override Value? Visit(Expression.StructType sty)
        {
            var refSymbols = new CollectLocalRefs(System).Collect(sty);

            // For struct types we create an array of N+1 user types, where N is the number of fields
            // The first element will be pointer to this expression
            // The second is an array of pair of (variable symbol, value) used
            // The rest are the actual field types
            var arrayValues = new List<Value>();
            arrayValues.Add(new Value.User(sty));
            // Evaluate and store ref-ed symbols
            var refSymbolValues = new List<Value>();
            foreach (var symbol in refSymbols)
            {
                // Pair of symbol, value of symbol
                var pair = new Value[] { new Value.User(symbol), Builder.Cast(Lir.Types.Type.User_, CompileSymbol(symbol)) };
                var subarray = Builder.InitArray(Lir.Types.Type.User_, pair);
                refSymbolValues.Add(Builder.Cast(Lir.Types.Type.User_, Builder.Load(subarray)));
            }
            arrayValues.Add(Builder.Cast(Lir.Types.Type.User_, Builder.Load(Builder.InitArray(Lir.Types.Type.User_, refSymbolValues.ToArray()))));
            foreach (var field in sty.Fields) arrayValues.Add(VisitNonNull(field.Type));
            var arraySpace = Builder.InitArray(Lir.Types.Type.User_, arrayValues.ToArray());
            // We cast it to a singular user type
            return Builder.Cast(Lir.Types.Type.User_, Builder.Load(arraySpace));
        }

        protected override Value? Visit(Expression.StructValue sval)
        {
            var structType = EvaluateType(sval.StructType);
            var lirType = TranslateToLirType(structType);
            // Use the initializer to allocate and initialize space
            var structSpace = Builder.InitStruct(
                structType: lirType,
                fieldValues: sval.Fields.Select(field => new KeyValuePair<int, Value>(
                    key: FieldIndex(structType, field.Name), 
                    value: VisitNonNull(field.Value))));
            return Builder.Load(structSpace);
        }

        private Value CompileSymbol(Symbol symbol)
        {
            if (symbol is Symbol.Var varSym)
            {
                if (globalContext.Variables.TryGetValue(symbol, out var globReg))
                {
                    // Load the value
                    return Builder.Load(globReg);
                }
                else if (context.Variables.TryGetValue(symbol, out var reg))
                {
                    // Load the value
                    return Builder.Load(reg);
                }
                else if (varSym.Kind == Symbol.VarKind.Param)
                {
                    // Dependent read
                    return new Value.User(new Semantic.Types.Type.Dependent(varSym));
                }
                else
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }
            else
            {
                var constSymbol = (Symbol.Const)symbol;
                return EvaluateConst(constSymbol);
            }
        }

        private Value Lvalue(Expression expression)
        {
            switch (expression)
            {
            case Expression.Identifier ident:
            {
                var symbol = SymbolTable.ReferredSymbol(ident);
                if (globalContext.Variables.TryGetValue(symbol, out var globalReg)) return globalReg;
                return context.Variables[symbol];
            }

            case Expression.DotPath dotPath:
            {
                var leftType = (Semantic.Types.Type.Struct)TypeOf(dotPath.Left);
                var left = Lvalue(dotPath.Left);
                var index = FieldIndex(leftType, dotPath.Right);
                return Builder.ElementPtr(left, index);
            }

            case Expression.Unary ury when ury.Operator == Expression.UnaryOp.Dereference:
                return VisitNonNull(ury.Operand);

            case Expression.Subscript sub:
            {
                var arrayType = (Semantic.Types.Type.Array)TypeOf(sub.Array);
                var elementType = TranslateToLirType(arrayType.ElementType);
                var array = Builder.Cast(new Lir.Types.Type.Ptr(elementType), Lvalue(sub.Array));
                var index = VisitNonNull(sub.Index);
                return Builder.Add(array, index);
            }

            default: throw new NotImplementedException();
            }
        }
    }
}
