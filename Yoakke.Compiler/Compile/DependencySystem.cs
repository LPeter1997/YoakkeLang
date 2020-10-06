﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Lir;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// The standard, global <see cref="IDependencySystem"/>.
    /// </summary>
    public class DependencySystem : IDependencySystem
    {
        public SymbolTable SymbolTable { get; }

        private Codegen codegen;
        private Dictionary<Symbol.Const, Value> constValues = new Dictionary<Symbol.Const, Value>();
        private string? procNameHint = null;
        private Dictionary<(Type.Struct, string), int> fieldIndices = new Dictionary<(Type.Struct, string), int>();
        private Dictionary<Symbol.Var, Type> varTypes = new Dictionary<Symbol.Var, Type>();

        public DependencySystem(SymbolTable symbolTable)
        {
            SymbolTable = symbolTable;
            codegen = new Codegen(this);
        }

        public Assembly? Compile(Declaration.File file, BuildStatus status)
        {
            var asm = codegen.Generate(file, status);
            if (status.Errors.Count > 0)
            {
                // TODO: Debug dump
                Console.WriteLine(asm);
                return null;
            }
            return asm;
        }

        public Type TypeOf(Expression expression)
        {
            // TODO
            if (expression is Expression.Proc)
            {
                return new Type.Proc(new ValueList<Type> { }, Type.I32);
            }
            if (expression is Expression.If || expression is Expression.Call)
            {
                return Type.I32;
            }
            if (expression is Expression.Literal lit)
            {
                if (lit.Type == TokenType.IntLiteral) return Type.I32;
                if (lit.Type == TokenType.KwTrue) return Type.Bool;
                if (lit.Type == TokenType.KwFalse) return Type.Bool;
            }
            if (expression is Expression.StructType sty)
            {
                return Type.Type_;
            }
            if (expression is Expression.StructValue sval)
            {
                return EvaluateType(sval.StructType);
            }
            if (expression is Expression.Identifier ident)
            {
                var symbol = SymbolTable.ReferredSymbol(ident);
                if (symbol is Symbol.Const constSym)
                {
                    // TODO
                    throw new NotImplementedException();
                }
                if (symbol is Symbol.Var varSym)
                {
                    return varTypes[varSym];
                }
            }
            throw new NotImplementedException();
        }

        public void TypeCheck(Statement statement)
        {
            // TODO: Assume correct
        }

        public Value Evaluate(Expression expression)
        {
            // TODO: We could mostly avoid special casing?
            // TODO: We need to capture local variables at evaluation for things like generics to work
            // TODO: A local context should be passed that's used for cacheing!
            if (expression is Expression.Proc procExpr)
            {
                Debug.Assert(procNameHint != null);
                var procName = procNameHint;
                procNameHint = null;
                return codegen.Generate(procExpr, procName);
            }
            else if (expression is Expression.StructType structType)
            {
                // TODO: We could probably remove this case and just let evaluation go
                Debug.Assert(structType.ParseTreeNode != null);
                var parseTreeNode = (Syntax.ParseTree.Expression.StructType)structType.ParseTreeNode;
                int fieldCount = 0;
                var resultType = new Type.Struct(
                    parseTreeNode.KwStruct, 
                    structType.Fields.ToDictionary(
                        field => field.Name ?? $"unnamed_field_{fieldCount++}",
                        field => EvaluateType(field.Type)
                    ).AsValueDictionary());
                return new Value.User(resultType);
            }
            else
            {
                // TODO: This should not be part of the final assembly!
                // We need to erase it before finishing compiling the file

                // TODO: It's also kinda expensive to just instantiate a new VM for the whole assembly
                // Can't we just track partially what this expression needs and include that?
                // We could also just have a VM that could build code incrementally
                // Like appending unknown procedures and such

                // It's an unknown expression we have to evaluate
                // We compile the expression into an evaluation procedure, run it through the VM and return the result
                var proc = codegen.GenerateEvaluationProc(expression);
                var status = new BuildStatus();
                var asm = codegen.Builder.Assembly.Check(status);
                if (status.Errors.Count > 0)
                {
                    throw new NotImplementedException();
                }
                var vm = new VirtualMachine(asm);
                return vm.Execute(proc, new Value[] { });
            }
        }

        public Value EvaluateConst(Declaration.Const constDecl)
        {
            var symbol = (Symbol.Const)SymbolTable.DefinedSymbol(constDecl);
            // Check if there's a pre-stored value
            if (symbol.Value != null) return symbol.Value;
            // We need to evaluate based on the definition
            // Check if it's cached
            if (!constValues.TryGetValue(symbol, out var value))
            {
                // Not cached, evaluate and then cache
                if (constDecl.Value is Expression.Proc)
                {
                    procNameHint = constDecl.Name;
                }
                value = Evaluate(constDecl.Value);
                // NOTE: We check here again because of recursion
                if (!constValues.ContainsKey(symbol))
                {
                    constValues.Add(symbol, value);
                }
            }
            return value;
        }

        public Value EvaluateConst(Symbol.Const constSym)
        {
            if (constSym.Value != null) return constSym.Value;
            Debug.Assert(constSym.Definition != null);
            var constDecl = (Declaration.Const)constSym.Definition;
            return EvaluateConst(constDecl);
        }

        public Type EvaluateType(Expression expression)
        {
            if (expression is Expression.Identifier ident)
            {
                var symbol = SymbolTable.ReferredSymbol(ident);
                if (symbol is Symbol.Const constSymbol)
                {
                    var value = EvaluateConst(constSymbol);
                    if (value is Value.User uvalue)
                    {
                        if (uvalue.Payload is Type type) return type;

                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            throw new NotImplementedException();
        }

        public Lir.Types.Type TranslateToLirType(Type type)
        {
            // TODO: Check cache?
            switch (type)
            {
            case Type.Prim prim: return prim.Type;

            case Type.Struct sty:
            {
                var fields = sty.Fields.Values.Select(TranslateToLirType);
                var result = codegen.Builder.DefineStruct(fields);
                int index = 0;
                foreach (var field in sty.Fields)
                {
                    // TODO: We use this because the type is translated multiple times for some reason...
                    fieldIndices[(sty, field.Key)] = index;
                    ++index;
                }
                return result;
            }

            default: throw new NotImplementedException();
            }
        }

        public int FieldIndex(Type.Struct structType, string name) => fieldIndices[(structType, name)];

        public void SetVarType(Symbol.Var varSym, Type type) => varTypes[varSym] = type;
    }
}
