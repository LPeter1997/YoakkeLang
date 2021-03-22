using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Dependency;
using Yoakke.Syntax;
using Yoakke.Syntax.Error;
using Yoakke.Text;

namespace Yoakke.Compiler.Services.Impl
{
    internal class SyntaxService : ISyntaxService
    {
        // TODO: Move AST into the compiler as it's kind of an internal thing

#pragma warning disable CS8618
        [QueryGroup]
        public IInputService Input { get; set; }

        [QueryGroup]
        public ISyntaxService Syntax { get; set; }
#pragma warning restore CS8618

        public event EventHandler<ISyntaxError>? OnError;

        public IValueList<Token> LexTokens(string path)
        {
            var sourceText = Input.SourceText(path);
            // TODO: This will cause re-splitting the file each time
            // We could have SourceFile instances elsewhere to incrementally handle that too
            var sourceFile = new SourceFile(path, sourceText);
            var lexer = new Lexer(sourceFile);
            lexer.SyntaxError += ReportError;
            var tokens = lexer.Lex().ToList().AsValueList();
            lexer.SyntaxError -= ReportError;
            return tokens;
        }

        public Syntax.ParseTree.Declaration.File ParseFile(string path)
        {
            var tokens = Syntax.LexTokens(path);
            var parser = new Parser(tokens);
            parser.SyntaxError += ReportError;
            var parseTree = parser.ParseFile();
            parser.SyntaxError -= ReportError;
            return parseTree;
        }

        public Syntax.Ast.Declaration.File ParseFileToAst(string path)
        {
            var parseTree = Syntax.ParseFile(path);
            var ast = ParseTreeToAst.Convert(parseTree);
            return ast;
        }

        public Syntax.Ast.Declaration.File ParseFileToDesugaredAst(string path)
        {
            var ast = Syntax.ParseFileToAst(path);
            // TODO: Bad pattern, ctor + call
            // Shouldn't we publish just a public static Desugaring.Desugar?
            var desugaredAst = new Desugaring().Desugar(ast);
            return desugaredAst;
        }

        private void ReportError(object? sender, ISyntaxError syntaxError) => 
            OnError?.Invoke(this, syntaxError);
    }
}
