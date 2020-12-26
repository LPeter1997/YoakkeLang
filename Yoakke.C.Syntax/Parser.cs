using System.Collections.Generic;
using Yoakke.DataStructures;

namespace Yoakke.C.Syntax
{
    // TODO: Doc
    public class Parser
    {
        private PeekBuffer<Token> tokens;

        public Parser(IEnumerable<Token> tokens)
        {
            this.tokens = new PeekBuffer<Token>(tokens);
        }

        // TODO: Returns

        public void ParseTranslationUnit()
        {
            while (!tokens.IsEnd) ParseExternalDeclaration();
        }

        private void ParseExternalDeclaration()
        {
            // TODO
        }

        private static bool IsTypeQualifier(Token token) =>
               token.Type == TokenType.KwConst
            || token.Type == TokenType.KwRestrict
            || token.Type == TokenType.KwVolatile;

        private static bool IsStorageClassSpecifier(Token token) =>
               token.Type == TokenType.KwTypedef
            || token.Type == TokenType.KwExtern
            || token.Type == TokenType.KwStatic
            || token.Type == TokenType.KwAuto
            || token.Type == TokenType.KwRegister;

        private static bool IsFunctionSpecifier(Token token) =>
            token.Type == TokenType.KwInline;
    }
}
