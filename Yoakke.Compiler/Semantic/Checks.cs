using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Yoakke.Compiler.Ast;
using Yoakke.Compiler.Syntax;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// All semantic checks grouped.
    /// </summary>
    static class Checks
    {
        /// <summary>
        /// Does the entire semantic checking for the whole AST.
        /// </summary>
        /// <param name="program">The root AST to perform the semantic checks on.</param>
        public static void CheckAll(Declaration.Program program)
        {
            // Setting up the symbol table /////////////////////////////////////

            var symbolTable = new SymbolTable();

            symbolTable.DefineBuiltinType("type", Type.Type_);
            symbolTable.DefineBuiltinType("i32", Type.I32);
            symbolTable.DefineBuiltinType("bool", Type.Bool);

            symbolTable.DefineBuiltinConstant("@extern", new Value.IntrinsicProc(
                new Type.Proc(new List<Type> { Type.Str, Type.Type_ }, Type.Any_),
                args =>
                {
                    // TODO: Help type assertions
                    Debug.Assert(args.Count == 2);
                    var symbolName = (Value.Primitive<string>)args[0];
                    var symbolType = (Type)args[1];
                    return new Value.Extern(symbolName.Value, symbolType);
                }));

            // TODO: Serious API improvements are needed for this to be defined nicely
            // TODO: Some hacky construction
            // Source for prelude
            var preludeSource = new Source("prelude", string.Empty);
            var preludePosition = new Position(preludeSource, 0, 0);
            var irScope = new Scope(ScopeTag.None, symbolTable.GlobalScope);
            symbolTable.DefineBuiltinConstant("@ir", 
                new Type.Struct(
                    new Token(preludePosition, TokenType.KwStruct, "struct"),
                    new Dictionary<string, Type>(),
                    irScope));
            irScope.Define(new Symbol.Const("integer", new Value.IntrinsicProc(
                new Type.Proc(new List<Type> { Type.Str, Type.I32, Type.Bool }, Type.Type_),
                args =>
                {
                    // TODO: Help type assertions
                    Debug.Assert(args.Count == 3);
                    var name = (Value.Primitive<string>)args[0];
                    var bits = (Value.Primitive<BigInteger>)args[1];
                    var signed = (Value.Primitive<bool>)args[2];
                    return new Type.Primitive(name.Value, new IR.Type.Int(signed.Value, (int)bits.Value));
                })));
            // End of abomination ///////////////////////////////////////////////////

            // Actual checks ///////////////////////////////////////////////////

            // Step one
            // ========
            // Preconditions: 
            //   None.
            // Description:
            //   Assigns each node it's corresponding scope.
            // Postconditions:
            //   Every node has Scope != null.
            DeclareScope.Declare(symbolTable, program);

            // Step one
            // ========
            // Preconditions: 
            //   Every node has Scope != null.
            // Description:
            //   Creates and registers a symbol for each constant definition.
            // Postconditions:
            //   Every node has Scope != null.
            //   Every ConstDefinition has a Symbol != null.
            DeclareSymbol.Declare(program);

            // Step two
            // ========
            // Preconditions: 
            //   Every order-independent symbol must be defined already.
            //   Every node has Scope != null.
            // Description:
            //   Defines each order-dependent symbol.
            //   Also resolves every symbol reference.
            // Postconditions:
            //   Every node has Symbol != null.
            DefineSymbol.Define(program);

            // Step three
            // ==========
            // This is a complicated step, so no description yet, as things are subject to change here.
            TypeCheck.Check(program);
        }
    }
}
