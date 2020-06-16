﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yoakke.Ast;
using Yoakke.IR;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Enforces type-checking rules in the program.
    /// </summary>
    static class TypeCheck
    {
        /// <summary>
        /// Type-checks the given <see cref="Statement"/>.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to type-check.</param>
        public static void Check(Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                foreach (var decl in program.Declarations) Check(decl);
                break;

            case Declaration.ConstDef constDef:
                // Check subelements
                if (constDef.Type != null) Check(constDef.Type);
                Check(constDef.Value);
                // Let constant evaluation do the work here
                ConstEval.Evaluate(constDef);
                break;

            case Statement.Expression_ expression:
            {
                // Check subelement
                Check(expression.Expression);
                // NOTE: This is one of the most important calls here, as most of our type-safety rules are enforced by 
                // evaluating every expression's type!
                // The only other major place comes from unifying return type and block return type for procedures.
                // We force evaluation here to ensure checks
                var ty = TypeEval.Evaluate(expression.Expression);
                // An expression in a statement's position must produce a unit type, if it's not terminated by a semicolon
                if (!expression.HasSemicolon) Type.Unit.Unify(ty);
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Type-checks a <see cref="Expression.StructValue"/>.
        /// </summary>
        /// <param name="structValue">The <see cref="Expression.StructValue"/> to type-check.</param>
        public static void Check(Expression.StructValue structValue)
        {
            // We need to get the struct type
            var structType = ConstEval.EvaluateAsType(structValue.StructType);
            // TODO: Is this OK? Can we enforce that it's a struct right here?
            if (!(structType is Type.Struct ty)) throw new NotImplementedException("Not a struct!");
            // Check if all fields are initialized to their correct type
            var uninitFields = ty.Fields.Keys.ToHashSet();
            foreach (var field in structValue.Fields)
            {
                var fieldName = field.Item1.Value;
                if (!uninitFields.Remove(fieldName))
                {
                    // Something is wrong with the name, either double-initialization or unknown field
                    if (ty.Fields.ContainsKey(fieldName)) throw new NotImplementedException("Double field init!");
                    else throw new NotImplementedException("Struct has no field!");
                }
                // Unify type with the initialization value's type
                var fieldType = ty.Fields[fieldName];
                TypeEval.Evaluate(field.Item2).Unify(fieldType);
            }
            // Check if we have uninitialized fields remaining
            if (uninitFields.Count > 0)
            {
                throw new NotImplementedException("Uninitialized field!");
            }
        }

        private static void Check(Expression expression)
        {
            switch (expression)
            {
            // Nothing to do, leaf elements
            case Expression.IntLit _:
            case Expression.StrLit _:
            case Expression.Ident _:
            case Expression.Intrinsic _:
                break;

            case Expression.StructType structType:
                // Just check field types
                foreach (var (_, type) in structType.Fields) Check(type);
                break;

            case Expression.StructValue structValue:
            {
                // Check field values
                foreach (var (_, value) in structValue.Fields) Check(value);
                Check(structValue);
            }
            break;

            case Expression.ProcType procType:
                // Just check subelements
                foreach (var param in procType.ParameterTypes) Check(param);
                if (procType.ReturnType != null) Check(procType.ReturnType);
                break;

            case Expression.Proc proc:
            {
                // Check argument and return type
                foreach (var param in proc.Parameters) Check(param.Type);
                if (proc.ReturnType != null) Check(proc.ReturnType);
                // Before checking the body, we assign each parameter symbol it's proper type
                foreach (var param in proc.Parameters)
                {
                    Assert.NonNull(param.Symbol);
                    Debug.Assert(param.Symbol.Type == null);
                    param.Symbol.Type = ConstEval.EvaluateAsType(param.Type);
                }
                // Now we can check the body
                Check(proc.Body);
                // Unify the return type with the body's return type
                var procRetTy = proc.ReturnType == null ? Type.Unit : ConstEval.EvaluateAsType(proc.ReturnType);
                var bodyRetTy = TypeEval.Evaluate(proc.Body);
                procRetTy.Unify(bodyRetTy);
            }
            break;

            case Expression.Block block:
                // Just check subelements
                foreach (var stmt in block.Statements) Check(stmt);
                if (block.Value != null) Check(block.Value);
                break;

            case Expression.Call call:
                // Just check subelements
                Check(call.Proc);
                foreach (var arg in call.Arguments) Check(arg);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}