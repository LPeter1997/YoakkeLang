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

            /*
             Plan for proper steps after this:
               For each expression we need it's type.
               For types, we might need to evaluate constants.
               For constants we might need to evaluate types, which means this whole thing is circular, can't really separate steps.
               To deduce types, we need to enforce the strongest inference possible immediately, so that can't be a separate step either.
               If something has a constant value, it **must also have a type along with it**.

               We want signatures to be fully typed, so a procedure's signature must be fully typed, before we even consider it's body.
               We want each procedure body to be fully typed, so each overload and type variable must be fully determined when we are
               about to leave the function body.
             */

            // For every constant definition they call EvaluateConst.Evaluate and store the result in the symbol.
            //AssignConstantSymbol.Assign(program);

            // For every expression it tries to assign some type.
            //AssignType.Assign(program);
            
            // Does type unification where needed.
            //InferType.Infer(program);

            // Finally, EvaluateConst.Evaluate is a mess!
        }
    }
}
