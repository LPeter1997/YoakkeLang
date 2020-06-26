﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yoakke.Compiler.Ast;

namespace Yoakke.Compiler.Syntax
{
    using Input = ReadOnlySpan<Token>;

    /// <summary>
    /// Implements the parser for the language, that creates the AST from the <see cref="Token"/>s.
    /// </summary>
    class Parser
    {
        private delegate T ParseFunc<T>(ref Input input);

        /// <summary>
        /// Parses the <see cref="IEnumerable{Token}"/> into a program AST.
        /// </summary>
        /// <param name="tokens">The list of <see cref="Token"/>s to parse.</param>
        /// <returns>The parsed AST.</returns>
        public static Declaration.Program ParseProgram(IEnumerable<Token> tokens)
        {
            Input input = tokens.ToArray().AsSpan();
            return ParseProgram(ref input);
        }

        // Declarations ////////////////////////////////////////////////////////

        private static Declaration.Program ParseProgram(ref Input input)
        {
            var declarations = new List<Declaration>();
            while (Peek(input) != TokenType.End)
            {
                declarations.Add(ParseDeclaration(ref input));
            }
            return new Declaration.Program(declarations);
        }

        private static Declaration ParseDeclaration(ref Input input) =>
            Peek(input) switch
            {
                TokenType.KwConst => ParseConstDefinition(ref input),
                _ => throw new ExpectedError("declaration", input[0]),
            };

        private static Declaration ParseConstDefinition(ref Input input)
        {
            Expect(ref input, TokenType.KwConst);
            Expect(ref input, TokenType.Identifier, out var name);

            Expression? type = null;
            if (Match(ref input, TokenType.Colon)) type = ParseExpression(ref input, ExprState.TypeOnly);

            Expect(ref input, TokenType.Assign);
            var value = ParseExpression(ref input, ExprState.None);
            Expect(ref input, TokenType.Semicolon);

            return new Declaration.ConstDef(name, type, value);
        }

        // Statements //////////////////////////////////////////////////////////

        private static Statement ParseStatement(ref Input input)
        {
            switch (Peek(input))
            {
            case TokenType.KwVar: return ParseVarStatement(ref input);
            case TokenType.KwReturn: return ParseReturnStatement(ref input);
            // Greedy consumption of these expressions to avoid ambiguity
            case TokenType.OpenBrace: return new Statement.Expression_(ParseBlockExpression(ref input), false);
            case TokenType.KwIf: return new Statement.Expression_(ParseIfExpression(ref input), false);
            }

            var declaration = TryParse(ref input, ParseDeclaration);
            if (declaration != null) return declaration;

            var expressionStatement = TryParse(ref input, ParseExpressionStatement);
            if (expressionStatement != null) return expressionStatement;

            throw new ExpectedError("statement", input[0]);
        }

        private static Statement ParseVarStatement(ref Input input)
        {
            Expect(ref input, TokenType.KwVar);
            Expect(ref input, TokenType.Identifier, out var name);

            Expression? type = null;
            if (Match(ref input, TokenType.Colon)) type = ParseExpression(ref input, ExprState.TypeOnly);

            Expect(ref input, TokenType.Assign);
            var value = ParseExpression(ref input, ExprState.None);

            Expect(ref input, TokenType.Semicolon);

            return new Statement.VarDef(name, type, value);
        }

        private static Statement ParseReturnStatement(ref Input input)
        {
            Expect(ref input, TokenType.KwReturn);
            
            Expression? value = null;
            if (Peek(input) != TokenType.Semicolon) value = ParseExpression(ref input, ExprState.None);
            
            Expect(ref input, TokenType.Semicolon);

            return new Statement.Return(value);
        }

        private static Statement ParseExpressionStatement(ref Input input)
        {
            var expression = ParseExpression(ref input, ExprState.None);
            Expect(ref input, TokenType.Semicolon);
            return new Statement.Expression_(expression, true);
        }

        // Expressions /////////////////////////////////////////////////////////

        [Flags]
        private enum ExprState
        {
            None = 0,
            TypeOnly = 1,
            NoBraced = 2,
        }

        private static Expression ParseExpression(ref Input input, ExprState state) =>
            ParseBinaryExpression(ref input, state);

        private static Expression ParseBinaryExpression(ref Input input, ExprState state)
        {
            // TODO: For now this is fine, but later we'd need a precedence and associativity table

            var left = ParsePrefixExpression(ref input, state);
            if (!state.HasFlag(ExprState.TypeOnly) && Match(ref input, TokenType.Assign, out var op))
            {
                // '=' is right-associative, recurse
                var right = ParseBinaryExpression(ref input, state);
                // Fold right
                left = new Expression.BinOp(left, op, right);
            }

            return left;
        }

