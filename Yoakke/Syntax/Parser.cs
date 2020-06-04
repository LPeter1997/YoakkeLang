using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Syntax
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
        public static ProgramDeclaration ParseProgram(IEnumerable<Token> tokens)
        {
            Input input = tokens.ToArray().AsSpan();
            return ParseProgram(ref input);
        }

        // Declarations ////////////////////////////////////////////////////////

        private static ProgramDeclaration ParseProgram(ref Input input)
        {
            var declarations = new List<Declaration>();
            while (Peek(input) != TokenType.End)
            {
               declarations.Add(ParseDeclaration(ref input));
            }
            return new ProgramDeclaration(declarations);
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
            if (Match(ref input, TokenType.Colon)) type = ParseExpression(ref input);

            Expect(ref input, TokenType.Assign);
            var value = ParseExpression(ref input);

            return new ConstDefinition(name, type, value);
        }

        // Statements //////////////////////////////////////////////////////////

        private static Statement ParseStatement(ref Input input)
        {
            var declaration = TryParse(ref input, ParseDeclaration);
            if (declaration != null) return declaration;
            
            var expressionStatement = TryParse(ref input, ParseExpressionStatement);
            if (expressionStatement != null) return expressionStatement;

            throw new ExpectedError("statement", input[0]);
        }

        private static Statement ParseExpressionStatement(ref Input input)
        {
            var expression = ParseExpression(ref input);
            if (!IsBracedExpression(expression))
            {
                // ';' required
                Expect(ref input, TokenType.Semicolon);
            }
            return new ExpressionStatement(expression);
        }

        private static bool IsBracedExpression(Expression expression) =>
            expression is BlockExpression;

        // Expressions /////////////////////////////////////////////////////////

        private static Expression ParseExpression(ref Input input) =>
            ParseAtomicExpression(ref input);

        private static Expression ParseAtomicExpression(ref Input input)
        {
            if (Peek(input) == TokenType.KwProc) return ParseProcExpression(ref input);
            if (Peek(input) == TokenType.OpenBrace) return ParseBlockExpression(ref input);

            if (Match(ref input, TokenType.Identifier, out var token)) return new IdentifierExpression(token);
            if (Match(ref input, TokenType.IntLiteral, out token)) return new IntLiteralExpression(token);

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
                var expression = TryParse(ref input, ParseExpression);
                if (expression != null)
                {
                    returnValue = expression;
                    Expect(ref input, TokenType.CloseBrace);
                    break;
                }
                throw new ExpectedError("'}' or statement", input[0], "code block");
            }
            if (   returnValue == null 
                && statements.Count > 0 
                && statements[statements.Count - 1] is ExpressionStatement expr
                && IsBracedExpression(expr.Expression))
            {
                statements.RemoveAt(statements.Count - 1);
                returnValue = expr.Expression;
            }
            return new BlockExpression(statements, returnValue);
        }

        private static Expression ParseProcExpression(ref Input input)
        {
            Expect(ref input, TokenType.KwProc);
            Expect(ref input, TokenType.OpenParen);
            // Parameters
            var parameters = new List<ProcExpression.Parameter>();
            {
                while (true)
                {
                    if (Match(ref input, TokenType.CloseParen)) break;
                    parameters.Add(ParseProcParameter(ref input));
                    if (Match(ref input, TokenType.Comma)) continue;

                    Expect(ref input, TokenType.CloseParen);
                    break;
                }
            }

            Expression? returnType = null;
            if (Match(ref input, TokenType.Arrow)) returnType = ParseExpression(ref input);

            var body = ParseBlockExpression(ref input);
            return new ProcExpression(parameters, returnType, body);
        }
    
        private static ProcExpression.Parameter ParseProcParameter(ref Input input)
        {
            Expect(ref input, TokenType.Identifier, out var name);
            Expect(ref input, TokenType.Colon);
            var type = ParseExpression(ref input);
            return new ProcExpression.Parameter(name, type);
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

        private static TokenType Peek(Input input) =>
            input[0].Type;
    }
}
