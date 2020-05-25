using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Syntax
{
    using Input = ReadOnlySpan<Token>;

    class Parser
    {
        public static Statement ParseProgram(IEnumerable<Token> tokens)
        {
            Input input = tokens.ToArray().AsSpan();
            var statements = new List<Statement>();
            while (input[0].Type != TokenType.End)
            {
               statements.Add(ParseStatement(ref input));
            }
            return new BlockExpression(false, statements);
        }

        private static Statement ParseStatement(ref Input input)
        {
            var input2 = input;
            try
            { 
                var declaration = ParseDeclaration(ref input2);
                input = input2;
                return declaration;
            }
            catch (Exception) { }

            var expression = ParseExpression(ref input2);
            input = input2;
            return new ExpressionStatement(expression);
        }

        private static Statement ParseBracedStatement(ref Input input)
        {
            if (!Match(ref input, TokenType.OpenBrace)) throw new NotImplementedException("Expected {!");
            var statements = new List<Statement>();
            while (!Match(ref input, TokenType.CloseBrace))
            {
                statements.Add(ParseStatement(ref input));
            }
            return new BlockExpression(true, statements);
        }

        private static Declaration ParseDeclaration(ref Input input) =>
            input[0].Type switch
            {
                TokenType.KwConst => ParseConstDefinition(ref input),
                _ => throw new NotImplementedException("Expected some declaration!"),
            };

        private static Declaration ParseConstDefinition(ref Input input)
        {
            if (!Match(ref input, TokenType.KwConst, out var _)) throw new NotImplementedException("Expected const!");
            if (!Match(ref input, TokenType.Identifier, out var name)) throw new NotImplementedException("Expected ident!");

            Expression? type = null;
            if (Match(ref input, TokenType.Colon, out var _))
            {
                type = ParseExpression(ref input);
            }

            if (!Match(ref input, TokenType.Assign, out var _)) throw new NotImplementedException("Expected =!");

            var value = ParseExpression(ref input);
            return new ConstDefinition(name, type, value);
        }

        private static Expression ParseExpression(ref Input input) =>
            ParseAtomicExpression(ref input);

        private static Expression ParseAtomicExpression(ref Input input)
        {
            if (input[0].Type == TokenType.KwProc) return ParseProcExpression(ref input);

            if (Match(ref input, TokenType.Identifier, out var token)) return new IdentifierExpression(token);
            if (Match(ref input, TokenType.IntLiteral, out token)) return new IntLiteralExpression(token);

            throw new NotImplementedException("Expected some atomic expression!");
        }

        private static Expression ParseProcExpression(ref Input input)
        {
            if (!Match(ref input, TokenType.KwProc, out var _)) throw new NotImplementedException("Expected proc!");
            if (!Match(ref input, TokenType.OpenParen, out var _)) throw new NotImplementedException("Expected (!");

            // Parameters
            var parameters = new List<ProcExpression.Parameter>();
            {
                while (true)
                {
                    if (Match(ref input, TokenType.CloseParen)) break;
                    parameters.Add(ParseProcParameter(ref input));
                    if (Match(ref input, TokenType.Comma)) continue;

                    if (Match(ref input, TokenType.CloseParen)) break;
                    else throw new NotImplementedException("Expected )!");
                }
            }

            Expression? returnType = null;
            if (Match(ref input, TokenType.Arrow))
            {
                returnType = ParseExpression(ref input);
            }

            var body = ParseBracedStatement(ref input);

            return new ProcExpression(parameters, returnType, body);
        }
    
        private static ProcExpression.Parameter ParseProcParameter(ref Input input)
        {
            if (!Match(ref input, TokenType.Identifier, out var name)) throw new NotImplementedException("Expected ident!");
            if (!Match(ref input, TokenType.Colon)) throw new NotImplementedException("Expected :!");
            var type = ParseExpression(ref input);
            return new ProcExpression.Parameter(name, type);
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
    }
}