        private static Expression ParsePrefixExpression(ref Input input, ExprState state)
        {
            // NOTE: For now we just use this for extendability
            return ParsePostfixExpression(ref input, state);
        }

        private static Expression ParsePostfixExpression(ref Input input, ExprState state)
        {
            var result = ParseAtomicExpression(ref input, state);
            while (true)
            {
                if (Match(ref input, TokenType.OpenParen))
                {
                    // Call expression
                    var args = new List<Expression>();
                    while (true)
                    {
                        if (Match(ref input, TokenType.CloseParen)) break;
                        args.Add(ParseExpression(ref input, ExprState.None));
                        if (Match(ref input, TokenType.Comma)) continue;

                        Expect(ref input, TokenType.CloseParen);
                        break;
                    }
                    result = new Expression.Call(result, args);
                }
                else if (!state.HasFlag(ExprState.TypeOnly) 
                      && !state.HasFlag(ExprState.NoBraced) 
                      && Match(ref input, TokenType.OpenBrace))
                {
                    var fields = new List<Expression.StructValue.Field>();
                    while (!Match(ref input, TokenType.CloseBrace))
                    {
                        Expect(ref input, TokenType.Identifier, out var name);
                        Expect(ref input, TokenType.Assign);
                        var value = ParseExpression(ref input, ExprState.None);
                        Expect(ref input, TokenType.Semicolon);

                        fields.Add(new Expression.StructValue.Field(name, value));
                    }
                    result = new Expression.StructValue(result, fields);
                }
                else if (Match(ref input, TokenType.Dot))
                {
                    Expect(ref input, TokenType.Identifier, out var ident);
                    result = new Expression.DotPath(result, ident);
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private static Expression ParseAtomicExpression(ref Input input, ExprState state)
        {
            if (Peek(input) == TokenType.KwProc) return ParseProcExpression(ref input, state);
            if (Peek(input) == TokenType.KwIf) return ParseIfExpression(ref input);
            if (Peek(input) == TokenType.KwStruct) return ParseStructTypeExpression(ref input);
            if (!state.HasFlag(ExprState.NoBraced) && Peek(input) == TokenType.OpenBrace) return ParseBlockExpression(ref input);

            // Single-token
            if (Match(ref input, TokenType.Identifier, out var token)) return new Expression.Ident(token);
            if (Match(ref input, TokenType.IntrinsicIdentifier, out token)) return new Expression.Ident(token);
            if (Match(ref input, TokenType.IntLiteral, out token)) return new Expression.IntLit(token);
            if (Match(ref input, TokenType.StringLiteral, out token)) return new Expression.StrLit(token);
            if (Match(ref input, TokenType.KwTrue, out token)) return new Expression.BoolLit(token);
            if (Match(ref input, TokenType.KwFalse, out token)) return new Expression.BoolLit(token);

            throw new ExpectedError("expression", input[0]);
        }

        private static Expression ParseBlockExpression(ref Input input)
        {
            Expect(ref input, TokenType.OpenBrace);
            var statements = new List<Statement>();
            Expression? returnValue = null;
            while (!Match(ref input, TokenType.CloseBrace))
            {
                var statement = TryParse(ref input, ParseStatement);
                if (statement != null)
                {
                    statements.Add(statement);
                    continue;
                }
                var expression = TryParse(ref input, (ref Input i) => ParseExpression(ref i, ExprState.None));
                if (expression != null)
                {
                    returnValue = expression;
                    Expect(ref input, TokenType.CloseBrace);
                    break;
                }
                throw new ExpectedError("'}' or statement", input[0], "code block");
            }
            return new Expression.Block(statements, returnValue);
        }

        private static Expression ParseProcExpression(ref Input input, ExprState state)
        {
            // We just try both the type and the value
            // First the value, as that's the more "elaborate" one

            var input2 = input;

            if (!state.HasFlag(ExprState.TypeOnly))
            {
                var procValue = TryParse(ref input, ParseProcValueExpression);
                if (procValue != null) return procValue;
            }

            var procType = TryParse(ref input, ParseProcTypeExpression);
            if (procType != null)
            {
                // This check eases the errors a bit
                if (((Expression.ProcType)procType).ParameterTypes.Count > 0 || Peek(input) != TokenType.OpenBrace)
                {
                    return procType;
                }
            }

            // NOTE: This is weird (calling it when already failed) but helps us raising better errors
            return state.HasFlag(ExprState.TypeOnly) 
                 ? ParseProcTypeExpression(ref input2) 
                 : ParseProcValueExpression(ref input2);
        }

        private static Expression ParseIfExpression(ref Input input)
        {
            Expect(ref input, TokenType.KwIf);

            var condition = ParseExpression(ref input, ExprState.NoBraced);
            var then = ParseBlockExpression(ref input);

            Expression? els = null;
            if (Match(ref input, TokenType.KwElse))
            {
                if (Peek(input) == TokenType.KwIf) els = ParseIfExpression(ref input);
                else els = ParseBlockExpression(ref input);
            }

            return new Expression.If(condition, then, els);
        }

        private static Expression ParseStructTypeExpression(ref Input input)
        {
            Expect(ref input, TokenType.KwStruct, out var token);
            Expect(ref input, TokenType.OpenBrace);

            var fields = new List<Expression.StructType.Field>();
            var declarations = new List<Declaration>();

            while (!Match(ref input, TokenType.CloseBrace))
            {
                // TODO: Our parsing is flawed, somtimes not the furthest error gets exposed!
                // Instead of hiding failed alternatives, we should collect the furthest ones and return those!

                // First we try a declaration
                var decl = TryParse(ref input, ParseDeclaration);
                if (decl != null)
                {
                    declarations.Add(decl);
                    continue;
                }
                // We parse a field
                // It's in the form of `identifier: Type expression`
                Expect(ref input, TokenType.Identifier, out var ident);
                Expect(ref input, TokenType.Colon);
                var type = ParseExpression(ref input, ExprState.TypeOnly);
                Expect(ref input, TokenType.Semicolon);
                fields.Add(new Expression.StructType.Field(ident, type));
            }

            return new Expression.StructType(token, fields, declarations);
        }

        private static Expression ParseProcValueExpression(ref Input input)
        {
            Expect(ref input, TokenType.KwProc);
            Expect(ref input, TokenType.OpenParen);
            // Parameters
            var parameters = new List<Expression.ProcValue.Parameter>();
            while (true)
            {
                if (Match(ref input, TokenType.CloseParen)) break;
                parameters.Add(ParseProcParameter(ref input));
                if (Match(ref input, TokenType.Comma)) continue;

                Expect(ref input, TokenType.CloseParen);
                break;
            }

            Expression? returnType = null;
            if (Match(ref input, TokenType.Arrow)) returnType = ParseExpression(ref input, ExprState.TypeOnly);

            var body = ParseBlockExpression(ref input);

            return new Expression.ProcValue(parameters, returnType, body);
        }

        private static Expression ParseProcTypeExpression(ref Input input)
        {
            Expect(ref input, TokenType.KwProc);
            Expect(ref input, TokenType.OpenParen);
            // Arguments
            var arguments = new List<Expression>();
            while (true)
            {
                if (Match(ref input, TokenType.CloseParen)) break;
                arguments.Add(ParseExpression(ref input, ExprState.TypeOnly));
                if (Match(ref input, TokenType.Comma)) continue;

                Expect(ref input, TokenType.CloseParen);
                break;
            }

            Expression? returnType = null;
            if (Match(ref input, TokenType.Arrow)) returnType = ParseExpression(ref input, ExprState.TypeOnly);

            return new Expression.ProcType(arguments, returnType);
        }

        private static Expression.ProcValue.Parameter ParseProcParameter(ref Input input)
        {
            Expect(ref input, TokenType.Identifier, out var name);
            Expect(ref input, TokenType.Colon);
            var type = ParseExpression(ref input, ExprState.TypeOnly);
            return new Expression.ProcValue.Parameter(name, type);
        }

        // Helpers /////////////////////////////////////////////////////////////

        private static T? TryParse<T>(ref Input input, ParseFunc<T> parseFunc) where T: class
        {
            try
            {
                var input2 = input;
                var result = parseFunc(ref input2);
                input = input2;
                return result;
            }
            catch (Exception) { }
            return null;
        }

        private static void Expect(ref Input input, TokenType type) =>
            Expect(ref input, type, out var _);

        private static void Expect(ref Input input, TokenType type, out Token token)
        {
            if (!Match(ref input, type, out token))
            {
                throw new ExpectedError(type.ToString(), token);
            }
        }

        private static bool Match(ref Input input, TokenType type) =>
            Match(ref input, type, out var _);

        private static bool Match(ref Input input, TokenType type, out Token token)
        {
            token = input[0];
            if (token.Type == type)
            {
                input = input.Slice(1);
                return true;
            }
            return false;
        }

        private static TokenType Peek(Input input, int ahead = 0) =>
            ahead < input.Length ? input[ahead].Type : TokenType.End;
    }
}
