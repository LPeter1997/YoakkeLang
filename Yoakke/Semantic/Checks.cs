using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Semantic
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

            symbolTable.DefineIntrinsicFunction("@extern",
                new Type.Proc(new List<Type> { Type.Str, Type.Type_ }, Type.Any_),
                args =>
                {
                    // TODO: Help type assertions
                    Debug.Assert(args.Count == 2);
                    var symbolName = (Value.Str)args[0];
                    var symbolType = (Type)args[1];
                    return new Value.Extern(symbolName.Value, symbolType);
                });

            // Actual checks ///////////////////////////////////////////////////

            // Step one
            // ========
            // Preconditions: 
            //   None.
            // Description:
            //   Assigns each node it's corresponding scope.
            //   Also creates and registers a symbol for each constant definition.
            // Postconditions:
            //   Every node has Scope != null.
            //   Every ConstDefinition has a Symbol != null.
            DeclareSymbol.Declare(symbolTable, program);

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

            // TODO: Doc
            TypeCheck.Check(program);
        }
    }
}
