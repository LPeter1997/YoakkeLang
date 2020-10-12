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
        public IDependencySystem System { get; }
        public Builder Builder { get; }

        private TypeTranslator typeTranslator;
        private Dictionary<Expression.Proc, Proc> compiledProcs;
        private Dictionary<Symbol, Value> variablesToRegisters;
        private string? nameHint;

        public Codegen(IDependencySystem system, Builder builder)
        {
            System = system;
            Builder = builder;
            typeTranslator = new TypeTranslator();
            variablesToRegisters = new Dictionary<Symbol, Value>();
            compiledProcs = new Dictionary<Expression.Proc, Proc>();
        }

        public Codegen(IDependencySystem system)
            : this(system, new Builder(new UncheckedAssembly(string.Empty)))
        {
        }

        // External interface //////////////////////////////////////////////////

        private SymbolTable SymbolTable => System.SymbolTable;

        private void TypeCheck(Node node) => System.TypeCheck(node);
        private Semantic.Type TypeOf(Expression expression) => System.TypeOf(expression);
        private Value EvaluateConst(Declaration.Const constDecl) => System.EvaluateConst(constDecl);
        private Value EvaluateConst(Symbol.Const constSym) => System.EvaluateConst(constSym);
        private Semantic.Type EvaluateType(Expression expression) => System.EvaluateType(expression);
        private int FieldIndex(Semantic.Type sty, string name) => typeTranslator.FieldIndex(sty, name);

        // Public interface ////////////////////////////////////////////////////

        public void HintName(string name) => nameHint = name;

        public UncheckedAssembly Generate(Declaration.File file)
        {
            // Rename the assembly
            var parseTreeNode = (Syntax.ParseTree.Declaration.File?)file.ParseTreeNode;
            var fileName = parseTreeNode?.Name ?? "unnamed";
            Builder.Assembly.Name = fileName;
            // For something to be compiled, it has to be type-checked
            TypeCheck(file);
            // If the type-checking succeeded, we can compile
            Visit(file);
            // We close the prelude function
            if (Builder.Assembly.Prelude != null)
            {
                Builder.WithPrelude(b => b.Ret());
            }
            return Builder.Assembly;
        }

        public Proc GenerateEvaluationProc(Expression expr)
        {
            TypeCheck(expr);
            Proc? procValue = null;
            Builder.WithSubcontext(b =>
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

        // Actual code-generation //////////////////////////////////////////////

        private Lir.Types.Type TranslateToLirType(Semantic.Type type) => typeTranslator.ToLirType(type, Builder);

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
                type = EvaluateType(var.Type);
            }
            // Globals and locals are very different
            Value varSpace;
            if (SymbolTable.IsGlobal(var))
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
            Builder.WithSubcontext(b =>
            {
                procVal = Builder.DefineProc(nameHint ?? $"unnamed_proc_{Builder.Assembly.Procedures.Count}");
                nameHint = null;
                // Add it to the cache here to get ready for recursion
                compiledProcs.Add(proc, procVal);
                // Dow now we make every procedure public
                procVal.Visibility = Visibility.Public;
                // We need the return type
                var returnType = Semantic.Type.Unit;
                if (proc.Signature.Return != null)
                {
                    returnType = EvaluateType(proc.Signature.Return);
                }
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
                        variablesToRegisters.Add(symbol, paramSpace);
                    }
                }
                // Now we can compile the body
                Visit(proc.Body);
                // Add a return, if there's none and the return-type is unit
                if (!Builder.CurrentBasicBlock.EndsInBranch && returnType.Equals(Semantic.Type.Unit))
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
                if (retType.Equals(Semantic.Type.Unit))
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
            // Check what kind of symbol it is
            if (symbol is Symbol.Var)
            {
                // Handle the variable
                var reg = variablesToRegisters[symbol];
                // Load the value
                return Builder.Load(reg);
            }
            else
            {
                var constSymbol = (Symbol.Const)symbol;
                return EvaluateConst(constSymbol);
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
            return Builder.Call(proc, args);
        }

        protected override Value? Visit(Expression.Subscript sub)
        {
            // TODO
            //var array = VisitNonNull(sub.Array);
            throw new NotImplementedException();
        }

        protected override Value? Visit(Expression.Binary bin)
        {
            if (bin.Operator == TokenType.Assign)
            {
                var left = Lvalue(bin.Left);
                var right = VisitNonNull(bin.Right);
                // Write out the store
                Builder.Store(left, right);
                return null;
            }
            else if (bin.Operator.IsCompoundAssignment(out var op))
            {
                // TODO: Do proper type-checking, for now we blindly assume builtin operations
                // Here we need to handle the case when there's a user-defined operator!
                var leftPlace = Lvalue(bin.Left);
                var left = Builder.Load(leftPlace);
                var right = VisitNonNull(bin.Right);
                var result = op switch
                {
                    TokenType.Add => Builder.Add(left, right),
                    TokenType.Subtract => Builder.Sub(left, right),
                    TokenType.Multiply => Builder.Mul(left, right),
                    TokenType.Divide => Builder.Div(left, right),
                    TokenType.Modulo => Builder.Mod(left, right),

                    _ => throw new NotImplementedException(),
                };
                Builder.Store(leftPlace, result);
                return null;
            }
            else if (bin.Operator == TokenType.And || bin.Operator == TokenType.Or)
            {
                // TODO: Do proper type-checking, for now we blindly assume builtin operations
                // NOTE: Different case as these are lazy operators
                if (bin.Operator == TokenType.And)
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
                    TokenType.Add      => Builder.Add(left, right),
                    TokenType.Subtract => Builder.Sub(left, right),
                    TokenType.Multiply => Builder.Mul(left, right),
                    TokenType.Divide   => Builder.Div(left, right),
                    TokenType.Modulo   => Builder.Mod(left, right),

                    TokenType.LeftShift  => Builder.Shl(left, right),
                    TokenType.RightShift => Builder.Shr(left, right),

                    TokenType.Bitand => Builder.BitAnd(left, right),
                    TokenType.Bitor  => Builder.BitOr(left, right),
                    TokenType.Bitxor => Builder.BitXor(left, right),

                    TokenType.Equal        => Builder.CmpEq(left, right),
                    TokenType.NotEqual     => Builder.CmpNe(left, right),
                    TokenType.Greater      => Builder.CmpGr(left, right),
                    TokenType.Less         => Builder.CmpLe(left, right),
                    TokenType.GreaterEqual => Builder.CmpGrEq(left, right),
                    TokenType.LessEqual    => Builder.CmpLeEq(left, right),

                    _ => throw new NotImplementedException(),
                };
            }
        }

        protected override Value? Visit(Expression.Prefix pre)
        {
            switch (pre.Operator)
            {
            case TokenType.Add:
                // No-op for numbers
                return VisitNonNull(pre.Operand);

            case TokenType.Subtract:
            {
                // Find the integer type
                var intType = (Lir.Types.Type.Int)((Semantic.Type.Prim)TypeOf(pre.Operand)).Type;
                Debug.Assert(intType.Signed);
                // Multiply by -1
                var sub = VisitNonNull(pre.Operand);
                return Builder.Mul(sub, intType.NewValue(-1));
            }

            case TokenType.Not:
            {
                // Either bitwise or bool not
                var subType = TypeOf(pre.Operand);
                var sub = VisitNonNull(pre.Operand);
                if (subType.Equals(Semantic.Type.Bool))
                {
                    // Bool-not
                    var intType = (Lir.Types.Type.Int)((Semantic.Type.Prim)subType).Type;
                    return Builder.BitXor(sub, intType.NewValue(1));
                }
                else
                {
                    // Assume bitwise not
                    return Builder.BitNot(sub);
                }
            }

            // Address-of
            case TokenType.Bitand: return Lvalue(pre.Operand);

            // Pointer type construction
            case TokenType.Multiply:
            {
                // The first element will be pointer to this expression
                // The second one will be the subtype
                var arrayValues = new List<Value>();
                arrayValues.Add(new Value.User(pre));
                arrayValues.Add(VisitNonNull(pre.Operand));
                var arraySpace = Builder.InitArray(Lir.Types.Type.User_, arrayValues.ToArray());
                // We cast it to a singular user type
                return Builder.Cast(Lir.Types.Type.User_, Builder.Load(arraySpace));
            }

            default: throw new NotImplementedException();
            }
        }

        protected override Value? Visit(Expression.Postfix post)
        {
            switch (post.Operator)
            {
            // Dereference
            case TokenType.Bitnot:
                return Builder.Load(VisitNonNull(post.Operand));

            default: throw new NotImplementedException();
            }
        }

        protected override Value? Visit(Expression.DotPath dot)
        {
            var leftType = TypeOf(dot.Left);
            var left = VisitNonNull(dot.Left);
            if (leftType is Semantic.Type.Struct sty)
            {
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
            // TODO: Now we can properly construct types (like structs)
            // No reason to avoid this!
            throw new NotImplementedException();
        }

        protected override Value? Visit(Expression.ArrayType aty)
        {
            // The first element will be a pointer to this expression
            // The second one will be the length
            // The third will be the array element type
            var arrayValues = new List<Value>();
            arrayValues.Add(new Value.User(aty));
            arrayValues.Add(VisitNonNull(aty.Length));
            arrayValues.Add(VisitNonNull(aty.ElementType));
            var arraySpace = Builder.InitArray(Lir.Types.Type.User_, arrayValues.ToArray());
            // We cast it to a singular user type
            return Builder.Cast(Lir.Types.Type.User_, Builder.Load(arraySpace));
        }

        protected override Value? Visit(Expression.StructType sty)
        {
            // For struct types we create an array of N+1 user types, where N is the number of fields
            // The first element will be pointer to this expression
            // The rest are the actual field types
            var arrayValues = new List<Value>();
            arrayValues.Add(new Value.User(sty));
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

        private Value Lvalue(Expression expression)
        {
            switch (expression)
            {
            case Expression.Identifier ident:
            {
                var symbol = SymbolTable.ReferredSymbol(ident);
                return variablesToRegisters[symbol];
            }

            case Expression.DotPath dotPath:
            {
                var leftType = (Semantic.Type.Struct)TypeOf(dotPath.Left);
                var left = Lvalue(dotPath.Left);
                var index = FieldIndex(leftType, dotPath.Right);
                return Builder.ElementPtr(left, index);
            }

            case Expression.Postfix postfix when postfix.Operator == TokenType.Bitnot:
                return Builder.Load(Lvalue(postfix.Operand));

            default: throw new NotImplementedException();
            }
        }
    }
}
