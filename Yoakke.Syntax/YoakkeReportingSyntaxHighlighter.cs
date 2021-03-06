﻿using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.Reporting.Render;
using Yoakke.Syntax.Error;
using Yoakke.Text;

namespace Yoakke.Syntax
{
    /// <summary>
    /// An <see cref="ISyntaxHighlighter"/> for Yoakke's syntax.
    /// </summary>
    public class YoakkeReportingSyntaxHighlighter : ISyntaxHighlighter
    {
        private static readonly TokenKind[] tokenTypeLut = new TokenKind[]
        {
            TokenKind.Other, TokenKind.Other,
            TokenKind.Comment,
            
            TokenKind.Punctuation, TokenKind.Punctuation, 
            TokenKind.Punctuation, TokenKind.Punctuation, 
            TokenKind.Punctuation, TokenKind.Punctuation,

            TokenKind.Punctuation, TokenKind.Punctuation, TokenKind.Punctuation, TokenKind.Punctuation, TokenKind.Punctuation,

            TokenKind.Operator, TokenKind.Operator, TokenKind.Operator, TokenKind.Operator,
            TokenKind.Operator, TokenKind.Operator, TokenKind.Operator, TokenKind.Operator,
            TokenKind.Operator, TokenKind.Operator, TokenKind.Operator, TokenKind.Operator,
            TokenKind.Operator, TokenKind.Operator, TokenKind.Operator, TokenKind.Operator,
            TokenKind.Operator, TokenKind.Operator, TokenKind.Operator, TokenKind.Operator,
            TokenKind.Operator, TokenKind.Operator, TokenKind.Operator, TokenKind.Operator,
            TokenKind.Operator, TokenKind.Operator,

            TokenKind.Name,
            TokenKind.Literal, TokenKind.Literal,

            TokenKind.Keyword, TokenKind.Keyword, TokenKind.Keyword,
            TokenKind.Literal, TokenKind.Literal,
            TokenKind.Keyword, TokenKind.Keyword, TokenKind.Keyword, TokenKind.Keyword, TokenKind.Keyword,
        };

        public IEnumerable<ColoredToken> GetHighlightingForLine(SourceFile sourceFile, int lineIndex)
        {
            var line = sourceFile.Line(lineIndex).TrimEnd().ToString();
            var lineSource = new SourceFile(sourceFile.Path, line);
            return new Lexer(lineSource).Lex()
                .Select(t => new ColoredToken
                { 
                    StartIndex = t.Span.Start.Column,
                    Length = t.Span.End.Column - t.Span.Start.Column,
                    Kind = TokenTypeToKind(t.Type),
                });
        }

        private TokenKind TokenTypeToKind(TokenType tokenType) => tokenTypeLut[(int)tokenType];
    }
}
